UnsafeFixedPoolAllocator - Pre-Allocated Fixed-Size Pool
=========================================================

Source: BovineLabs.Core/Memory/UnsafeFixedPoolAllocator.cs

OVERVIEW
--------
UnsafeFixedPoolAllocator<T> pre-allocates exactly maxItems contiguous slots
at construction time. A UnsafeParallelHashSet<Ptr> tracks which slots are free.
No resizing ever occurs. When the pool is exhausted, Alloc() returns null.
This provides stable, predictable memory usage and pointer stability.

MEMORY LAYOUT
=============

    UnsafeFixedPoolAllocator<T> Instance
    ┌──────────────────────────────────────────────────────────┐
    │                                                          │
    │  maxItems: int            (fixed capacity, e.g. 16)      │
    │  allocator: AllocatorHandle                              │
    │                                                          │
    │  buffer: Ptr ─────────────────────────────────┐          │
    │        (single contiguous allocation)         │          │
    │                                                │          │
    │  freeIndex: UnsafeParallelHashSet<Ptr>         │          │
    │    (tracks which slots are free)               │          │
    │                                                │          │
    └────────────────────────────────────────────────┼──────────┘
                                                     │
                                                     ▼
    buffer (contiguous T[maxItems] block)
    ┌──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┐
    │ T[0] │ T[1] │ T[2] │ T[3] │ T[4] │ T[5] │ T[6] │ T[7] │
    └──────┴──────┴──────┴──────┴──────┴──────┴──────┴──────┘
      ▲      ▲      ▲      ▲      ▲      ▲      ▲      ▲
      │      │      │      │      │      │      │      │
    ptrs: buffer+0, buffer+1, buffer+2, ... buffer+7

    Construction: ALL slots added to freeIndex
    ┌────────────────────────────────────────────────────┐
    │ freeIndex = { &T[0], &T[1], &T[2], ..., &T[15] } │
    └────────────────────────────────────────────────────┘


ALLOCATION FLOW
================

    T* Alloc():
    ┌───────────────────────────────────────────────────┐
    │ if freeIndex.Count() == 0:                        │
    │   ┌─────────────────────────────────────────┐     │
    │   │ Pool exhausted! Return null             │     │
    │   │ (caller must handle failure)            │     │
    │   └─────────────────────────────────────────┘     │
    │                                                   │
    │ e = freeIndex.GetEnumerator()                     │
    │ ptr = e.Current    (arbitrary free slot pointer)  │
    │ freeIndex.Remove(ptr)                             │
    │ return (T*)ptr                                    │
    └───────────────────────────────────────────────────┘

    Example: Alloc() removes &T[3] from free set

    Before:                               After:
    ┌──────┬──────┬──────┬──────┐        ┌──────┬──────┬──────┐
    │ T[0] │ T[1] │ T[2] │ T[3] │ ...    │ T[0] │ T[1] │ T[2] │ ...
    │ FREE │ FREE │ FREE │ FREE │        │ FREE │ FREE │ FREE │ USED│
    └──────┴──────┴──────┴──────┘        └──────┴──────┴──────┘
    free={0,1,2,3,...}                    free={0,1,2,...}
          ^^^^ removed 3                        ^^^ no 3


FREE FLOW
==========

    Free(T* p):
    ┌───────────────────────────────────────────────────┐
    │ ValidatePtr(p)  ←── safety checks (editor only)   │
    │                                                   │
    │ freeIndex.Add(p)                                  │
    └───────────────────────────────────────────────────┘

    Validation checks (ENABLE_UNITY_COLLECTIONS_CHECKS):
    ┌───────────────────────────────────────────────┐
    │ 1. p != null                                  │
    │ 2. buffer <= p < buffer + maxItems            │
    │    (pointer must be from this pool)           │
    │ 3. !freeIndex.Contains(p)                     │
    │    (double-free detection)                    │
    │ 4. freeIndex.Count() < maxItems               │
    │    (corruption check)                         │
    └───────────────────────────────────────────────┘


POOL STATE LIFECYCLE
====================

    State 1: Just constructed (all free)
    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
    │F │F │F │F │F │F │F │F │F │F │F │F │F │F │F │F │  maxItems=16
    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
    freeIndex.Count() = 16

    State 2: After 10 allocations
    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
    │U │U │U │U │U │U │U │U │U │U │F │F │F │F │F │F │
    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
    freeIndex.Count() = 6

    State 3: After freeing 3 slots
    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
    │U │F │U │F │U │U │U │U │U │U │F │F │F │U │F │F │
    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
    freeIndex.Count() = 9

    State 4: Fully exhausted
    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
    │U │U │U │U │U │U │U │U │U │U │U │U │U │U │U │U │
    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
    freeIndex.Count() = 0  →  next Alloc() returns null!


POINTER VALIDATION DETAIL
==========================

    Valid pointer check (editor only):

    buffer                                         buffer + maxItems
      │                                                   │
      ▼                                                   ▼
    ┌──────────────────────────────────────────────────────┐
    │  ✓ p inside this range                               │
    └──────────────────────────────────────────────────────┘

    ┌───────────────────┐
    │ p == null?        │──► YES → ArgumentException
    │                   │
    │ p < buffer?       │──► YES → "Ptr not from this allocator"
    │                   │
    │ p >= buffer+N?    │──► YES → "Ptr not from this allocator"
    │                   │
    │ freeIndex.Has(p)? │──► YES → "Ptr already returned" (double free!)
    │                   │
    │ All OK            │──► proceed with free
    └───────────────────┘


DISPOSE
=======

    Dispose():
    ┌────────────────────────────────────────────────────┐
    │ Unmanaged.Free(buffer)    ← free the T[] block    │
    │ freeIndex.Dispose()       ← free the hash set     │
    │ buffer = Ptr.Zero                                │
    └────────────────────────────────────────────────────┘


KEY PROPERTIES
==============

  * Type:              struct UnsafeFixedPoolAllocator<T> : IDisposable
  * Thread Safety:     Partial (UnsafeParallelHashSet is concurrent-read)
  * Capacity:          Fixed at construction (maxItems), never resizes
  * Alloc:             O(1) amortized (hashset enumerate + remove)
  * Free:              O(1) (hashset add)
  * Overflow:          Returns null (caller must handle)
  * Safety:            Double-free, null, bounds checking (editor only)
  * Burst Compatible:  Yes
  * Memory Layout:     Single contiguous block for all items
  * Overhead:          UnsafeParallelHashSet<Ptr> with maxItems entries
  * Use Case:          Fixed-capacity pools where max count is known upfront

## Verified Data

> [Run test snippet](../snippets/memory-allocators/UnsafeFixedPoolAllocator.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Memory/UnsafeFixedPoolAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/UnsafeFixedPoolAllocator.cs)
- [BovineLabs.Core.Tests/Memory/PoolAllocatorTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Memory/PoolAllocatorTests.cs)
