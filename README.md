NativePerfectHashMap — Inner Workings
=======================================

Collision-free hash map ensuring ultra-fast guaranteed O(1) lookups.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │            NativePerfectHashMap<TKey, TValue>                │
  │                                                              │
  │  ┌────────────────────────────────────────────┐              │
  │  │ UnsafePerfectHashMap<TKey,TValue>* data    │              │
  │  │  (pointer to heap-allocated struct)        │              │
  │  └──────────────┬─────────────────────────────┘              │
  │                 │                                            │
  │                 ▼                                            │
  │  ┌──────────────────────────────────────────────────────┐    │
  │  │        UnsafePerfectHashMap<TKey, TValue>            │    │
  │  │                                                       │    │
  │  │  TKey* Keys      → Keys array (power-of-2 sized)    │    │
  │  │  TValue* Values  → Values array (power-of-2 sized)  │    │
  │  │  int Size        → Array length (always power of 2)  │    │
  │  │  TValue NullValue→ Sentinel for empty slots          │    │
  │  │  Allocator allocator                                │    │
  │  └──────────────────────────────────────────────────────┘    │
  │                                                              │
  │  [Safety: AtomicSafetyHandle m_Safety]                       │
  └──────────────────────────────────────────────────────────────┘


WHAT MAKES IT "PERFECT"?
────────────────────────

  A "perfect" hash function maps every key to a UNIQUE slot — zero collisions.

  ┌──────────────────────────────────────────────────────────────┐
  │  Standard HashMap:                                           │
  │  key1 ──► hash ──► bucket ──► might collide ──► chain walk  │
  │  key2 ──► hash ──► bucket ──► collision! ──► linear probe   │
  │                                                              │
  │  Perfect HashMap:                                            │
  │  key1 ──► hash & (size-1) ──► UNIQUE slot ──► direct read   │
  │  key2 ──► hash & (size-1) ──► UNIQUE slot ──► direct read   │
  │                                                              │
  │  GUARANTEED: No two keys map to the same index!              │
  └──────────────────────────────────────────────────────────────┘

  Trade-off: The array size may be larger than the number of keys
  to find a collision-free power-of-2 table size.


CONSTRUCTION: FINDING THE PERFECT SIZE
──────────────────────────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  new NativePerfectHashMap(keys, values, nullValue, allocator)│
  │                                                              │
  │  Step 1: Validate no hash collisions among keys              │
  │  ┌────────────────────────────────────────────────────┐      │
  │  │  uniqueSet = NativeHashSet<int>(keys.Length, Temp) │      │
  │  │  foreach key in keys:                              │      │
  │  │    if (!uniqueSet.Add(key.GetHashCode()))          │      │
  │  │      throw "HashCode collision!"                   │      │
  │  │                                                    │      │
  │  │  ⚠ Keys must have UNIQUE hash codes!               │      │
  │  │  This is checked at construction time.              │      │
  │  └────────────────────────────────────────────────────┘      │
  │                                                              │
  │  Step 2: Find smallest power-of-2 with no collisions         │
  │  ┌────────────────────────────────────────────────────┐      │
  │  │  size = 1                                          │      │
  │  │  while (HasCollisions(size, keys)):                │      │
  │  │    size <<= 1   // double the table                │      │
  │  │                                                    │      │
  │  │  HasCollisions checks:                             │      │
  │  │    usedIndexes.Clear()                             │      │
  │  │    foreach key:                                    │      │
  │  │      index = key.GetHashCode() & (size - 1)       │      │
  │  │      if (!usedIndexes.Add(index)):                 │      │
  │  │        return true  // collision found             │      │
  │  │    return false  // no collisions at this size     │      │
  │  └────────────────────────────────────────────────────┘      │
  │                                                              │
  │  Step 3: Allocate and populate                               │
  │  ┌────────────────────────────────────────────────────┐      │
  │  │  totalSize = sizeof(TKey) * size + sizeof(TValue) *│      │
  │  │              size                                   │      │
  │  │  ptr = Allocate(totalSize, CacheLineSize)          │      │
  │  │  Keys = (TKey*)ptr                                 │      │
  │  │  Values = (TValue*)(ptr + sizeof(TKey) * size)     │      │
  │  │                                                    │      │
  │  │  MemCpyReplicate(Values, &nullValue, size)         │      │
  │  │  // Fill ALL slots with nullValue                  │      │
  │  │                                                    │      │
  │  │  for i = 0 to keys.Length:                         │      │
  │  │    index = IndexFor(keys[i], size)                 │      │
  │  │    Keys[index] = keys[i]                           │      │
  │  │    Values[index] = values[i]                       │      │
  │  └────────────────────────────────────────────────────┘      │
  └──────────────────────────────────────────────────────────────┘


