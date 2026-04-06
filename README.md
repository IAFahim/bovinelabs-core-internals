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
