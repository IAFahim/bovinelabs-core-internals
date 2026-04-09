BlobHashMapData - Core Unmanaged Memory Layout for Blob-Based Hash Maps
========================================================================

Overview
--------

BlobHashMapData<TKey, TValue> is the internal shared storage engine that
underlies both BlobHashMap and BlobMultiHashMap.  It implements a chained
open-addressing scheme where parallel arrays (Keys, Values, Next, Buckets)
are all embedded as BlobArray<T> inside a single contiguous blob allocation.

This struct is never used directly by consumers -- it is accessed through
the public BlobHashMap or BlobMultiHashMap wrapper types, which delegate
all lookup and iteration to BlobHashMapData.

Architecture: Full Memory Layout
---------------------------------

  BlobAssetReference (contiguous memory)
  ╔═══════════════════════════════════════════════════════════════════════╗
  ║  BlobAssetHeader                                                     ║
  ║  ┌─────────┬────────┬──────────────┬────────────────┐               ║
  ║  │  Hash   │ Length │  Allocator   │ ValidationPtr  │               ║
  ║  │ (uint)  │ (int)  │ (Allocator)  │   (byte*)      │               ║
  ║  └─────────┴────────┴──────────────┴────────────────┘               ║
  ╠═══════════════════════════════════════════════════════════════════════╣
  ║  BlobHashMapData<TKey, TValue>   (or embedded in BlobHashMap/Multi)  ║
  ║                                                                      ║
  ║  Offset  Field              Type             Description             ║
  ║  ──────  ─────────────────  ───────────────  ────────────────────    ║
  ║                                                                      ║
  ║  0x000   Values             BlobArray<TValue>                        ║
  ║          ┌─────────────────────────────────────────────────────────┐ ║
  ║          │  ┌─ BlobArray Header (8 bytes) ──────────────────────┐  │ ║
  ║          │  │  int m_Offset    (relative to this field)         │  │ ║
  ║          │  │  int m_Length    (= capacity)                     │  │ ║
  ║          │  └───────────────────────────────────────────────────┘  │ ║
  ║          │  ┌─ Data (capacity × sizeof(TValue)) ────────────────┐  │ ║
  ║          │  │  [0]     [1]     [2]     ...  [capacity-1]       │  │ ║
  ║          │  │  TValue  TValue  TValue  ...  TValue             │  │ ║
  ║          │  └───────────────────────────────────────────────────┘  │ ║
  ║          └─────────────────────────────────────────────────────────┘ ║
  ║                                                                      ║
  ║  0x008   Keys               BlobArray<TKey>                         ║
  ║          ┌─────────────────────────────────────────────────────────┐ ║
  ║          │  [offset, length] + data[capacity]                      │ ║
  ║          │  TKey[0]  TKey[1]  TKey[2]  ...  TKey[capacity-1]     │ ║
  ║          └─────────────────────────────────────────────────────────┘ ║
  ║                                                                      ║
  ║  0x010   Next               BlobArray<int>                          ║
  ║          ┌─────────────────────────────────────────────────────────┐ ║
  ║          │  [offset, length] + data[capacity]                      │ ║
  ║          │  Chain links: Next[i] = next index in same bucket,      │ ║
  ║          │              or -1 if end of chain                       │ ║
  ║          │  int[0]  int[1]  int[2]  ...  int[capacity-1]          │ ║
  ║          └─────────────────────────────────────────────────────────┘ ║
  ║                                                                      ║
  ║  0x018   Buckets            BlobArray<int>                          ║
  ║          ┌─────────────────────────────────────────────────────────┐ ║
  ║          │  [offset, length] + data[bucketCapacity]                │ ║
  ║          │  Head pointers: Buckets[b] = first index in chain,     │ ║
  ║          │                  or -1 if bucket is empty               │ ║
  ║          │  bucketCapacity = ceilpow2(capacity × ratio)           │ ║
  ║          │  int[0]  int[1]  ...  int[bucketCapacity-1]            │ ║
  ║          └─────────────────────────────────────────────────────────┘ ║
  ║                                                                      ║
  ║  0x020   Count              BlobArray<int>  (length=1)             ║
  ║          ┌─────────────────────────────────────────────────────────┐ ║
  ║          │  [offset, length=1] + data[1]                           │ ║
  ║          │  Count[0] = actual number of entries (≤ capacity)       │ ║
  ║          └─────────────────────────────────────────────────────────┘ ║
  ║                                                                      ║
  ║  0x028   BucketCapacityMask int                                      ║
  ║          ┌─────────────────────────────────────────────────────────┐ ║
  ║          │  = Buckets.Length - 1  (always power-of-2 minus 1)     │ ║
  ║          │  Used for: bucket = hash & BucketCapacityMask           │ ║
  ║          └─────────────────────────────────────────────────────────┘ ║
  ╚═══════════════════════════════════════════════════════════════════════╝


