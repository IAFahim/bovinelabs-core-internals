UnmanagedPool - Spin-Lock Protected Object Pool
=================================================

Source: BovineLabs.Core/Collections/UnmanagedPool.cs

OVERVIEW
--------
UnmanagedPool<T> is a fixed-capacity, stack-based pool for unmanaged objects.
It uses a single contiguous buffer as a stack, with a SpinLock for thread safety.
TryAdd() pushes onto the stack (return to pool), TryGet() pops (acquire from pool).
Capacity is always a power-of-2, minimum one cache line's worth of elements.

MEMORY LAYOUT
=============

    UnmanagedPool<T> Instance (readonly struct)
    ┌────────────────────────────────────────────────────────────┐
    │                                                            │
    │  capacity: int         (power-of-2, >= cacheLineSize/sizeof(T))│
    │  allocator: Allocator  (backing allocator)                 │
    │                                                            │
    │  buffer: T* ────────────────────────────────────┐          │
    │        (contiguous T[capacity] array)           │          │
    │                                                  │          │
    │  length: int* ──────────────────────────┐       │          │
    │        (stack pointer: next free index) │       │          │
    │                                          │       │          │
    │  spinner: SpinLock* ───────────┐        │       │          │
    │        (thread synchronization)│        │       │          │
    │                                 │        │       │          │
    └─────────────────────────────────┼────────┼───────┼──────────┘
                                      │        │       │
                                      │        │       │
    SpinLock                          │        │       │
    ┌─────────────────┐               │        │       │
    │ Acquire()  ─────┤► lock         │        │       │
    │ Release()  ─────┤► unlock       │        │       │
    └─────────────────┘               │        │       │
                                      │        │       │
    *length (stack top index)         │        │       │
    ┌───────────┐                     │        │       │
    │  3        │ (= 3 items pooled)  │        │       │
    └───────────┘                     │        │       │
                                      │        │       │
    buffer[capacity]                  │        │       │
    ┌─────┬─────┬─────┬─────┬─────┐  │        │       │
    │ T_a │ T_b │ T_c │  ?  │  ?  │  │        │       │
    └─────┴─────┴─────┴─────┴─────┘  │        │       │
      [0]   [1]   [2]   [3]   [4]    │        │       │
      ▲                    ▲          │        │       │
      │                    │          │        │       │
      │    *length=3 ──────┘          │        │       │
      │    (stack top = next push)    │        │       │
      │                              │        │       │
      └──────────────────────────────┼────────┘       │
                                     └────────────────┘


STACK-BASED POOL MECHANICS
===========================

    The pool operates as a LIFO stack:
    - buffer[0..length-1] = pooled items (available for TryGet)
    - buffer[length..capacity-1] = unused space

    TryAdd(element) - Return item to pool:
    ┌─────────────────────────────────────────────────────────┐
    │  spinner->Acquire()        ← lock                       │
    │                                                          │
    │  if (*length < capacity):                                │
    │    ┌───────────────────────────────────────────────┐    │
    │    │ buffer[*length] = element                     │    │
    │    │ (*length)++                                   │    │
    │    │ spinner->Release()                            │    │
    │    │ return true  (success)                        │    │
    │    └───────────────────────────────────────────────┘    │
    │                                                          │
    │  spinner->Release()        ← unlock                     │
    │  return false  (pool full)                              │
    └─────────────────────────────────────────────────────────┘

    TryGet(out element) - Acquire item from pool:
    ┌─────────────────────────────────────────────────────────┐
    │  spinner->Acquire()        ← lock                       │
    │                                                          │
    │  if (*length > 0):                                       │
    │    ┌───────────────────────────────────────────────┐    │
    │    │ (*length)--                                    │    │
    │    │ element = buffer[*length]                      │    │
    │    │ spinner->Release()                            │    │
    │    │ return true  (success)                        │    │
    │    └───────────────────────────────────────────────┘    │
    │                                                          │
    │  element = default;                                     │
    │  spinner->Release()        ← unlock                     │
    │  return false  (pool empty)                             │
    └─────────────────────────────────────────────────────────┘


