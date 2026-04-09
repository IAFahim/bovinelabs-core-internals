BlobHashMap - Read-Only Hash Maps Embedded Entirely Inside BlobAssetReference Data
==================================================================================

Overview
--------

BlobHashMap<TKey, TValue> is a read-only hash map that lives entirely inside
Unity's BlobAssetReference storage. It is constructed at bake time using a
BlobBuilder and then frozen into a contiguous block of unmanaged memory that
can be accessed at runtime from Burst-compiled jobs with zero allocations.

The design follows a chained-bucket approach similar to Unity's
NativeParallelHashMap but adapted so that all pointers become relative offsets
(BlobArray<T>), making the entire structure relocatable and serialisable.

Architecture Diagram
--------------------

         BlobAssetReference<BlobHashMap<TKey,TValue>>
         ┌──────────────────────────────────────────────────────────────────┐
         │ BlobAssetHeader  (Unity internal)                               │
         │   ┌─────────────────────────────────┐                           │
         │   │ Hash │ Length │ Allocator │ ValidationPtr                    │
         │   └─────────────────────────────────┘                           │
         ├──────────────────────────────────────────────────────────────────┤
         │ BlobHashMap<TKey, TValue>                                        │
         │   ┌───────────────────────────────────────────────────────────┐  │
         │   │ Data: BlobHashMapData<TKey, TValue>                      │  │
         │   │                                                          │  │
         │   │  ┌─ Values ──────────────────────────────────────────┐   │  │
         │   │  │  BlobArray<TValue>  [0]   [1]   [2]   ... [N-1]   │   │  │
         │   │  │  offset+length     val₀  val₁  val₂  ... valₙ₋₁  │   │  │
         │   │  └───────────────────────────────────────────────────┘   │  │
         │   │                                                          │  │
         │   │  ┌─ Keys ─────────────────────────────────────────────┐   │  │
         │   │  │  BlobArray<TKey>    [0]   [1]   [2]   ... [N-1]   │   │  │
         │   │  │                     k₀    k₁    k₂    ... kₙ₋₁   │   │  │
         │   │  └───────────────────────────────────────────────────┘   │  │
         │   │                                                          │  │
         │   │  ┌─ Next ────────────────────────────────────────────┐   │  │
         │   │  │  BlobArray<int>    [0]   [1]   [2]   ... [N-1]   │   │  │
         │   │  │  (chain links)     -1    0     -1    ...  2      │   │  │
         │   │  └───────────────────────────────────────────────────┘   │  │
         │   │                                                          │  │
         │   │  ┌─ Buckets ─────────────────────────────────────────┐   │  │
         │   │  │  BlobArray<int>    [0]   [1]   [2]  ... [B-1]    │   │  │
         │   │  │  (head indices)    3    -1     0    ... -1       │   │  │
         │   │  │  length = ceilpow2(capacity * ratio)             │   │  │
         │   │  └───────────────────────────────────────────────────┘   │  │
         │   │                                                          │  │
         │   │  ┌─ Count ─────────────┐                                 │  │
         │   │  │  BlobArray<int>[0]  │  actual number of entries      │  │
         │   │  │  (single element)   │  (≤ capacity)                  │  │
         │   │  └─────────────────────┘                                 │  │
         │   │                                                          │  │
         │   │  BucketCapacityMask: int  (= Buckets.Length - 1)        │  │
         │   └───────────────────────────────────────────────────────────┘  │
         └──────────────────────────────────────────────────────────────────┘