Chained Bucket Scheme (Visual)
-------------------------------

  After inserting 6 entries into a map with capacity=8, 8 buckets:

  Hash function: key.GetHashCode() & 7  (mask = 7, 8 buckets)

  Entries inserted (index → key, value):
    0: key=42, val="A"    hash=42&7=2  → bucket 2
    1: key=17, val="B"    hash=17&7=1  → bucket 1
    2: key=58, val="C"    hash=58&7=2  → bucket 2 (collision with 42!)
    3: key=33, val="D"    hash=33&7=1  → bucket 1 (collision with 17!)
    4: key=99, val="E"    hash=99&7=3  → bucket 3
    5: key=10, val="F"    hash=10&7=2  → bucket 2 (triple collision!)

  Buckets array (head pointers):
    ┌───┬───┬───┬───┬───┬───┬───┬───┐
    │ -1│ 3 │ 5 │ 4 │-1 │-1 │-1 │-1 │
    └───┴─┬─┴─┬─┴─┬─┴───┴───┴───┴───┘
     B0    B1  B2  B3  B4  B5  B6  B7

  Chains (follow Next links):
    Bucket 1: 3 → 1 → -1     (keys: 33, 17)
    Bucket 2: 5 → 2 → 0 → -1 (keys: 10, 58, 42)
    Bucket 3: 4 → -1          (key: 99)

  Keys:     [42, 17, 58, 33, 99, 10, __, __]
  Values:   [ A,  B,  C,  D,  E,  F, __, __]
  Next:     [-1, -1,  0, -1, -1,  2, __, __]
                        ↑         ↑
                   58→42 chain  10→58 chain

  Lookup key=58:
    bucket = 58.GetHashCode() & 7 = 2
    index = Buckets[2] = 5
    Keys[5]=10 ≠ 58 → Next[5]=2
    Keys[2]=58 == 58 → FOUND! return &Values[2] = "C"


TryGetFirstValue Algorithm (Step by Step)
------------------------------------------

  ┌──────────────────────────────────────────────────────────────┐
  │  TryGetFirstValue(TKey key, out Ptr<TValue> item,           │
  │                    out BlobMultiHashMapIterator<TKey> it)    │
  │                                                              │
  │  1.  it.Key = key                                           │
  │  2.  if BucketCapacityMask < 0:                             │
  │        (map was never populated)                             │
  │        it.NextIndex = -1; item = default; return false      │
  │                                                              │
  │  3.  bucket = key.GetHashCode() & BucketCapacityMask        │
  │  4.  it.NextIndex = Buckets[bucket]                         │
  │  5.  return TryGetNextValue(out item, ref it)               │
  └──────────────────────────────┬───────────────────────────────┘
                                 │
                                 ▼
  ┌──────────────────────────────────────────────────────────────┐
  │  TryGetNextValue(out Ptr<TValue> item,                      │
  │                  ref BlobMultiHashMapIterator<TKey> it)      │
  │                                                              │
  │  index = it.NextIndex                                       │
  │  it.NextIndex = -1    (consume the link)                    │
  │                                                              │
  │  if index < 0 → return false  (end of chain / not found)    │
  │                                                              │
  │  ┌─────── LOOP ──────────────────────────┐                  │
  │  │  Keys[index].Equals(it.Key)?           │                  │
  │  │    YES:                                │                  │
  │  │      it.NextIndex = Next[index]        │                  │
  │  │      item = Ptr(&Values[index])        │                  │
  │  │      return true                       │                  │
  │  │    NO:                                 │                  │
  │  │      index = Next[index]               │                  │
  │  │      if index < 0 → return false       │                  │
  │  └────────────────────────────────────────┘                  │
  └──────────────────────────────────────────────────────────────┘

  For BlobHashMap: TryGetFirstValue is called once, chain walk finds match.
  For BlobMultiHashMap: TryGetNextValue is called repeatedly to get all
  values for the same key.


