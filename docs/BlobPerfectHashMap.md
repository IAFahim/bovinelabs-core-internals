BlobPerfectHashMap - Zero-Collision Hash Maps Inside Blob Assets for Instant Lookups
======================================================================================

Overview
--------

BlobPerfectHashMap<TKey, TValue> is a special-purpose hash map that guarantees
zero hash collisions by construction. During baking, it finds a power-of-two
table size where every key's hash maps to a unique slot. The result is a flat
array lookup: `index = key.GetHashCode() & (Capacity - 1)` with no chaining,
no probing, and no comparison needed beyond a null-value sentinel check.

This trades construction time and potentially wasted memory for O(1) worst-case
lookups -- ideal for hot-path runtime data accessed millions of times per frame.

Architecture Diagram
--------------------

  BlobAssetReference<BlobPerfectHashMap<TKey,TValue>>
  ┌─────────────────────────────────────────────────────────────┐
  │ BlobAssetHeader (Unity internal)                             │
  ├─────────────────────────────────────────────────────────────┤
  │ BlobPerfectHashMap<TKey, TValue>                             │
  │                                                             │
  │  ┌─ Values ───────────────────────────────────────────────┐ │
  │  │  BlobArray<TValue>  length = Capacity (power of 2)     │ │
  │  │                                                        │ │
  │  │  Slot:  [0]    [1]    [2]    [3]    ...  [Capacity-1]  │ │
  │  │        val₀  <null>  val₂   val₃   ...   <null>       │ │
  │  │                   ↑                        ↑           │ │
  │  │               unused                    unused         │ │
  │  │  (empty slots contain NullValue)                        │ │
  │  └────────────────────────────────────────────────────────┘ │
  │                                                             │
  │  Capacity: int    (always power of 2, may be > entry count) │
  │  NullValue: TValue  (sentinel for "no key mapped here")     │
  └─────────────────────────────────────────────────────────────┘


How It Differs From BlobHashMap
--------------------------------

  ┌──────────────────────┬─────────────────────┬───────────────────────┐
  │                      │  BlobHashMap         │  BlobPerfectHashMap   │
  ├──────────────────────┼─────────────────────┼───────────────────────┤
  │  Collision handling  │  Chained buckets     │  None (by design)     │
  │  Lookup              │  hash → walk chain   │  hash → single index  │
  │  Worst case          │  O(n) degenerate     │  O(1) guaranteed      │
  │  Extra arrays        │  Keys, Next, Buckets │  None                 │
  │  Memory efficiency   │  Tight to capacity   │  May over-allocate    │
  │  Construction cost   │  O(n) insert         │  O(n×k) find size     │
  │  Key requirement     │  IEquatable<TKey>    │  IEquatable<TKey>     │
  │  Value requirement   │  unmanaged           │  IEquatable<TValue>   │
  │  Supports null keys  │  No                  │  No (hash collision)  │
  └──────────────────────┴─────────────────────┴───────────────────────┘


Lookup Algorithm (TryGetValue)
------------------------------

  ┌──────────────────────────────────────────────────────┐
  │  1.  index = key.GetHashCode() & (Capacity - 1)     │
  │                                                      │
  │  2.  if index < 0 or index >= Capacity → NOT FOUND   │
  │                                                      │
  │  3.  ref value = Values[index]                       │
  │                                                      │
  │  4.  if value.Equals(NullValue) → NOT FOUND          │
  │      else → return &value                            │
  └──────────────────────────────────────────────────────┘

  That's it. No loop, no chain walk, no key comparison.
  The sentinel NullValue check replaces the need for a separate key array.


Construction: Finding the Perfect Size
---------------------------------------

  Input:  NativeHashMap<TKey, TValue> source
          TValue nullValue (sentinel)

  ┌────────────────────────────────────────────────────────────────┐
  │ STEP 1: Assert no hash collisions among keys                   │
  │                                                                │
  │   For each key in source:                                      │
  │     if hash already seen → THROW "HashCode collision."         │
  │     else add hash to unique set                                │
  │                                                                │
  │   (Two keys with the same hash can NEVER be in a perfect map)  │
  └──────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
  ┌────────────────────────────────────────────────────────────────┐
  │ STEP 2: Find the smallest power-of-2 size with zero collisions │
  │                                                                │
  │   size = ceilpow2(source.Count)   ← starting point            │
  │                                                                │
  │   LOOP:                                                        │
  │     ┌─────────────────────────────────────────────────┐        │
  │     │  For each (key,_) in source:                    │        │
  │     │    idx = key.GetHashCode() & (size - 1)         │        │
  │     │    if idx already used by another key:           │        │
  │     │      → COLLISION, break                         │        │
  │     └─────────────────────────────────────────────────┘        │
  │                                                                │
  │   if collision:  size <<= 1  (double the size)                 │
  │                   goto LOOP                                    │
  │   else:          found!  size is the Capacity                  │
  └──────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
  ┌────────────────────────────────────────────────────────────────┐
  │ STEP 3: Allocate and populate                                  │
  │                                                                │
  │   Allocate Values[Capacity]                                    │
  │   MemCpyReplicate NullValue into all slots                     │
  │                                                                │
  │   For each (key, value) in source:                             │
  │     idx = key.GetHashCode() & (Capacity - 1)                   │
  │     Values[idx] = value                                        │
  └────────────────────────────────────────────────────────────────┘


