MemoryLabelAllocator - ECS Memory Profiler Integration
========================================================

Source: BovineLabs.Core/Memory/MemoryLabelAllocator.cs

OVERVIEW
--------
MemoryLabelAllocator implements Unity's AllocatorManager.IAllocator interface.
It routes all allocations to Allocator.Persistent but wraps them with
UnsafeUtility.MallocTracked/FreeTracked and a custom MemoryLabel. This allows
Unity's memory profiler to show exactly which subsystem owns each allocation,
while maintaining Burst compatibility via a static function pointer callback.

ARCHITECTURE
============

    ┌──────────────────────────────────────────────────────────────────┐
    │                    MemoryLabelAllocator                         │
    │                    : IAllocator                                 │
    │                                                                  │
    │  ┌─────────────────────┐    ┌─────────────────────────────┐     │
    │  │ handle:             │    │ memoryLabel: MemoryLabel     │     │
    │  │   AllocatorHandle   │    │   areaName:   "Network"     │     │
    │  │   (custom alloc ID) │    │   objectName: "PacketBuffer"│     │
    │  └─────────────────────┘    └──────────┬──────────────────┘     │
    │                                        │                        │
    │  ┌─────────────────────┐               │                        │
    │  │ allocationCount:int │◄──────────────┤ atomic tracking        │
    │  │  (Interlocked)      │               │                        │
    │  └─────────────────────┘               │                        │
    │                                        │                        │
    │  Function => Try (static burst-compiled callback)                │
    └────────────────────────┬───────────────────────────────────────┘
                             │
                             ▼
    ┌────────────────────────────────────────────────────┐
    │          AllocatorManager dispatch                  │
    │                                                    │
    │  When Unity needs to allocate using this handle:   │
    │  1. Constructs a Block struct with request info    │
    │  2. Calls Try(ref block) via function pointer      │
    │  3. Block.Range.Pointer updated with result        │
    └────────────────────────────────────────────────────┘


ALLOCATION PATH (Try → Allocate)
=================================

    Try(ref Block):
    ┌────────────────────────────────────────────┐
    │ if block.Range.Pointer == IntPtr.Zero:      │
    │     → Allocate(ref block)  ──────────────┐  │
    │ elif block.Bytes == 0:                    │  │
    │     → Free(ref block)  ───────────────┐   │  │
    │ else:                                 │   │  │
    │     → Reallocate(ref block) ──────┐   │   │  │
    └───────────────────────────────────│───│───│──┘
                                        │   │   │
    Allocate(ref block):                │   │   │
    ┌───────────────────────────────────│───│───│──┐
    │                                   │   │   │  │
    │  1. Validate size:                ▼   │   │  │
    │  ┌──────────────────────────────────┐│   │  │
    │  │ bytes = BytesPerItem × Items     ││   │  │
    │  │ Must be: 0 < bytes <= 1TB       ││   │  │
    │  └──────────────────────────────────┘│   │  │
    │                                      │   │  │
    │  2. Align to cache line:             │   │  │
    │  ┌──────────────────────────────────┐│   │  │
    │  │ alignment = max(cacheLineSize,   ││   │  │
    │  │                  block.Align)    ││   │  │
    │  └──────────────────────────────────┘│   │  │
    │                                      │   │  │
    │  3. Tracked malloc:                  │   │  │
    │  ┌──────────────────────────────────┐│   │  │
    │  │ ptr = UnsafeUtility              ││   │  │
    │  │   .MallocTracked(               ││   │  │
    │  │     bytes, alignment,           ││   │  │
    │  │     memoryLabel, 0)             ││   │  │
    │  └──────────────────────────────────┘│   │  │
    │                                      │   │  │
    │  4. Update block:                    │   │  │
    │  ┌──────────────────────────────────┐│   │  │
    │  │ block.Range.Pointer = ptr        ││   │  │
    │  │ block.AllocatedItems = Items     ││   │  │
    │  └──────────────────────────────────┘│   │  │
    │                                      │   │  │
    │  5. Atomic increment:                │   │  │
    │  ┌──────────────────────────────────┐│   │  │
    │  │ Interlocked.Increment(           ││   │  │
    │  │   ref allocationCount)           ││   │  │
    │  └──────────────────────────────────┘│   │  │
    └──────────────────────────────────────┘   │   │
                                               │   │
    Free(ref block):                           │   │
    ┌──────────────────────────────────────────┘   │
    │                                               │
    │  1. UnsafeUtility.FreeTracked(ptr, label)     │
    │  2. block.Range.Pointer = Zero                │
    │  3. Interlocked.Decrement(ref allocationCount)│
    └───────────────────────────────────────────────┘
                                                    │
    Reallocate(ref block):                          │
    ┌───────────────────────────────────────────────┘
    │
    │  1. Allocate new tracked buffer
    │  2. MemCpy old → new (min of old/new size)
    │  3. FreeTracked old pointer
    │  4. Update block with new pointer
    └──────────────────────────────────────────────


