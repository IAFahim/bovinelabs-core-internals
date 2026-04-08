NativeUntypedHashMap — Inner Workings
=======================================

Stores mixed unmanaged types directly using offset calculations inside buckets.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────────────────┐
  │              NativeUntypedHashMap<TKey>                          │
  │                                                                  │
  │  ┌──────────────────────────────────────────────────────┐        │
  │  │ NativeUntypedHashMapHelper<TKey>* data               │        │
  │  │  (pointer to heap-allocated helper struct)           │        │
  │  └──────────────┬───────────────────────────────────────┘        │
  │                 │                                                │
  │                 ▼                                                │
  │  ┌────────────────────────────────────────────────────────────┐  │
  │  │          NativeUntypedHashMapHelper<TKey>                  │  │
  │  │                                                             │  │
  │  │  byte* Values   → int-sized slots (small) or offsets (big) │  │
  │  │  TKey*  Keys    → Typed key array                          │  │
  │  │  int*   Next    → Chain pointers (linked list per bucket)  │  │
  │  │  int*   Buckets → Bucket heads (hash → first entry)        │  │
  │  │  int*   Types   → BurstRuntime type hash per entry         │  │
  │  │  int*   Data    → Large value storage (variable offsets)   │  │
  │  │  byte*  Buffer  → Base pointer for entire allocation       │  │
  │  │                                                             │  │
  │  │  int Count, Capacity, DataCapacity                         │  │
  │  │  int BucketCapacityMask, Log2MinGrowth                     │  │
  │  │  int DataAllocatedIndex (bump allocator for Data)           │  │
  │  └────────────────────────────────────────────────────────────┘  │
  │                                                                  │
  │  [Safety: AtomicSafetyHandle m_Safety]                           │
  └──────────────────────────────────────────────────────────────────┘


WHAT "UNTYPED" MEANS
────────────────────

  Unlike NativeHashMap<TKey, TValue> where TValue is fixed at compile time,
  this map stores ANY unmanaged type per entry:

  ┌──────────────────────────────────────────────────────────────┐
  │  Standard HashMap:                                           │
  │  map<int, float> — all values must be float                  │
  │                                                              │
  │  Untyped HashMap:                                            │
  │  map.AddOrSet<int>(key1, 42);        ← stores int           │
  │  map.AddOrSet<float>(key2, 3.14f);   ← stores float         │
  │  map.AddOrSet<float3>(key3, pos);    ← stores float3         │
  │  map.AddOrSet<MyStruct>(key4, data); ← stores any unmanaged  │
  │                                                              │
  │  Each entry can be a DIFFERENT type!                         │
  └──────────────────────────────────────────────────────────────┘

  Type safety is maintained via BurstRuntime.GetHashCode32<TValue>()
  stored per entry. Mismatches throw at runtime.


SINGLE BUFFER MEMORY LAYOUT
────────────────────────────

  All data in ONE contiguous allocation, tightly packed:

  ┌──────────────────────────────────────────────────────────────────┐
  │                         Buffer (single alloc)                    │
  │                                                                  │
  │  Values           Keys          Next          Buckets            │
  │  int[capacity]    TKey[cap]     int[cap]      int[bucketCap]    │
  │ ┌───────┬─────┬──┬─────┬─────┬──┬─────┬─────┬──┬─────┬────────┐ │
  │ │int[0] │ ... │  │K[0] │ ... │  │N[0] │ ... │  │B[0] │  ...   │ │
  │ └───────┴─────┴──┴─────┴─────┴──┴─────┴─────┴──┴─────┴────────┘ │
  │                                                                  │
  │       Types            Data                                      │
  │       int[capacity]    int[dataCapacity]                         │
  │ ┌─────┬─────────────┬──────────────┬──────────────────────────┐ │
  │ │T[0] │    ...      │  Data[0]     │      ...                 │ │
  │ └─────┴─────────────┴──────────────┴──────────────────────────┘ │
  │                                                                  │
  │  Each section is aligned (potentially to 16 bytes for Data)      │
  └──────────────────────────────────────────────────────────────────┘

  Layout offsets computed by CalculateDataSize():
  keyOffset    = Align(valuesSize,            alignof(TKey))
  nextOffset   = Align(keyOffset + keysSize,  sizeof(int))
  bucketOffset = Align(nextOffset + nextSize,  sizeof(int))
  typeOffset   = Align(bucketOffset + bktSize, sizeof(int))
  dataOffset   = Align(typeOffset + typeSize,  16)  ← 16-byte align


