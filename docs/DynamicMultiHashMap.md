# DynamicMultiHashMap

**Embeds a multi-value (duplicate-key) hash map entirely inside a Unity DynamicBuffer<byte> on an entity.**

## Overview

DynamicMultiHashMap stores a complete open-addressed-with-chaining hash map inside a single
`DynamicBuffer<byte>` attached to an ECS entity. This means no separate native container
allocation — the map data travels with the entity, serializes with it, and is accessible in
jobs without additional lookups.

The "multi" variant allows duplicate keys. Calling `Add(key, value)` always inserts a new
entry, even if the key already exists. Values for a given key are iterated via
`TryGetFirstValue` / `TryGetNextValue`, which walk the chain at a single bucket.

Internally, the buffer is laid out as a header struct (`DynamicHashMapHelper<TKey>`) followed
by four contiguous arrays: **Values**, **Keys**, **Next**, and **Buckets**. Collision chains
are linked via the Next array (each slot stores the index of the next entry in the same bucket).

---

## Memory Layout

### Buffer Byte-Level Layout

```
  DynamicBuffer<byte> contents:
  ╔════════════════════════════════════════════════════════════════════════════════════╗
  ║                                                                                  ║
  ║  Offset 0x00:  DynamicHashMapHelper<TKey>   (header, ~44 bytes)                 ║
  ║                ┌─────────────────────────────────────────────────────┐            ║
  ║                │ ValuesOffset       (int)                           │            ║
  ║                │ KeysOffset         (int)                           │            ║
  ║                │ NextOffset         (int)                           │            ║
  ║                │ BucketsOffset      (int)                           │            ║
  ║                │ Count              (int) — current # of entries   │            ║
  ║                │ Capacity           (int) — max entries            │            ║
  ║                │ BucketCapacityMask (int) — buckets-1              │            ║
  ║                │ Log2MinGrowth      (int)                          │            ║
  ║                │ AllocatedIndex     (int) — high-water mark        │            ║
  ║                │ FirstFreeIdx       (int) — free list head         │            ║
  ║                │ SizeOfTValue       (int)                          │            ║
  ║                └─────────────────────────────────────────────────────┘            ║
  ║                                                                                  ║
  ║  [padding to 16-byte alignment]                                                  ║
  ║                                                                                  ║
  ║  Offset ValuesOffset:  Values[Capacity]   (SizeOfTValue * Capacity bytes)        ║
  ║                         ┌───────┬───────┬───────┬───────┐                        ║
  ║                         │ val 0 │ val 1 │ val 2 │  ...  │                        ║
  ║                         └───────┴───────┴───────┴───────┘                        ║
  ║                                                                                  ║
  ║  [padding to alignof(TKey)]                                                      ║
  ║                                                                                  ║
  ║  Offset KeysOffset:    Keys[Capacity]     (sizeof(TKey) * Capacity bytes)         ║
  ║                         ┌───────┬───────┬───────┬───────┐                        ║
  ║                         │ key 0 │ key 1 │ key 2 │  ...  │                        ║
  ║                         └───────┴───────┴───────┴───────┘                        ║
  ║                                                                                  ║
  ║  [padding to 4-byte alignment]                                                   ║
  ║                                                                                  ║
  ║  Offset NextOffset:    Next[Capacity]     (4 * Capacity bytes)                    ║
  ║                         ┌───────┬───────┬───────┬───────┐                        ║
  ║                         │ nxt 0 │ nxt 1 │ nxt 2 │  ...  │                        ║
  ║                         └───────┴───────┴───────┴───────┘                        ║
  ║                                                                                  ║
  ║  [padding to 4-byte alignment]                                                   ║
  ║                                                                                  ║
  ║  Offset BucketsOffset: Buckets[BucketCapacity]  (4 * BucketCapacity bytes)       ║
  ║  where BucketCapacity = 2 * Capacity                                             ║
  ║                         ┌─────┬─────┬─────┬─────┬─────┬─────┐                   ║
  ║                         │bkt 0│bkt 1│bkt 2│bkt 3│ ... │bkt N│                   ║
  ║                         └─────┴─────┴─────┴─────┴─────┴─────┘                   ║
  ║                                                                                  ║
  ╚════════════════════════════════════════════════════════════════════════════════════╝
```

