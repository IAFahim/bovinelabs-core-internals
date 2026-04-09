NativeSlabAllocator - Safety-Wrapped Slab Allocator
====================================================

Source: BovineLabs.Core/Memory/NativeSlabAllocator.cs

OVERVIEW
--------
NativeSlabAllocator<T> is a [NativeContainer]-decorated wrapper around
UnsafeSlabAllocator<T>. It adds Unity's AtomicSafetyHandle system, enabling
editor-time detection of use-after-free, write-during-read, and other safety
violations. The underlying allocation mechanics are identical to
UnsafeSlabAllocator - bump allocation within fixed-size slabs.

ARCHITECTURE - WRAPPER PATTERN
===============================

    ┌──────────────────────────────────────────────────────────┐
    │  [NativeContainer]                                       │
    │  NativeSlabAllocator<T>                                  │
    │                                                          │
    │  ┌────────────────────────────────────────────────────┐  │
    │  │  AtomicSafetyHandle m_Safety                       │  │
    │  │  SharedStatic<int> s_staticSafetyId                │  │
    │  │                                                    │  │
    │  │  (only exist in ENABLE_UNITY_COLLECTIONS_CHECKS)   │  │
    │  └────────────────────────────────────────────────────┘  │
    │                                                          │
    │  ┌────────────────────────────────────────────────────┐  │
    │  │  slabAllocator: UnsafeSlabAllocator<T>             │  │
    │  │                                                    │  │
    │  │  ┌──────────────────────────────────────────────┐  │  │
    │  │  │ countPerSlab                                 │  │  │
    │  │  │ allocator: AllocatorHandle                   │  │  │
    │  │  │ slabs: UnsafeList<Ptr>*                      │  │  │
    │  │  │ count: int*                                  │  │  │
    │  │  └──────────────────────────────────────────────┘  │  │
    │  └────────────────────────────────────────────────────┘  │
    │                                                          │
    └──────────────────────────────────────────────────────────┘

    Every public method checks AtomicSafetyHandle BEFORE delegating:


OPERATION FLOW WITH SAFETY CHECKS
==================================

    Alloc():
    ┌───────────────────────────────────────────────┐
    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    │   AtomicSafetyHandle.CheckWriteAndThrow(      │
    │       m_Safety)                               │
    │   // Throws if:                               │
    │   //   - Already disposed                     │
    │   //   - Another job is reading               │
    │   //   - Safety handle invalidated            │
    │ #endif                                        │
    │                                               │
    │ return slabAllocator.Alloc()  ──────────────► │
    │                              ┌──────────────┐ │
    │                              │ Bump allocate │ │
    │                              │ from current  │ │
    │                              │ slab or add   │ │
    │                              │ new slab      │ │
    │                              └──────────────┘ │
    └───────────────────────────────────────────────┘

    Clear():
    ┌───────────────────────────────────────────────┐
    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    │   AtomicSafetyHandle.CheckWriteAndThrow(      │
    │       m_Safety)                               │
    │ #endif                                        │
    │                                               │
    │ slabAllocator.Clear()                         │
    │   └── Free all slabs, reset to empty state    │
    └───────────────────────────────────────────────┘

    AllocationCount (get):
    ┌───────────────────────────────────────────────┐
    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    │   AtomicSafetyHandle.CheckReadAndThrow(       │
    │       m_Safety)                               │
    │   // Write check NOT needed for reads         │
    │ #endif                                        │
    │                                               │
    │ return slabAllocator.AllocationCount          │
    └───────────────────────────────────────────────┘

    Dispose():
    ┌───────────────────────────────────────────────┐
    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    │   CollectionHelper                            │
    │     .DisposeSafetyHandle(ref m_Safety)        │
    │   // Invalidates the handle so any future     │
    │   // access throws immediately                │
    │ #endif                                        │
    │                                               │
    │ slabAllocator.Dispose()                       │
    │   └── Full teardown of all slabs              │
    └───────────────────────────────────────────────┘


