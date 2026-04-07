UnsafeParallelPoolAllocator - Thread-Local Pool Array
======================================================

Source: BovineLabs.Core/Memory/UnsafeParallelPoolAllocator.cs

OVERVIEW
--------
UnsafeParallelPoolAllocator<T> eliminates lock contention by giving each Unity
job worker thread its own UnsafePoolAllocator<T>. Allocation and free operations
index into a contiguous array of pools using [NativeSetThreadIndex], ensuring
each thread only accesses its own pool. No locks, no atomics on the hot path.

ARCHITECTURE - THREAD-LOCAL POOL ARRAY
=======================================

    UnsafeParallelPoolAllocator<T> Instance
    ┌─────────────────────────────────────────────────────┐
    │                                                     │
    │  allocator: Allocator  (backing allocator)          │
    │                                                     │
    │  pools: UnsafePoolAllocator<T>* ──────────┐         │
    │       (contiguous array of N pools)       │         │
    │                                            │         │
    │  threadIndex: int  [NativeSetThreadIndex]  │         │
    │       (auto-set by Unity job system)       │         │
    └─────────────────────────────────────────────┼───────┘
                                                  │
                                                  ▼
    pools array (JobsUtility.ThreadIndexCount entries)
    ┌──────────────────────────────────────────────────────────────┐
    │                                                              │
    │  [0] UnsafePoolAllocator<T>    ◄── Thread 0 uses this       │
    │  ┌───────────────────────────┐                               │
    │  │ slabAllocator + free set  │                               │
    │  └───────────────────────────┘                               │
    │                                                              │
    │  [1] UnsafePoolAllocator<T>    ◄── Thread 1 uses this       │
    │  ┌───────────────────────────┐                               │
    │  │ slabAllocator + free set  │                               │
    │  └───────────────────────────┘                               │
    │                                                              │
    │  [2] UnsafePoolAllocator<T>    ◄── Thread 2 uses this       │
    │  ┌───────────────────────────┐                               │
    │  │ slabAllocator + free set  │                               │
    │  └───────────────────────────┘                               │
    │                                                              │
    │  ...                                                         │
    │                                                              │
    │  [N-1] UnsafePoolAllocator<T> ◄── Thread N-1 uses this      │
    │  ┌───────────────────────────┐                               │
    │  │ slabAllocator + free set  │                               │
    │  └───────────────────────────┘                               │
    │                                                              │
    └──────────────────────────────────────────────────────────────┘

    where N = JobsUtility.ThreadIndexCount (e.g. 128 on typical platforms)


ALLOCATION FLOW
===============

    T* Alloc():
    ┌──────────────────────────────────────────────────────┐
    │                                                      │
    │   threadIndex is auto-set by Unity job system        │
    │   e.g. threadIndex = 3                               │
    │                                                      │
    │   return pools[threadIndex].Alloc()                  │
    │                  │                                   │
    │                  ▼                                   │
    │   ┌─────────────────────────────────────┐            │
    │   │ pools[3].Alloc()                    │            │
    │   │                                     │            │
    │   │  (delegates to UnsafePoolAllocator) │            │
    │   │  1. Check free set for recycled ptr │            │
    │   │  2. If none, slab-allocate new      │            │
    │   └─────────────────────────────────────┘            │
    │                                                      │
    └──────────────────────────────────────────────────────┘


    Free(T* p):
    ┌──────────────────────────────────────────────────────┐
    │                                                      │
    │   pools[threadIndex].Free(p)                         │
    │                                                      │
    │   Thread 0: pools[0].Free(ptr_A)                     │
    │   Thread 3: pools[3].Free(ptr_B)                     │
    │   Thread 7: pools[7].Free(ptr_C)                     │
    │                                                      │
    │   No contention - each thread has its own pool!      │
    │                                                      │
    └──────────────────────────────────────────────────────┘


THREAD ISOLATION DIAGRAM
=========================

    Timeline showing 4 threads allocating simultaneously:

    Time ──────────────────────────────────────────────────►

    Thread 0:  Alloc()──►pools[0]  ────Free()──►pools[0]
                                  │                    │
    Thread 1:  Alloc()──►pools[1]  │                    │
                                  │  No locks needed!  │
    Thread 2:  Alloc()──►pools[2]  │                    │
                                  │                    │
    Thread 3:  Alloc()──►pools[3]  Alloc()──►pools[3]   │
                                                       │
    Each arrow goes to a DIFFERENT pool.                │
    Zero synchronization between threads.               │