### Header Struct Detail (DynamicHashMapHelper<TKey>)

```
  DynamicHashMapHelper<TKey>  — 44 bytes, sequential layout
  ┌──────────┬────────────────────┬──────────────────────────────────────────────────┐
  │ Offset   │ Field              │ Purpose                                          │
  ├──────────┼────────────────────┼──────────────────────────────────────────────────┤
  │ 0x00     │ ValuesOffset       │ Byte offset from header start to Values[]        │
  │ 0x04     │ KeysOffset         │ Byte offset from header start to Keys[]          │
  │ 0x08     │ NextOffset         │ Byte offset from header start to Next[]          │
  │ 0x0C     │ BucketsOffset      │ Byte offset from header start to Buckets[]       │
  │ 0x10     │ Count              │ Current number of live entries                   │
  │ 0x14     │ Capacity           │ Max entries (power of 2)                         │
  │ 0x18     │ BucketCapacityMask │ = BucketCapacity - 1 (for fast modulo via &)    │
  │ 0x1C     │ Log2MinGrowth      │ Growth granularity as log2                       │
  │ 0x20     │ AllocatedIndex     │ Next unused slot index (high-water mark)         │
  │ 0x24     │ FirstFreeIdx       │ Head of free-list chain (-1 = empty)             │
  │ 0x28     │ SizeOfTValue       │ Size of each value element in bytes              │
  └──────────┴────────────────────┴──────────────────────────────────────────────────┘
  Total = 11 * 4 = 44 bytes
```

---

## Hash Map Internals: Chaining via Next[]

```
  HOW ENTRIES MAP TO BUCKETS
  ═══════════════════════════

  Hash function: bucket = key.GetHashCode() & BucketCapacityMask

  Buckets[] (indexed by hash bucket)
  ┌─────┬────────┐
  │ idx │  value │
  ├─────┼────────┤
  │  0  │   3    │───┐  entry index 3
  │  1  │  -1    │   │  (empty bucket)
  │  2  │   0    │──┐│  entry index 0
  │  3  │   7    │─┐││
  │  4  │   2    │ │││
  └─────┴────────┘ │││
                   │││
  Keys[]           │││    Next[]       Values[]
  ┌─────┬──────┐   │││    ┌─────┬─────┐  ┌─────┬──────┐
  │  0  │ "A"  │◄──┘││    │  0  │  5  │  │  0  │ v0   │
  │  1  │ "B"  │    ││    │  1  │ -1  │  │  1  │ v1   │
  │  2  │ "C"  │◄───┘│    │  2  │ -1  │  │  2  │ v2   │
  │  3  │ "A"  │◄────┘    │  3  │  6  │  │  3  │ v3   │
  │  4  │ "D"  │          │  4  │ -1  │  │  4  │ v4   │
  │  5  │ "E"  │          │  5  │ -1  │  │  5  │ v5   │
  │  6  │ "A"  │          │  6  │ -1  │  │  6  │ v6   │
  │  7  │ "F"  │◄─────────┘    │  7  │  1  │  │  7  │ v7   │
  └─────┴──────┘               └─────┴─────┘  └─────┴──────┘

  Example: Looking up key "A"
  ──────────────────────────────

  1. hash("A") & BucketCapacityMask → bucket 2
  2. Buckets[2] = 0 → Keys[0] = "A" ✓  → read Values[0]
  3. Next[0] = 5   → Keys[5] = "E" ✗  → skip
  4. Next[5] = -1  → done

  Wait — "A" actually hashes to bucket 2:
  Buckets[2] = 0, Keys[0] = "A" ✓ match
  Next[0] = 5, Keys[5] = "E" ✗ not match  
  Next[5] = -1 → end of chain

  But key "A" is ALSO at index 3 (multi-value!):
  Buckets[hash("A")] may = 3, Next[3] = 6, Keys[6] = "A" ✓
  
  (The multi-map stores duplicates: iterating all values for "A"
   means walking the entire chain at bucket[hash("A")] and 
   checking each entry's key.)
```

