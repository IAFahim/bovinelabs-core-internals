# DynamicHashSet

**Embeds a hash set directly inside a Unity DynamicBuffer<byte> on an entity — zero external allocations.**

## Overview

DynamicHashSet reuses the same `DynamicHashMapHelper<T>` infrastructure as the hash map
variants, but with a critical optimization: it passes `sizeOfValueT = 0` during initialization.
This means no Values array is allocated at all. The set elements are stored in the Keys[]
array, and the entire structure fits in a single contiguous `DynamicBuffer<byte>`.

Like the hash map, it uses separate chaining via a Next[] array for collision resolution.
Buckets[] provides O(1) average lookup. Removed entries go onto a free list for reuse.

The set enforces uniqueness: `Add()` calls `TryAdd`, which first checks `Find()` and only
inserts if the element is not already present.

---

## Memory Layout

### Key Difference from HashMap: No Values Array

```
  DynamicBuffer<byte> for DynamicHashSet<int> (Capacity = 8, BucketCapacity = 16):

  ╔══════════════════════════════════════════════════════════════════════════╗
  ║                                                                        ║
  ║  [0x00]  DynamicHashMapHelper<int>   (44 bytes header)                ║
  ║          ┌──────────────────────────────────────────────┐              ║
  ║          │ ValuesOffset       = aligned(header_end)     │              ║
  ║          │ KeysOffset         = ValuesOffset + 0        │ ← NO VALUES ║
  ║          │ NextOffset         = KeysOffset + 32         │              ║
  ║          │ BucketsOffset      = NextOffset + 32         │              ║
  ║          │ Count              = 5                       │              ║
  ║          │ Capacity           = 8                       │              ║
  ║          │ BucketCapacityMask = 15  (0xF)               │              ║
  ║          │ Log2MinGrowth      = 0                       │              ║
  ║          │ AllocatedIndex     = 5                       │              ║
  ║          │ FirstFreeIdx       = -1                      │              ║
  ║          │ SizeOfTValue       = 0          ← KEY FIELD  │              ║
  ║          └──────────────────────────────────────────────┘              ║
  ║                                                                        ║
  ║  [aligned to 16]                                                       ║
  ║                                                                        ║
  ║  Values[0] — DOES NOT EXIST (SizeOfTValue = 0, 0 bytes allocated)     ║
  ║                                                                        ║
  ║  Keys[0..7]  (sizeof(int) * 8 = 32 bytes)                             ║
  ║          ┌──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┐    ║
  ║          │  42  │  17  │  99  │   7  │  23  │  --  │  --  │  --  │    ║
  ║          └──────┴──────┴──────┴──────┴──────┴──────┴──────┴──────┘    ║
  ║                                                                        ║
  ║  Next[0..7]  (4 * 8 = 32 bytes)                                       ║
  ║          ┌────┬────┬────┬────┬────┬────┬────┬────┐                     ║
  ║          │ -1 │ -1 │ -1 │ -1 │ -1 │ -1 │ -1 │ -1 │                     ║
  ║          └────┴────┴────┴────┴────┴────┴────┴────┘                     ║
  ║                                                                        ║
  ║  Buckets[0..15]  (4 * 16 = 64 bytes)                                  ║
  ║          ┌────┬────┬────┬────┬────┬────┬────┬────┐                     ║
  ║          │ -1 │ -1 │  0 │ -1 │  1 │ -1 │ -1 │  3 │ ...               ║
  ║          └────┴────┴────┴────┴────┴────┴────┴────┘                     ║
  ║                                                                        ║
  ╚══════════════════════════════════════════════════════════════════════════╝
  
  Total buffer size = header + keys + next + buckets
                    = 48    + 32    + 32   + 64      = 176 bytes
  (compared to a full HashMap<int,int> which would add 32 bytes for Values)
```

### Comparison: HashSet vs HashMap Memory