Lookup Algorithm (TryGetValue)
------------------------------

  Input: TKey key
  Output: Ptr<TValue> item (pointer into blob storage)

  ┌─────────────────────────────────────────────────────────┐
  │  1.  bucket = key.GetHashCode() & BucketCapacityMask    │
  │                        │                                │
  │                        ▼                                │
  │  2.  index = Buckets[bucket]                            │
  │         ┌────────────────────────────┐                  │
  │         │  if index < 0 → NOT FOUND  │                  │
  │         │  (bucket is empty)         │                  │
  │         └────────────────────────────┘                  │
  │                        │                                │
  │                        ▼  index >= 0                    │
  │  3.  ┌─────────── LOOP ───────────┐                    │
  │      │  Keys[index] == key?        │                    │
  │      │     YES → return &Values[index]                  │
  │      │     NO  → index = Next[index]                    │
  │      │           ┌──────────────────────────┐           │
  │      │           │  if index < 0 → NOT FOUND│           │
  │      │           └──────────────────────────┘           │
  │      └──────────────────────────────────┘               │
  └─────────────────────────────────────────────────────────┘


Concrete Example: 4 entries, bucket ratio 2, 8 buckets
-------------------------------------------------------

  Insert: (42, "alpha"), (10, "beta"), (74, "gamma"), (26, "delta")
  Hashes: 42→6, 10→2, 74→2*, 26→2**  (assume modulo 8)

  Buckets[0..7]:  [-1, -1, 3, -1, -1, -1, 0, -1]
                          ↑              ↑
                     bucket 2         bucket 6

  Keys:   [42, 10, 74, 26]
  Values: ["alpha", "beta", "gamma", "delta"]
  Next:   [-1,  2,  -1,  -1]
                ↑
           chain: 10→74

  Lookup key=74:
    bucket = 74.GetHashCode() & 7 = 2
    index = Buckets[2] = 3
    Keys[3]=26 ≠ 74 → index = Next[3] = -1 → NOT FOUND (different hash)

  Lookup key=74 (hash=2):
    bucket = 2
    index = Buckets[2] = 3
    Keys[3]=26 ≠ 74 → Next[3] = -1 → miss

  [In practice keys 10,74,26 have different hashes; collisions only
   occur when two keys land in the same bucket slot.]


Class Hierarchy
---------------

  ┌───────────────────────┐
  │  BlobHashMap<TKey,TValue>    ← public read-only map
  │  ┌───────────────────┐ │
  │  │ Data: BlobHashMapData  │  ← internal shared storage
  │  └───────────────────┘ │
  └───────────┬───────────┘
              │ shares
  ┌───────────▼───────────┐
  │ BlobMultiHashMap<TKey,TValue>  ← public multi-value map
  │  ┌───────────────────┐ │
  │  │ Data: BlobHashMapData  │  ← same internal layout
  │  └───────────────────┘ │
  └───────────────────────┘


Construction Flow (Build Time)
------------------------------

  NativeParallelHashMap<TKey,TValue>
                │
                │  ConstructHashMap()
                ▼
  ┌─────────────────────────────────────────────┐
  │  BlobBuilderExtensions                      │
  │    └─ AllocateHashMap(capacity, ratio)      │
  │         │                                   │
  │         ▼                                   │
  │    BlobBuilderHashMap<TKey,TValue>          │
  │      └─ new BlobBuilderHashMapData(...)     │
  │           │                                 │
  │           ├─ Allocate Values[capacity]      │
  │           ├─ Allocate Keys[capacity]        │
  │           ├─ Allocate Next[capacity]        │
  │           ├─ Allocate Buckets[ceilpow2(     │
  │           │       capacity * ratio)]        │
  │           ├─ Allocate Count[1]              │
  │           └─ Clear: Buckets→-1, Next→-1     │
  │                                              │
  │    Then Add(k,v) per entry:                  │
  │      bucket = hash & mask                    │
  │      keys[count] = k                         │
  │      values[count] = v                       │
  │      next[count] = buckets[bucket]           │
  │      buckets[bucket] = count++               │
  └─────────────────────────────────────────────┘
                │
                │  CreateBlobAssetReference()
                ▼
         BlobAssetReference<BlobHashMap<TKey,TValue>>
         (frozen, contiguous, read-only)


