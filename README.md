NativeKeyedMap — Inner Workings
================================

Optimizes hash map performance by specifically targeting integer keys.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌─────────────────────────────────────────────────────────────┐
  │               NativeKeyedMap<TValue>                        │
  │                                                             │
  │  ┌─────────────────────┐                                    │
  │  │ UnsafeKeyedMap<TVal>│  ← Core implementation (unsafe)    │
  │  │  keyedMapData       │                                    │
  │  └────────┬────────────┘                                    │
  │           │                                                 │
  │           ▼                                                 │
  │  ┌─────────────────┐                                        │
  │  │ KeyedMapData*   │  ← Single heap allocation              │
  │  │ buffer          │                                        │
  │  └────────┬────────┘                                        │
  │           │                                                 │
  │           ▼                                                 │
  │  ┌──────────────────────────────────────────────┐           │
  │  │            KeyedMapData (heap struct)         │           │
  │  │  byte* Values    → TValue array               │           │
  │  │  byte* Keys      → int array (integer keys!)  │           │
  │  │  byte* Next      → int linked-list pointers   │           │
  │  │  byte* Buckets   → int bucket heads           │           │
  │  │  int   KeyCapacity                             │           │
  │  │  int   BucketCapacity (= maxKey parameter)    │           │
  │  │  int   Length (current entry count)            │           │
  │  └──────────────────────────────────────────────┘           │
  │                                                             │
  │  [Safety: AtomicSafetyHandle m_Safety]                      │
  └─────────────────────────────────────────────────────────────┘


WHY INTEGER KEYS ARE SPECIAL
─────────────────────────────

  Standard NativeHashMap:              NativeKeyedMap:
  ┌───────────────────────┐            ┌───────────────────────┐
  │ hash = key.GetHashCode()│           │ bucket = key          │
  │ bucket = hash % capacity│           │ (key IS the bucket!)  │
  │ (collision-prone)       │           │ (zero-collision for   │
  │                         │           │  unique int keys)     │
  └───────────────────────┘            └───────────────────────┘

  Key insight: The key IS the bucket index. No hash function needed.
  Buckets array is sized to maxKey, so key → direct bucket lookup.


BUFFER MEMORY LAYOUT (Single Allocation)
────────────────────────────────────────

  Values[capacity]       Keys[capacity]       Next[capacity]       Buckets[bucketCap]
  ┌─────────────────┬─────────────────┬─────────────────┬───────────────────────┐
  │ TValue[0]       │ int key[0]      │ int next[0]     │ int bucket[0]         │
  │ TValue[1]       │ int key[1]      │ int next[1]     │ int bucket[1]         │
  │ TValue[2]       │ int key[2]      │ int next[2]     │ int bucket[2]         │
  │ ...             │ ...             │ ...             │ ...                   │
  │ TValue[cap-1]   │ int key[cap-1]  │ int next[cap-1] │ int bucket[cap-1]     │
  └─────────────────┴─────────────────┴─────────────────┴───────────────────────┘
  ←── cache-line aligned boundaries between each section ──→

  Each section is aligned to JobsUtility.CacheLineSize (64 bytes).


ADD OPERATION
─────────────

  Add(key=3, item=value):
  ┌───────────────────────────────────────────────────────────────┐
  │ 1. Check: 0 <= key < BucketCapacity (maxKey)                  │
  │                                                               │
  │ 2. Grow if Length >= KeyCapacity                              │
  │    newCap = KeyCapacity * 2 (doubling strategy)               │
  │                                                               │
  │ 3. idx = Length++   (allocate next slot)                      │
  │                                                               │
  │ 4. Write:                                                     │
  │    Keys[idx]   = key    (3)                                   │
  │    Values[idx] = item   (value)                               │
  │                                                               │
  │ 5. Insert into bucket chain:                                  │
  │    Next[idx]    = Buckets[key]   (link to previous head)      │
  │    Buckets[key] = idx            (become new head)            │
  │                                                               │
  │ This supports MULTI-value per key (like NativeMultiHashMap).  │
  └───────────────────────────────────────────────────────────────┘

  Visual example after adding (key=5,A), (key=5,B), (key=3,C):

  Buckets (indexed by key):
  Index:  0    1    2    3    4    5    ...
        ┌────┬────┬────┬────┬────┬────┬───┐
        │ -1 │ -1 │ -1 │ 2  │ -1 │ 1  │...│
        └────┴────┴────┴────┴────┴────┴───┘
                           │         │
                           ▼         ▼
  Entries:          ┌─────────────────────────────┐
  idx=0: Keys[0]=5  │ Values[0]=A  Next[0]=-1    │ ← first add
  idx=1: Keys[1]=5  │ Values[1]=B  Next[1]=0     │ ← second add (same key=5)
  idx=2: Keys[2]=3  │ Values[2]=C  Next[2]=-1    │ ← key=3
                    └─────────────────────────────┘

  Buckets[5] → 1 → 0 → -1   (two values for key 5)
  Buckets[3] → 2 → -1        (one value for key 3)


