     1|# DynamicMultiHashMap
     2|
     3|**Embeds a multi-value (duplicate-key) hash map entirely inside a Unity DynamicBuffer<byte> on an entity.**
     4|
     5|## Overview
     6|
     7|DynamicMultiHashMap stores a complete open-addressed-with-chaining hash map inside a single
     8|`DynamicBuffer<byte>` attached to an ECS entity. This means no separate native container
     9|allocation — the map data travels with the entity, serializes with it, and is accessible in
    10|jobs without additional lookups.
    11|
    12|The "multi" variant allows duplicate keys. Calling `Add(key, value)` always inserts a new
    13|entry, even if the key already exists. Values for a given key are iterated via
    14|`TryGetFirstValue` / `TryGetNextValue`, which walk the chain at a single bucket.
    15|
    16|Internally, the buffer is laid out as a header struct (`DynamicHashMapHelper<TKey>`) followed
    17|by four contiguous arrays: **Values**, **Keys**, **Next**, and **Buckets**. Collision chains
    18|are linked via the Next array (each slot stores the index of the next entry in the same bucket).
    19|
    20|---
    21|
    22|## Memory Layout
    23|
    24|### Buffer Byte-Level Layout
    25|
    26|```
    27|  DynamicBuffer<byte> contents:
    28|  ╔════════════════════════════════════════════════════════════════════════════════════╗
    29|  ║                                                                                  ║
    30|  ║  Offset 0x00:  DynamicHashMapHelper<TKey>   (header, ~44 bytes)                 ║
    31|  ║                ┌─────────────────────────────────────────────────────┐            ║
    32|  ║                │ ValuesOffset       (int)                           │            ║
    33|  ║                │ KeysOffset         (int)                           │            ║
    34|  ║                │ NextOffset         (int)                           │            ║
    35|  ║                │ BucketsOffset      (int)                           │            ║
    36|  ║                │ Count              (int) — current # of entries   │            ║
    37|  ║                │ Capacity           (int) — max entries            │            ║
    38|  ║                │ BucketCapacityMask (int) — buckets-1              │            ║
    39|  ║                │ Log2MinGrowth      (int)                          │            ║
    40|  ║                │ AllocatedIndex     (int) — high-water mark        │            ║
    41|  ║                │ FirstFreeIdx       (int) — free list head         │            ║
    42|  ║                │ SizeOfTValue       (int)                          │            ║
    43|  ║                └─────────────────────────────────────────────────────┘            ║
    44|  ║                                                                                  ║
    45|  ║  [padding to 16-byte alignment]                                                  ║
    46|  ║                                                                                  ║
    47|  ║  Offset ValuesOffset:  Values[Capacity]   (SizeOfTValue * Capacity bytes)        ║
    48|  ║                         ┌───────┬───────┬───────┬───────┐                        ║
    49|  ║                         │ val 0 │ val 1 │ val 2 │  ...  │                        ║
    50|  ║                         └───────┴───────┴───────┴───────┘                        ║
    51|  ║                                                                                  ║
    52|  ║  [padding to alignof(TKey)]                                                      ║
    53|  ║                                                                                  ║
    54|  ║  Offset KeysOffset:    Keys[Capacity]     (sizeof(TKey) * Capacity bytes)         ║
    55|  ║                         ┌───────┬───────┬───────┬───────┐                        ║
    56|  ║                         │ key 0 │ key 1 │ key 2 │  ...  │                        ║
    57|  ║                         └───────┴───────┴───────┴───────┘                        ║
    58|  ║                                                                                  ║
    59|  ║  [padding to 4-byte alignment]                                                   ║
    60|  ║                                                                                  ║
    61|  ║  Offset NextOffset:    Next[Capacity]     (4 * Capacity bytes)                    ║
    62|  ║                         ┌───────┬───────┬───────┬───────┐                        ║
    63|  ║                         │ nxt 0 │ nxt 1 │ nxt 2 │  ...  │                        ║
    64|  ║                         └───────┴───────┴───────┴───────┘                        ║
    65|  ║                                                                                  ║
    66|  ║  [padding to 4-byte alignment]                                                   ║
    67|  ║                                                                                  ║
    68|  ║  Offset BucketsOffset: Buckets[BucketCapacity]  (4 * BucketCapacity bytes)       ║
    69|  ║  where BucketCapacity = 2 * Capacity                                             ║
    70|  ║                         ┌─────┬─────┬─────┬─────┬─────┬─────┐                   ║
    71|  ║                         │bkt 0│bkt 1│bkt 2│bkt 3│ ... │bkt N│                   ║
    72|  ║                         └─────┴─────┴─────┴─────┴─────┴─────┘                   ║
    73|  ║                                                                                  ║
    74|  ╚════════════════════════════════════════════════════════════════════════════════════╝
    75|```
    76|
    77|### Header Struct Detail (DynamicHashMapHelper<TKey>)
    78|
    79|```
    80|  DynamicHashMapHelper<TKey>  — 44 bytes, sequential layout
    81|  ┌──────────┬────────────────────┬──────────────────────────────────────────────────┐
    82|  │ Offset   │ Field              │ Purpose                                          │
    83|  ├──────────┼────────────────────┼──────────────────────────────────────────────────┤
    84|  │ 0x00     │ ValuesOffset       │ Byte offset from header start to Values[]        │
    85|  │ 0x04     │ KeysOffset         │ Byte offset from header start to Keys[]          │
    86|  │ 0x08     │ NextOffset         │ Byte offset from header start to Next[]          │
    87|  │ 0x0C     │ BucketsOffset      │ Byte offset from header start to Buckets[]       │
    88|  │ 0x10     │ Count              │ Current number of live entries                   │
    89|  │ 0x14     │ Capacity           │ Max entries (power of 2)                         │
    90|  │ 0x18     │ BucketCapacityMask │ = BucketCapacity - 1 (for fast modulo via &)    │
    91|  │ 0x1C     │ Log2MinGrowth      │ Growth granularity as log2                       │
    92|  │ 0x20     │ AllocatedIndex     │ Next unused slot index (high-water mark)         │
    93|  │ 0x24     │ FirstFreeIdx       │ Head of free-list chain (-1 = empty)             │
    94|  │ 0x28     │ SizeOfTValue       │ Size of each value element in bytes              │
    95|  └──────────┴────────────────────┴──────────────────────────────────────────────────┘
    96|  Total = 11 * 4 = 44 bytes
    97|```
    98|
    99|---
   100|
   101|## Hash Map Internals: Chaining via Next[]
   102|
   103|```
   104|  HOW ENTRIES MAP TO BUCKETS
   105|  ═══════════════════════════
   106|
   107|  Hash function: bucket = key.GetHashCode() & BucketCapacityMask
   108|
   109|  Buckets[] (indexed by hash bucket)
   110|  ┌─────┬────────┐
   111|  │ idx │  value │
   112|  ├─────┼────────┤
   113|  │  0  │   3    │───┐  entry index 3
   114|  │  1  │  -1    │   │  (empty bucket)
   115|  │  2  │   0    │──┐│  entry index 0
   116|  │  3  │   7    │─┐││
   117|  │  4  │   2    │ │││
   118|  └─────┴────────┘ │││
   119|                   │││
   120|  Keys[]           │││    Next[]       Values[]
   121|  ┌─────┬──────┐   │││    ┌─────┬─────┐  ┌─────┬──────┐
   122|  │  0  │ "A"  │◄──┘││    │  0  │  5  │  │  0  │ v0   │
   123|  │  1  │ "B"  │    ││    │  1  │ -1  │  │  1  │ v1   │
   124|  │  2  │ "C"  │◄───┘│    │  2  │ -1  │  │  2  │ v2   │
   125|  │  3  │ "A"  │◄────┘    │  3  │  6  │  │  3  │ v3   │
   126|  │  4  │ "D"  │          │  4  │ -1  │  │  4  │ v4   │
   127|  │  5  │ "E"  │          │  5  │ -1  │  │  5  │ v5   │
   128|  │  6  │ "A"  │          │  6  │ -1  │  │  6  │ v6   │
   129|  │  7  │ "F"  │◄─────────┘    │  7  │  1  │  │  7  │ v7   │
   130|  └─────┴──────┘               └─────┴─────┘  └─────┴──────┘
   131|
   132|  Example: Looking up key "A"
   133|  ──────────────────────────────
   134|
   135|  1. hash("A") & BucketCapacityMask → bucket 2
   136|  2. Buckets[2] = 0 → Keys[0] = "A" ✓  → read Values[0]
   137|  3. Next[0] = 5   → Keys[5] = "E" ✗  → skip
   138|  4. Next[5] = -1  → done
   139|
   140|  Wait — "A" actually hashes to bucket 2:
   141|  Buckets[2] = 0, Keys[0] = "A" ✓ match
   142|  Next[0] = 5, Keys[5] = "E" ✗ not match  
   143|  Next[5] = -1 → end of chain
   144|
   145|  But key "A" is ALSO at index 3 (multi-value!):
   146|  Buckets[hash("A")] may = 3, Next[3] = 6, Keys[6] = "A" ✓
   147|  
   148|  (The multi-map stores duplicates: iterating all values for "A"
   149|   means walking the entire chain at bucket[hash("A")] and 
   150|   checking each entry's key.)
   151|```
   152|
   153|### Multi-Value Chain Walk (TryGetFirstValue / TryGetNextValue)
   154|
   155|```
   156|  TryGetFirstValue("A"):
   157|  ────────────────────────
   158|  1. bucket = hash("A") & mask
   159|  2. entryIdx = Buckets[bucket]
   160|  3. WHILE Keys[entryIdx] != "A":
   161|         entryIdx = Next[entryIdx]
   162|         if entryIdx >= Capacity → return false
   163|  4. it.NextEntryIndex = Next[entryIdx]     // remember chain position
   164|  5. RETURN Values[entryIdx], iterator
   165|  
   166|  TryGetNextValue(ref it):
   167|  ─────────────────────────
   168|  1. entryIdx = it.NextEntryIndex
   169|  2. WHILE Keys[entryIdx] != "A":
   170|         entryIdx = Next[entryIdx]
   171|         if entryIdx >= Capacity → return false
   172|  3. it.NextEntryIndex = Next[entryIdx]
   173|  4. RETURN Values[entryIdx]
   174|```
   175|
   176|---
   177|
   178|## Add Algorithm (AddMulti → AddNewKey)
   179|
   180|```
   181|  DynamicMultiHashMap.Add(key, value):
   182|  ─────────────────────────────────────
   183|  
   184|  1. idx = AddMulti(buffer, ref helper, key)
   185|     └─ AddNewKey(buffer, ref helper, key):
   186|        │
   187|        ├─ IF AllocatedIndex >= Capacity AND FirstFreeIdx < 0:
   188|        │  └─ RESIZE: CalcCapacityCeilPow2, Resize buffer
   189|        │
   190|        ├─ idx = FirstFreeIdx          // try free list first
   191|        │  IF idx >= 0:
   192|        │    FirstFreeIdx = Next[idx]  // pop from free list
   193|        │  ELSE:
   194|        │    idx = AllocatedIndex++    // use next fresh slot
   195|        │
   196|        ├─ Keys[idx] = key
   197|        ├─ bucket = hash(key) & BucketCapacityMask
   198|        ├─ Next[idx] = Buckets[bucket]    // prepend to chain
   199|        ├─ Buckets[bucket] = idx
   200|        ├─ Count++
   201|        └─ RETURN idx
   202|
   203|  2. Values[idx] = value              // write value at returned index
   204|```
   205|
   206|### Visual: Add Sequence
   207|
   208|```
   209|  INITIAL STATE (empty, Capacity=4, BucketCapacity=8):
   210|  
   211|  Buckets: [-1, -1, -1, -1, -1, -1, -1, -1]
   212|  Keys:    [ _,  _,  _,  _]
   213|  Values:  [ _,  _,  _,  _]
   214|  Next:    [-1, -1, -1, -1]
   215|  Count=0, AllocatedIndex=0, FirstFreeIdx=-1
   216|
   217|  ── Add(key="X", value=10): hash("X") & 7 = 3 ──────────────────
   218|
   219|  Buckets: [-1, -1, -1,  0, -1, -1, -1, -1]   ← bucket 3 → idx 0
   220|                                  ↑
   221|  Keys:    ["X",  _,  _,  _]
   222|  Values:  [ 10,  _,  _,  _]
   223|  Next:    [-1, -1, -1, -1]
   224|  Count=1, AllocatedIndex=1
   225|
   226|  ── Add(key="X", value=20): hash("X") & 7 = 3 (DUPLICATE KEY!) ──
   227|
   228|  Buckets: [-1, -1, -1,  1, -1, -1, -1, -1]   ← bucket 3 → idx 1
   229|                                  ↑
   230|  Keys:    ["X","X",  _,  _]
   231|  Values:  [ 10, 20,  _,  _]
   232|  Next:    [  0,-1, -1, -1]        ← Next[1]=0 (chain: 1→0)
   233|  Count=2, AllocatedIndex=2
   234|
   235|  Iterating "X": idx=1→val=20, Next[1]=0→idx=0→val=10, Next[0]=-1→done
   236|```
   237|
   238|---
   239|
   240|## Remove Algorithm
   241|
   242|```
   243|  Remove(key):
   244|  ────────────
   245|
   246|  1. bucket = hash(key) & mask
   247|  2. prevEntry = -1
   248|  3. entryIdx = Buckets[bucket]
   249|  4. WHILE entryIdx is valid:
   250|     ├─ IF Keys[entryIdx] == key:
   251|     │   ├─ Unlink from chain:
   252|     │   │   IF prevEntry < 0:
   253|     │   │     Buckets[bucket] = Next[entryIdx]
   254|     │   │   ELSE:
   255|     │   │     Next[prevEntry] = Next[entryIdx]
   256|     │   │
   257|     │   ├─ Add to free list:
   258|     │   │   Next[entryIdx] = FirstFreeIdx
   259|     │   │   FirstFreeIdx = entryIdx
   260|     │   │
   261|     │   ├─ Count--
   262|     │   └─ Continue walking (multi: may have more!)
   263|     │      entryIdx = Next[entryIdx] (saved before overwrite)
   264|     │
   265|     └─ ELSE:
   266|        ├─ prevEntry = entryIdx
   267|        └─ entryIdx = Next[entryIdx]
   268|```
   269|
   270|### Free List After Removal
   271|
   272|```
   273|  Before removal of idx 1:
   274|  
   275|  Buckets[3] → 1 → 0 → -1
   276|  
   277|  After removal of idx 1:
   278|  
   279|  Buckets[3] → 0 → -1           (chain skips removed idx)
   280|  
   281|  Next[1] = FirstFreeIdx         (idx 1 joins free list)
   282|  FirstFreeIdx = 1
   283|  
   284|  ┌──────────────────────────────────────┐
   285|  │ Free list: FirstFreeIdx → 1 → -1    │
   286|  │                                      │
   287|  │ Next[] values repurposed as links    │
   288|  │ in the free list chain. Next[1]      │
   289|  │ previously pointed to 0 (in chain), │
   290|  │ now points to old FirstFreeIdx (-1). │
   291|  └──────────────────────────────────────┘
   292|```
   293|
   294|---
   295|
   296|## Resize and Flatten
   297|
   298|```
   299|  Resize (when Capacity is exhausted and free list is empty):
   300|  ──────────────────────────────────────────────────────────
   301|  
   302|  1. newCapacity = ceilpow2(max(Count, Capacity + growth))
   303|  2. newBucketCapacity = ceilpow2(2 * newCapacity)
   304|  3. Allocate temp copies of Values, Keys, Next, Buckets
   305|  4. buffer.ResizeUninitialized(totalSize)
   306|  5. Recalculate all offsets
   307|  6. Clear() — resets Buckets/Next to 0xFF, Count=0, etc.
   308|  7. Rebuild: iterate old buckets/chains, re-insert all entries
   309|  
   310|  Fast-path optimization:
   311|  ────────────────────────
   312|  IF no holes (FirstFreeIdx == -1 && AllocatedIndex == Count)
   313|     AND newCapacity > oldCapacity:
   314|    
   315|    → Copy Values[] and Keys[] directly (dense, no gaps)
   316|    → Only rebuild Buckets[] and Next[] chains
   317|    → Much faster than full rebuild
   318|  
   319|  Flatten():
   320|  ──────────
   321|  Removes all holes by resizing to exact Count.
   322|  Compacts the map to minimum capacity.
   323|```
   324|
   325|---
   326|
   327|## Data Flow: Full Lifecycle
   328|
   329|```
   330|  ╔═══════════════════════════════════════════════════════════════════════════╗
   331|  ║                    DYNAMIC MULTI HASH MAP LIFECYCLE                      ║
   332|  ╠═══════════════════════════════════════════════════════════════════════════╣
   333|  ║                                                                         ║
   334|  ║  1. INIT                                                                 ║
   335|  ║     DynamicHashMapHelper<TKey>.Init(buffer, capacity, sizeof(TValue))   ║
   336|  ║         │                                                               ║
   337|  ║         ├─ Calculate total buffer size (header + 4 arrays)              ║
   338|  ║         ├─ buffer.ResizeUninitialized(totalSize)                        ║
   339|  ║         ├─ Write header fields (offsets, capacity, etc.)                ║
   340|  ║         └─ Clear(): MemSet Buckets/Next to 0xFF (-1), reset counts     ║
   341|  ║                                                                         ║
   342|  ║  2. ADD (allows duplicate keys)                                         ║
   343|  ║     map.Add(key, value)                                                 ║
   344|  ║         │                                                               ║
   345|  ║         ├─ Allocate slot (from free list or AllocatedIndex++)           ║
   346|  ║         ├─ Keys[idx] = key                                              ║
   347|  ║         ├─ Prepend to bucket chain: Next[idx] = Buckets[bkt]            ║
   348|  ║         ├─ Buckets[bkt] = idx                                           ║
   349|  ║         └─ Values[idx] = value                                          ║
   350|  ║                                                                         ║
   351|  ║  3. QUERY (iterate all values for a key)                                ║
   352|  ║     TryGetFirstValue(key, out val, out it)                              ║
   353|  ║     while (TryGetNextValue(out val, ref it)) { ... }                    ║
   354|  ║         │                                                               ║
   355|  ║         ├─ Hash key → bucket index                                      ║
   356|  ║         ├─ Walk chain: Buckets[bkt] → Next[] → Next[] → ...            ║
   357|  ║         └─ Yield Values[] at each matching entry                        ║
   358|  ║                                                                         ║
   359|  ║  4. REMOVE (removes ALL entries for key)                                ║
   360|  ║     map.Remove(key) → returns count of removed items                    ║
   361|  ║         │                                                               ║
   362|  ║         ├─ Walk chain at bucket[hash(key)]                              ║
   363|  ║         ├─ Unlink matching entries from chain                           ║
   364|  ║         └─ Push each freed index onto free list                         ║
   365|  ║                                                                         ║
   366|  ║  5. FLATTEN (optional compaction)                                       ║
   367|  ║     map.Flatten()                                                       ║
   368|  ║         │                                                               ║
   369|  ║         └─ ResizeExact(Count) — removes holes, compacts to minimum     ║
   370|  ║                                                                         ║
   371|  ╚═══════════════════════════════════════════════════════════════════════════╝
   372|```
   373|
   374|---
   375|
   376|## Key Design Decisions
   377|
   378|1. **Embedded in DynamicBuffer<byte>**: All data lives inside the entity's buffer. No external
   379|   allocations, no indirection. The map is serialized/deserialized with the entity automatically.
   380|
   381|2. **Offset-based internal pointers**: Instead of raw pointers (which would break across
   382|   serialization), all array locations are stored as byte offsets from the header start.
   383|   After resize, offsets are recalculated.
   384|
   385|3. **Separate chaining via Next[]**: Each entry has a `Next[index]` field forming a linked
   386|   list within the same array. This avoids the clustering problems of open addressing and
   387|   makes deletion simple.
   388|
   389|4. **Bucket capacity = 2x entry capacity**: Keeping the load factor below 0.5 reduces chain
   390|   length, giving O(1) average lookup even with multi-value duplicates.
   391|
   392|5. **Free list for removed entries**: Removed slots aren't immediately compacted; instead they
   393|   form a free list (linked via Next[]) that new additions can reuse. `Flatten()` compacts.
   394|
   395|6. **Multi-value semantics**: `AddMulti` always appends without checking for existing keys.
   396|   This is faster than `TryAdd` (which calls `Find` first) for the common case where the
   397|   caller knows duplicates are desired.
   398|
   399|7. **Fast-path resize when no holes**: If the map has no gaps (no deletions since last
   400|   compaction), resize just copies the dense key/value arrays and rebuilds only the bucket
   401|   chains, avoiding a full rehash.
   402|
   403|---
   404|
   405|## Performance Characteristics
   406|
   407|| Operation              | Average  | Worst    | Notes                              |
   408||------------------------|----------|----------|------------------------------------|
   409|| Add                    | O(1)     | O(N)     | Worst = resize + rehash            |
   410|| Add (multi, no find)   | O(1)     | O(N)     | Skips duplicate check             |
   411|| Find / ContainsKey     | O(1)     | O(N)     | Walk chain at bucket               |
   412|| TryGetFirstValue       | O(1)     | O(N)     | Walk chain until key match         |
   413|| TryGetNextValue        | O(1)     | O(N)     | Continue walking chain             |
   414|| Remove (all for key)   | O(k)     | O(N)     | k = values for that key            |
   415|| Clear                  | O(B+C)   | O(B+C)   | MemSet buckets + next arrays       |
   416|| Flatten                | O(N)     | O(N)     | Full rebuild at minimum capacity   |
   417|| Resize                 | O(N)     | O(N)     | Copy + rehash all entries          |
   418|| AddBatchUnsafe         | O(n)     | O(N+n)   | n = batch size, no hole check      |
   419|
   420|**Memory overhead**: Header (44 bytes) + Buckets (8*Capacity bytes) + Next (4*Capacity bytes)
   421|+ alignment padding. For Capacity=16 with int keys/int values: ~44 + 128 + 64 + 64 + 128 = ~428 bytes total.
   422|
   423|**Load factor**: Always ≤ 0.5 (BucketCapacity = 2 * Capacity), ensuring short chains.
   424|
   425|**Fragmentation**: Removed entries create holes in the dense arrays. Call `Flatten()` to
   426|compact if needed. Holes do not affect correctness, only waste space.
   427|
   428|## Verified Data