MEMORY PROFILER VISIBILITY
===========================

    Without MemoryLabelAllocator:
    ┌────────────────────────────────────────┐
    │ Unity Profiler → Memory → "Other"      │
    │   ???: 2.4 MB                          │
    │   ???: 1.1 MB                          │
    │   ???: 0.8 MB                          │
    │   (no way to identify owner)           │
    └────────────────────────────────────────┘

    With MemoryLabelAllocator:
    ┌────────────────────────────────────────┐
    │ Unity Profiler → Memory                │
    │                                        │
    │ ▸ Network                              │
    │     PacketBuffer: 2.4 MB              │
    │ ▸ Physics                             │
    │     CollisionGrid: 1.1 MB             │
    │ ▸ Audio                               │
    │     SampleBuffer: 0.8 MB              │
    └────────────────────────────────────────┘

    Each MemoryLabel = (areaName, objectName)
    Appears as labeled entry in Unity Memory Profiler


BURST COMPATIBILITY
====================

    The static callback pattern:

    ┌──────────────────────────────────────────────────────┐
    │ [BurstCompile]                                       │
    │ static int Try(IntPtr state, ref Block block)        │
    │ {                                                    │
    │     return ((MemoryLabelAllocator*)state)            │
    │         ->Try(ref block);                            │
    │ }                                                    │
    │                                                      │
    │ Function property returns this static method.        │
    │ AllocatorManager calls via function pointer.         │
    └──────────────────────────────────────────────────────┘

    ┌──────────────┐     function pointer     ┌──────────────┐
    │              │ ────────────────────────► │              │
    │ Allocator   │     Try(IntPtr, ref)      │  Burst-      │
    │ Manager     │                           │  compiled    │
    │             │ ◄────────────────────────  │  static      │
    │             │     error code / block     │  method      │
    └──────────────┘                           └──────────────┘


SAFETY AND DISPOSAL
====================

    Dispose():
    ┌────────────────────────────────────────────────────────┐
    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS                    │
    │   remaining = Volatile.Read(ref allocationCount)       │
    │   if remaining > 0:                                    │
    │     LOG ERROR:                                         │
    │     "disposed with N outstanding allocations"          │
    │ #endif                                                 │
    │                                                        │
    │ handle.Dispose()  ← unregisters custom allocator       │
    └────────────────────────────────────────────────────────┘


CONSTRAINTS
===========

    Maximum Allocation:  1 << 40 = 1 TB
    Minimum Alignment:   JobsUtility.CacheLineSize (typically 64 bytes)
    IsAutoDispose:       false (manual lifetime management)
    IsCustomAllocator:   true (registered via AllocatorManager)


KEY PROPERTIES
==============

  * Type:              struct MemoryLabelAllocator : IAllocator
  * Thread Safety:     YES (Interlocked.Increment/Decrement for count)
  * Backend:           Allocator.Persistent + tracked malloc/free
  * Profiler Labels:   MemoryLabel(areaName, objectName)
  * Realloc:           Alloc new → MemCpy → Free old (no in-place)
  * Burst Compatible:  Yes (static P/Invoke callback)
  * Safety Checks:     Logs outstanding allocations on dispose (editor only)
  * Atomic Count:      allocationCount via Interlocked ops

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/memory-allocators/MemoryLabelAllocator.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Burst"
> ```

```
BovineLabs.Core.Memory.MemoryLabelAllocator
  Kind: struct (ValueType=True)
  Size: 32 bytes

  Interfaces:
    System.IDisposable
    Unity.Collections.AllocatorManager+IAllocator

  Properties:
    public TryFunction Function { get }
    public AllocatorHandle Handle { get set }
    public Allocator ToAllocator { get }
    public Boolean IsCustomAllocator { get }
    public Boolean IsAutoDispose { get }

  Methods:
    public Void Initialize(String areaName, String objectName)
    public Void Dispose()
    public Int32 Try(Block& block)
    private Int32 Allocate(Block& block)
    private Int32 Free(Block& block)
    private Int32 Reallocate(Block& block)

  Attributes:
    [BurstCompileAttribute]

  Runtime Behavior:
    Can construct default instance: true
    IsAutoDispose: False
    IsCustomAllocator: False

Verified: 16 checks, 0 failures
```

## Source

- [BovineLabs.Core/Memory/MemoryLabelAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/MemoryLabelAllocator.cs)
