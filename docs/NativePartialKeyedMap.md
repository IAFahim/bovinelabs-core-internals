NativePartialKeyedMap — Inner Workings
=======================================

Highly constrained integer map optimized for extreme performance needs.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌─────────────────────────────────────────────────────────────┐
  │           NativePartialKeyedMap<TValue>                      │
  │                                                             │
  │  ┌───────────────────────────────────────┐                  │
  │  │ UnsafePartialKeyedMap<TValue>* map    │                  │
  │  │  (pointer to heap-allocated struct)   │                  │
  │  └───────────────┬───────────────────────┘                  │
  │                  │                                          │
  │                  ▼                                          │
  │  ┌──────────────────────────────────────────────────────┐   │
  │  │          UnsafePartialKeyedMap<TValue>                │   │
  │  │                                                       │   │
  │  │  TValue* values   ← EXTERNAL (not owned!)            │   │
  │  │  int*    keys     ← EXTERNAL (not owned!)            │   │
  │  │  int*    Next     ← Allocated (chain pointers)       │   │
  │  │  int*    Buckets  ← Allocated (bucket heads)         │   │
  │  │                                                       │   │
  │  │  int count            (current entry count)           │   │
  │  │  int nextCapacity     (for growth tracking)           │   │
  │  │  int bucketCapacity   (max key range)                 │   │
  │  │  Allocator allocator                                 │   │
  │  └──────────────────────────────────────────────────────┘   │
  └─────────────────────────────────────────────────────────────┘


KEY INSIGHT: EXTERNAL DATA OWNERSHIP
─────────────────────────────────────

  Unlike NativeKeyedMap, this map does NOT own its keys and values:

  ┌──────────────────────────────────────────────────────────┐
  │                  External Source                          │
  │  ┌──────────────┐  ┌──────────────┐                     │
  │  │ keys[]       │  │ values[]     │                     │
  │  │ (from user)  │  │ (from user)  │                     │
  │  └──────┬───────┘  └──────┬───────┘                     │
  │         │                 │                              │
  │         ▼                 ▼                              │
  │  ┌───────────────────────────────────────────────────┐   │
  │  │       UnsafePartialKeyedMap                       │   │
  │  │  keys ──► external array (NOT copied)             │   │
  │  │  values ─► external array (NOT copied)            │   │
  │  │  Next ──► allocated array (chain links)           │   │
  │  │  Buckets ─► allocated array (bucket heads)        │   │
  │  └───────────────────────────────────────────────────┘   │
  └──────────────────────────────────────────────────────────┘

  This means:
    • Keys/values are provided as raw pointers
    • Only Buckets and Next are allocated
    • Can be rebuilt (Update()) with new external data


MEMORY LAYOUT
─────────────

  External arrays (user-managed):
  ┌────┬────┬────┬────┬────┬────┬────┬────┐
  │ k0 │ k1 │ k2 │ k3 │ k4 │ k5 │ k6 │ k7 │  keys[]
  └────┴────┴────┴────┴────┴────┴────┴────┘
  ┌────┬────┬────┬────┬────┬────┬────┬────┐
  │ v0 │ v1 │ v2 │ v3 │ v4 │ v5 │ v6 │ v7 │  values[]
  └────┴────┴────┴────┴────┴────┴────┴────┘

  Allocated arrays (map-owned):
  Buckets[bucketCapacity]:
  ┌─────┬─────┬─────┬─────┬─────┬─────┬───┬─────┐
  │ -1  │ -1  │ -1  │ -1  │ -1  │ -1  │...│ -1  │  (0xFF = empty)
  └─────┴─────┴─────┴─────┴─────┴─────┴───┴─────┘
  Index:  0     1     2     3     4     5   ... maxKey-1

  Next[count]:
  ┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┐
  │ -1  │  0  │ -1  │ -1  │  2  │ -1  │ -1  │ -1  │
  └─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┘
  Index:  0     1     2     3     4     5     6     7