STEP-BY-STEP EXAMPLE
=====================

    Initial state: capacity=4, *length=0

    ┌─────┬─────┬─────┬─────┐
    │  ?  │  ?  │  ?  │  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
      ^length = 0 (empty pool)

    Step 1: TryAdd(objA)  →  buffer[0] = objA, length=1
    ┌─────┬─────┬─────┬─────┐
    │ objA│  ?  │  ?  │  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
             ^length = 1

    Step 2: TryAdd(objB)  →  buffer[1] = objB, length=2
    ┌─────┬─────┬─────┬─────┐
    │ objA│ objB│  ?  │  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
                   ^length = 2

    Step 3: TryAdd(objC)  →  buffer[2] = objC, length=3
    ┌─────┬─────┬─────┬─────┐
    │ objA│ objB│ objC│  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
                         ^length = 3

    Step 4: TryGet()  →  length--, return objC (LIFO!)
    ┌─────┬─────┬─────┬─────┐
    │ objA│ objB│ objC│  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
                   ^length = 2
    returns: objC (last in, first out)

    Step 5: TryGet()  →  length--, return objB
    ┌─────┬─────┬─────┬─────┐
    │ objA│ objB│ objC│  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
             ^length = 1
    returns: objB

    Step 6: TryAdd(objD)  →  buffer[1] = objD, length=2
    ┌─────┬─────┬─────┬─────┐
    │ objA│ objD│ objC│  ?  │
    └─────┴─────┴─────┴─────┘
      [0]   [1]   [2]   [3]
                   ^length = 2
    (slot [1] overwritten with objD, objC is stale but doesn't matter)


THREAD SAFETY - SPINLOCK
=========================

    ┌─────────────────────────────────────────────────────────┐
    │                                                         │
    │  Thread A: TryAdd(obj)        Thread B: TryGet(out obj) │
    │  ┌─────────────────┐         ┌─────────────────┐       │
    │  │ spinner.Acquire()│         │ spinner.Acquire()│       │
    │  │  ...SPIN...      │         │  ...SPIN...     │       │
    │  │  GOT LOCK        │         │  waiting...     │       │
    │  │ buffer[l] = obj  │         │                 │       │
    │  │ length++         │         │                 │       │
    │  │ spinner.Release()│──lock──►│  GOT LOCK       │       │
    │  │ return true      │         │ obj = buffer[l] │       │
    │  └─────────────────┘         │ length--        │       │
    │                               │ spinner.Release()│       │
    │                               │ return true     │       │
    │                               └─────────────────┘       │
    │                                                         │
    │  SpinLock: lightweight, no allocation,                   │
    │  busy-wait for short critical sections                  │
    └─────────────────────────────────────────────────────────┘


CAPACITY CALCULATION
====================

    GetCapacity(capacity):
    ┌───────────────────────────────────────────────────┐
    │ capacity = max(capacity, CacheLineSize / sizeof(T))│
    │ capacity = ceilpow2(capacity)                     │
    │                                                   │
    │ Examples (CacheLineSize = 64):                    │
    │                                                   │
    │   T = int (4 bytes):                              │
    │     min = 64/4 = 16                               │
    │     ceilpow2(16) = 16                             │
    │                                                   │
    │   T = UnsafeList<int> (16 bytes):                 │
    │     min = 64/16 = 4                               │
    │     ceilpow2(4) = 4                               │
    │                                                   │
    │   Input 8, T = byte:                              │
    │     min = 64/1 = 64                               │
    │     ceilpow2(64) = 64                             │
    └───────────────────────────────────────────────────┘


USAGE WITH UnsafeListPool
==========================

    UnsafeListPool<T> wraps UnmanagedPool<UnsafeList<T>>:

    ┌─────────────────────────────────────────────────────┐
    │  UnsafeListPool<T>                                  │
    │                                                     │
    │  pool: UnmanagedPool<UnsafeList<T>>                 │
    │                                                     │
    │  GetOrCreate(minCapacity, allocator):               │
    │    if TryGet(list): return list   ← recycled!      │
    │    else: return new UnsafeList<T>(minCapacity, alloc)│
    │                                                     │
    │  ReturnOrDispose(list):                             │
    │    if TryAdd(list): return     ← returned to pool   │
    │    else: list.Dispose()        ← pool full, discard │
    │                                                     │
    │  Dispose():                                         │
    │    while TryGet(list): list.Dispose()               │
    │    pool.Dispose()                                   │
    └─────────────────────────────────────────────────────┘


MEMORY TRACKING
===============

    All allocations use UnsafeUtility.MallocTracked:
    ┌───────────────────────────────────────────────────┐
    │ buffer   = MallocTracked(sizeof(T)*capacity, ...) │
    │ length   = MallocTracked(sizeof(int), ...)        │
    │ spinner  = MallocTracked(sizeof(SpinLock), ...)   │
    │                                                   │
    │ All freed with UnsafeUtility.FreeTracked(...)     │
    └───────────────────────────────────────────────────┘


KEY PROPERTIES
==============

  * Type:              readonly struct UnmanagedPool<T> : IDisposable
  * Thread Safety:     YES (SpinLock protects all operations)
  * Data Structure:    Stack (LIFO) - array + length pointer
  * Capacity:          Fixed, power-of-2, minimum one cache line
  * TryAdd:            O(1) - push to stack (locked)
  * TryGet:            O(1) - pop from stack (locked)
  * SpinLock:          Lightweight busy-wait, ideal for short sections
  * Burst Compatible:  Yes
  * Generic:           T : unmanaged
  * Failure Mode:      Returns false (not exception) when full/empty
  * Tracking:          MallocTracked/FreeTracked (visible in profiler)
  * Use Case:          Object pooling for lists, buffers, reusable objects

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/memory-allocators/UnmanagedPool.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Mathematics"
> ```

```
BovineLabs.Core.Collections.UnmanagedPool<T>
  Kind: struct (ValueType=True)
  Size (T=int): 32 bytes
  Generic params: Int32

  Interfaces:
    System.IDisposable

  Constructors:
    .ctor(Int32 capacity, Allocator allocator)

  Properties:
    public Boolean IsCreated

  Methods:
    public Void Dispose()
    public Boolean TryAdd(Int32 element)
    public Boolean TryGet(Int32& element)

  Functional Tests:
    byte pool (capacity=8): TryAdd(42) = True
    byte pool: TryGet = True, value = 42
    byte pool full test: last add=True, overflow=False
    int pool LIFO: pop1=30
    int pool LIFO: pop2=20
    int pool LIFO: pop3=10
    int pool: empty TryGet=False

Verified: 15 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/UnmanagedPool.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnmanagedPool.cs)
- [BovineLabs.Core/Collections/UnsafeListPool.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafeListPool.cs)
- [BovineLabs.Core.Tests/Collections/UnsafeListPoolTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/UnsafeListPoolTests.cs)