SAFETY SYSTEM INTEGRATION
==========================

    Editor (ENABLE_UNITY_COLLECTIONS_CHECKS):

    ┌───────────────────────────┐
    │   NativeSlabAllocator<T>  │
    │         │                 │
    │         ▼                 │
    │   ┌─────────────┐        │
    │   │ SafetyId    │───────►│ Static type ID
    │   └─────────────┘        │ registered once
    │         │                 │
    │         ▼                 │
    │   ┌──────────────────┐   │
    │   │ AtomicSafety     │   │
    │   │ Handle           │   │
    │   │                  │   │
    │   │ • Version check  │   │
    │   │ • Read/write     │   │
    │   │   tracking       │   │
    │   │ • Dispose detect │   │
    │   └──────────────────┘   │
    └───────────────────────────┘

    ┌────────────────────────────────────────────────────┐
    │ Safety Handle Lifecycle:                           │
    │                                                    │
    │ Constructor:                                       │
    │   CreateSafetyHandle(allocator)                    │
    │   InitNativeContainer<T>(handle)                   │
    │   SetStaticSafetyId<T>(ref handle, ref id)         │
    │   SetBumpSecondaryVersionOnScheduleWrite(true)     │
    │                                                    │
    │ Each Access:                                       │
    │   CheckReadAndThrow  ← for reads (AllocationCount)│
    │   CheckWriteAndThrow ← for writes (Alloc, Clear)  │
    │                                                    │
    │ Dispose:                                           │
    │   DisposeSafetyHandle(ref handle)                  │
    │   → Invalidates handle, future access throws       │
    └────────────────────────────────────────────────────┘

    Release builds: All safety code compiled out (zero overhead).


UNDERLYING SLAB MEMORY LAYOUT
==============================

    (Identical to UnsafeSlabAllocator - shown here for completeness)

    Slabs grow on demand:
    ┌──────────────────────────────────────────────────┐
    │                                                  │
    │  Slab 0 (FULL)        Slab 1 (ACTIVE)            │
    │  ┌──┬──┬──┬──┐      ┌──┬──┬──┬──┐              │
    │  │T0│T1│T2│T3│      │T4│T5│??│??│  count=2      │
    │  └──┴──┴──┴──┘      └──┴──┴──┴──┘              │
    │                                                  │
    │  countPerSlab = 4  in this example               │
    │                                                  │
    │  AllocationCount = (4 × (2-1)) + 2 = 6          │
    └──────────────────────────────────────────────────┘


COMPARISON: Native vs Unsafe
=============================

    ┌──────────────────────┬────────────────────┬───────────────────┐
    │ Feature              │ NativeSlab         │ UnsafeSlab        │
    ├──────────────────────┼────────────────────┼───────────────────┤
    │ [NativeContainer]    │ Yes                │ No                │
    │ Safety Handle        │ Yes (editor)       │ No                │
    │ Dispose Detection    │ Automatic          │ Manual            │
    │ Read/Write Tracking  │ Yes                │ No                │
    │ Job System Integration│ Properly tracked  │ Untracked         │
    │ Runtime Overhead     │ Zero (editor only) │ Zero              │
    │ Burst Compatible     │ Yes                │ Yes               │
    │ Use Case             │ Shared containers  │ Internal use      │
    └──────────────────────┴────────────────────┴───────────────────┘


KEY PROPERTIES
==============

  * Type:              struct NativeSlabAllocator<T> : IDisposable
  * Attribute:         [NativeContainer]
  * Thread Safety:     Safety-checked (editor); AtomicSafetyHandle
  * Underlying:        UnsafeSlabAllocator<T> (same bump allocation)
  * Safety:            AtomicSafetyHandle + static safety ID
  * Write Checks:      Alloc(), Clear()
  * Read Checks:       AllocationCount
  * BumpSecondary:     SetBumpSecondaryVersionOnScheduleWrite(true)
  * Allocator Check:   CollectionHelper.CheckAllocator in constructor
  * Burst Compatible:  Yes
  * Zero Runtime Cost: All checks behind #if ENABLE_UNITY_COLLECTIONS_CHECKS

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/memory-allocators/NativeSlabAllocator.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
> ```

```
PASS: NativeSlabAllocator<T> type exists
PASS: Is a struct (ValueType)
PASS: Implements IDisposable
PASS: Has [NativeContainer] attribute
PASS: Has 1 generic parameter
PASS: Has constructor
INFO: Constructor params: Int32 countPerSlab, AllocatorHandle allocator
PASS: Constructor takes int countPerSlab
PASS: Has IsCreated property
PASS: IsCreated returns bool
PASS: Has AllocationCount property
PASS: AllocationCount returns int
PASS: Has Alloc method
PASS: Alloc returns pointer
PASS: Has Clear method
PASS: Has Dispose method
INFO: Fields: slabAllocator, m_Safety
PASS: Has slabAllocator field (wraps UnsafeSlabAllocator)
PASS: Has safety handle field (m_Safety)
INFO: Static fields: s_staticSafetyId
PASS: Has static safety ID field

=== 18 PASSED, 0 FAILED ===
```

## Source

- [BovineLabs.Core/Memory/NativeSlabAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/NativeSlabAllocator.cs)
- [BovineLabs.Core.Tests/Memory/SlabAllocatorTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Memory/SlabAllocatorTests.cs)
