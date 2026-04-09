# DynamicVariableMap

**Maps keys to values with one or more pluggable "column" indexes — secondary data structures (hash indexes or sorted lists) embedded alongside the primary hash map in a single DynamicBuffer<byte>.**

## Overview

DynamicVariableMap combines a standard key-value hash map with one or more secondary
"column" indexes. Each entry in the map has a TKey, a TValue, and a column value per
column. The columns are indexed independently — you can look up entries by column value
as well as by key.

This is useful when you need multiple access patterns on the same data. For example:
- Map entity IDs to components, with a hash index on a "category" column for fast filtering
- Map IDs to data, with a sorted column for range queries

Two column implementations are provided:
- **MultiHashColumn<T>**: Hash-based multi-value index (multiple entries can share the same column value)
- **OrderedListColumn<T>**: Sorted doubly-linked list for ordered traversal and range queries

Variants exist for 1 column (`DynamicVariableMap<TKey, TValue, T, TC>`) and
2 columns (`DynamicVariableMap<TKey, TValue, T1, TC1, T2, TC2>`).

---

## Memory Layout

### Single-Column Variant

```
  DynamicBuffer<byte> for DynamicVariableMap<int, float, int, MultiHashColumn<int>>
  Capacity=4, BucketCapacity=8

  ╔════════════════════════════════════════════════════════════════════════════════════╗
  ║                                                                                  ║
  ║  [0x00]  DynamicVariableMapHelper Header                                         ║
  ║          ┌──────────────────────────────────────────────────────┐                  ║
  ║          │ HashHelper<TKey> KeyHash                             │                  ║
  ║          │   ├─ keyOffset      (int) ─→ Keys[]                  │                  ║
  ║          │   ├─ nextOffset     (int) ─→ Next[]                  │                  ║
  ║          │   └─ bucketsOffset  (int) ─→ Buckets[]               │                  ║
  ║          │                                                    │                  ║
  ║          │ TC Column  (MultiHashColumn<int>)                    │                  ║
  ║          │   ├─ keysOffset     (int) ─→ Column Keys[]           │                  ║
  ║          │   ├─ nextOffset     (int) ─→ Column Next[]           │                  ║
  ║          │   ├─ bucketsOffset  (int) ─→ Column Buckets[]        │                  ║
  ║          │   └─ capacity       (int)                            │                  ║
  ║          │                                                    │                  ║
  ║          │ ValuesOffset       (int)                             │                  ║
  ║          │ Count              (int)                             │                  ║
  ║          │ Capacity           (int)                             │                  ║
  ║          │ BucketCapacityMask (int) = bucketCap-1               │                  ║
  ║          │ Log2MinGrowth      (int)                             │                  ║
  ║          │ AllocatedIndex     (int)                             │                  ║
  ║          │ FirstFreeIdx       (int)                             │                  ║
  ║          └──────────────────────────────────────────────────────┘                  ║
  ║                                                                                  ║
  ║  [aligned to max(16, alignof(TValue))]                                           ║
  ║                                                                                  ║
  ║  Values[0..3]  (sizeof(float) * 4 = 16 bytes)                                   ║
  ║          ┌───────┬───────┬───────┬───────┐                                      ║
  ║          │ 1.0f  │ 2.0f  │ 3.0f  │  --   │                                      ║
  ║          └───────┴───────┴───────┴───────┘                                      ║
  ║                                                                                  ║
  ║  Keys[0..3]  (sizeof(int) * 4 = 16 bytes)  ← Primary hash map keys              ║
  ║          ┌──────┬──────┬──────┬──────┐                                          ║
  ║          │  10  │  20  │  30  │  --  │                                          ║
  ║          └──────┴──────┴──────┴──────┘                                          ║
  ║                                                                                  ║
  ║  Next[0..3]  (4 * 4 = 16 bytes)                                                 ║
  ║          ┌────┬────┬────┬────┐                                                  ║
  ║          │ -1 │ -1 │ -1 │ -1 │                                                  ║
  ║          └────┴────┴────┴────┘                                                  ║
  ║                                                                                  ║
  ║  Buckets[0..7]  (4 * 8 = 32 bytes)                                              ║
  ║          ┌────┬────┬────┬────┬────┬────┬────┬────┐                              ║
  ║          │ -1 │ -1 │  0 │ -1 │  1 │ -1 │  2 │ -1 │                             ║
  ║          └────┴────┴────┴────┴────┴────┴────┴────┘                              ║
  ║                                                                                  ║
  ║  [aligned to 16] ── Column Data Area ──                                         ║
  ║                                                                                  ║
  ║  Column Keys[0..3]  (sizeof(int) * 4 = 16 bytes)  ← Secondary index values      ║
  ║          ┌──────┬──────┬──────┬──────┐                                          ║
  ║          │ 100  │ 200  │ 100  │  --  │  (entries 0,2 share column value 100)     ║
  ║          └──────┴──────┴──────┴──────┘                                          ║
  ║                                                                                  ║
  ║  Column Next[0..3]  (4 * 4 = 16 bytes)                                          ║
  ║          ┌────┬────┬────┬────┐                                                  ║
  ║          │  2 │ -1 │ -1 │ -1 │  chain: bucket→0→2 (both have col=100)           ║
  ║          └────┴────┴────┴────┘                                                  ║
  ║                                                                                  ║
  ║  Column Buckets[0..7]  (4 * 8 = 32 bytes)                                       ║
  ║          ┌────┬────┬────┬────┬────┬────┬────┬────┐                              ║
  ║          │ -1 │ -1 │  0 │ -1 │  1 │ -1 │ -1 │ -1 │                             ║
  ║          └────┴────┴────┴────┴────┴────┴────┴────┘                              ║
  ║                                                                                  ║
  ╚════════════════════════════════════════════════════════════════════════════════════╝

  Per-entry view (each slot index spans all arrays):
  
  Slot 0:  Key=10,  Value=1.0f,  Column=100
  Slot 1:  Key=20,  Value=2.0f,  Column=200
  Slot 2:  Key=30,  Value=3.0f,  Column=100   ← same column value as slot 0
  Slot 3:  (free)
```