```
  DynamicHashMap<int, int>           DynamicHashSet<int>
  ════════════════════════           ════════════════════
  
  ┌────────────────────┐             ┌────────────────────┐
  │ Header (44 bytes)  │             │ Header (44 bytes)  │
  ├────────────────────┤             ├────────────────────┤
  │ Values[]           │             │ (no Values array)  │
  │ sizeof(int)*Cap    │             │ 0 bytes            │
  │ = 32 bytes         │             ├────────────────────┤
  ├────────────────────┤             │ Keys[]             │
  │ Keys[]             │             │ sizeof(int)*Cap    │
  │ sizeof(int)*Cap    │             │ = 32 bytes         │
  │ = 32 bytes         │             ├────────────────────┤
  ├────────────────────┤             │ Next[]             │
  │ Next[]             │             │ 4*Cap = 32 bytes   │
  │ 4*Cap = 32 bytes   │             ├────────────────────┤
  ├────────────────────┤             │ Buckets[]          │
  │ Buckets[]          │             │ 4*2*Cap = 64 bytes │
  │ 4*2*Cap = 64 bytes │             └────────────────────┘
  └────────────────────┘
  Total = 44+32+32+32+64 = 204       Total = 44+0+32+32+64 = 172
  
  Savings: Values[] is eliminated entirely
```

---

## Initialization

```
  InitializeHashSet<TBuffer, TKey>(buffer, capacity=0, minGrowth=0):
  ────────────────────────────────────────────────────────────────
  
  1. bytes = buffer.Reinterpret<byte>()
  2. DynamicHashMapHelper<TKey>.Init(bytes, capacity,
       sizeOfValueT: 0,     ← KEY: zero-size values
       minGrowth)
  3. Inside Init():
     └─ CalculateDataSize(capacity, bucketCapacity,
          sizeOfValueT: 0, ...)
        ┌─────────────────────────────────────────────────────┐
        │ valuesOffset = Align(sizeof(header), 16) = 48      │
        │ valuesSize   = 0 * capacity = 0        ← no space  │
        │ keysOffset   = Align(48 + 0, 4) = 48              │
        │ keysSize     = sizeof(TKey) * capacity              │
        │ nextOffset   = Align(keysOffset + keysSize, 4)     │
        │ nextSize     = 4 * capacity                         │
        │ bucketOffset = Align(nextOffset + nextSize, 4)     │
        │ bucketSize   = 4 * (2 * capacity)                   │
        │ totalSize    = bucketOffset + bucketSize             │
        └─────────────────────────────────────────────────────┘
  
  Result: Keys[] starts immediately after header with no gap for Values
```

---

## Add Algorithm (Unique Check)

```
  DynamicHashSet<T>.Add(item):
  ─────────────────────────────
  
  return DynamicHashMapHelper<T>.TryAdd(buffer, ref helper, item) != -1
  
  TryAdd():
  ──────────
  1. IF helper->Find(item) == -1:     ← CHECK FOR EXISTENCE FIRST
  │     return AddNewKey(buffer, ref helper, item)
  │  ELSE:
  │     return -1                       ← already exists, do nothing
  │
  └─ AddNewKey():
     1. IF AllocatedIndex >= Capacity AND FirstFreeIdx < 0:
        └─ Resize to larger capacity
     2. idx = FirstFreeIdx (if >= 0) else AllocatedIndex++
     3. Keys[idx] = item              ← element stored as a "key"
     4. bucket = hash(item) & BucketCapacityMask
     5. Next[idx] = Buckets[bucket]   ← prepend to chain
     6. Buckets[bucket] = idx
     7. Count++
     8. RETURN idx
  
  Compare to DynamicMultiHashMap.Add:
  ────────────────────────────────────
  MultiHashMap: AddMulti() → AddNewKey() directly (NO Find check)
  HashSet:      TryAdd()   → Find() first, then AddNewKey()
```

### Visual: Adding Elements

