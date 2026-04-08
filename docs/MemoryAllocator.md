MemoryAllocator - Tracked Multi-Allocation Manager
====================================================

Source: BovineLabs.Core/Memory/MemoryAllocator.cs

OVERVIEW
--------
MemoryAllocator wraps Unity's AllocatorManager to provide simple "allocate many,
free all" semantics. Every allocation is tracked in a NativeHashSet<Ptr>.
Individual frees are NOT supported - call FreeAll() to release everything at once.
This is ideal for scoped subsystems that create many temporary unmanaged buffers.

ARCHITECTURE
============

     MemoryAllocator Instance
    ┌─────────────────────────────────────────────────────┐
    │                                                     │
    │  Allocator: Allocator (e.g. Allocator.Persistent)   │
    │                                                     │
    │  allocated: NativeHashSet<Ptr>                      │──────┐
    │    (tracks every live pointer)                      │      │
    │                                                     │      │
    └─────────────────────────────────────────────────────┘      │
                                                                 │
         allocated (NativeHashSet<Ptr>)                          │
        ┌───────────────────────────────────────┐                │
        │  Buckets (hash-based)                 │                │
        │  ┌─────────┬──┐ → Ptr 0xDEADBEEF     │                │
        │  │ bucket 0│──┤ → Ptr 0x12340000     │                │
        │  ├─────────┼──┤ → Ptr 0xAAAA0000     │                │
        │  │ bucket 1│──┤                       │                │
        │  ├─────────┼──┤ → Ptr 0xBBBB0000     │                │
        │  │ bucket 2│──┤ → Ptr 0xCCCC0000     │                │
        │  ├─────────┼──┤                       │                │
        │  │ ...     │  │                       │                │
        │  └─────────┴──┘                       │                │
        └───────────────────────────────────────┘                │
                                                                 │
    Actual Allocations (via AllocatorManager)                    │
    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
    │ Ptr 0xDEAD... │  │ Ptr 0x1234...│  │ Ptr 0xAAAA...│◄───────┘
    │ sizeof(X)×N  │  │ sizeof(Y)×M  │  │ sizeof(Z)×K  │  (each tracked)
    └──────────────┘  └──────────────┘  └──────────────┘


ALLOCATION FLOW
===============

    Allocate(itemSizeInBytes, alignmentInBytes, items=1):

    ┌───────────────────────────────────────────────────┐
    │ ptr = AllocatorManager.Allocate(                  │
    │         allocator, itemSizeInBytes,               │
    │         alignmentInBytes, items)                  │
    └───────────────────────┬───────────────────────────┘
                            │
                            ▼
                ┌─────────────────────┐
                │ allocated.Add(ptr)  │
                └─────────────────────┘
                            │
                            ▼
                    return ptr;


    Create<T>(count=1):

    ┌──────────────────────────────────────────────┐
    │ return (T*)Allocate(                         │
    │     sizeof(T),                               │
    │     alignof(T),                              │
    │     count)                                   │
    └──────────────────────────────────────────────┘


    CreateList<T>(capacity):

    ┌───────────────────────────────────────────────────┐
    │ capacity = max(capacity, 64/sizeof(T))            │
    │ capacity = ceilpow2(capacity)                     │
    │ buffer = Create<T>(capacity)                      │
    │                                                   │
    │ return UnsafeList<T> {                            │
    │     Ptr       = buffer,                           │
    │     Capacity  = capacity,                         │
    │     Allocator = Allocator.None  (we own it!)      │
    │ }                                                 │
    └───────────────────────────────────────────────────┘

    NOTE: Allocator.None means the list itself won't free the buffer.
    MemoryAllocator.FreeAll() handles that.


FREEALL FLOW
============

    FreeAll():

    ┌─────────────────────────────────────────────┐
    │ array = allocated.ToNativeArray(Temp)        │
    │                                             │
    │ ┌───────────────────────────────────────┐   │
    │ │ foreach ptr in array:                 │   │
    │ │   AllocatorManager.Free(allocator,ptr)│   │
    │ │                                       │   │
    │ │   ┌──────────┐  ┌──────────┐         │   │
    │ │   │ Ptr 0xA  │  │ Ptr 0xB  │  ...    │   │
    │ │   │  FREE    │  │  FREE    │         │   │
    │ │   └──────────┘  └──────────┘         │   │
    │ └───────────────────────────────────────┘   │
    │                                             │
    │ allocated.Clear()                           │
    └─────────────────────────────────────────────┘


DISPOSE FLOW
============

    Dispose():

    ┌──────────────────────────────┐
    │ FreeAll()                    │
    │   └─ frees all tracked ptrs │
    │   └─ clears the hashset     │
    │                              │
    │ allocated.Dispose()          │
    │   └─ frees the hashset itself│
    └──────────────────────────────┘


USAGE PATTERN
=============

    ┌────────────────────────────────────────────────────┐
    │  // Setup phase                                    │
    │  var mem = new MemoryAllocator(Allocator.Persistent)│
    │                                                    │
    │  // Allocate many buffers                          │
    │  int* a = mem.Create<int>(100);                    │
    │  float* b = mem.Create<float>(256);                │
    │  var list = mem.CreateList<byte>(1024);            │
    │  void* c = mem.Allocate(64, 8);                    │
    │                                                    │
    │  // Use them...                                    │
    │  // ...                                            │
    │                                                    │
    │  // Tear down everything at once                   │
    │  mem.Dispose();                                    │
    └────────────────────────────────────────────────────┘


KEY PROPERTIES
==============

  * Type:              struct MemoryAllocator : IDisposable
  * Thread Safety:     NONE (not thread-safe)
  * Individual Free:   NOT supported (use FreeAll or Dispose)
  * Tracking:          NativeHashSet<Ptr> - O(1) add, O(n) free-all
  * Alloc Speed:       O(1) + hashset insert
  * FreeAll Speed:     O(n) where n = number of allocations
  * Burst Compatible:  Yes
  * List Capacity:     Minimum 64/sizeof(T), always power-of-2
  * List Ownership:    Buffer tracked by MemoryAllocator, not by list

## Source

- [BovineLabs.Core/Memory/MemoryAllocator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Memory/MemoryAllocator.cs)
- [BovineLabs.Core/Collections/Reference.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Reference.cs)