### Two-Column Variant Layout

```
  DynamicVariableMapHelper<TKey, TValue, T1, TC1, T2, TC2>

  Header:
  ┌─────────────────────────────────────────────────────┐
  │ HashHelper<TKey> KeyHash                             │
  │ TC1 Column1  (e.g. MultiHashColumn<Category>)        │
  │ TC2 Column2  (e.g. OrderedListColumn<Priority>)      │
  │ ValuesOffset, Count, Capacity, ...                   │
  └─────────────────────────────────────────────────────┘

  Data sections (in order):
  ┌───────────────────┐
  │ Values[Capacity]  │  ← TValue array
  ├───────────────────┤
  │ Keys[Capacity]    │  ← TKey array (primary hash)
  ├───────────────────┤
  │ Next[Capacity]    │  ← chain links (primary hash)
  ├───────────────────┤
  │ Buckets[2*Cap]    │  ← buckets (primary hash)
  ├───────────────────┤
  │ Column1 Data      │  ← TC1.CalculateDataSize(capacity)
  │  (e.g. ColKeys,   │     MultiHashColumn: Keys+Next+Buckets
  │   ColNext,        │     OrderedListColumn: Keys+Next+Prev
  │   ColBuckets)     │
  ├───────────────────┤
  │ Column2 Data      │  ← TC2.CalculateDataSize(capacity)
  │  (e.g. ColKeys,   │
  │   ColNext,        │
  │   ColPrev)        │
  └───────────────────┘
```

---

## Column Implementations

### MultiHashColumn<T> — Hash-Based Multi-Value Index

```
  MultiHashColumn stores column values in a separate chaining hash map,
  allowing multiple entries to share the same column value.
  
  Internal layout (per entry capacity N):
  ┌──────────────────────────────────────────────────┐
  │ Keys[N]     (T[])     Column value per slot      │
  │ Next[N]     (int[])   Chain links                │
  │ Buckets[2N] (int[])   Hash buckets               │
  └──────────────────────────────────────────────────┘
  
  Lookup: column.TryGetFirst(columnValue, out iterator)
  ┌──────────────────────────────────────────────────────┐
  │ 1. bucket = hash(columnValue) & (2*capacity - 1)    │
  │ 2. Walk chain: Buckets[bucket] → Next[] → ...       │
  │ 3. Return first slot where Keys[slot] == columnValue │
  │ 4. Use TryGetNext(ref iterator) for subsequent       │
  └──────────────────────────────────────────────────────┘
  
  Example: Column = Category, entries at slots 0,1,2 have cats A,A,B
  
  Keys:   [A, A, B, --]      Buckets: [-1, -1, 0, -1, 2, -1, -1, -1]
  Next:   [1, -1, -1, -1]               ↑ cat A: 0→1    ↑ cat B: 2
  
  TryGetFirst(A, out it):
    bucket 2 → Buckets[2]=0 → Keys[0]=A ✓ → EntryIndex=0, NextEntryIndex=1
  TryGetNext(ref it):
    Next[0]=1 → Keys[1]=A ✓ → EntryIndex=1, NextEntryIndex=-1
  TryGetNext(ref it):
    Next[1]=-1 → return false
```

