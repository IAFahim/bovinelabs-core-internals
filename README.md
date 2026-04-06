UnsafeSlabAllocator - Block-Based Chunk Allocation
===================================================

Source: BovineLabs.Core/Memory/UnsafeSlabAllocator.cs

OVERVIEW
--------
UnsafeSlabAllocator<T> prevents memory fragmentation by allocating fixed-size
slabs (chunks) and bump-allocating within each slab. Items are never freed
individually - only bulk deallocation via Clear() or Dispose(). This trades
granular deallocation for zero fragmentation and O(1) allocation speed.

MEMORY LAYOUT
=============

     UnsafeSlabAllocator<T> Instance
    ┌──────────────────────────────────────────┐
    │  countPerSlab: int    (e.g. 64)          │
    │  allocator:    AllocatorHandle           │
    │  slabs:        UnsafeList<Ptr>*          │──────┐
    │  count:        int*  (current slab idx)  │──┐   │
    └──────────────────────────────────────────┘  │   │
                                                  │   │
         slabs list ( UnsafeList<Ptr> )           │   │
    ┌────────────────────────────────┐            │   │
    │  [0] Ptr ────────────────────────────► Slab 0   │
    │  [1] Ptr ────────────────────────────► Slab 1   │
    │  [2] Ptr ────────────────────────────► Slab 2   │
    └────────────────────────────────┘       │       │
                                             │       │
    *count (bump counter)                    │       │
    ┌─────────┐                              │       │
    │  n      │ (n < countPerSlab)           │       │
    └─────────┘                              │       │
                                             ▼       ▼
    Each Slab = countPerSlab * sizeof(T) bytes, aligned to alignof(T)

    SLAB MEMORY (example: countPerSlab=8, T=int)

    Slab 0 (FULL - count was 8, now bumping in slab 1)
    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    │ T0 │ T1 │ T2 │ T3 │ T4 │ T5 │ T6 │ T7 │
    └────┴────┴────┴────┴────┴────┴────┴────┘

    Slab 1 (ACTIVE - *count = 5, next alloc at index 5)
    ┌────┬────┬────┬────┬────┬────┬────┬────┐
    │ T0 │ T1 │ T2 │ T3 │ T4 │ ?? │ ?? │ ?? │
    └────┴────┴────┴────┴────┴────┴────┴────┘
     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   ^^^^^^^^
     Allocated (in use)              Free (unused)
                 *count = 5 ──────────^


ALLOCATION ALGORITHM
====================

    Alloc() Flow:

    ┌─────────────────────────────────┐
    │  *count == countPerSlab ?       │
    │  (Is current slab full?)        │
    └──────────┬──────────────────────┘
               │
       ┌───────┴───────┐
       │               │
      YES              NO
       │               │
       ▼               ▼
    ┌──────────────┐  ┌──────────────────────────────┐
    │ *count = 0   │  │ ptr = slabs[last][*count]     │
    │ allocate new │  │ *count++                      │
    │ slab via     │  │ return ptr                    │
    │ Unmanaged    │  └──────────────────────────────┘
    │ .Allocate()  │
    │ slabs.Add()  │
    └──────┬───────┘
           │
           ▼
    ┌──────────────────────────────┐
    │ ptr = slabs[last][0]         │
    │ *count++  (= 1)              │
    │ return ptr                   │
    └──────────────────────────────┘


CLEAR / DISPOSE
===============

    Clear():                         Dispose():
    ┌───────────────────────┐       ┌───────────────────────┐
    │ for each slab:        │       │ Clear()               │
    │   Unmanaged.Free(slab)│──────►│ Destroy slabs list    │
    │ slabs.Clear()         │       │ Free count pointer    │
    │ *count = countPerSlab │       │ null out fields       │
    └───────────────────────┘       └───────────────────────┘

    (Resets to "no slabs" state -    (Complete teardown,
     next Alloc() triggers new        cannot be reused)
     slab allocation)


ALLOCATION COUNT TRACKING
=========================

    AllocationCount = (countPerSlab × (slabs.Length - 1)) + *count

    Example: countPerSlab=8, 3 slabs, *count=5
    ┌──────────────────────────────────────────────────┐
    │ (8 × (3 - 1)) + 5 = 16 + 5 = 21 items total    │
    └──────────────────────────────────────────────────┘

    Full slabs: 2 × 8 = 16     Active slab: 5
    ┌───┬───┬───┬───┬───┬───┬───┬───┐  ┌───┬───┬───┬───┬───┬───┬───┬───┐
    │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │  │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │
    └───┴───┴───┴───┴───┴───┴───┴───┘  └───┴───┴───┴───┴───┴───┴───┴───┘
      Slab 0: 8/8 (full)                Slab 1: 5/8 (active)
                                         ^count = 5


KEY PROPERTIES
==============

  * Generic:           T : unmanaged
  * Thread Safety:     NONE (caller must synchronize)
  * Fragmentation:     ZERO (slabs are contiguous blocks)
  * Alloc Speed:       O(1) pointer bump
  * Free Speed:        N/A (no individual free)
  * Clear Speed:       O(n) slabs to free
  * Allocated():       slabs.Length × countPerSlab × sizeof(T)
  * Safety:            No bounds checking, no AtomicSafetyHandle
  * Burst Compatible:  Yes (uses unsafe pointers, NativeDisableUnsafePtrRestriction)
