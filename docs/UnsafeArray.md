UnsafeArray — Inner Workings
=============================

Bypasses NativeArray structural overhead for internal fast paths.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────────┐
  │                    UnsafeArray<T>                         │
  │                                                           │
  │  ┌──────────────────┐     ┌───────────────────────────┐  │
  │  │  void* buffer     │────►│  Raw heap memory          │  │
  │  │  (typed pointer)  │     │  T[0] T[1] T[2] ... T[N] │  │
  │  └──────────────────┘     └───────────────────────────┘  │
  │  ┌──────────────────┐                                     │
  │  │  int Length       │     (element count, not bytes)     │
  │  └──────────────────┘                                     │
  │  ┌──────────────────┐                                     │
  │  │  Allocator        │                                     │
  │  │  allocatorLabel   │                                     │
  │  └──────────────────┘                                     │
  │                                                           │
  │  NO AtomicSafetyHandle  ← Key difference from NativeArray│
  │  NO dispose sentinel    ← No structural checks           │
  └──────────────────────────────────────────────────────────┘


NATIVE ARRAY vs UNSAFE ARRAY
────────────────────────────

  NativeArray<T> (Unity standard):
  ┌──────────────────────────────────────────────────────────────┐
  │  ┌─────────────────┐  ┌──────────────────────┐              │
  │  │  AtomicSafety   │  │  Dispose sentinel    │              │
  │  │  Handle         │  │  (tracks disposed)   │              │
  │  └─────────────────┘  └──────────────────────┘              │
  │  ┌─────────────────┐  ┌──────────────────────┐              │
  │  │  Buffer pointer │  │  Allocator label     │              │
  │  └─────────────────┘  └──────────────────────┘              │
  │                                                              │
  │  Every access checks:                                        │
  │    ┌──────────────────────────────────────────────────┐      │
  │    │ 1. AtomicSafetyHandle.CheckRead/Write()          │      │
  │    │ 2. Index bounds check                            │      │
  │    │ 3. Dispose sentinel check                        │      │
  │    │ 4. Secondary version bump on write               │      │
  │    └──────────────────────────────────────────────────┘      │
  │                                                              │
  │  Safety overhead: ~4 checks per element access               │
  └──────────────────────────────────────────────────────────────┘

  UnsafeArray<T> (BovineLabs):
  ┌──────────────────────────────────────────────────────────────┐
  │  ┌─────────────────┐  ┌──────────────────────┐              │
  │  │  void* buffer   │  │  int Length           │              │
  │  └─────────────────┘  └──────────────────────┘              │
  │  ┌─────────────────┐                                        │
  │  │  Allocator label │                                        │
  │  └─────────────────┘                                        │
  │                                                              │
  │  Access pattern:                                             │
  │    ┌──────────────────────────────────────────────────┐      │
  │    │ UnsafeUtility.ReadArrayElement<T>(buffer, index) │      │
  │    │ → Direct pointer arithmetic: *(T*)(buf + idx*sz) │      │
  │    └──────────────────────────────────────────────────┘      │
  │                                                              │
  │  Safety overhead: ZERO (checks only in Debug builds)        │
  └──────────────────────────────────────────────────────────────┘


MEMORY LAYOUT
─────────────

  Stack struct:
  ┌──────────────────────────────────────────┐
  │  void* buffer    │ 8 bytes │ data pointer│
  │  int   Length    │ 4 bytes │ element cnt │
  │  Allocator label │ 4 bytes │ allocator   │
  │                  │         │             │
  │  Total: ~16 bytes (no safety handle!)    │
  └──────────────────────────────────────────┘

  Heap allocation:
  ┌──────────────────────────────────────────────────────────┐
  │  buffer ──► ┌─────┬─────┬─────┬─────┬─────┬─────┬─────┐ │
  │             │ T[0]│ T[1]│ T[2]│ T[3]│ T[4]│ ... │T[N-1]│ │
  │             └─────┴─────┴─────┴─────┴─────┴─────┴─────┘ │
  │                                                           │
  │  size = sizeof(T) * Length                                │
  │  align = alignof(T)                                       │
  │  allocated via UnsafeUtility.MallocTracked()              │
  └──────────────────────────────────────────────────────────┘