### OrderedListColumn<T> — Sorted Doubly-Linked List

```
  OrderedListColumn maintains entries in sorted order via a
  doubly-linked list. T must implement IComparable<T>.
  
  Internal layout (per entry capacity N):
  ┌──────────────────────────────────────────────────┐
  │ Keys[N]  (T[])    Sorted value per slot          │
  │ Next[N]  (int[])  Forward links                  │
  │ Prev[N]  (int[])  Backward links                 │
  │ head     (int)    Index of first (smallest) entry │
  └──────────────────────────────────────────────────┘
  
  Add(key, idx): Insert in sorted position
  ┌──────────────────────────────────────────────────────┐
  │ 1. IF head == -1 → head = idx                       │
  │ 2. IF key < Keys[head] → insert before head         │
  │ 3. ELSE walk forward until correct position found    │
  │ 4. Update Next/Prev links of neighbors               │
  └──────────────────────────────────────────────────────┘
  
  Example: Capacity=4, entries with priorities 30, 10, 20 at slots 0,1,2
  
  After Add(30, 0):  head=0
    0: Keys=30, Next=-1, Prev=-1
    
  After Add(10, 1):  head=1
    1: Keys=10, Next=0,  Prev=-1
    0: Keys=30, Next=-1, Prev=1
    
  After Add(20, 2):  head=1
    1: Keys=10, Next=2,  Prev=-1    ← sorted: 10 → 20 → 30
    2: Keys=20, Next=0,  Prev=1
    0: Keys=30, Next=-1, Prev=2
    
  Sorted iteration: head=1 → Next[1]=2 → Next[2]=0 → Next[0]=-1
  Yields: (10, slot1) → (20, slot2) → (30, slot0)
  
  Replace optimization:
  ┌──────────────────────────────────────────────────────┐
  │ Replace checks if the new value can stay in the same │
  │ sorted position (prev <= new <= next). If so, it     │
  │ updates in-place. Otherwise, remove + re-insert.     │
  └──────────────────────────────────────────────────────┘
```

---

## Add Algorithm

```
  DynamicVariableMap.TryAdd(key, value, column):
  ──────────────────────────────────────────────

  1. Find(key) → IF found, return -1 (key must be unique)
  
  2. AddInternal():
     ┌──────────────────────────────────────────────────────────┐
     │ IF AllocatedIndex >= Capacity AND FirstFreeIdx < 0:     │
     │   └─ Resize (grow capacity, rehash primary + columns)   │
     │                                                        │
     │ idx = FirstFreeIdx (if >= 0) else AllocatedIndex++     │
     │                                                        │
     │ Keys[idx] = key          ← primary key                 │
     │ Values[idx] = value      ← primary value               │
     │                                                        │
     │ bucket = hash(key) & BucketCapacityMask                │
     │ Next[idx] = Buckets[bucket]                            │
     │ Buckets[bucket] = idx      ← link into primary chain   │
     │                                                        │
     │ Column.Add(column, idx)   ← link into column index     │
     │   MultiHashColumn: add to column hash chain            │
     │   OrderedListColumn: insert into sorted position        │
     │                                                        │
     │ Count++                                                │
     │ RETURN idx                                             │
     └──────────────────────────────────────────────────────────┘
```

### Visual: Adding Entries

```
  Add(key=10, value=1.0f, column=100)  →  idx 0
  ┌───────────────────────────────────────────────────────────────┐
  │ Primary Hash:                                                │
  │   bucket = hash(10) & 7 = 2                                 │
  │   Buckets[2] = 0 → Next[0] = -1                             │
  │   Keys[0] = 10, Values[0] = 1.0f                            │
  │                                                             │
  │ Column (MultiHashColumn):                                    │
  │   colBucket = hash(100) & 7 = 4                             │
  │   ColBuckets[4] = 0 → ColNext[0] = -1                       │
  │   ColKeys[0] = 100                                          │
  └───────────────────────────────────────────────────────────────┘

  Add(key=20, value=2.0f, column=100)  →  idx 1
  ┌───────────────────────────────────────────────────────────────┐
  │ Primary Hash:                                                │
  │   bucket = hash(20) & 7 = 4                                 │
  │   Buckets[4] = 1 → Next[1] = -1                             │
  │   Keys[1] = 20, Values[1] = 2.0f                            │
  │                                                             │
  │ Column (MultiHashColumn):                                    │
  │   colBucket = hash(100) & 7 = 4                             │
  │   ColBuckets[4] = 1 → ColNext[1] = 0 → ColNext[0] = -1      │
  │   ColKeys[1] = 100     ← Both slots 0 and 1 have col=100    │
  │                                                             │
  │   Column chain for value 100: 1 → 0 → -1                    │
  └───────────────────────────────────────────────────────────────┘
```