TWO VALUE STORAGE STRATEGIES
────────────────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  sizeof(TValue) <= sizeof(int):  SMALL VALUE                │
  │                                                              │
  │  Values[idx] stores the value DIRECTLY:                     │
  │  ┌─────────────────────────────────────────────┐            │
  │  │ Values (int-sized slots)                     │            │
  │  │  ┌───────┬───────┬───────┬───────┐          │            │
  │  │  │ val[0]│ val[1]│ val[2]│ val[3]│          │            │
  │  │  │ (int) │ (int) │ (int) │ (int) │          │            │
  │  │  └───────┴───────┴───────┴───────┘          │            │
  │  │  reinterpret as TValue*: *(TValue*)(Values+idx*4)        │
  │  └─────────────────────────────────────────────┘            │
  │                                                              │
  │  Example: TValue = byte, short, int                         │
  └──────────────────────────────────────────────────────────────┘

  ┌──────────────────────────────────────────────────────────────┐
  │  sizeof(TValue) > sizeof(int):  LARGE VALUE                 │
  │                                                              │
  │  Values[idx] stores an OFFSET into Data array:              │
  │  ┌──────────────────┐      ┌──────────────────────────┐     │
  │  │ Values (int[])   │      │ Data (int[])             │     │
  │  │ ┌──────┐         │      │ ┌──────────────────────┐ │     │
  │  │ │  0   │─────────│─────►│ │  float3 data at [0]  │ │     │
  │  │ ├──────┤         │      │ ├──────────────────────┤ │     │
  │  │ │  3   │─────────│──┐   │ │  MyStruct at [3]     │ │     │
  │  │ ├──────┤         │  │   │ ├──────────────────────┤ │     │
  │  │ │ ... │         │  │   │ │ ...                  │ │     │
  │  │ └──────┘         │  │   │ └──────────────────────┘ │     │
  │  │                   │  │   │  DataAllocatedIndex →    │     │
  │  │  Offset values    │  │   │  bump allocator cursor   │     │
  │  │  (not real data!) │  │   └──────────────────────────┘     │
  │  └──────────────────┘  │                                     │
  │                        └─────────────────────────────────────│
  │  Data region acts as a bump allocator for large values.      │
  │  Each large value gets aligned space at DataAllocatedIndex,  │
  │  then the cursor advances by sizeof(TValue) / sizeof(int).  │
  └──────────────────────────────────────────────────────────────┘


TYPE TRACKING
─────────────

  Each entry stores a type hash for runtime type checking:

  ┌──────────────────────────────────────────────────────────────┐
  │  Types[capacity] = BurstRuntime.GetHashCode32<TValue>()      │
  │                                                              │
  │  ┌─────────────┬─────────────┬─────────────┬─────┐          │
  │  │ hash<int>   │ hash<float> │hash<float3> │ ... │          │
  │  │  0x5A5A...  │  0xB3B3... │  0x7C7C...  │     │          │
  │  └─────────────┴─────────────┴─────────────┴─────┘          │
  │                                                              │
  │  On read: CheckType<TValue>(idx)                             │
  │    expected = BurstRuntime.GetHashCode32<TValue>()            │
  │    actual   = Types[idx]                                     │
  │    if (expected != actual):                                  │
  │      throw "Type mismatch!"                                 │
  │                                                              │
  │  This prevents reading a float as an int, etc.               │
  └──────────────────────────────────────────────────────────────┘


ADD OR SET FLOW
───────────────

  AddOrSet<TValue>(key, value):
  ┌──────────────────────────────────────────────────────────────────┐
  │  1. Find existing entry:                                         │
  │     idx = Find(key)  → bucket chain lookup                      │
  │                                                                  │
  │  2. Is it a new key? (idx == -1)                                │
  │     YES → Add:                                                   │
  │     ┌─────────────────────────────────────────────────────┐      │
  │     │ if Count >= Capacity: Resize(grow)                  │      │
  │     │ idx = Count++                                       │      │
  │     │ Keys[idx] = key                                     │      │
  │     │ Types[idx] = BurstRuntime.GetHashCode32<TValue>()   │      │
  │     │                                                      │      │
  │     │ // Insert into bucket chain                          │      │
  │     │ bucket = GetHashCode(key) & BucketCapacityMask       │      │
  │     │ Next[idx] = Buckets[bucket]                          │      │
  │     │ Buckets[bucket] = idx                                │      │
  │     └─────────────────────────────────────────────────────┘      │
  │                                                                  │
  │     NO → Set (key already exists):                               │
  │     ┌─────────────────────────────────────────────────────┐      │
  │     │ CheckType<TValue>(idx)  // verify same type!        │      │
  │     └─────────────────────────────────────────────────────┘      │
  │                                                                  │
  │  3. Store value:                                                 │
  │     ┌──────────────────────────────────────────────────────────┐ │
  │     │ if sizeof(TValue) <= sizeof(int):  // SMALL            │ │
  │     │   *(TValue*)(Values + idx * sizeof(int)) = value        │ │
  │     │                                                          │ │
  │     │ if sizeof(TValue) > sizeof(int):   // LARGE            │ │
  │     │   if adding (new):                                       │ │
  │     │     DataAllocatedIndex = Align<TValue>(DataAllocIndex)  │ │
  │     │     if DataAllocIndex + size > DataCapacity:             │ │
  │     │       ResizeData(grow)                                   │ │
  │     │     Values[idx] = DataAllocatedIndex  (store offset)    │ │
  │     │     DataAllocIndex += sizeof(TValue) / sizeof(int)      │ │
  │     │   if setting (existing):                                 │ │
  │     │     offset = Values[idx]  (read stored offset)          │ │
  │     │   MemCpy(Data + offset, &value, sizeof(TValue))         │ │
  │     └──────────────────────────────────────────────────────────┘ │
  └──────────────────────────────────────────────────────────────────┘


