     1|UnsafePoolAllocator - Slab + Free-List Hybrid Pool
     2|====================================================
     3|
     4|Source: BovineLabs.Core/Memory/UnsafePoolAllocator.cs
     5|
     6|OVERVIEW
     7|--------
     8|UnsafePoolAllocator<T> combines two strategies: slab allocation for growing
     9|capacity and a free-list (hash set) for recycling returned pointers. Items are
    10|allocated from slabs in contiguous chunks. When freed, pointers are added to
    11|a hash set. On next allocation, freed pointers are recycled first, falling back
    12|to slab allocation only when the free list is empty.
    13|
    14|ARCHITECTURE
    15|=============
    16|
    17|    UnsafePoolAllocator<T> Instance
    18|    ┌─────────────────────────────────────────────────────────┐
    19|    │                                                         │
    20|    │  slabAllocator: UnsafeSlabAllocator<T>                  │
    21|    │    (grows capacity in fixed-size chunks)                │
    22|    │    ┌───────────────────────────────────────────┐        │
    23|    │    │ slabs: [Slab0][Slab1][Slab2]...           │        │
    24|    │    │ count: bump counter in current slab        │        │
    25|    │    │ countPerSlab: items per slab               │        │
    26|    │    └───────────────────────────────────────────┘        │
    27|    │                                                         │
    28|    │  free: UnsafeParallelHashSet<Ptr>                       │
    29|    │    (recycled pointers waiting for reuse)                │
    30|    │    ┌───────────────────────────────────┐                │
    31|    │    │ { ptr_A, ptr_B, ptr_C, ... }      │                │
    32|    │    └───────────────────────────────────┘                │
    33|    │                                                         │
    34|    └─────────────────────────────────────────────────────────┘
    35|
    36|
    37|MEMORY LAYOUT - SLAB ALLOCATION + FREE RECYCLING
    38|=================================================
    39|
    40|    Slab Allocator provides raw memory:
    41|
    42|    Slab 0 (countPerSlab = 8)
    43|    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    44|    │ T0 │ T1 │ T2 │ T3 │ T4 │ T5 │ T6 │ T7 │
    45|    └────┴────┴────┴────┴────┴────┴────┴────┘
    46|
    47|    Slab 1 (countPerSlab = 8)
    48|    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    49|    │ T8 │ T9 │T10 │T11 │T12 │T13 │T14 │T15 │
    50|    └────┴────┴────┴────┴────┴────┴────┴────┘
    51|
    52|    Slab 2 (partially used - bump counter at 3)
    53|    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    54|    │T16 │T17 │T18 │ ?? │ ?? │ ?? │ ?? │ ?? │
    55|    └────┴────┴────┴────┴────┴────┴────┴────┘
    56|                      ▲
    57|                      └── next slab alloc goes here
    58|
    59|    Free List (recycled pointers):
    60|    ┌──────────────────────────────────────┐
    61|    │  free hash set:                      │
    62|    │  ┌───────┬───────┬───────┬─────┐    │
    63|    │  │ &T[3] │ &T[7] │ &T[9] │ ... │    │
    64|    │  └───────┴───────┴───────┴─────┘    │
    65|    │                                      │
    66|    │  These slots were freed and can      │
    67|    │  be reused before new slab allocs    │
    68|    └──────────────────────────────────────┘
    69|
    70|
    71|ALLOCATION ALGORITHM
    72|====================
    73|
    74|    T* Alloc():
    75|
    76|    ┌─────────────────────────────────────────┐
    77|    │  Try to get recycled pointer:           │
    78|    │  e = free.GetEnumerator()               │
    79|    │  if e.MoveNext():                       │
    80|    └──────────────┬──────────────────────────┘
    81|                   │
    82|           ┌───────┴───────┐
    83|           │               │
    84|          HAS FREE        EMPTY
    85|           │               │
    86|           ▼               ▼
    87|    ┌──────────────┐  ┌──────────────────────────┐
    88|    │ ptr = e.Current│ │ Delegate to slab:        │
    89|    │ free.Remove()│  │ return slabAllocator     │
    90|    │ return ptr   │  │         .Alloc()         │
    91|    │              │  │                          │
    92|    │ RECYCLED!    │  │ NEW ALLOCATION!          │
    93|    └──────────────┘  └──────────────────────────┘
    94|
    95|    Slab .Alloc() bumps pointer:
    96|    ┌─────────────────────────────────────────────┐
    97|    │ if *count == countPerSlab:                  │
    98|    │   allocate new slab block                   │
    99|    │   *count = 0                                │
   100|    │ ptr = slab[last][*count]                    │
   101|    │ (*count)++                                  │
   102|    │ return ptr                                  │
   103|    └─────────────────────────────────────────────┘
   104|
   105|
   106|FREE ALGORITHM
   107|==============
   108|
   109|    Free(T* p):
   110|
   111|    ┌───────────────────────────────────────────┐
   112|    │  free.Add(new Ptr(p))                     │
   113|    │                                           │
   114|    │  Pointer goes into the free hash set.     │
   115|    │  Memory is NOT zeroed or returned to OS.  │
   116|    │  Next Alloc() may recycle this pointer.   │
   117|    └───────────────────────────────────────────┘
   118|
   119|    Example lifecycle:
   120|
   121|    1. ptr = Alloc()  →  slab allocates &T[5]
   122|       free = {}  (empty)
   123|
   124|    2. ptr = Alloc()  →  slab allocates &T[6]
   125|       free = {}  (empty)
   126|
   127|    3. Free(ptr_to_T5)  →  free.Add(&T[5])
   128|       free = { &T[5] }
   129|
   130|    4. ptr = Alloc()  →  recycled from free set!
   131|       free = {}  (empty again)
   132|       returns &T[5]  (same memory reused)
   133|
   134|
   135|FULL LIFECYCLE EXAMPLE
   136|======================
   137|
   138|    Time ──►
   139|
   140|    Step 1: Initial state (empty)
   141|    ┌───────────────────┐
   142|    │ slabs: []         │
   143|    │ free: {}          │
   144|    └───────────────────┘
   145|
   146|    Step 2: Alloc() × 4  →  new slab created, 4 items bumped
   147|    ┌───────────────────────────────────┐
   148|    │ slabs: [Slab0]                   │
   149|    │ Slab0: [T0][T1][T2][T3][ ][ ][ ][ ]│
   150|    │ free: {}                          │
   151|    └───────────────────────────────────┘
   152|
   153|    Step 3: Alloc() × 5  →  slab fills, new slab allocated
   154|    ┌──────────────────────────────────────────────────┐
   155|    │ slabs: [Slab0][Slab1]                            │
   156|    │ Slab0: [T0][T1][T2][T3][T4][T5][T6][T7]  FULL   │
   157|    │ Slab1: [T8][ ][ ][ ][ ][ ][ ][ ]  count=1       │
   158|    │ free: {}                                         │
   159|    └──────────────────────────────────────────────────┘
   160|
   161|    Step 4: Free(&T3), Free(&T7), Free(&T8)
   162|    ┌──────────────────────────────────────────────────┐
   163|    │ slabs: [Slab0][Slab1]                            │
   164|    │ Slab0: [T0][T1][T2][##][T4][T5][T6][##]         │
   165|    │ Slab1: [##][ ][ ][ ][ ][ ][ ][ ]                │
   166|    │ free: { &T3, &T7, &T8 }                         │
   167|    └──────────────────────────────────────────────────┘
   168|
   169|    Step 5: Alloc()  →  RECYCLES from free (e.g. &T8)
   170|    ┌──────────────────────────────────────────────────┐
   171|    │ free: { &T3, &T7 }  (removed &T8)               │
   172|    │ Slab1: [T8][ ][ ][ ][ ][ ][ ][ ]  (reused!)     │
   173|    └──────────────────────────────────────────────────┘
   174|
   175|
   176|ALLOCATED() METRIC
   177|===================
   178|
   179|    Allocated():
   180|    ┌────────────────────────────────────────────────────┐
   181|    │ return slabAllocator.Allocated()                   │
   182|    │                                                    │
   183|    │ = slabs.Length × countPerSlab × sizeof(T)         │
   184|    │                                                    │
   185|    │ NOTE: This is TOTAL capacity, not items-in-use.    │
   186|    │ Does not subtract free-list entries.               │
   187|    └────────────────────────────────────────────────────┘
   188|
   189|
   190|DISPOSE
   191|=======
   192|
   193|    Dispose():
   194|    ┌─────────────────────────────────────────┐
   195|    │ slabAllocator.Dispose()                 │
   196|    │   └── Frees all slab blocks             │
   197|    │   └── Destroys slab list                │
   198|    │   └── Frees count pointer               │
   199|    │                                         │
   200|    │ free.Dispose()                          │
   201|    │   └── Frees the hash set                │
   202|    │                                         │
   203|    │ slabAllocator = default                 │
   204|    │ free = default                          │
   205|    └─────────────────────────────────────────┘
   206|
   207|
   208|KEY PROPERTIES
   209|==============
   210|
   211|  * Type:              struct UnsafePoolAllocator<T> : IDisposable
   212|  * Thread Safety:     NONE (caller must synchronize)
   213|  * Growth Strategy:   Slab-based (countPerChunk per slab)
   214|  * Recycling:         UnsafeParallelHashSet<Ptr> free-list
   215|  * Alloc:             O(1) - free-list hit or slab bump
   216|  * Free:              O(1) - hashset add
   217|  * Memory:            Never shrinks (slabs persist until Dispose)
   218|  * Burst Compatible:  Yes
   219|  * Fragmentation:     None within slabs; freed slots are reused
   220|  * Use Case:          General purpose pool with individual free support
   221|
   222|## Verified Data

## Verified Data

```
BovineLabs.Core.Memory.UnsafePoolAllocator<T>
  Kind: struct (ValueType=True)
  Size (T=int): 40 bytes
  Interfaces:
    System.IDisposable
  Constructors:
    .ctor(Int32 countPerChunk, Allocator allocator)
  Properties:
    public Boolean IsCreated
  Methods:
    public Void Dispose()
    public Int32* Alloc()
    public Void Free(Int32* p)
    public Int32 Allocated()
  Fields:
    private UnsafeSlabAllocator`1 slabAllocator
    private UnsafeParallelHashSet`1 free
Verified: 8 checks, 0 failures
```