---

## Remove Algorithm

```
  DynamicVariableMap.Remove(key):
  ───────────────────────────────

  1. Walk primary hash chain to find slot with Keys[slot] == key
  2. Unlink from primary chain (update Buckets/Next)
  3. Push slot onto free list (Next[slot] = FirstFreeIdx)
  4. Column.Remove(slot)   ← remove from column index too!
  5. Count--
  
  RemoveAt(idx) variant:
  ┌──────────────────────────────────────────────────────┐
  │ 1. Read key = Keys[idx]                              │
  │ 2. Validate Find(key) == idx (entry exists at slot)  │
  │ 3. Walk bucket chain to find prev → unlink           │
  │ 4. Free list + Column.Remove(idx) + Count--          │
  └──────────────────────────────────────────────────────┘
  
  Note: All column indexes are updated atomically with the primary
  hash removal. An entry is either fully present or fully removed.
```

---

## Resize Algorithm

```
  When capacity is exhausted:
  ───────────────────────────

  ┌───────────────────────────────────────────────────────────────┐
  │ 1. Save old Values to temp allocation                        │
  │ 2. Save old primary hash data via HashHelper.Resize          │
  │    (oldKeys, oldNext, oldBuckets)                             │
  │ 3. Save old column data via Column.StartResize()             │
  │    (column-specific temp copies)                              │
  │ 4. buffer.ResizeUninitialized(newTotalSize)                  │
  │ 5. Update helper pointer (buffer may have moved!)            │
  │ 6. Initialize new header + offsets                            │
  │ 7. IF growing (newCap > oldCap):                             │
  │    ├─ Copy old Values back                                   │
  │    ├─ HashHelper.Resize.Increase (rehash buckets)            │
  │    └─ Column.ApplyResize (copy + rehash/rebuild column)      │
  │ 8. IF shrinking:                                             │
  │    └─ Re-insert all entries from old data (AddNoCollideNoAlloc)│
  └───────────────────────────────────────────────────────────────┘
  
  All column indexes are rebuilt during resize, just like the
  primary hash. This ensures consistent indexing.
```

---

## IColumn<T> Interface

```
  ┌──────────────────────────────────────────────────────────────┐
  │  IColumn<T> where T : unmanaged, IEquatable<T>              │
  │                                                              │
  │  Initialize(offset, capacity)  → Set up internal pointers    │
  │  CalculateDataSize(capacity)   → How many bytes needed       │
  │  GetValue(idx)                 → Read column value at slot   │
  │  Add(key, idx)                 → Index entry at slot          │
  │  Replace(newKey, idx)          → Update column value         │
  │  Remove(idx)                   → De-index entry at slot      │
  │  Clear()                       → Reset all index structures  │
  │  StartResize()                 → Save state before resize    │
  │  ApplyResize(ptr)              → Restore after resize        │
  │  GetValueOld(ptr, idx)         → Read from pre-resize state  │
  └──────────────────────────────────────────────────────────────┘
  
  Custom column types can be created by implementing this interface.
  The offset passed to Initialize is relative to the helper struct,
  accounting for the column struct's position within the helper.
```

---

## Data Flow: Complete Lifecycle

