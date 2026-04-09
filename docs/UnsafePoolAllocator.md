UnsafePoolAllocator - Slab + Free-List Hybrid Pool
====================================================

Source: BovineLabs.Core/Memory/UnsafePoolAllocator.cs

OVERVIEW
--------
UnsafePoolAllocator<T> combines two strategies: slab allocation for growing
capacity and a free-list (hash set) for recycling returned pointers. Items are
allocated from slabs in contiguous chunks. When freed, pointers are added to
a hash set. On next allocation, freed pointers are recycled first, falling back
to slab allocation only when the free list is empty.

ARCHITECTURE
=============

    UnsafePoolAllocator<T> Instance
    ┌─────────────────────────────────────────────────────────┐
    │                                                         │
    │  slabAllocator: UnsafeSlabAllocator<T>                  │
    │    (grows capacity in fixed-size chunks)                │
    │    ┌───────────────────────────────────────────┐        │
    │    │ slabs: [Slab0][Slab1][Slab2]...           │        │
    │    │ count: bump counter in current slab        │        │
    │    │ countPerSlab: items per slab               │        │
    │    └───────────────────────────────────────────┘        │
    │                                                         │
    │  free: UnsafeParallelHashSet<Ptr>                       │
    │    (recycled pointers waiting for reuse)                │
    │    ┌───────────────────────────────────┐                │
    │    │ { ptr_A, ptr_B, ptr_C, ... }      │                │
    │    └───────────────────────────────────┘                │
    │                                                         │
    └─────────────────────────────────────────────────────────┘


MEMORY LAYOUT - SLAB ALLOCATION + FREE RECYCLING
=================================================

    Slab Allocator provides raw memory:

    Slab 0 (countPerSlab = 8)
    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    │ T0 │ T1 │ T2 │ T3 │ T4 │ T5 │ T6 │ T7 │
    └────┴────┴────┴────┴────┴────┴────┴────┘

    Slab 1 (countPerSlab = 8)
    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    │ T8 │ T9 │T10 │T11 │T12 │T13 │T14 │T15 │
    └────┴────┴────┴────┴────┴────┴────┴────┘

    Slab 2 (partially used - bump counter at 3)
    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    │T16 │T17 │T18 │ ?? │ ?? │ ?? │ ?? │ ?? │
    └────┴────┴────┴────┴────┴────┴────┴────┘
                      ▲
                      └── next slab alloc goes here

    Free List (recycled pointers):
    ┌──────────────────────────────────────┐
    │  free hash set:                      │
    │  ┌───────┬───────┬───────┬─────┐    │
    │  │ &T[3] │ &T[7] │ &T[9] │ ... │    │
    │  └───────┴───────┴───────┴─────┘    │
    │                                      │
    │  These slots were freed and can      │
    │  be reused before new slab allocs    │
    └──────────────────────────────────────┘


ALLOCATION ALGORITHM
====================

    T* Alloc():

    ┌─────────────────────────────────────────┐
    │  Try to get recycled pointer:           │
    │  e = free.GetEnumerator()               │
    │  if e.MoveNext():                       │
    └──────────────┬──────────────────────────┘
                   │
           ┌───────┴───────┐
           │               │
          HAS FREE        EMPTY
           │               │
           ▼               ▼
    ┌──────────────┐  ┌──────────────────────────┐
    │ ptr = e.Current│ │ Delegate to slab:        │
    │ free.Remove()│  │ return slabAllocator     │
    │ return ptr   │  │         .Alloc()         │
    │              │  │                          │
    │ RECYCLED!    │  │ NEW ALLOCATION!          │
    └──────────────┘  └──────────────────────────┘

    Slab .Alloc() bumps pointer:
    ┌─────────────────────────────────────────────┐
    │ if *count == countPerSlab:                  │
    │   allocate new slab block                   │
    │   *count = 0                                │
    │ ptr = slab[last][*count]                    │
    │ (*count)++                                  │
    │ return ptr                                  │
    └─────────────────────────────────────────────┘


FREE ALGORITHM
==============

    Free(T* p):

    ┌───────────────────────────────────────────┐
    │  free.Add(new Ptr(p))                     │
    │                                           │
    │  Pointer goes into the free hash set.     │
    │  Memory is NOT zeroed or returned to OS.  │
    │  Next Alloc() may recycle this pointer.   │
    └───────────────────────────────────────────┘

    Example lifecycle:

    1. ptr = Alloc()  →  slab allocates &T[5]
       free = {}  (empty)

    2. ptr = Alloc()  →  slab allocates &T[6]
       free = {}  (empty)

    3. Free(ptr_to_T5)  →  free.Add(&T[5])
       free = { &T[5] }

    4. ptr = Alloc()  →  recycled from free set!
       free = {}  (empty again)
       returns &T[5]  (same memory reused)