LOOKUP OPERATION
────────────────

  TryGetFirstValue(key=5):
  ┌──────────────────────────────────────────────┐
  │ 1. Check: 0 <= key < BucketCapacity          │
  │                                              │
  │ 2. it.NextEntryIndex = Buckets[key]          │
  │    (= Buckets[5] = 1)                        │
  │                                              │
  │ 3. TryGetNextValue:                          │
  │    it.EntryIndex = it.NextEntryIndex (= 1)   │
  │    item = Values[1] (= B)                    │
  │    it.NextEntryIndex = Next[1] (= 0)         │
  │    return true                               │
  │                                              │
  │ 4. Next TryGetNextValue:                     │
  │    it.EntryIndex = 0                         │
  │    item = Values[0] (= A)                    │
  │    it.NextEntryIndex = Next[0] (= -1)        │
  │    return true                               │
  │                                              │
  │ 5. Next TryGetNextValue:                     │
  │    it.EntryIndex = -1 → return false         │
  └──────────────────────────────────────────────┘


NO HASH FUNCTION — DIRECT BUCKET INDEX
───────────────────────────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │                  Traditional HashMap                         │
  │  key ──► GetHashCode() ──► modulo buckets ──► bucket index  │
  │          (expensive)       (collision risk)                  │
  └──────────────────────────────────────────────────────────────┘
                            vs
  ┌──────────────────────────────────────────────────────────────┐
  │                  NativeKeyedMap                              │
  │  key ──► Buckets[key]     (O(1) direct access)              │
  │          (zero computation)                                  │
  └──────────────────────────────────────────────────────────────┘

  Trade-off: BucketCapacity must be >= maxKey, which can be wasteful
  for sparse key ranges. But for dense integer keys, it's ideal.


RECALCULATE BUCKETS
───────────────────

  Used when entries are modified externally (e.g., from jobs):

  ┌─────────────────────────────────────────────────────┐
  │ RecalculateBuckets():                               │
  │   for idx = 0 to Length:                            │
  │     key = Keys[idx]                                 │
  │     Next[idx]  = Buckets[key]   (chain to existing) │
  │     Buckets[key] = idx          (become new head)   │
  └─────────────────────────────────────────────────────┘


CLEAR OPERATION
───────────────

  ┌──────────────────────────────────────────────────────┐
  │ MemSet(Next, 0xFF, KeyCapacity * 4)   → all -1      │
  │ MemSet(Buckets, 0xFF, BucketCapacity * 4)  → all -1 │
  │ Length = 0                                           │
  │                                                      │
  │ Note: 0xFF filled = -1 (sentinel for empty chain)    │
  └──────────────────────────────────────────────────────┘


CAPACITY GROWTH
───────────────

  GrowCapacity:
  ┌──────────────────────────────────────┐
  │ if capacity == 0: return 1           │
  │ else: return capacity * 2            │
  └──────────────────────────────────────┘

  BucketCapacity remains constant (= maxKey parameter).
  Only KeyCapacity (entry slots) grows.


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌─────────────────────────┬─────────────────────────────────────┐
  │ Operation               │ Cost                                │
  ├─────────────────────────┼─────────────────────────────────────┤
  │ Add                     │ O(1) amortized (no hash compute)   │
  │ TryGetFirstValue        │ O(1) direct bucket index           │
  │ TryGetNextValue         │ O(k) where k = values per key      │
  │ Clear                   │ O(capacity + bucketCapacity)       │
  │ RecalculateBuckets      │ O(Length)                          │
  │ Memory overhead         │ Buckets[maxKey] — can be large     │
  └─────────────────────────┴─────────────────────────────────────┘

## Source

- [BovineLabs.Core/Collections/NativeKeyedMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/NativeKeyedMap.cs)
- [BovineLabs.Core.Tests/Collections/NativeKeyedMapTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/NativeKeyedMapTests.cs)
- [BovineLabs.Core/Spatial/SpatialKeyedMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Spatial/SpatialKeyedMap.cs)
