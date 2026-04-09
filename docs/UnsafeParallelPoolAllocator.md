     1|UnsafeParallelPoolAllocator - Thread-Local Pool Array
     2|======================================================
     3|
     4|Source: BovineLabs.Core/Memory/UnsafeParallelPoolAllocator.cs
     5|
     6|OVERVIEW
     7|--------
     8|UnsafeParallelPoolAllocator<T> eliminates lock contention by giving each Unity
     9|job worker thread its own UnsafePoolAllocator<T>. Allocation and free operations
    10|index into a contiguous array of pools using [NativeSetThreadIndex], ensuring
    11|each thread only accesses its own pool. No locks, no atomics on the hot path.
    12|
    13|ARCHITECTURE - THREAD-LOCAL POOL ARRAY
    14|=======================================
    15|
    16|    UnsafeParallelPoolAllocator<T> Instance
    17|    ┌─────────────────────────────────────────────────────┐
    18|    │                                                     │
    19|    │  allocator: Allocator  (backing allocator)          │
    20|    │                                                     │
    21|    │  pools: UnsafePoolAllocator<T>* ──────────┐         │
    22|    │       (contiguous array of N pools)       │         │
    23|    │                                            │         │
    24|    │  threadIndex: int  [NativeSetThreadIndex]  │         │
    25|    │       (auto-set by Unity job system)       │         │
    26|    └─────────────────────────────────────────────┼───────┘
    27|                                                  │
    28|                                                  ▼
    29|    pools array (JobsUtility.ThreadIndexCount entries)
    30|    ┌──────────────────────────────────────────────────────────────┐
    31|    │                                                              │
    32|    │  [0] UnsafePoolAllocator<T>    ◄── Thread 0 uses this       │
    33|    │  ┌───────────────────────────┐                               │
    34|    │  │ slabAllocator + free set  │                               │
    35|    │  └───────────────────────────┘                               │
    36|    │                                                              │
    37|    │  [1] UnsafePoolAllocator<T>    ◄── Thread 1 uses this       │
    38|    │  ┌───────────────────────────┐                               │
    39|    │  │ slabAllocator + free set  │                               │
    40|    │  └───────────────────────────┘                               │
    41|    │                                                              │
    42|    │  [2] UnsafePoolAllocator<T>    ◄── Thread 2 uses this       │
    43|    │  ┌───────────────────────────┐                               │
    44|    │  │ slabAllocator + free set  │                               │
    45|    │  └───────────────────────────┘                               │
    46|    │                                                              │
    47|    │  ...                                                         │
    48|    │                                                              │
    49|    │  [N-1] UnsafePoolAllocator<T> ◄── Thread N-1 uses this      │
    50|    │  ┌───────────────────────────┐                               │
    51|    │  │ slabAllocator + free set  │                               │
    52|    │  └───────────────────────────┘                               │
    53|    │                                                              │
    54|    └──────────────────────────────────────────────────────────────┘
    55|
    56|    where N = JobsUtility.ThreadIndexCount (e.g. 128 on typical platforms)
    57|
    58|
    59|ALLOCATION FLOW
    60|===============
    61|
    62|    T* Alloc():
    63|    ┌──────────────────────────────────────────────────────┐
    64|    │                                                      │
    65|    │   threadIndex is auto-set by Unity job system        │
    66|    │   e.g. threadIndex = 3                               │
    67|    │                                                      │
    68|    │   return pools[threadIndex].Alloc()                  │
    69|    │                  │                                   │
    70|    │                  ▼                                   │
    71|    │   ┌─────────────────────────────────────┐            │
    72|    │   │ pools[3].Alloc()                    │            │
    73|    │   │                                     │            │
    74|    │   │  (delegates to UnsafePoolAllocator) │            │
    75|    │   │  1. Check free set for recycled ptr │            │
    76|    │   │  2. If none, slab-allocate new      │            │
    77|    │   └─────────────────────────────────────┘            │
    78|    │                                                      │
    79|    └──────────────────────────────────────────────────────┘
    80|
    81|
    82|    Free(T* p):
    83|    ┌──────────────────────────────────────────────────────┐
    84|    │                                                      │
    85|    │   pools[threadIndex].Free(p)                         │
    86|    │                                                      │
    87|    │   Thread 0: pools[0].Free(ptr_A)                     │
    88|    │   Thread 3: pools[3].Free(ptr_B)                     │
    89|    │   Thread 7: pools[7].Free(ptr_C)                     │
    90|    │                                                      │
    91|    │   No contention - each thread has its own pool!      │
    92|    │                                                      │
    93|    └──────────────────────────────────────────────────────┘
    94|
    95|
    96|THREAD ISOLATION DIAGRAM
    97|=========================
    98|
    99|    Timeline showing 4 threads allocating simultaneously:
   100|
   101|    Time ──────────────────────────────────────────────────►
   102|
   103|    Thread 0:  Alloc()──►pools[0]  ────Free()──►pools[0]
   104|                                  │                    │
   105|    Thread 1:  Alloc()──►pools[1]  │                    │
   106|                                  │  No locks needed!  │
   107|    Thread 2:  Alloc()──►pools[2]  │                    │
   108|                                  │                    │
   109|    Thread 3:  Alloc()──►pools[3]  Alloc()──►pools[3]   │
   110|                                                       │
   111|    Each arrow goes to a DIFFERENT pool.                │
   112|    Zero synchronization between threads.               │
   113|
   114|
   115|MEMORY LAYOUT (each pool's internals)
   116|======================================
   117|
   118|    Each pools[i] is a full UnsafePoolAllocator<T>:
   119|
   120|    ┌──────────────────────────────────────────────┐
   121|    │  UnsafePoolAllocator<T>                      │
   122|    │                                              │
   123|    │  ┌────────────────────────────────────────┐  │
   124|    │  │ slabAllocator: UnsafeSlabAllocator<T>  │  │
   125|    │  │                                        │  │
   126|    │  │  Slabs: [Ptr0][Ptr1][Ptr2]...          │  │
   127|    │  │         │     │     │                  │  │
   128|    │  │         ▼     ▼     ▼                  │  │
   129|    │  │       ┌───┐┌───┐┌───┐                 │  │
   130|    │  │       │   ││   ││   │ slab memory      │  │
   131|    │  │       │ T ││ T ││ T │ blocks           │  │
   132|    │  │       │   ││   ││   │                  │  │
   133|    │  │       └───┘└───┘└───┘                 │  │
   134|    │  └────────────────────────────────────────┘  │
   135|    │                                              │
   136|    │  ┌────────────────────────────────────────┐  │
   137|    │  │ free: UnsafeParallelHashSet<Ptr>       │  │
   138|    │  │  ┌───┬───┬───┐                         │  │
   139|    │  │  │ptr│ptr│ptr│  recycled pointers       │  │
   140|    │  │  └───┴───┴───┘                         │  │
   141|    │  └────────────────────────────────────────┘  │
   142|    └──────────────────────────────────────────────┘
   143|
   144|
   145|CONSTRUCTION
   146|=============
   147|
   148|    new UnsafeParallelPoolAllocator(countPerChunk, allocator):
   149|
   150|    ┌────────────────────────────────────────────────────────┐
   151|    │ 1. Allocate raw memory for all pools:                  │
   152|    │    pools = malloc(                                     │
   153|    │      sizeof(UnsafePoolAllocator<T>)                    │
   154|    │      × ThreadIndexCount)                               │
   155|    │                                                        │
   156|    │ 2. Initialize each pool:                               │
   157|    │    for i in 0..ThreadIndexCount:                       │
   158|    │      pools[i] = new UnsafePoolAllocator<T>(            │
   159|    │                    countPerChunk, allocator)            │
   160|    │                                                        │
   161|    │ 3. threadIndex = 0 (default, overridden at job time)   │
   162|    └────────────────────────────────────────────────────────┘
   163|
   164|
   165|ALLOCATED() AGGREGATION
   166|========================
   167|
   168|    Allocated():
   169|    ┌────────────────────────────────────────────────────────┐
   170|    │ int total = 0;                                        │
   171|    │ for i in 0..ThreadIndexCount:                         │
   172|    │   total += pools[i].Allocated()                       │
   173|    │        │                                              │
   174|    │        └──► slabAllocator.Allocated()                 │
   175|    │             = slabs.Length × countPerChunk × sizeof(T)│
   176|    │                                                       │
   177|    │ return total;                                         │
   178|    └────────────────────────────────────────────────────────┘
   179|
   180|
   181|DISPOSE
   182|=======
   183|
   184|    Dispose():
   185|    ┌────────────────────────────────────────────────────────┐
   186|    │ for i in 0..ThreadIndexCount:                         │
   187|    │   pools[i].Dispose()                                  │
   188|    │     └── slabAllocator.Dispose() + free set dispose    │
   189|    │                                                       │
   190|    │ Unmanaged.Free(pools)   ← free the array itself       │
   191|    │ pools = null                                         │
   192|    └────────────────────────────────────────────────────────┘
   193|
   194|
   195|KEY PROPERTIES
   196|==============
   197|
   198|  * Type:              struct UnsafeParallelPoolAllocator<T> : IDisposable
   199|  * Thread Safety:     YES via thread-isolated pools (no locks!)
   200|  * Pool Count:        JobsUtility.ThreadIndexCount (platform-dependent)
   201|  * Thread Routing:    [NativeSetThreadIndex] auto-populated
   202|  * Per-Thread Pool:   Each is a full UnsafePoolAllocator<T>
   203|  * Alloc Speed:       O(1) per thread, zero contention
   204|  * Free Speed:        O(1) per thread, zero contention
   205|  * Burst Compatible:  Yes
   206|  * Memory Overhead:   N × UnsafePoolAllocator<T> (one per worker thread)
   207|  * Constraint:        Free must be on same thread that allocated
   208|
   209|## Verified Data

## Verified Data

```
BovineLabs.Core.Memory.UnsafeParallelPoolAllocator<T>
  Kind: struct (ValueType=True)
  Size (T=int): 24 bytes
  Generic params: Int32
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
    private Allocator allocator 
    private UnsafePoolAllocator`1* pools [NativeDisableUnsafePtrRestrictionAttribute]
    private Int32 threadIndex [NativeSetThreadIndexAttribute]
Verified: 9 checks, 0 failures
```