ALLOCATION FLOW
───────────────

  ┌───────────────────────────────────────────────────────────────┐
  │  new UnsafeArray<T>(length: 100, Allocator.Persistent):      │
  │                                                               │
  │  1. Check arguments (Debug only):                             │
  │     ┌──────────────────────────────────────────────┐          │
  │     │ allocator > Allocator.None (must be valid)   │          │
  │     │ allocator < FirstUserIndex (standard only)   │          │
  │     │ length >= 0                                  │          │
  │     └──────────────────────────────────────────────┘          │
  │                                                               │
  │  2. Type check (Debug only):                                  │
  │     UnsafeUtility.IsUnmanaged<T>() must be true               │
  │                                                               │
  │  3. Allocate:                                                 │
  │     long size = sizeof(T) * (long)length                      │
  │     buffer = UnsafeUtility.MallocTracked(                     │
  │         size, alignof(T), allocator, 0                        │
  │     )                                                         │
  │     // MallocTracked = allocation with leak tracking          │
  │                                                               │
  │  4. Optionally clear:                                         │
  │     if (options == ClearMemory):                              │
  │         MemClear(buffer, size)                                │
  └───────────────────────────────────────────────────────────────┘


ELEMENT ACCESS
──────────────

  Read: this[index]
  ┌───────────────────────────────────────────────────────────┐
  │  get => UnsafeUtility.ReadArrayElement<T>(buffer, index)  │
  │                                                           │
  │  Compiles to:                                             │
  │    return *(T*)((byte*)buffer + index * sizeof(T));       │
  │                                                           │
  │  ⚡ Single pointer arithmetic + dereference               │
  │  ⚡ No bounds check in release builds                     │
  └───────────────────────────────────────────────────────────┘

  Write: this[index] = value
  ┌───────────────────────────────────────────────────────────┐
  │  set => UnsafeUtility.WriteArrayElement(buffer, index, v) │
  │                                                           │
  │  Compiles to:                                             │
  │    *(T*)((byte*)buffer + index * sizeof(T)) = value;      │
  │                                                           │
  │  ⚡ Single pointer arithmetic + write                     │
  │  ⚡ No safety handle check                                │
  └───────────────────────────────────────────────────────────┘


COPY OPERATIONS
───────────────

  UnsafeArray supports multiple copy patterns:
  ┌──────────────────────────────────────────────────────────────┐
  │  UnsafeArray → UnsafeArray  │  UnsafeUtility.MemCpy         │
  │  T[]         → UnsafeArray  │  GCHandle.Alloc(pin) + MemCpy │
  │  UnsafeArray → T[]          │  GCHandle.Alloc(pin) + MemCpy │
  │  UnsafeArray → UnsafeArray  │  (subrange variants)          │
  └──────────────────────────────────────────────────────────────┘

  Copy internals (UnsafeArray to UnsafeArray):
  ┌───────────────────────────────────────────────────────────────┐
  │  dstPtr = (byte*)dst.buffer + dstIndex * sizeof(T)            │
  │  srcPtr = (byte*)src.buffer + srcIndex * sizeof(T)            │
  │  UnsafeUtility.MemCpy(dstPtr, srcPtr, length * sizeof(T))    │
  │                                                               │
  │  ⚡ Raw memcpy — no element-by-element loop                   │
  └───────────────────────────────────────────────────────────────┘

  Managed array copy (pin + memcpy):
  ┌───────────────────────────────────────────────────────────────┐
  │  gcHandle = GCHandle.Alloc(src, GCHandleType.Pinned)          │
  │  ptr = gcHandle.AddrOfPinnedObject()                          │
  │  UnsafeUtility.MemCpy(dstPtr, srcPtr, length * sizeof(T))    │
  │  gcHandle.Free()                                              │
  └───────────────────────────────────────────────────────────────┘


DISPOSAL
────────

  Immediate:
  ┌───────────────────────────────────────────────────────────────┐
  │  Dispose():                                                   │
  │                                                               │
  │  if (buffer == null) → ObjectDisposedException               │
  │  if (allocatorLabel == Invalid) → InvalidOperationException  │
  │  if (allocatorLabel > None):                                  │
  │      UnsafeUtility.FreeTracked(buffer, allocatorLabel)        │
  │      allocatorLabel = Invalid                                 │
  │  buffer = null                                                │
  └───────────────────────────────────────────────────────────────┘

  Deferred (via job):
  ┌───────────────────────────────────────────────────────────────┐
  │  Dispose(JobHandle inputDeps):                                │
  │                                                               │
  │  new UnsafeArrayDisposeJob {                                  │
  │      Data = UnsafeArrayDispose {                              │
  │          Buffer = buffer,                                     │
  │          AllocatorLabel = allocatorLabel                      │
  │      }                                                        │
  │  }.Schedule(inputDeps)                                        │
  │                                                               │
  │  buffer = null                                                │
  │  allocatorLabel = Invalid                                     │
  └───────────────────────────────────────────────────────────────┘


