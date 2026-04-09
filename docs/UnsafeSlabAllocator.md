     1|UnsafeSlabAllocator - Block-Based Chunk Allocation
     2|===================================================
     3|
     4|Source: BovineLabs.Core/Memory/UnsafeSlabAllocator.cs
     5|
     6|OVERVIEW
     7|--------
     8|UnsafeSlabAllocator<T> prevents memory fragmentation by allocating fixed-size
     9|slabs (chunks) and bump-allocating within each slab. Items are never freed
    10|individually - only bulk deallocation via Clear() or Dispose(). This trades
    11|granular deallocation for zero fragmentation and O(1) allocation speed.
    12|
    13|MEMORY LAYOUT
    14|=============
    15|
    16|     UnsafeSlabAllocator<T> Instance
    17|    ┌──────────────────────────────────────────┐
    18|    │  countPerSlab: int    (e.g. 64)          │
    19|    │  allocator:    AllocatorHandle           │
    20|    │  slabs:        UnsafeList<Ptr>*          │──────┐
    21|    │  count:        int*  (current slab idx)  │──┐   │
    22|    └──────────────────────────────────────────┘  │   │
    23|                                                  │   │
    24|         slabs list ( UnsafeList<Ptr> )           │   │
    25|    ┌────────────────────────────────┐            │   │
    26|    │  [0] Ptr ────────────────────────────► Slab 0   │
    27|    │  [1] Ptr ────────────────────────────► Slab 1   │
    28|    │  [2] Ptr ────────────────────────────► Slab 2   │
    29|    └────────────────────────────────┘       │       │
    30|                                             │       │
    31|    *count (bump counter)                    │       │
    32|    ┌─────────┐                              │       │
    33|    │  n      │ (n < countPerSlab)           │       │
    34|    └─────────┘                              │       │
    35|                                             ▼       ▼
    36|    Each Slab = countPerSlab * sizeof(T) bytes, aligned to alignof(T)
    37|
    38|    SLAB MEMORY (example: countPerSlab=8, T=int)
    39|
    40|    Slab 0 (FULL - count was 8, now bumping in slab 1)
    41|    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    42|    │ T0 │ T1 │ T2 │ T3 │ T4 │ T5 │ T6 │ T7 │
    43|    └────┴────┴────┴────┴────┴────┴────┴────┘
    44|
    45|    Slab 1 (ACTIVE - *count = 5, next alloc at index 5)
    46|    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    47|    │ T0 │ T1 │ T2 │ T3 │ T4 │ ?? │ ?? │ ?? │
    48|    └────┴────┴────┴────┴────┴────┴────┴────┘
    49|     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   ^^^^^^^^
    50|     Allocated (in use)              Free (unused)
    51|                 *count = 5 ──────────^
    52|
    53|
    54|ALLOCATION ALGORITHM
    55|====================
    56|
    57|    Alloc() Flow:
    58|
    59|    ┌─────────────────────────────────┐
    60|    │  *count == countPerSlab ?       │
    61|    │  (Is current slab full?)        │
    62|    └──────────┬──────────────────────┘
    63|               │
    64|       ┌───────┴───────┐
    65|       │               │
    66|      YES              NO
    67|       │               │
    68|       ▼               ▼
    69|    ┌──────────────┐  ┌──────────────────────────────┐
    70|    │ *count = 0   │  │ ptr = slabs[last][*count]     │
    71|    │ allocate new │  │ *count++                      │
    72|    │ slab via     │  │ return ptr                    │
    73|    │ Unmanaged    │  └──────────────────────────────┘
    74|    │ .Allocate()  │
    75|    │ slabs.Add()  │
    76|    └──────┬───────┘
    77|           │
    78|           ▼
    79|    ┌──────────────────────────────┐
    80|    │ ptr = slabs[last][0]         │
    81|    │ *count++  (= 1)              │
    82|    │ return ptr                   │
    83|    └──────────────────────────────┘
    84|
    85|
    86|CLEAR / DISPOSE
    87|===============
    88|
    89|    Clear():                         Dispose():
    90|    ┌───────────────────────┐       ┌───────────────────────┐
    91|    │ for each slab:        │       │ Clear()               │
    92|    │   Unmanaged.Free(slab)│──────►│ Destroy slabs list    │
    93|    │ slabs.Clear()         │       │ Free count pointer    │
    94|    │ *count = countPerSlab │       │ null out fields       │
    95|    └───────────────────────┘       └───────────────────────┘
    96|
    97|    (Resets to "no slabs" state -    (Complete teardown,
    98|     next Alloc() triggers new        cannot be reused)
    99|     slab allocation)
   100|
   101|
   102|ALLOCATION COUNT TRACKING
   103|=========================
   104|
   105|    AllocationCount = (countPerSlab × (slabs.Length - 1)) + *count
   106|
   107|    Example: countPerSlab=8, 3 slabs, *count=5
   108|    ┌──────────────────────────────────────────────────┐
   109|    │ (8 × (3 - 1)) + 5 = 16 + 5 = 21 items total    │
   110|    └──────────────────────────────────────────────────┘
   111|
   112|    Full slabs: 2 × 8 = 16     Active slab: 5
   113|    ┌───┬───┬───┬───┬───┬───┬───┬───┐  ┌───┬───┬───┬───┬───┬───┬───┬───┐
   114|    │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │  │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │
   115|    └───┴───┴───┴───┴───┴───┴───┴───┘  └───┴───┴───┴───┴───┴───┴───┴───┘
   116|      Slab 0: 8/8 (full)                Slab 1: 5/8 (active)
   117|                                         ^count = 5
   118|
   119|
   120|KEY PROPERTIES
   121|==============
   122|
   123|  * Generic:           T : unmanaged
   124|  * Thread Safety:     NONE (caller must synchronize)
   125|  * Fragmentation:     ZERO (slabs are contiguous blocks)
   126|  * Alloc Speed:       O(1) pointer bump
   127|  * Free Speed:        N/A (no individual free)
   128|  * Clear Speed:       O(n) slabs to free
   129|  * Allocated():       slabs.Length × countPerSlab × sizeof(T)
   130|  * Safety:            No bounds checking, no AtomicSafetyHandle
   131|  * Burst Compatible:  Yes (uses unsafe pointers, NativeDisableUnsafePtrRestriction)
   132|
   133|## Verified Data

## Verified Data

```
BovineLabs.Core.Memory.UnsafeSlabAllocator<T>
  Kind: struct (ValueType=True)
  Size (T=int): 24 bytes
  Interfaces:
    System.IDisposable
  Constructors:
    .ctor(Int32 countPerSlab, AllocatorHandle allocator)
  Properties:
    public Int32 AllocationCount
    public Boolean IsCreated
  Methods:
    public Int32* Alloc()
    public Void Clear()
    public Void Dispose()
    public Int32 Allocated()
  Fields:
    private Int32 countPerSlab
    private AllocatorHandle allocator
    private UnsafeList`1* slabs
    private Int32* count
Verified: 10 checks, 0 failures
```