Enumeration Pattern
-------------------

  BlobHashMapEnumerator iterates by walking bucket slots:

  ┌─────────────────────────────────────────────────────┐
  │  bucketIndex = 0                                     │
  │  nextIndex = -1                                      │
  │                                                      │
  │  MOVE NEXT:                                          │
  │    if nextIndex != -1:                                │
  │      → follow chain (Next[nextIndex])                │
  │    else:                                             │
  │      → scan Buckets[bucketIndex..] for non -1 entry  │
  │      → return index, set nextIndex = Next[index]     │
  │      → advance bucketIndex                           │
  └─────────────────────────────────────────────────────┘


Key Design Decisions
--------------------

1. **Chained bucket with index-based linking.**  The `Next` array stores the
   index of the next entry in the same bucket chain.  Using array indices
   rather than raw pointers keeps the data relocatable inside BlobAsset
   storage where all references are relative offsets.

2. **Count stored as BlobArray<int> of length 1.**  BlobAsset fields must be
   BlobArray, BlobPtr, or plain blittable types -- there is no way to write
   to a scalar int field after the initial `ConstructRoot`.  Wrapping count
   in a single-element BlobArray lets the builder increment it via
   `BlobBuilderArray<int>`.

3. **BucketCapacityMask instead of modulo.**  The bucket count is always a
   power of two (via `math.ceilpow2`), so `hash & mask` replaces the
   expensive `%` operation with a single bitwise AND.

4. **Adaptive bucket ratio.**  For capacities ≤ 16384 the ratio defaults to 3
   (3× more buckets than entries); above that it drops to 2.  More buckets
   means fewer collisions and faster lookups at the cost of memory.

5. **Shared data between HashMap and MultiHashMap.**  Both BlobHashMap and
   BlobMultiHashMap wrap the same BlobHashMapData core.  The difference is
   only in the builder: BlobBuilderHashMap rejects duplicate keys;
   BlobBuilderMultiHashMap accepts them.

6. **Ptr<TValue> return type.**  TryGetValue returns `Ptr<TValue>` (a safe
   raw pointer wrapper) rather than `ref TValue` because the value lives in
   frozen blob storage and callers need a stable reference they can hold.


Performance Characteristics
---------------------------

| Operation          | Time Complexity     | Notes                           |
|--------------------|---------------------|---------------------------------|
| TryGetValue        | O(1) amortized      | Single hash + chain walk        |
| ContainsKey        | O(1) amortized      | Same as TryGetValue             |
| this[key]          | O(1) amortized      | TryGetValue + throw on miss     |
| GetEnumerator      | O(n)                | Walks all bucket chains         |
| Construction       | O(n)                | Per-element Add during baking   |

Memory overhead:
  - Keys array:    capacity × sizeof(TKey)
  - Values array:  capacity × sizeof(TValue)
  - Next array:    capacity × 4 bytes
  - Buckets array: ceilpow2(capacity × ratio) × 4 bytes
  - Count array:   4 bytes
  - Mask:           4 bytes
  - Total:         capacity × (sizeof(TKey) + sizeof(TValue) + 4)
                   + ceilpow2(capacity × ratio) × 4  + 8

Burst-compatible:   Yes (100% unmanaged, no managed allocations)
Thread-safe reads:  Yes (immutable after construction)
Cache-friendly:     Moderate (bucket chains may scatter, but all data
                    is within the same contiguous blob allocation)

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobHashMap.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections"
> ```

```
BlobHashMap<TKey,TValue>
  Kind: struct, 44 bytes
  Generic params: 2 (TKey, TValue)

  Fields:
    [0] BlobHashMapData`2 Data (private)

  Properties:
    Int32 Count { get }
    Int32& Item { get }

  Methods:
    Boolean TryGetValue(Int32 key, Ptr`1& out item)
    Boolean ContainsKey(Int32 key)
    BlobHashMapEnumerator`2 GetEnumerator()

  Indexer: this[Int32] -> Int32& { get }


Verified: 7 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMap.cs)
- [BovineLabs.Core.Tests/Collections/Blobs/BlobHashMapTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/Blobs/BlobHashMapTests.cs)
- [BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMapEnumerator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMapEnumerator.cs)