```
  STEP 1: Add(42)  →  hash(42) & 0xF = 2

  Buckets: [-1,-1, 0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1]
                              ↑ bucket 2 → idx 0
  Keys:   [ 42, --, --, --, --, --, --, --]
  Next:   [-1, -1, -1, -1, -1, -1, -1, -1]
  Count=1, AllocatedIndex=1

  STEP 2: Add(17)  →  hash(17) & 0xF = 4

  Buckets: [-1,-1, 0,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1]
                              ↑        ↑ bucket 4 → idx 1
  Keys:   [ 42, 17, --, --, --, --, --, --]
  Next:   [-1, -1, -1, -1, -1, -1, -1, -1]
  Count=2, AllocatedIndex=2

  STEP 3: Add(42) AGAIN  →  Find(42) returns 0 → NOT -1 → return false
  ┌──────────────────────────────────────────────┐
  │ 42 already exists at index 0. Add rejected.  │
  │ No change to the set.                        │
  └──────────────────────────────────────────────┘

  STEP 4: Add(58)  →  hash(58) & 0xF = 2  (COLLISION with 42!)

  Buckets: [-1,-1, 2,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1]
                              ↑ bucket 2 → idx 2 (replaced 0)
  Keys:   [ 42, 17, 58, --, --, --, --, --]
  Next:   [ -1,-1,  0,-1, -1, -1, -1, -1]
                       ↑ Next[2] = 0 (chain: 2→0)

  Looking up bucket 2: Buckets[2]=2→Keys[2]=58, Next[2]=0→Keys[0]=42
  Both 42 and 58 are at bucket 2, chained: 2 → 0 → -1
```

---

## Remove Algorithm

```
  DynamicHashSet<T>.Remove(item):
  ─────────────────────────────────
  
  return helper->TryRemove(item) != -1
  
  TryRemove():
  ────────────
  1. bucket = hash(item) & mask
  2. Walk chain: Buckets[bucket] → Next[] → ...
  3. Find entry where Keys[entryIdx] == item
  4. Unlink from chain:
     IF prevEntry < 0:
       Buckets[bucket] = Next[entryIdx]     // was head of chain
     ELSE:
       Next[prevEntry] = Next[entryIdx]     // skip over
  5. Push onto free list:
     Next[entryIdx] = FirstFreeIdx
     FirstFreeIdx = entryIdx
  6. Count--
  7. RETURN 1 (removed)
  
  (TryRemove only removes first match then stops — 
   but sets have no duplicates, so exactly 0 or 1 removed)
```

### Visual: Removal and Free List

```
  Before: Remove(17)
  
  Buckets[4] → 1 → -1
  Keys: [42, 17, 58, --, ...]
  
  ── After unlinking idx 1 from chain: ──
  
  Buckets[4] → -1                    // chain now empty
  Next[1] = FirstFreeIdx (-1)        // idx 1 is free
  FirstFreeIdx = 1
  
  Keys: [42, --, 58, --, ...]        // slot 1 available
  Next: [-1, -1, 0, -1, ...]
               ↑
         FirstFreeIdx ──────→ 1
  
  ── Next Add will reuse slot 1: ──
  
  Add(99): hash(99) & 0xF = some_bucket
  idx = FirstFreeIdx = 1
  FirstFreeIdx = Next[1] = -1        // pop from free list
  
  Keys[1] = 99                       // reuse the hole!
  ... re-link into bucket chain
```

---

## Contains / Find

```
  DynamicHashSet<T>.Contains(item):
  ────────────────────────────────────
  
  return helper->Find(item) != -1
  
  Find(item):
  ───────────
  1. IF AllocatedIndex == 0 → return -1
  2. bucket = hash(item) & BucketCapacityMask
  3. entryIdx = Buckets[bucket]
  4. WHILE Keys[entryIdx] != item:
     │  entryIdx = Next[entryIdx]
     └─ IF entryIdx >= Capacity → return -1
  5. RETURN entryIdx        // found!
  
  Average: O(1) with load factor ≤ 0.5
  Worst case: O(n) if all items hash to same bucket
```

---

## Data Flow: Complete Lifecycle

```
  ╔═════════════════════════════════════════════════════════════════════════╗
  ║                    DYNAMIC HASH SET LIFECYCLE                         ║
  ╠═════════════════════════════════════════════════════════════════════════╣
  ║                                                                       ║
  ║  1. DECLARE (authoring / archetype)                                   ║
  ║     struct MySet : IDynamicHashSet<int> { byte Value { get; } }       ║
  ║     → Buffer element is 1 byte (IBufferElementData)                  ║
  ║                                                                       ║
  ║  2. INITIALIZE (system / job)                                         ║
  ║     buffer.InitializeHashSet<MySet, int>(capacity: 16)               ║
  ║         │                                                             ║
  ║         ├─ Reinterpret buffer as byte[]                               ║
  ║         ├─ DynamicHashMapHelper<int>.Init(buf, 16, sizeOfValue: 0)   ║
  ║         └─ Resizes buffer, writes header, clears arrays              ║
  ║                                                                       ║
  ║  3. USE (in jobs / systems)                                           ║
  ║     var set = buffer.AsHashSet<MySet, int>();                         ║
  ║                                                                       ║
  ║     set.Add(42)      → Find(42)==-1 → insert → true                  ║
  ║     set.Add(42)      → Find(42)==0  → skip   → false                 ║
  ║     set.Contains(42) → Find(42)==0  → true                           ║
  ║     set.Remove(42)   → unlink + free list → true                     ║
  ║     set.Add(42)      → reuses freed slot   → true                    ║
  ║                                                                       ║
  ║  4. FLATTEN (optional, after many removes)                            ║
  ║     set.Flatten()                                                     ║
  ║         │                                                             ║
  ║         └─ ResizeExact(Count) — removes holes, compacts              ║
  ║                                                                       ║
  ╚═════════════════════════════════════════════════════════════════════════╝
```