```
  ╔═══════════════════════════════════════════════════════════════════════════╗
  ║                DYNAMIC VARIABLE MAP LIFECYCLE                            ║
  ╠═══════════════════════════════════════════════════════════════════════════╣
  ║                                                                         ║
  ║  1. DECLARE                                                              ║
  ║     struct MyMap : IDynamicVariableMap<int, float, int, MultiHashColumn<int>> ║
  ║     { byte Value { get; } }                                             ║
  ║                                                                         ║
  ║  2. INITIALIZE                                                           ║
  ║     buffer.InitializeVariableMap<MyMap>(capacity: 16)                   ║
  ║         │                                                               ║
  ║         ├─ Calculate layout: header + values + keys + next + buckets    ║
  ║         │  + column data (keys + next + buckets for MultiHashColumn)    ║
  ║         └─ Resize buffer, write header, initialize column               ║
  ║                                                                         ║
  ║  3. ADD ENTRIES                                                          ║
  ║     var map = buffer.AsVariableMap<MyMap>();                            ║
  ║     map.Add(key: 10, value: 1.0f, column: 100)                         ║
  ║     map.Add(key: 20, value: 2.0f, column: 100)  ← same column value    ║
  ║     map.Add(key: 30, value: 3.0f, column: 200)                         ║
  ║                                                                         ║
  ║  4. LOOKUP BY KEY                                                        ║
  ║     map.TryGetValue(10, out val, out col)  → val=1.0f, col=100         ║
  ║                                                                         ║
  ║  5. LOOKUP BY COLUMN                                                     ║
  ║     ref var col = ref map.Column;                                      ║
  ║     col.TryGetFirst(100, out it)  → finds slot 0 (key=10)             ║
  ║     col.TryGetNext(ref it)        → finds slot 1 (key=20)             ║
  ║     col.TryGetNext(ref it)        → false (no more with col=100)       ║
  ║                                                                         ║
  ║  6. REPLACE COLUMN VALUE                                                 ║
  ║     map.ReplaceColumn(0, 300)  ← update column for slot 0             ║
  ║     Column de-indexes from 100, re-indexes into 300                    ║
  ║                                                                         ║
  ║  7. REMOVE                                                               ║
  ║     map.Remove(10)  → removes from primary hash + column index         ║
  ║                                                                         ║
  ╚═══════════════════════════════════════════════════════════════════════════╝
```

---

## Offset-Based Pointer System

```
  All internal arrays use OFFSET-BASED pointers, not raw pointers.
  This is critical because DynamicBuffer<byte> can be relocated in
  memory during resize operations.
  
  How it works:
  ┌───────────────────────────────────────────────────────────┐
  │                                                           │
  │  Header struct lives at the start of DynamicBuffer<byte>  │
  │                                                           │
  │  ValuesOffset = absolute byte offset from buffer start    │
  │  Values = (TValue*)(bufferStart + ValuesOffset)           │
  │                                                           │
  │  HashHelper stores offsets RELATIVE TO ITSELF:            │
  │    keyOffset = (dataOffset + computedKeyOffset)           │
  │              - offsetOf(HashHelper within header)         │
  │    Keys = (TKey*)(addressOf(this) + keyOffset)            │
  │                                                           │
  │  Column stores offsets RELATIVE TO ITSELF:                │
  │    keysOffset = (dataOffset + indexOffset)                │
  │               - offsetOf(Column within header)            │
  │    ColKeys = (T*)(addressOf(this) + keysOffset)           │
  │                                                           │
  │  This means after a buffer resize:                        │
  │  1. buffer may be at a new address                        │
  │  2. helper pointer is refreshed from buffer               │
  │  3. All offset-based pointers automatically work          │
  │     because they're relative to self, not absolute        │
  │                                                           │
  └───────────────────────────────────────────────────────────┘
```

---

## Key Design Decisions

1. **Pluggable columns via IColumn<T>**: The column type is a generic parameter.
   Different column implementations (hash, sorted list, custom) can be mixed and matched.
   The map doesn't need to know the column's internal structure.

2. **Columns stored inline**: Column data arrays live in the same DynamicBuffer as the
   primary hash map. No external allocations. Everything is contiguous and cache-friendly.

3. **1-column and 2-column variants**: Separate helper structs for one vs two columns.
   This avoids runtime polymorphism and keeps the common single-column case lean.

4. **Free list reuse**: When entries are removed, their slots go onto a free list.
   New inserts reuse freed slots, avoiding wasted memory.

5. **Atomic column updates**: Adding or removing an entry updates the primary hash and
   all column indexes together. There's no window where an entry is in the primary hash
   but not in a column, or vice versa.

6. **Column-level Replace optimization**: Both MultiHashColumn and OrderedListColumn
   optimize the Replace path. If the new column value maps to the same bucket (hash)
   or sorted position (list), the update is done in-place without full remove+re-add.

---

## Performance Characteristics