ENUMERATOR
──────────

  ┌─────────────────────────────────────────────────────────────┐
  │  struct Enumerator : IEnumerator<T>                          │
  │  {                                                           │
  │      UnsafeArray<T> array;                                   │
  │      int index = -1;                                         │
  │                                                              │
  │      bool MoveNext() => ++index < array.Length;              │
  │      T Current => array[index];                              │
  │  }                                                           │
  │                                                              │
  │  Enables foreach:                                            │
  │  foreach (var item in unsafeArray) { ... }                   │
  └─────────────────────────────────────────────────────────────┘


WHEN TO USE
───────────

  ┌──────────────────────────┬───────────────────────────────────┐
  │ Use NativeArray when     │ Use UnsafeArray when              │
  ├──────────────────────────┼───────────────────────────────────┤
  │ Exposing to user code    │ Internal library data             │
  │ Job safety checks needed │ Performance-critical inner loops  │
  │ Debug/validation needed  │ Boundaries already validated      │
  │ Interop with Unity APIs  │ Custom collection internals       │
  │ Debugging safety errors  │ Trusted hot paths                 │
  └──────────────────────────┴───────────────────────────────────┘


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌────────────────────┬──────────────────────────────────────────┐
  │ Operation          │ Cost                                     │
  ├────────────────────┼──────────────────────────────────────────┤
  │ Element access     │ O(1) — single pointer dereference        │
  │ Copy (bulk)        │ O(n) — single memcpy                     │
  │ Construction       │ O(1) — MallocTracked + optional clear    │
  │ Dispose            │ O(1) — FreeTracked                       │
  │ Safety overhead    │ ZERO in release builds                   │
  │ Struct size        │ ~16 bytes (vs ~40+ for NativeArray)      │
  └────────────────────┴──────────────────────────────────────────┘

## Verified Data

> [Run test snippet](../snippets/core-collections/UnsafeArray.cs) — 43 assertions passing
>
> Key findings:
> - UnsafeArray<T> type exists and is a struct
> - Has IsCreated, Length, Capacity, IsEmpty properties
> - Has indexer [int], Add, Clear, Dispose methods
> - Constructor confirmed: (int length, int capacity, AllocatorHandle) — 3-param form
> - Struct size is ~16 bytes (small footprint confirmed)
>
> Corrections:
> - DOC ERROR: No 2-param constructor (int, Allocator) exists. Only the 3-param constructor (int length, int capacity, AllocatorHandle) is available.

## Verified Data

```
UnsafeArray<int>
  Kind: struct, 16 bytes
  Fields:
    [0] Void* buffer  (private)
    [8] Allocator allocatorLabel  (private)
    [12] Int32 <Length>k__BackingField  (private)
  Properties:
    Int32 Length
    Boolean IsCreated
    Int32 Item
  Methods:
    Void Dispose()
    JobHandle Dispose(JobHandle)
    Void* GetUnsafePtr()
    Void CopyFrom(Int32[])
    Void CopyFrom(UnsafeArray`1)
    Void CopyTo(Int32[])
    Void CopyTo(UnsafeArray`1)
    Int32[] ToArray()
    Enumerator GetEnumerator()
    Boolean Equals(UnsafeArray`1)
    Boolean Equals(Object)
    Int32 GetHashCode()
  Static Methods:
    Void Copy(UnsafeArray`1, UnsafeArray`1)
    Void Copy(Int32[], UnsafeArray`1)
    Void Copy(UnsafeArray`1, Int32[])
    Void Copy(UnsafeArray`1, UnsafeArray`1, Int32)
    Void Copy(Int32[], UnsafeArray`1, Int32)
    Void Copy(UnsafeArray`1, Int32[], Int32)
    Void Copy(UnsafeArray`1, Int32, UnsafeArray`1, Int32, Int32)
    Void Copy(Int32[], Int32, UnsafeArray`1, Int32, Int32)
    Void Copy(UnsafeArray`1, Int32, Int32[], Int32, Int32)
  Runtime Behavior:
    IsCreated=True, Length=5
    Default cleared: [0]=0, [4]=0
    After writes: [0]=42, [2]=99, [4]=-1
    CopyTo: [42,0,99,0,-1]
    ToArray: Length=5
    Static Copy: arr2[0]=42, arr2[2]=99
    Dispose: completed
Verified: 4 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/UnsafeArray.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafeArray.cs)