Builder Construction Detail (BlobBuilderHashMapData)
-----------------------------------------------------

  ┌───────────────────────────────────────────────────────────────────┐
  │  BlobBuilderHashMapData<TKey, TValue>  (ref struct)              │
  │                                                                   │
  │  Stored in builder (NOT in blob):                                 │
  │    KeyCapacity        int          (= entries capacity)           │
  │    bucketCapacityMask int          (= Buckets.Length - 1)         │
  │                                                                   │
  │  Builder array handles (write-through to blob chunks):            │
  │    values  BlobBuilderArray<TValue>   → writes to blob Values     │
  │    keys    BlobBuilderArray<TKey>     → writes to blob Keys       │
  │    next    BlobBuilderArray<int>      → writes to blob Next       │
  │    buckets BlobBuilderArray<int>      → writes to blob Buckets    │
  │    count   BlobBuilderArray<int>      → writes to blob Count      │
  │                                                                   │
  │  Constructor(capacity, ratio, ref blobBuilder, ref BlobHashMapData│
  │    bucketCap = ceilpow2(capacity × ratio)                         │
  │    mask = bucketCap - 1                                           │
  │    data.BucketCapacityMask = mask  (set on the blob data)         │
  │    Allocate all arrays                                            │
  │    Clear: Buckets→-1, Next→-1                                     │
  │                                                                   │
  │  TryAdd(key, value, multi):                                       │
  │    ┌────────────────────────────────────────────────────────┐     │
  │    │  count_ref = &count[0]                                 │     │
  │    │  if count_ref >= KeyCapacity → FULL (throw in checks)  │     │
  │    │                                                        │     │
  │    │  bucket = key.GetHashCode() & bucketCapacityMask       │     │
  │    │                                                        │     │
  │    │  if !multi && ContainsKey(bucket, key):                │     │
  │    │    return false  (duplicate key rejected)              │     │
  │    │                                                        │     │
  │    │  index = count_ref++                                   │     │
  │    │  keys[index] = key                                    │     │
  │    │  values[index] = value                                │     │
  │    │  next[index] = buckets[bucket]   // link to old head  │     │
  │    │  buckets[bucket] = index         // new head          │     │
  │    │  return true                                           │     │
  │    └────────────────────────────────────────────────────────┘     │
  │                                                                   │
  │  Insert visualized:                                               │
  │    Before Add(42, "A"):                                           │
  │      Buckets[2] = -1                                              │
  │    After:                                                         │
  │      Keys[0] = 42, Values[0] = "A", Next[0] = -1                 │
  │      Buckets[2] = 0         (head of chain)                      │
  │                                                                   │
  │    Before Add(58, "C"):  (hash=2, same bucket)                    │
  │      Buckets[2] = 0                                               │
  │    After:                                                         │
  │      Keys[2] = 58, Values[2] = "C", Next[2] = 0  (→ old head)   │
  │      Buckets[2] = 2         (new head)                           │
  └───────────────────────────────────────────────────────────────────┘


Enumeration Algorithm (BlobHashMapEnumerator)
---------------------------------------------

  ┌─────────────────────────────────────────────────────────────────┐
  │  BlobHashMapEnumerator<TKey, TValue>                            │
  │                                                                 │
  │  State:                                                         │
  │    data: BlobHashMapData*   (pointer into blob)                 │
  │    index: int               (current entry index, -1 = start)   │
  │    bucketIndex: int         (next bucket to scan)               │
  │    nextIndex: int           (next chain link, -1 = need scan)   │
  │                                                                 │
  │  MoveNext():                                                    │
  │    if nextIndex != -1:                                          │
  │      // Continue current chain                                  │
  │      index = nextIndex                                         │
  │      nextIndex = data->Next[nextIndex]                          │
  │      return true                                                │
  │    else:                                                        │
  │      // Find next non-empty bucket                              │
  │      return MoveNextSearch(...)                                 │
  │                                                                 │
  │  MoveNextSearch():                                              │
  │    for i = bucketIndex to (BucketCapacityMask + 1):             │
  │      idx = Buckets[i]                                           │
  │      if idx != -1:                                              │
  │        index = idx                                              │
  │        bucketIndex = i + 1                                      │
  │        nextIndex = Next[idx]                                    │
  │        return true                                              │
  │    return false  (all buckets exhausted)                        │
  └─────────────────────────────────────────────────────────────────┘

  Traversal order for the example above:
    bucket 0: empty → skip
    bucket 1: head=3 → emit 3, chain→1 → emit 1, chain→-1 → done
    bucket 2: head=5 → emit 5, chain→2 → emit 2, chain→0 → emit 0, done
    bucket 3: head=4 → emit 4, chain→-1 → done
    buckets 4-7: empty → skip
    Total: emits indices [3, 1, 5, 2, 0, 4] (insertion-order within chains)


KVPair Struct (Enumerator Output)
----------------------------------

  ┌────────────────────────────────────────────────────────────┐
  │  KVPair<TKey, TValue>  (readonly unsafe struct)            │
  │                                                            │
  │  data:  BlobHashMapData*   (pointer to the blob data)      │
  │  index: int                (entry index)                   │
  │                                                            │
  │  Key:   ref TKey   → data->Keys[index]                     │
  │  Value: ref TValue → data->Values[index]                   │
  │                                                            │
  │  Uses raw pointer to blob data, no managed references.     │
  │  DebuggerDisplay: "Key = {Key}, Value = {Value}"           │
  └────────────────────────────────────────────────────────────┘


BlobMultiHashMapIterator
-------------------------

  ┌──────────────────────────────────────────────┐
  │  BlobMultiHashMapIterator<TKey>               │
  │                                               │
  │  Key:       TKey    (the key being iterated)  │
  │  NextIndex: int     (next chain link or -1)   │
  │                                               │
  │  Used by BlobMultiHashMap to walk all values  │
  │  for a given key:                             │
  │                                               │
  │    it.Key = key                               │
  │    it.NextIndex = Buckets[hash & mask]        │
  │    while TryGetNextValue(out val, ref it):    │
  │      process(val)                             │
  └──────────────────────────────────────────────┘


Struct Layout Summary (byte offsets, 64-bit)
---------------------------------------------

  BlobHashMapData<int, int> (example: TKey=int, TValue=int):

  Offset  Field            Size   Content
  ──────  ────────          ────   ───────
  0x00    Values.offset     4      relative offset to values data
  0x04    Values.length     4      = capacity
          Values data       cap×4  follows at resolved offset

  0x08    Keys.offset       4      relative offset to keys data
  0x0C    Keys.length       4      = capacity
          Keys data         cap×4  follows at resolved offset

  0x10    Next.offset       4      relative offset to next data
  0x14    Next.length       4      = capacity
          Next data         cap×4  follows at resolved offset

  0x18    Buckets.offset    4      relative offset to buckets data
  0x1C    Buckets.length    4      = bucketCapacity
          Buckets data      bCap×4 follows at resolved offset

  0x20    Count.offset      4      relative offset
  0x24    Count.length      4      = 1
          Count data        4      actual count

  0x28    BucketCapacityMask 4     = bucketCapacity - 1
  ──────
  0x2C    Total fixed       44 bytes (data arrays allocated separately)


Key Design Decisions
--------------------

1. **Parallel arrays, not structs-of-kv-pairs.**  Keys, Values, and Next
   are stored in separate arrays rather than interleaved as KVPair structs.
   This improves cache locality when searching by key (only the Keys array
   is walked) and when reading values (only the Values array is touched).

2. **BlobArray<int> Count[1] instead of a plain int.**  Unity's blob system
   does not allow writing to scalar fields after ConstructRoot.  Wrapping
   the count in a single-element BlobArray lets the builder update it via
   BlobBuilderArray<int>[0] during construction.

3. **BucketCapacityMask instead of length.**  Storing `length - 1` as a mask
   eliminates the subtraction during every lookup.  A negative mask (< 0)
   doubles as a "never populated" sentinel (empty map).

4. **Forward-chaining with head insertion.**  New entries are prepended to
   the bucket chain (next[new] = buckets[b]; buckets[b] = new).  This is
   O(1) insertion and works naturally with the append-only builder pattern.

5. **Shared between HashMap and MultiHashMap.**  The same BlobHashMapData
   and BlobBuilderHashMapData types serve both single-value and multi-value
   maps.  The `multi` flag on TryAdd controls whether duplicate keys are
   rejected (HashMap) or accepted (MultiHashMap).

6. **Ptr<TValue> return for safe pointer wrapping.**  TryGetValue and
   TryGetNextValue return `Ptr<TValue>` rather than a raw pointer or ref,
   providing a safe, nullable wrapper that can be checked with `.IsCreated`.


Performance Characteristics
---------------------------

| Operation            | Time Complexity   | Notes                          |
|----------------------|-------------------|--------------------------------|
| TryGetFirstValue     | O(1 + c)          | c = chain length in bucket     |
| TryGetNextValue      | O(c)              | walk chain for multi-map       |
| Builder TryAdd       | O(1 + c)          | check existing + insert head   |
| Builder AddUnique    | O(1 + c)          | same + return ref to value     |
| Enumeration          | O(n + b)          | n = entries, b = bucket count  |
| Construction         | O(n)              | per-element add + O(b) clear   |

Memory:
  Fixed overhead:   5 × BlobArray headers (40 bytes) + 1 int (4 bytes)
  Variable:         capacity × (sizeof(TKey) + sizeof(TValue) + 4)
                    + bucketCapacity × 4
                    + 4 (count)

  With ratio=3, capacity=N:
    bucketCapacity ≈ 3N (rounded up to power of 2)
    Total ≈ N × (sizeof(K) + sizeof(V) + 4) + 3N × 4 + 44

Cache behavior:
  - Best case: hash distributes evenly, chains of length 1
  - Worst case: all keys in one bucket, linear scan
  - Buckets array is scanned once per lookup (1 cache line typically)
  - Chain walks touch scattered Keys/Next entries
  - Values array is only touched once the correct index is found

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobHashMapData.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobHashMapData<TKey,TValue>
  Kind: struct, internal

  Size: 44 bytes

  Fields:
    BlobArray`1 Values (private)
    BlobArray`1 Keys (private)
    BlobArray`1 Next (private)
    BlobArray`1 Buckets (private)
    BlobArray`1 Count (private)
    Int32 BucketCapacityMask (private)

  Methods:
    Boolean TryGetFirstValue(Int32 key, Ptr`1& item, BlobMultiHashMapIterator`1& it)
    Boolean TryGetNextValue(Ptr`1& item, BlobMultiHashMapIterator`1& it)

  Related: KVPair<TKey,TValue>
    Kind: struct, 16 bytes
    Int32& Key
    Int32& Value

Verified: 10 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMapData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMapData.cs)
- [BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMapDataBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobHashMapDataBuilder.cs)
- [BovineLabs.Core/Collections/Blobs/HashMap/BlobMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobMultiHashMap.cs)