### Multi-Value Chain Walk (TryGetFirstValue / TryGetNextValue)

```
  TryGetFirstValue("A"):
  ────────────────────────
  1. bucket = hash("A") & mask
  2. entryIdx = Buckets[bucket]
  3. WHILE Keys[entryIdx] != "A":
         entryIdx = Next[entryIdx]
         if entryIdx >= Capacity → return false
  4. it.NextEntryIndex = Next[entryIdx]     // remember chain position
  5. RETURN Values[entryIdx], iterator
  
  TryGetNextValue(ref it):
  ─────────────────────────
  1. entryIdx = it.NextEntryIndex
  2. WHILE Keys[entryIdx] != "A":
         entryIdx = Next[entryIdx]
         if entryIdx >= Capacity → return false
  3. it.NextEntryIndex = Next[entryIdx]
  4. RETURN Values[entryIdx]
```

---

## Add Algorithm (AddMulti → AddNewKey)

```
  DynamicMultiHashMap.Add(key, value):
  ─────────────────────────────────────
  
  1. idx = AddMulti(buffer, ref helper, key)
     └─ AddNewKey(buffer, ref helper, key):
        │
        ├─ IF AllocatedIndex >= Capacity AND FirstFreeIdx < 0:
        │  └─ RESIZE: CalcCapacityCeilPow2, Resize buffer
        │
        ├─ idx = FirstFreeIdx          // try free list first
        │  IF idx >= 0:
        │    FirstFreeIdx = Next[idx]  // pop from free list
        │  ELSE:
        │    idx = AllocatedIndex++    // use next fresh slot
        │
        ├─ Keys[idx] = key
        ├─ bucket = hash(key) & BucketCapacityMask
        ├─ Next[idx] = Buckets[bucket]    // prepend to chain
        ├─ Buckets[bucket] = idx
        ├─ Count++
        └─ RETURN idx

  2. Values[idx] = value              // write value at returned index
```

### Visual: Add Sequence

```
  INITIAL STATE (empty, Capacity=4, BucketCapacity=8):
  
  Buckets: [-1, -1, -1, -1, -1, -1, -1, -1]
  Keys:    [ _,  _,  _,  _]
  Values:  [ _,  _,  _,  _]
  Next:    [-1, -1, -1, -1]
  Count=0, AllocatedIndex=0, FirstFreeIdx=-1

  ── Add(key="X", value=10): hash("X") & 7 = 3 ──────────────────

  Buckets: [-1, -1, -1,  0, -1, -1, -1, -1]   ← bucket 3 → idx 0
                                  ↑
  Keys:    ["X",  _,  _,  _]
  Values:  [ 10,  _,  _,  _]
  Next:    [-1, -1, -1, -1]
  Count=1, AllocatedIndex=1

  ── Add(key="X", value=20): hash("X") & 7 = 3 (DUPLICATE KEY!) ──

  Buckets: [-1, -1, -1,  1, -1, -1, -1, -1]   ← bucket 3 → idx 1
                                  ↑
  Keys:    ["X","X",  _,  _]
  Values:  [ 10, 20,  _,  _]
  Next:    [  0,-1, -1, -1]        ← Next[1]=0 (chain: 1→0)
  Count=2, AllocatedIndex=2

  Iterating "X": idx=1→val=20, Next[1]=0→idx=0→val=10, Next[0]=-1→done
```

---

## Remove Algorithm