SIZE FINDING EXAMPLE
────────────────────

  Keys: [A(hash=3), B(hash=7), C(hash=11)]

  size = 1:  mask = 0  → all map to index 0  → COLLISION ✗
  size = 2:  mask = 1  → 3&1=1, 7&1=1        → COLLISION ✗
  size = 4:  mask = 3  → 3&3=3, 7&3=3        → COLLISION ✗
  size = 8:  mask = 7  → 3&7=3, 7&7=7, 11&7=3 → COLLISION ✗
  size = 16: mask = 15 → 3&15=3, 7&15=7, 11&15=11 → ALL UNIQUE ✓

  Final layout:
  Index:  0    1    2    3    4    5    6    7    8  ... 11   ... 15
  Keys:  [  ] [  ] [  ] [ A] [  ] [  ] [  ] [ B] [  ] ... [ C] ... [  ]
  Values:[NV] [NV] [NV] [vA] [NV] [NV] [NV] [vB] [NV] ... [vC] ... [NV]
          ↑NV = NullValue (sentinel)

  3 keys need 16 slots → 13 slots are "wasted"
  But: lookups are guaranteed O(1) with zero probing!


MEMORY LAYOUT
─────────────

  Single contiguous allocation:
  ┌───────────────────────────────────────────────────────────┐
  │                    Buffer                                  │
  │  ┌─────────────────────────┬───────────────────────────┐ │
  │  │     Keys[size]          │     Values[size]          │ │
  │  │  TKey * size bytes      │  TValue * size bytes      │ │
  │  │                         │                           │ │
  │  │  [0] [1] [2] ... [N-1] │  [0]  [1]  [2] ... [N-1] │ │
  │  │   ↑                     │   ↑                       │ │
  │  │   Filled only at        │   Initialized to          │ │
  │  │   occupied indices      │   NullValue, then         │ │
  │  │                         │   filled at occupied idxs  │ │
  │  └─────────────────────────┴───────────────────────────┘ │
  │  ←── valueOffset = sizeof(TKey) * size ──→               │
  └───────────────────────────────────────────────────────────┘


LOOKUP: TryGetValue
───────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  TryGetValue(key, out item):                                 │
  │                                                              │
  │  1. Compute index:                                           │
  │     index = key.GetHashCode() & (Size - 1)                  │
  │              ↑                                               │
  │     Power-of-2 modulo via bitwise AND (extremely fast)      │
  │                                                              │
  │  2. Bounds check:                                            │
  │     if (index < 0 || index >= Size) → return false          │
  │     (Always passes if hash is well-behaved)                 │
  │                                                              │
  │  3. Read value:                                              │
  │     item = Values[index]                                     │
  │                                                              │
  │  4. Null check:                                              │
  │     return !item.Equals(NullValue)                           │
  │     (NullValue means slot is empty)                         │
  │                                                              │
  │  ⚡ Total: 1 hash + 1 AND + 1 array read + 1 comparison     │
  │  ⚡ NO probing, NO chaining, NO linear search               │
  │  ⚡ Guaranteed single memory access                          │
  └──────────────────────────────────────────────────────────────┘


INDEX COMPUTATION
─────────────────

  ┌─────────────────────────────────────────────────────────────┐
  │  IndexFor(key, size):                                       │
  │    return key.GetHashCode() & (size - 1)                    │
  │                                                             │
  │  Why does this work?                                        │
  │  • Size is always a power of 2: 1, 2, 4, 8, 16, 32, ...   │
  │  • (size - 1) gives a bitmask:                             │
  │    size=8  → mask=0b111  → keeps lower 3 bits              │
  │    size=16 → mask=0b1111 → keeps lower 4 bits              │
  │  • & is equivalent to % size but MUCH faster               │
  │  • Guaranteed collision-free by construction                │
  └─────────────────────────────────────────────────────────────┘


CONSTRAINTS
───────────

  ┌──────────────────────────────────────────────────────────────┐
  │  1. Keys must have UNIQUE hash codes                        │
  │     → Verified at construction, throws if violated          │
  │                                                              │
  │  2. Keys and values are fixed after construction            │
  │     → No Add/Remove at runtime                              │
  │     → Set existing key's value is supported                 │
  │                                                              │
  │  3. Memory may be wasteful for sparse key sets              │
  │     → Table size can be much larger than key count          │
  │                                                              │
  │  4. NullValue must be chosen carefully                      │
  │     → Must not be a valid value in your data set            │
  │     → Used as sentinel to detect "not present"              │
  └──────────────────────────────────────────────────────────────┘


DISPOSAL
────────

  ┌──────────────────────────────────────────────────────────────┐
  │  Dispose():                                                  │
  │  1. Memory.Unmanaged.Free(Keys, allocator)                  │
  │     (Keys is the base pointer; frees entire buffer)         │
  │  2. data = default (null out the pointer)                    │
  └──────────────────────────────────────────────────────────────┘


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌─────────────────────────┬──────────────────────────────────────┐
  │ Operation               │ Cost                                 │
  ├─────────────────────────┼──────────────────────────────────────┤
  │ TryGetValue             │ O(1) guaranteed — single array read  │
  │ this[key] get           │ O(1) + branch on null sentinel       │
  │ this[key] set           │ O(1) — direct index write            │
  │ Construction            │ O(n * log(max_hash)) to find size    │
  │ Memory                  │ sizeof(TKey+TValue) * Size           │
  │                         │ Size >= n, can be >> n               │
  │ Collision rate          │ ZERO by construction                 │
  │ Cache friendliness      │ Excellent — linear array access      │
  └─────────────────────────┴──────────────────────────────────────┘