CONSTRUCTION FLOW
─────────────────

  ┌───────────────────────────────────────────────────────────────┐
  │ new NativePartialKeyedMap<TValue>(                            │
  │     keys*, values*, length, bucketCapacity, allocator)        │
  │                                                               │
  │  1. Validate ALL keys: 0 <= key[i] < bucketCapacity          │
  │     ┌──────────────────────────────────────────────┐          │
  │     │ for i in 0..length:                          │          │
  │     │   CheckKeyOutOfBounds(keys[i], bucketCap)    │          │
  │     └──────────────────────────────────────────────┘          │
  │                                                               │
  │  2. Store external pointers (no copy!)                        │
  │     this.keys = keys                                         │
  │     this.values = values                                     │
  │                                                               │
  │  3. Allocate chain arrays                                    │
  │     Next = alloc(sizeof(int) * length)                        │
  │     Buckets = alloc(sizeof(int) * bucketCapacity)             │
  │                                                               │
  │  4. RecalculateBuckets() — build the chains                  │
  └───────────────────────────────────────────────────────────────┘


BUCKET CHAIN BUILDING
─────────────────────

  RecalculateBuckets():
  ┌───────────────────────────────────────────────────────────┐
  │  MemSet(Buckets, 0xFF, bucketCapacity)  // all = -1      │
  │                                                           │
  │  for idx = 0 to count:                                   │
  │    key = keys[idx]                                        │
  │    Next[idx]    = Buckets[key]  // chain to previous      │
  │    Buckets[key] = idx           // become new head        │
  └───────────────────────────────────────────────────────────┘

  Example: keys = [3, 5, 3, 5, 2], bucketCapacity = 8

  After RecalculateBuckets:
  Buckets: [-1, -1, 4, 2, -1, 3, -1, -1]
                ↑      ↑         ↑
                key=2  key=3     key=5

  Chains:
    key=2: Buckets[2]=4 → Next[4]=-1        → values[4]
    key=3: Buckets[3]=2 → Next[2]=0 → -1    → values[2], values[0]
    key=5: Buckets[5]=3 → Next[3]=1 → -1    → values[3], values[1]


LOOKUP OPERATION
────────────────

  TryGetFirstValue(key):
  ┌──────────────────────────────────────────────────────┐
  │ 1. CheckKeyOutOfBounds(key, bucketCapacity)          │
  │                                                      │
  │ 2. it.NextEntryIndex = Buckets[key]                  │
  │                                                      │
  │ 3. TryGetNextValue:                                  │
  │    it.EntryIndex = it.NextEntryIndex                 │
  │    if (EntryIndex < 0) → not found, return false     │
  │    item = values[it.EntryIndex]  ← EXTERNAL array   │
  │    it.NextEntryIndex = Next[it.EntryIndex]           │
  │    return true                                       │
  └──────────────────────────────────────────────────────┘


UPDATE OPERATION
────────────────

  Supports rebuilding with new external data without reallocation:

  ┌───────────────────────────────────────────────────────────────┐
  │ Update(newKeys*, newValues*, newLength):                       │
  │                                                               │
  │  1. Validate ALL new keys: 0 <= key < bucketCapacity         │
  │                                                               │
  │  2. Update external pointers:                                 │
  │     this.keys = newKeys                                       │
  │     this.values = newValues                                   │
  │     this.count = newLength                                    │
  │                                                               │
  │  3. Grow Next array if needed:                                │
  │     if (nextCapacity < newLength):                            │
  │         Free(Next)                                            │
  │         Next = alloc(sizeof(int) * newLength)                 │
  │         nextCapacity = newLength                              │
  │                                                               │
  │  4. RecalculateBuckets() — rebuild chains                    │
  └───────────────────────────────────────────────────────────────┘


