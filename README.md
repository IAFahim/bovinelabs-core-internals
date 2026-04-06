PooledNativeList — Inner Workings
==================================

Eliminates NativeList allocation overhead by reusing TLS-based memory pools.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌─────────────────────────────────────────────────────────────────────────┐
  │                        SharedStatic<Data> Pool                         │
  │                   (Global, one per process/domain)                     │
  │  ╔═══════════════════════════════════════════════════════════════════╗  │
  │  ║                          Data Struct                            ║  │
  │  ║  ┌──────────────────────────────────────────────────────────┐   ║  │
  │  ║  │  Allocator (AllocatorHandle — Allocator.Domain)          │   ║  │
  │  ║  │  buffer → ThreadData[JobsUtility.ThreadIndexCount]       │   ║  │
  │  ║  └──────────────────────────────────────────────────────────┘   ║  │
  │  ╚═══════════════════════════════════════════════════════════════════╝  │
  └─────────────────────────────────────────────────────────────────────────┘

  ThreadData is cache-line sized (64 bytes) to prevent false sharing:

  ┌─────────────────── Cache Line (64 bytes) ───────────────────┐
  │  ThreadData                                                    │
  │  ┌──────────────────────────────────────────────────────────┐ │
  │  │  UnsafeList<NativeList<byte>> ThreadList                  │ │
  │  │  (max 8 entries per thread — MaxPoolSizePerThread)        │ │
  │  └──────────────────────────────────────────────────────────┘ │
  └───────────────────────────────────────────────────────────────┘


THREAD-LOCAL POOL LAYOUT
────────────────────────

  Thread 0                Thread 1                Thread N
  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐
  │ ThreadData   │       │ ThreadData   │       │ ThreadData   │
  │ ┌──────────┐ │       │ ┌──────────┐ │       │ ┌──────────┐ │
  │ │ThreadList│ │       │ │ThreadList│ │       │ │ThreadList│ │
  │ │ ┌──────┐ │ │       │ │ ┌──────┐ │ │       │ │ ┌──────┐ │ │
  │ │ │byte[]│ │ │       │ │ │byte[]│ │ │       │ │ │byte[]│ │ │
  │ │ ├──────┤ │ │       │ │ ├──────┤ │ │       │ │ │ ...  │ │ │
  │ │ │byte[]│ │ │       │ │ │ ...  │ │ │       │ │ └──────┘ │ │
  │ │ ├──────┤ │ │       │ │ └──────┘ │ │       │ └──────────┘ │
  │ │ │ ...  │ │ │       │ └──────────┘ │       └──────────────┘
  │ │ │(max8)│ │ │       └──────────────┘
  │ │ └──────┘ │ │
  │ └──────────┘ │
  └──────────────┘
       ↑ No lock contention — each thread has its own pool


LIFECYCLE: Make() and Dispose()
───────────────────────────────

  PooledNativeList<T>.Make()
  │
  ├─► Get ThreadList via JobsUtility.ThreadIndex
  │
  ├─► Pool empty?
  │     YES ─► new NativeList<T>(0, Allocator.Domain)
  │             ┌─────────────────────────────┐
  │             │ Fresh allocation from heap   │
  │             └─────────────────────────────┘
  │
  │     NO  ─► Pop last NativeList<byte> from pool
  │             ┌─────────────────────────────────────────────┐
  │             │ byteList = lp[^1]  (stack-like LIFO reuse)  │
  │             │ lp.RemoveAt(lp.Length - 1)                  │
  │             │                                             │
  │             │ Reinterpret cast:                           │
  │             │   NativeList<byte> ──► NativeList<T>        │
  │             │   via UnsafeUtility.As<>()                  │
  │             │                                             │
  │             │ Recalculate capacity:                       │
  │             │   byteList.Capacity / sizeof(T)             │
  │             └─────────────────────────────────────────────┘
  │
  └─► Replace AtomicSafetyHandle (new handle for job injection)
      └─► Return PooledNativeList<T> ready for use


  PooledNativeList<T>.Dispose()
  │
  ├─► list.Clear()
  │
  ├─► Convert back to NativeList<byte>
  │     capacity_in_bytes = list.Capacity * sizeof(T)
  │
  ├─► Pool has room? (Length < MaxPoolSizePerThread = 8)
  │     YES ─► lp.Add(byteList)    ← return to pool
  │     NO  ─► byteList.Dispose()  ← free memory
  │
  └─► list = default


TYPE ERASURE STRATEGY
─────────────────────

  All lists are stored as NativeList<byte> regardless of T.
  This allows sharing pools across different element types:

    ┌──────────────────────┐      ┌──────────────────────┐
    │ PooledNativeList<int> │      │PooledNativeList<float>│
    │  NativeList<int>      │      │  NativeList<float>    │
    └──────────┬───────────┘      └──────────┬───────────┘
               │                              │
               │  UnsafeUtility.As<>          │  UnsafeUtility.As<>
               ▼                              ▼
    ┌──────────────────────────────────────────────────────┐
    │           ThreadList: UnsafeList<NativeList<byte>>    │
    │  ┌─────────┐ ┌─────────┐ ┌─────────┐                │
    │  │ byte[64] │ │ byte[32] │ │ byte[128│  ...          │
    │  └─────────┘ └─────────┘ └─────────┘                │
    └──────────────────────────────────────────────────────┘

  On checkout:  capacity = byteList.Capacity / sizeof(T)
  On check-in:  capacity = list.Capacity * sizeof(T)


INITIALIZATION FLOW
───────────────────

  [RuntimeInitializeOnLoadMethod]
            │
            ▼
  PooledNativeList.Initialize()
            │
            ├─► Allocate ThreadData[ThreadIndexCount]
            │     (ThreadIndexCount = max worker threads + 1)
            │
            └─► For each thread i:
                  buffer[i].ThreadList = new UnsafeList<NativeList<byte>>(0, allocator)
                              │
                              └─► Starts empty, lists added on first Dispose()


KEY CONSTANTS
─────────────

  MaxPoolSizePerThread = 8
    └─ Caps pool growth per thread to prevent unbounded memory use

  JobsUtility.CacheLineSize = 64
    └─ Each ThreadData padded to a full cache line to prevent false sharing

  Allocator = Allocator.Domain
    └─ Long-lived allocation that survives across frames


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌──────────────────────┬────────────────────────────────────────────┐
  │ Operation            │ Cost                                       │
  ├──────────────────────┼────────────────────────────────────────────┤
  │ Make() (pool hit)    │ ~O(1) — pop from end of UnsafeList         │
  │ Make() (pool miss)   │ O(heap alloc) — standard NativeList alloc  │
  │ Dispose()            │ ~O(1) — clear + push to UnsafeList         │
  │ Thread access        │ O(1) — index by ThreadIndex                │
  │ Lock contention      │ ZERO — each thread has isolated pool       │
  │ Safety handle swap   │ O(1) per make/dispose                      │
  └──────────────────────┴────────────────────────────────────────────┘


USE CASE
────────

  For high-frequency temporary NativeList usage in Burst-compiled jobs
  where repeated allocation/deallocation would cause GC pressure and
  allocator overhead. The pool recycles buffers frame-over-frame.

    Typical pattern:
    ┌─────────────────────────────────────────┐
    │ var list = PooledNativeList<T>.Make();  │
    │ // ... use list.List in job ...         │
    │ list.Dispose(); // returns to TLS pool  │
  └─────────────────────────────────────────┘