MEMORY LAYOUT (each pool's internals)
======================================

    Each pools[i] is a full UnsafePoolAllocator<T>:

    ┌──────────────────────────────────────────────┐
    │  UnsafePoolAllocator<T>                      │
    │                                              │
    │  ┌────────────────────────────────────────┐  │
    │  │ slabAllocator: UnsafeSlabAllocator<T>  │  │
    │  │                                        │  │
    │  │  Slabs: [Ptr0][Ptr1][Ptr2]...          │  │
    │  │         │     │     │                  │  │
    │  │         ▼     ▼     ▼                  │  │
    │  │       ┌───┐┌───┐┌───┐                 │  │
    │  │       │   ││   ││   │ slab memory      │  │
    │  │       │ T ││ T ││ T │ blocks           │  │
    │  │       │   ││   ││   │                  │  │
    │  │       └───┘└───┘└───┘                 │  │
    │  └────────────────────────────────────────┘  │
    │                                              │
    │  ┌────────────────────────────────────────┐  │
    │  │ free: UnsafeParallelHashSet<Ptr>       │  │
    │  │  ┌───┬───┬───┐                         │  │
    │  │  │ptr│ptr│ptr│  recycled pointers       │  │
    │  │  └───┴───┴───┘                         │  │
    │  └────────────────────────────────────────┘  │
    └──────────────────────────────────────────────┘


CONSTRUCTION
=============

    new UnsafeParallelPoolAllocator(countPerChunk, allocator):

    ┌────────────────────────────────────────────────────────┐
    │ 1. Allocate raw memory for all pools:                  │
    │    pools = malloc(                                     │
    │      sizeof(UnsafePoolAllocator<T>)                    │
    │      × ThreadIndexCount)                               │
    │                                                        │
    │ 2. Initialize each pool:                               │
    │    for i in 0..ThreadIndexCount:                       │
    │      pools[i] = new UnsafePoolAllocator<T>(            │
    │                    countPerChunk, allocator)            │
    │                                                        │
    │ 3. threadIndex = 0 (default, overridden at job time)   │
    └────────────────────────────────────────────────────────┘


ALLOCATED() AGGREGATION
========================

    Allocated():
    ┌────────────────────────────────────────────────────────┐
    │ int total = 0;                                        │
    │ for i in 0..ThreadIndexCount:                         │
    │   total += pools[i].Allocated()                       │
    │        │                                              │
    │        └──► slabAllocator.Allocated()                 │
    │             = slabs.Length × countPerChunk × sizeof(T)│
    │                                                       │
    │ return total;                                         │
    └────────────────────────────────────────────────────────┘


DISPOSE
=======

    Dispose():
    ┌────────────────────────────────────────────────────────┐
    │ for i in 0..ThreadIndexCount:                         │
    │   pools[i].Dispose()                                  │
    │     └── slabAllocator.Dispose() + free set dispose    │
    │                                                       │
    │ Unmanaged.Free(pools)   ← free the array itself       │
    │ pools = null                                         │
    └────────────────────────────────────────────────────────┘


KEY PROPERTIES
==============

  * Type:              struct UnsafeParallelPoolAllocator<T> : IDisposable
  * Thread Safety:     YES via thread-isolated pools (no locks!)
  * Pool Count:        JobsUtility.ThreadIndexCount (platform-dependent)
  * Thread Routing:    [NativeSetThreadIndex] auto-populated
  * Per-Thread Pool:   Each is a full UnsafePoolAllocator<T>
  * Alloc Speed:       O(1) per thread, zero contention
  * Free Speed:        O(1) per thread, zero contention
  * Burst Compatible:  Yes
  * Memory Overhead:   N × UnsafePoolAllocator<T> (one per worker thread)
  * Constraint:        Free must be on same thread that allocated

## Source

- [BovineLabs.Core/Memory/UnsafeParallelPoolAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/UnsafeParallelPoolAllocator.cs)