"PARTIAL" MEANING
─────────────────

  ┌──────────────────────────────────────────────────────────┐
  │  "Partial" = the map doesn't own or copy its data        │
  │                                                           │
  │  ┌─────────────────────┐  ┌───────────────────────────┐  │
  │  │ NativeKeyedMap      │  │ NativePartialKeyedMap     │  │
  │  ├─────────────────────┤  ├───────────────────────────┤  │
  │  │ Owns keys & values  │  │ Borrows keys & values     │  │
  │  │ Keys stored inline  │  │ Keys as external pointer  │  │
  │  │ Values stored inline│  │ Values as external pointer│  │
  │  │ Can Add() new items │  │ No Add() — set at create  │  │
  │  │ Fixed data          │  │ Updateable with new data  │  │
  │  │ 4 allocations       │  │ 2 allocations (Next+Bkt)  │  │
  │  └─────────────────────┘  └───────────────────────────┘  │
  └──────────────────────────────────────────────────────────┘

  Best used when:
    • You already have arrays of keys/values (e.g., from ECS chunks)
    • You need fast lookup by integer key
    • The data changes but the structure can be reused
    • Memory allocation must be minimized


DISPOSAL
────────

  ┌────────────────────────────────────────────────────────────┐
  │ Dispose():                                                 │
  │  1. Free(Buckets)   ← map-owned                           │
  │  2. Free(Next)      ← map-owned                           │
  │  3. Free(map struct) ← the UnsafePartialKeyedMap itself   │
  │                                                             │
  │  NOTE: External keys[] and values[] are NOT freed!         │
  │        The caller is responsible for their lifetime.        │
  └────────────────────────────────────────────────────────────┘


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌───────────────────────────┬──────────────────────────────────┐
  │ Operation                 │ Cost                             │
  ├───────────────────────────┼──────────────────────────────────┤
  │ TryGetFirstValue          │ O(1) direct bucket index        │
  │ TryGetNextValue           │ O(k) where k = values per key   │
  │ Update (same size)        │ O(count) bucket rebuild         │
  │ Update (larger)           │ O(count) + Next realloc         │
  │ Construction              │ O(count) validation + rebuild   │
  │ Memory (allocated)        │ Next[count] + Buckets[maxKey]   │
  │ Memory (external)         │ keys + values (zero copy)       │
  └───────────────────────────┴──────────────────────────────────┘

## Verified Data

> [Run test snippet](../snippets/core-collections/NativePartialKeyedMap.cs) — 21 assertions passing
>
> Key findings:
> - NativePartialKeyedMap<TValue> type exists and is a struct implementing INativeDisposable
> - Has IsCreated property, indexer [int], TryGetFirstValue, TryGetNextValue, Update, Dispose methods
> - Constructor takes (int*, TValue*, int length, int bucketCapacity, AllocatorHandle)
> - UnsafePartialKeyedMap<T> inner type also verified
> - Zero-copy external key/value storage confirmed via pointer-based construction

## Verified Data

```
NativePartialKeyedMap<int>
  Kind: struct, 8 bytes
  Fields:
    UnsafePartialKeyedMap`1* map
  Methods:
    Void Dispose()
    JobHandle Dispose(JobHandle)
    Void Update(Int32*, Int32*, Int32)
    Boolean TryGetFirstValue(Int32, out Int32&, out UnsafeKeyedMapIterator&)
    Boolean TryGetNextValue(out Int32&, UnsafeKeyedMapIterator&)
  UnsafePartialKeyedMap<int>
  Kind: struct, 56 bytes
  Methods:
    Void Dispose()
    JobHandle Dispose(JobHandle)
    Void Update(Int32*, Int32*, Int32)
    Boolean TryGetFirstValue(Int32, out Int32&, out UnsafeKeyedMapIterator&)
    Boolean TryGetNextValue(out Int32&, UnsafeKeyedMapIterator&)
Verified: 4 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/NativePartialKeyedMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/NativePartialKeyedMap.cs)