---

## Relationship to DynamicHashMap

```
  Shared Infrastructure:
  ══════════════════════
  
  DynamicHashMap<TKey, TValue>     DynamicMultiHashMap<TKey, TValue>
           │                                    │
           │  uses                               │  uses
           ▼                                    ▼
      DynamicHashMapHelper<TKey>          DynamicHashMapHelper<TKey>
           │                                    │
           │  Init(sizeOfValue: sizeof(TValue))  │  Init(sizeOfValue: sizeof(TValue))
           ▼                                    ▼
      [Header][Values][Keys][Next][Buckets] [Header][Values][Keys][Next][Buckets]

  DynamicHashSet<T>
           │
           │  uses
           ▼
      DynamicHashMapHelper<T>
           │
           │  Init(sizeOfValue: 0)  ← THE ONLY DIFFERENCE
           ▼
      [Header][Keys][Next][Buckets]
               ↑
               No Values array at all! Elements live in Keys[].
  
  Same helper, same algorithms. The set is a degenerate map where value size = 0.
```

---

## Key Design Decisions

1. **Reuse HashMapHelper**: DynamicHashSet doesn't have its own data structure. It's the same
   `DynamicHashMapHelper<T>` with `SizeOfTValue = 0`. This eliminates code duplication and
   leverages the well-tested chaining, free-list, resize, and flatten logic.

2. **Zero-size Values array**: `CalculateDataSize` with `sizeOfValueT=0` produces `valuesSize=0`,
   so the Values "array" occupies zero bytes. The KeysOffset equals ValuesOffset.

3. **Unique enforcement via TryAdd**: Unlike `DynamicMultiHashMap.AddMulti` (which always inserts),
   the set uses `TryAdd` which calls `Find` first. This guarantees no duplicates.

4. **TryRemove vs Remove**: The set uses `TryRemove` which stops after the first match (since
   duplicates can't exist). The multi-map uses `Remove` which walks the entire chain.

5. **Buffer element is 1 byte**: The `IDynamicHashSet<T>` interface requires `byte Value { get; }`,
   making the buffer element a single byte. The actual data is managed by the helper header,
   which can resize the buffer far beyond the initial byte.

---

## Performance Characteristics

| Operation     | Average  | Worst    | Notes                                   |
|---------------|----------|----------|-----------------------------------------|
| Add           | O(1)     | O(N)     | Find + insert; worst = resize           |
| Contains      | O(1)     | O(N)     | Walk chain at bucket                    |
| Remove        | O(1)     | O(N)     | Walk chain, unlink, push to free list   |
| Clear         | O(B+C)   | O(B+C)   | MemSet buckets + next arrays            |
| Flatten       | O(N)     | O(N)     | Full rebuild at minimum capacity        |
| ToNativeArray | O(N)     | O(N)     | Walk all buckets/chains                 |

**Memory per entry**: sizeof(TKey) + sizeof(int) = sizeof(T) + 4 bytes (Next pointer)
**Bucket overhead**: 2 * Capacity * 4 bytes (always 2x the entry capacity)
**Total overhead**: Header (44 bytes) + Buckets + Next + alignment padding

**Advantage over full HashMap**: Saves sizeof(TValue) * Capacity bytes — for large value types,
this is significant. For a set of ints with Capacity=1024: saves 0 bytes (SizeOfTValue=0 already)
but conceptually cleaner than a HashMap<T, byte>.