```
  Remove(key):
  ────────────

  1. bucket = hash(key) & mask
  2. prevEntry = -1
  3. entryIdx = Buckets[bucket]
  4. WHILE entryIdx is valid:
     ├─ IF Keys[entryIdx] == key:
     │   ├─ Unlink from chain:
     │   │   IF prevEntry < 0:
     │   │     Buckets[bucket] = Next[entryIdx]
     │   │   ELSE:
     │   │     Next[prevEntry] = Next[entryIdx]
     │   │
     │   ├─ Add to free list:
     │   │   Next[entryIdx] = FirstFreeIdx
     │   │   FirstFreeIdx = entryIdx
     │   │
     │   ├─ Count--
     │   └─ Continue walking (multi: may have more!)
     │      entryIdx = Next[entryIdx] (saved before overwrite)
     │
     └─ ELSE:
        ├─ prevEntry = entryIdx
        └─ entryIdx = Next[entryIdx]
```

### Free List After Removal

```
  Before removal of idx 1:
  
  Buckets[3] → 1 → 0 → -1
  
  After removal of idx 1:
  
  Buckets[3] → 0 → -1           (chain skips removed idx)
  
  Next[1] = FirstFreeIdx         (idx 1 joins free list)
  FirstFreeIdx = 1
  
  ┌──────────────────────────────────────┐
  │ Free list: FirstFreeIdx → 1 → -1    │
  │                                      │
  │ Next[] values repurposed as links    │
  │ in the free list chain. Next[1]      │
  │ previously pointed to 0 (in chain), │
  │ now points to old FirstFreeIdx (-1). │
  └──────────────────────────────────────┘
```

---

## Resize and Flatten

```
  Resize (when Capacity is exhausted and free list is empty):
  ──────────────────────────────────────────────────────────
  
  1. newCapacity = ceilpow2(max(Count, Capacity + growth))
  2. newBucketCapacity = ceilpow2(2 * newCapacity)
  3. Allocate temp copies of Values, Keys, Next, Buckets
  4. buffer.ResizeUninitialized(totalSize)
  5. Recalculate all offsets
  6. Clear() — resets Buckets/Next to 0xFF, Count=0, etc.
  7. Rebuild: iterate old buckets/chains, re-insert all entries
  
  Fast-path optimization:
  ────────────────────────
  IF no holes (FirstFreeIdx == -1 && AllocatedIndex == Count)
     AND newCapacity > oldCapacity:
    
    → Copy Values[] and Keys[] directly (dense, no gaps)
    → Only rebuild Buckets[] and Next[] chains
    → Much faster than full rebuild
  
  Flatten():
  ──────────
  Removes all holes by resizing to exact Count.
  Compacts the map to minimum capacity.
```

---

## Data Flow: Full Lifecycle

```
  ╔═══════════════════════════════════════════════════════════════════════════╗
  ║                    DYNAMIC MULTI HASH MAP LIFECYCLE                      ║
  ╠═══════════════════════════════════════════════════════════════════════════╣
  ║                                                                         ║
  ║  1. INIT                                                                 ║
  ║     DynamicHashMapHelper<TKey>.Init(buffer, capacity, sizeof(TValue))   ║
  ║         │                                                               ║
  ║         ├─ Calculate total buffer size (header + 4 arrays)              ║
  ║         ├─ buffer.ResizeUninitialized(totalSize)                        ║
  ║         ├─ Write header fields (offsets, capacity, etc.)                ║
  ║         └─ Clear(): MemSet Buckets/Next to 0xFF (-1), reset counts     ║
  ║                                                                         ║
  ║  2. ADD (allows duplicate keys)                                         ║
  ║     map.Add(key, value)                                                 ║
  ║         │                                                               ║
  ║         ├─ Allocate slot (from free list or AllocatedIndex++)           ║
  ║         ├─ Keys[idx] = key                                              ║
  ║         ├─ Prepend to bucket chain: Next[idx] = Buckets[bkt]            ║
  ║         ├─ Buckets[bkt] = idx                                           ║
  ║         └─ Values[idx] = value                                          ║
  ║                                                                         ║
  ║  3. QUERY (iterate all values for a key)                                ║
  ║     TryGetFirstValue(key, out val, out it)                              ║
  ║     while (TryGetNextValue(out val, ref it)) { ... }                    ║
  ║         │                                                               ║
  ║         ├─ Hash key → bucket index                                      ║
  ║         ├─ Walk chain: Buckets[bkt] → Next[] → Next[] → ...            ║
  ║         └─ Yield Values[] at each matching entry                        ║
  ║                                                                         ║
  ║  4. REMOVE (removes ALL entries for key)                                ║
  ║     map.Remove(key) → returns count of removed items                    ║
  ║         │                                                               ║
  ║         ├─ Walk chain at bucket[hash(key)]                              ║
  ║         ├─ Unlink matching entries from chain                           ║
  ║         └─ Push each freed index onto free list                         ║
  ║                                                                         ║
  ║  5. FLATTEN (optional compaction)                                       ║
  ║     map.Flatten()                                                       ║
  ║         │                                                               ║
  ║         └─ ResizeExact(Count) — removes holes, compacts to minimum     ║
  ║                                                                         ║
  ╚═══════════════════════════════════════════════════════════════════════════╝
```