FULL LIFECYCLE EXAMPLE
======================

    Time ──►

    Step 1: Initial state (empty)
    ┌───────────────────┐
    │ slabs: []         │
    │ free: {}          │
    └───────────────────┘

    Step 2: Alloc() × 4  →  new slab created, 4 items bumped
    ┌───────────────────────────────────┐
    │ slabs: [Slab0]                   │
    │ Slab0: [T0][T1][T2][T3][ ][ ][ ][ ]│
    │ free: {}                          │
    └───────────────────────────────────┘

    Step 3: Alloc() × 5  →  slab fills, new slab allocated
    ┌──────────────────────────────────────────────────┐
    │ slabs: [Slab0][Slab1]                            │
    │ Slab0: [T0][T1][T2][T3][T4][T5][T6][T7]  FULL   │
    │ Slab1: [T8][ ][ ][ ][ ][ ][ ][ ]  count=1       │
    │ free: {}                                         │
    └──────────────────────────────────────────────────┘

    Step 4: Free(&T3), Free(&T7), Free(&T8)
    ┌──────────────────────────────────────────────────┐
    │ slabs: [Slab0][Slab1]                            │
    │ Slab0: [T0][T1][T2][##][T4][T5][T6][##]         │
    │ Slab1: [##][ ][ ][ ][ ][ ][ ][ ]                │
    │ free: { &T3, &T7, &T8 }                         │
    └──────────────────────────────────────────────────┘

    Step 5: Alloc()  →  RECYCLES from free (e.g. &T8)
    ┌──────────────────────────────────────────────────┐
    │ free: { &T3, &T7 }  (removed &T8)               │
    │ Slab1: [T8][ ][ ][ ][ ][ ][ ][ ]  (reused!)     │
    └──────────────────────────────────────────────────┘


ALLOCATED() METRIC
===================

    Allocated():
    ┌────────────────────────────────────────────────────┐
    │ return slabAllocator.Allocated()                   │
    │                                                    │
    │ = slabs.Length × countPerSlab × sizeof(T)         │
    │                                                    │
    │ NOTE: This is TOTAL capacity, not items-in-use.    │
    │ Does not subtract free-list entries.               │
    └────────────────────────────────────────────────────┘


DISPOSE
=======

    Dispose():
    ┌─────────────────────────────────────────┐
    │ slabAllocator.Dispose()                 │
    │   └── Frees all slab blocks             │
    │   └── Destroys slab list                │
    │   └── Frees count pointer               │
    │                                         │
    │ free.Dispose()                          │
    │   └── Frees the hash set                │
    │                                         │
    │ slabAllocator = default                 │
    │ free = default                          │
    └─────────────────────────────────────────┘


KEY PROPERTIES
==============

  * Type:              struct UnsafePoolAllocator<T> : IDisposable
  * Thread Safety:     NONE (caller must synchronize)
  * Growth Strategy:   Slab-based (countPerChunk per slab)
  * Recycling:         UnsafeParallelHashSet<Ptr> free-list
  * Alloc:             O(1) - free-list hit or slab bump
  * Free:              O(1) - hashset add
  * Memory:            Never shrinks (slabs persist until Dispose)
  * Burst Compatible:  Yes
  * Fragmentation:     None within slabs; freed slots are reused
  * Use Case:          General purpose pool with individual free support

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/memory-allocators/UnsafePoolAllocator.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
> ```

```
PASS: UnsafePoolAllocator<T> type exists
PASS: Is a struct (ValueType)
PASS: Implements IDisposable
PASS: Has 1 generic parameter
PASS: Has constructor
INFO: Constructor params: Int32 countPerChunk, Allocator allocator
PASS: Constructor takes int countPerChunk
PASS: Constructor takes Allocator
PASS: Has IsCreated property
PASS: IsCreated returns bool
PASS: Has Alloc method
PASS: Alloc returns pointer
PASS: Has Free method
PASS: Free takes pointer parameter
PASS: Has Allocated method
PASS: Allocated returns int
PASS: Has Dispose method
INFO: Fields: slabAllocator, free
PASS: Has slabAllocator field (UnsafeSlabAllocator<T>)
PASS: Has free field (UnsafeParallelHashSet<Ptr>)
PASS: No lock/spinlock fields (not thread-safe)

=== 19 PASSED, 0 FAILED ===
```

## Source

- [BovineLabs.Core/Memory/UnsafePoolAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/UnsafePoolAllocator.cs)
- [BovineLabs.Core/Memory/UnsafeParallelPoolAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/UnsafeParallelPoolAllocator.cs)