Collision Search Example
------------------------

  Source keys: { A, B, C, D }  (4 entries)
  Hashes:       A→3, B→7, C→11, D→15

  Attempt 1: size = ceilpow2(4) = 4, mask = 3
    A → 3 & 3 = 3
    B → 7 & 3 = 3  ← COLLISION with A!
    FAIL → double

  Attempt 2: size = 8, mask = 7
    A → 3 & 7 = 3
    B → 7 & 7 = 7
    C → 11 & 7 = 3  ← COLLISION with A!
    FAIL → double

  Attempt 3: size = 16, mask = 15
    A → 3 & 15 = 3
    B → 7 & 15 = 7
    C → 11 & 15 = 11
    D → 15 & 15 = 15
    ALL UNIQUE → Capacity = 16

  Final layout:
    Values: [_, _, _, A, _, _, _, B, _, _, _, C, _, _, _, D]
              0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
            (_ = NullValue)

  4 entries × 16 slots = 25% utilization
  But every lookup is a single AND + array index.


Memory Layout (Byte-Level for int→int)
---------------------------------------

  Assume TKey=int, TValue=int, Capacity=16, 64-bit.

  Offset  Field          Size   Description
  ──────  ─────────────  ─────  ──────────────────────────────
  0x000   Values         16×4   BlobArray<int>: offset+length
         ┌───────────────────────────────────────────────┐
         │ int offset │ int length=16 │                  │
         └───────────────────────────────────────────────┘
         followed by 16 × 4 bytes of int values
  0x028   Capacity       4      = 16
  0x02C   NullValue      4      sentinel (e.g. -1 or 0)
  0x030   [padding]      4      alignment
  ──────  ─────────────  ─────  ──────────────────────────────

  Compare with BlobHashMap for same 4 entries, ratio=3, 8 buckets:
    Values: 4×4 = 16B
    Keys:   4×4 = 16B
    Next:   4×4 = 16B
    Buckets:8×4 = 32B
    Count:  1×4 =  4B
    Mask:   1×4 =  4B
    Total:         88B

  PerfectHashMap for same 4 entries, Capacity=16:
    Values: 16×4 = 64B
    Capacity: 4B
    NullValue: 4B
    Total:      72B  (trades keys/next/buckets for more value slots)


Data Flow: Bake → Runtime
--------------------------

  Bake Time (Editor)                    Runtime (Player)
  ═══════════════════                    ════════════════

  ┌────────────────────┐
  │ NativeHashMap<K,V> │
  │   {A→1, B→2,       │
  │    C→3, D→4}       │
  └────────┬───────────┘
           │
           ▼
  ┌────────────────────────────────┐
  │ ConstructPerfectHashMap()      │
  │  1. Check unique hashes        │
  │  2. Find min power-of-2 size   │
  │  3. Allocate + fill Values[]   │
  └────────┬───────────────────────┘
           │
           ▼  CreateBlobAssetReference()
  ┌────────────────────────────────┐     ┌─────────────────────────┐
  │ BlobAssetReference<            │────▶│ ref BlobPerfectHashMap   │
  │   BlobPerfectHashMap<K,V>>     │     │   .Values[idx]          │
  └────────────────────────────────┘     │   idx = hash & (Cap-1)  │
                                         └─────────────────────────┘


Key Design Decisions
--------------------

1. **No key storage at all.**  Because every key maps to a unique slot, the
   index itself encodes the key.  Only the NullValue sentinel is needed to
   distinguish "present" from "absent."  This halves the memory compared to
   a hash map that stores both keys and values.

2. **Hash uniqueness is required.**  If two keys produce the same hash code,
   no power-of-two table size can separate them.  The constructor asserts
   this upfront and throws if violated.  This makes BlobPerfectHashMap
   unsuitable for key types with poor hash distribution.

3. **NullValue sentinel pattern.**  TValue must implement IEquatable<TValue>
   so the map can compare against the user-provided sentinel.  The sentinel
   is replicated across all empty slots using MemCpyReplicate for speed.

4. **Exponential size search.**  Starting from ceilpow2(count), the algorithm
   doubles until collision-free.  In the worst case this could reach very
   large sizes, but in practice good hash functions find a fit within 2-4
   doublings.

5. **Separate builder type (BlobBuilderPerfectHashMap).**  The builder is a
   ref struct that performs the collision search and allocation.  It is
   intentionally not reusable -- it exists only during the Construct phase.

6. **Capacity is always a power of two.**  This allows the `hash & (Cap-1)`
   bitmask trick instead of modulo division.


Performance Characteristics
---------------------------

| Operation          | Time Complexity  | Notes                            |
|--------------------|------------------|----------------------------------|
| TryGetValue        | O(1) guaranteed  | hash + mask + sentinel check     |
| ContainsKey        | O(1) guaranteed  | Same as TryGetValue              |
| this[key]          | O(1) guaranteed  | TryGetValue + throw on miss      |
| Construction       | O(n × log n)     | Collision search may double many |

Memory characteristics:
  - Only stores: Values[Capacity] + Capacity(int) + NullValue(TValue)
  - NO Keys array, NO Next array, NO Buckets array
  - Capacity may be significantly larger than entry count
  - Best case utilization: ~50% (when hashes spread evenly)
  - Worst case utilization: arbitrarily low (poor hash distribution)

Burst-compatible:   Yes (100% unmanaged)
Thread-safe reads:  Yes (immutable after construction)
Cache-friendly:     Excellent (single flat array access, sequential layout)
Best suited for:    Small-to-medium static lookup tables accessed at high
                    frequency (e.g., entity property maps, type registries)

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobPerfectHashMap.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobPerfectHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobPerfectHashMap.cs)
- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderPerfectHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderPerfectHashMap.cs)
- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs)