FIND OPERATION
──────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  Find(key):                                                  │
  │                                                              │
  │  1. bucket = key.GetHashCode() & BucketCapacityMask          │
  │  2. entryIdx = Buckets[bucket]                               │
  │                                                              │
  │  3. while (Keys[entryIdx] != key):                           │
  │       entryIdx = Next[entryIdx]                              │
  │       if (entryIdx >= Capacity): return -1                   │
  │                                                              │
  │  4. return entryIdx                                          │
  │                                                              │
  │  Standard chained hash table traversal.                      │
  │  Buckets initialized to 0xFF (-1) = empty.                  │
  └──────────────────────────────────────────────────────────────┘


DATA BUMP ALLOCATOR
───────────────────

  Large values use a bump allocator in the Data region:

  ┌────────────────────────────────────────────────────────────┐
  │  Data region:                                              │
  │  ┌────────────┬────────────┬────────────┬────────────────┐ │
  │  │ float3     │ MyStruct   │ Matrix4x4  │  ... free ...  │ │
  │  │ (3 ints)   │ (8 ints)   │ (16 ints)  │                │ │
  │  └────────────┴────────────┴────────────┴────────────────┘ │
  │  ▲                          ▲              ▲                │
  │  offset=0              offset=11    DataAllocatedIndex      │
  │                                        = 27                 │
  │                                                            │
  │  Allocation:                                               │
  │  1. Align DataAllocatedIndex to alignof(TValue)            │
  │  2. Check if enough room: DataAllocIndex + size <= Cap     │
  │  3. If not: ResizeData (grow the Data region)              │
  │  4. Store value at Data + DataAllocatedIndex               │
  │  5. Advance DataAllocatedIndex                             │
  │                                                            │
  │  Note: Never frees individual entries. Only clears all.    │
  └────────────────────────────────────────────────────────────┘


RESIZE OPERATIONS
─────────────────

  Two resize paths:

  ┌──────────────────────────────────────────────────────────────┐
  │  Resize(newCapacity):                                        │
  │  └─ Grows Keys, Values, Next, Buckets, Types                │
  │     Re-hashes all bucket chains (must recompute buckets)    │
  │     Data region preserved (just copied)                     │
  │                                                              │
  │  ResizeData(newDataCapacity):                                │
  │  └─ Grows only the Data region                              │
  │     Buckets/Next unchanged (no rehash needed)               │
  │     Values offsets still valid                               │
  │                                                              │
  │  Growth strategy: CeilPow2 with minimum growth of           │
  │  2^Log2MinGrowth (default: 256 entries per growth step)     │
  └──────────────────────────────────────────────────────────────┘


API OVERVIEW
────────────

  ┌────────────────────────────────────────────────────────────────┐
  │ AddOrSet<TValue>(key, value)  Add new or update existing      │
  │ GetOrAddRef<TValue>(key, def) Get ref to value, add if new    │
  │ TryGetValue<TValue>(key, out) Get value with type check       │
  │ ContainsKey(key)              Check if key exists              │
  │ Clear()                       Reset all (no capacity change)  │
  │ Capacity { get/set }          Resize (grow only)               │
  └────────────────────────────────────────────────────────────────┘


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌──────────────────────────┬──────────────────────────────────────┐
  │ Operation                │ Cost                                 │
  ├──────────────────────────┼──────────────────────────────────────┤
  │ AddOrSet (small value)   │ O(1) amortized + hash + bucket walk │
  │ AddOrSet (large value)   │ O(1) amortized + Data bump alloc    │
  │ TryGetValue              │ O(1) avg + type check                │
  │ ContainsKey              │ O(1) avg — bucket chain lookup       │
  │ Clear                    │ O(Capacity + BucketCapacity) memset  │
  │ Resize                   │ O(Capacity) copy + rehash            │
  │ Memory overhead          │ Types[] + Data[] for large values    │
  │ Type safety              │ Runtime hash check per access        │
  └──────────────────────────┴──────────────────────────────────────┘

## Source

- [BovineLabs.Core/Collections/NativeUntypedHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/NativeUntypedHashMap.cs)