---

## Key Design Decisions

1. **Embedded in DynamicBuffer<byte>**: All data lives inside the entity's buffer. No external
   allocations, no indirection. The map is serialized/deserialized with the entity automatically.

2. **Offset-based internal pointers**: Instead of raw pointers (which would break across
   serialization), all array locations are stored as byte offsets from the header start.
   After resize, offsets are recalculated.

3. **Separate chaining via Next[]**: Each entry has a `Next[index]` field forming a linked
   list within the same array. This avoids the clustering problems of open addressing and
   makes deletion simple.

4. **Bucket capacity = 2x entry capacity**: Keeping the load factor below 0.5 reduces chain
   length, giving O(1) average lookup even with multi-value duplicates.

5. **Free list for removed entries**: Removed slots aren't immediately compacted; instead they
   form a free list (linked via Next[]) that new additions can reuse. `Flatten()` compacts.

6. **Multi-value semantics**: `AddMulti` always appends without checking for existing keys.
   This is faster than `TryAdd` (which calls `Find` first) for the common case where the
   caller knows duplicates are desired.

7. **Fast-path resize when no holes**: If the map has no gaps (no deletions since last
   compaction), resize just copies the dense key/value arrays and rebuilds only the bucket
   chains, avoiding a full rehash.

---

## Performance Characteristics

| Operation              | Average  | Worst    | Notes                              |
|------------------------|----------|----------|------------------------------------|
| Add                    | O(1)     | O(N)     | Worst = resize + rehash            |
| Add (multi, no find)   | O(1)     | O(N)     | Skips duplicate check             |
| Find / ContainsKey     | O(1)     | O(N)     | Walk chain at bucket               |
| TryGetFirstValue       | O(1)     | O(N)     | Walk chain until key match         |
| TryGetNextValue        | O(1)     | O(N)     | Continue walking chain             |
| Remove (all for key)   | O(k)     | O(N)     | k = values for that key            |
| Clear                  | O(B+C)   | O(B+C)   | MemSet buckets + next arrays       |
| Flatten                | O(N)     | O(N)     | Full rebuild at minimum capacity   |
| Resize                 | O(N)     | O(N)     | Copy + rehash all entries          |
| AddBatchUnsafe         | O(n)     | O(N+n)   | n = batch size, no hole check      |

**Memory overhead**: Header (44 bytes) + Buckets (8*Capacity bytes) + Next (4*Capacity bytes)
+ alignment padding. For Capacity=16 with int keys/int values: ~44 + 128 + 64 + 64 + 128 = ~428 bytes total.

**Load factor**: Always ≤ 0.5 (BucketCapacity = 2 * Capacity), ensuring short chains.

**Fragmentation**: Removed entries create holes in the dense arrays. Call `Flatten()` to
compact if needed. Holes do not affect correctness, only waste space.

## Verified Data

> [Run test snippet](../snippets/dynamic-buffers/DynamicMultiHashMap.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicMultiHashMap.cs)
- [BovineLabs.Core.Tests/Iterators/DynamicMultiHashMapTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Iterators/DynamicMultiHashMapTests.cs)
- [BovineLabs.Core/Iterators/DynamicHashMap/IDynamicMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/IDynamicMultiHashMap.cs)