## Verified Data

```
DynamicMultiHashMap<TKey,TValue>
  Kind: struct, 72 bytes
  Interfaces:
    System.Collections.IEnumerable
    System.Collections.Generic.IEnumerable`1[[BovineLabs.Core.Iterators.KVPair`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Byte, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], BovineLabs.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
  Properties:
    Boolean IsCreated
    Boolean IsEmpty
    Int32 Count
    Int32 Capacity
    DynamicHashMapHelper`1* Helper
  Methods:
    public Void Add(Int32 key, Byte item)
    public Void AddBatchUnsafe(NativeArray`1 keys, NativeArray`1 values)
    public Void AddBatchUnsafe(Int32* keys, Byte* values, Int32 length)
    private Void CheckLengthsMatch(Int32 keys, Int32 values)
    private Void CheckSize(DynamicBuffer`1 buffer)
    public Void Clear()
    public Boolean Contains(Int32 key, T value)
    public Boolean ContainsKey(Int32 key)
    public Int32 CountValuesForKey(Int32 key)
    public Boolean Equals(Object obj)
    public Void Flatten()
    public DynamicHashMapEnumerator`2 GetEnumerator()
    public Int32 GetHashCode()
    public NativeArray`1 GetKeyArray(AllocatorHandle allocator)
    public NativeKeyValueArrays`2 GetKeyValueArrays(AllocatorHandle allocator)
    public Type GetType()
    public NativeArray`1 GetValueArray(AllocatorHandle allocator)
    public DynamicHashMapKeyEnumerator`2 GetValuesForKey(Int32 key)
    private Void RefCheck()
    public Int32 Remove(Int32 key)
    private IEnumerator`1 System.Collections.Generic.IEnumerable<BovineLabs.Core.Iterators.KVPair<TKey,TValue>>.GetEnumerator()
    private IEnumerator System.Collections.IEnumerable.GetEnumerator()
    public String ToString()
    public Boolean TryGetFirstValue(Int32 key, Byte& item, HashMapIterator`1& it)
    public Boolean TryGetNextValue(Byte& item, HashMapIterator`1& it)
  DynamicHashMapHelper<TKey>
    Kind: struct, 44 bytes
    Layout: Sequential
    Fields:
      Int32 ValuesOffset (private)
      Int32 KeysOffset (private)
      Int32 NextOffset (private)
      Int32 BucketsOffset (private)
      Int32 Count (private)
      Int32 Capacity (private)
      Int32 BucketCapacityMask (private)
      Int32 Log2MinGrowth (private)
      Int32 AllocatedIndex (private)
      Int32 FirstFreeIdx (private)
      Int32 SizeOfTValue (private)
  IDynamicMultiHashMap<TKey,TValue>
    IsInterface: True
    Implements IBufferElementData: True
    Value property: Byte
Verified: 33 checks, 0 failures
```


## Source

- [BovineLabs.Core/Iterators/DynamicMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicMultiHashMap.cs)
- [BovineLabs.Core/Iterators/DynamicHashMapHelper.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMapHelper.cs)
- [BovineLabs.Core/Iterators/IDynamicMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/IDynamicMultiHashMap.cs)