| Operation            | Average  | Worst    | Notes                                |
|----------------------|----------|----------|--------------------------------------|
| TryAdd               | O(1)     | O(N)     | Worst = resize + rehash all columns  |
| TryGetValue          | O(1)     | O(N)     | Primary hash lookup + column read    |
| Remove(key)          | O(1)     | O(N)     | Primary + column unchain             |
| RemoveAt(idx)        | O(1)     | O(N)     | Same as Remove but starts from idx   |
| Column lookup (hash) | O(1)     | O(N)     | MultiHashColumn chain walk           |
| Column lookup (sort) | O(1)*    | O(N)     | OrderedListColumn: GetFirst/GetNext  |
| Column range query   | O(K)     | O(N+K)   | K = results in range                 |
| Clear                | O(C+B)   | O(C+B)   | MemSet all arrays + column clears    |
| Resize               | O(N)     | O(N)     | Copy + rehash primary + all columns  |

**Memory per entry** (with MultiHashColumn<int>):
- Primary: sizeof(TKey) + sizeof(TValue) + 2*sizeof(int) = 4 + 4 + 8 = 16 bytes
- Column: sizeof(T) + sizeof(int) = 4 + 4 = 8 bytes
- Bucket overhead: 2*Capacity * 4 (primary) + 2*Capacity * 4 (column)
- Total per entry: ~24 bytes data + ~16 bytes bucket overhead = ~40 bytes

**Memory per entry** (with OrderedListColumn<int>):
- Column: sizeof(T) + 2*sizeof(int) = 4 + 8 = 12 bytes (prev+next instead of buckets)
- No bucket overhead for the column — uses head pointer + linked list
- Trade-off: O(N) lookup by column value, but O(1) sorted traversal

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/dynamic-buffers/DynamicVariableMap.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,BovineLabs.Core.Iterators.Columns,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
> ```

```
DynamicVariableMap<TKey,TValue,T,TC>
  Kind: struct
  Size: 72 bytes
  Generic params: TKey=Int32, TValue=Single, T=Int16, TC=MultiHashColumn`1

  Fields:
    [0] DynamicBuffer`1 buffer
    [64] DynamicVariableMapHelper`4* helper

  Properties:
    Boolean IsCreated (read=True, write=False)
    Boolean IsEmpty (read=True, write=False)
    Int32 Count (read=True, write=False)
    Int32 Capacity (read=True, write=True)
    MultiHashColumn`1& Column (read=True, write=False)
    DynamicVariableMapHelper`4* Helper (read=True, write=False)

  Methods:
    Boolean TryAdd(Int32 key, Single item, Int16 column)
    Boolean Remove(Int32 key)
    Void RemoveAt(Int32 idx)
    Boolean TryGetValue(Int32 key, Single& item, Int16& column)
    Void ReplaceColumn(Int32 idx, Int16 column)
    Void Clear()

MultiHashColumn<T>
  Kind: struct
  Size: 16 bytes
  Fields:
    [0] Int32 keysOffset
    [4] Int32 nextOffset
    [8] Int32 bucketsOffset
    [12] Int32 capacity

OrderedListColumn<T>
  Kind: struct
  Size: 20 bytes
  Fields:
    [0] Int32 keysOffset
    [4] Int32 nextOffset
    [8] Int32 prevOffset
    [12] Int32 head
    [16] Int32 capacity

IColumn<T>
  Kind: interface
  Methods:
    Void Initialize(Int32, Int32)
    Int32 CalculateDataSize(Int32)
    Int32 GetValue(Int32)
    Void Add(Int32, Int32)
    Void Replace(Int32, Int32)
    Void Remove(Int32)
    Void Clear()
    Void* StartResize()
    Void ApplyResize(Void*)
    Int32 GetValueOld(Void*, Int32)

IDynamicVariableMap<TKey,TValue,T,TC>
  Kind: interface
  Interfaces:
    Unity.Entities.IBufferElementData

DynamicVariableMapHelper<TKey,TValue,T,TC> (internal)
  Kind: struct
  Fields:
    HashHelper`1 KeyHash
    TC Column
    Int32 ValuesOffset
    Int32 Count
    Int32 Capacity
    Int32 BucketCapacityMask
    Int32 Log2MinGrowth
    Int32 AllocatedIndex
    Int32 FirstFreeIdx

DynamicExtensions (relevant methods)
  InitializeVariableMap overloads: 2
  AsVariableMap overloads: 2

Verified: 27 checks, 1 failures
```

## Source

- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMap.cs)
- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMapHelper2.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMapHelper2.cs)
- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMapHelper.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMapHelper.cs)
