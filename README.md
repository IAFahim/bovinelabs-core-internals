NativeCounter — Inner Workings
===============================

Provides a Burst-compatible thread-safe counter for parallel jobs.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────┐
  │                  NativeCounter                        │
  │                                                       │
  │  ┌─────────────────┐     ┌────────────────────────┐  │
  │  │   int* count     │────►│   Heap-allocated int   │  │
  │  │   (raw pointer)  │     │   ┌────────────────┐   │  │
  │  └─────────────────┘     │   │    value: 0     │   │  │
  │                           │   └────────────────┘   │  │
  │  ┌─────────────────┐     └────────────────────────┘  │
  │  │   allocator     │                                  │
  │  └─────────────────┘                                  │
  │                                                       │
  │  [Safety: AtomicSafetyHandle m_Safety]                │
  └──────────────────────────────────────────────────────┘


MEMORY LAYOUT
─────────────

  NativeCounter struct (on stack or in job struct):
  ┌────────────────────────────────────────────────────┐
  │  Offset  │  Field         │  Size  │  Description  │
  ├──────────┼────────────────┼────────┼───────────────┤
  │  0x00    │  int* count    │  8     │ Ptr to value  │
  │  0x08    │  allocator     │  ~2    │ Alloc handle  │
  │  (0x0A)  │  m_Safety      │  ~16   │ [Debug only]  │
  └────────────────────────────────────────────────────┘

  Heap allocation (4 bytes):
  ┌────────┐
  │  int   │  ← count points here
  │  value │
  └────────┘


TWO MODES: SINGLE-THREAD vs PARALLEL
─────────────────────────────────────

  ┌─────────────────────────────────────────────────────────────┐
  │                    NativeCounter (main thread)               │
  │                                                              │
  │   Increment()  ─────►  (*count)++   ← NOT atomic            │
  │                          │                                   │
  │                          │ Simple pointer dereference + inc  │
  │                          │ No Interlocked overhead           │
  │                          ▼                                   │
  │                    ┌──────────┐                              │
  │                    │ *count++ │  Fast path for single-thread │
  │                    └──────────┘                              │
  └─────────────────────────────────────────────────────────────┘

  ┌─────────────────────────────────────────────────────────────┐
  │              ParallelWriter (worker threads)                 │
  │                                                              │
  │   Increment()  ─────►  Interlocked.Increment(ref *count)    │
  │                          │                                   │
  │                          │ CPU atomic operation              │
  │                          │ Full memory barrier               │
  │                          │ Thread-safe increment             │
  │                          ▼                                   │
  │                    ┌────────────────────────────┐           │
  │                    │ Interlocked.Increment(ref)  │           │
  │                    │ Returns NEW value           │           │
  │                    └────────────────────────────┘           │
  └─────────────────────────────────────────────────────────────┘


PARALLEL WRITER CONVERSION
──────────────────────────

  ┌───────────────────┐         ┌───────────────────────┐
  │   NativeCounter   │         │    ParallelWriter     │
  │                   │  AsPar- │                       │
  │  int* count  ─────┤ allelW- ├─► int* count (copy)   │
  │                   │ riter() │                       │
  │  m_Safety   ──────┤         ├─► m_Safety (secondary)│
  │                   │         │    [NativeContainer    │
  │                   │         │     IsAtomicWriteOnly] │
  └───────────────────┘         └───────────────────────┘

  The ParallelWriter is marked [NativeContainerIsAtomicWriteOnly].
  This tells the job system:
    ┌─ Allow concurrent writes from multiple jobs ─────────┐
    │  (no secondary-version bumping needed for atomics)   │
    └──────────────────────────────────────────────────────┘


THREAD SAFETY MODEL
───────────────────

  Main thread (no parallel):
  ┌────────┐    ┌─────────┐    ┌─────────┐
  │ Thread │───►│ (*cnt)++│───►│ (*cnt)++│   ← No sync needed
  │  A     │    └─────────┘    └─────────┘
  └────────┘

  Parallel jobs:
  ┌────────┐    ┌───────────────────────┐
  │Worker 0│───►│ Interlocked.Increment │───┐
  └────────┘    └───────────────────────┘   │
  ┌────────┐    ┌───────────────────────┐   │   ┌──────────┐
  │Worker 1│───►│ Interlocked.Increment │───┼──►│  count*  │
  └────────┘    └───────────────────────┘   │   │ (atomic) │
  ┌────────┐    ┌───────────────────────┐   │   └──────────┘
  │Worker 2│───►│ Interlocked.Increment │───┘
  └────────┘    └───────────────────────┘
                       │
          Hardware-level LOCK prefix on x86
          Ensures all increments are serialized


ALLOCATION AND DISPOSAL
────────────────────────

  Construction:
  ┌──────────────────────────────────────────────────────────────┐
  │ NativeCounter(allocator)                                     │
  │  ├─ count = Memory.Unmanaged.Allocate<int>(allocator)        │
  │  ├─ MemClear(count, sizeof(int))        → value starts at 0  │
  │  └─ [Debug] Create AtomicSafetyHandle                        │
  └──────────────────────────────────────────────────────────────┘

  Disposal:
  ┌──────────────────────────────────────────────────────────────┐
  │ Dispose()                                                    │
  │  ├─ [Debug] DisposeSafetyHandle                              │
  │  ├─ Memory.Unmanaged.Free(count, allocator)                  │
  │  └─ count = null                                             │
  └──────────────────────────────────────────────────────────────┘


INCREMENT FLOW DETAIL
─────────────────────

  NativeCounter.Increment() (single-thread):
  ┌────────────────────────────────────┐
  │ [Safety] CheckWriteAndThrow()      │
  │ int x = *count;                    │
  │ int x1 = x + 1;                   │
  │ *count = x1;                       │
  │ return x1;                         │
  │                                    │
  │ ⚡ 3 memory ops: read, add, write  │
  │ ⚡ No barrier — fastest path       │
  └────────────────────────────────────┘

  ParallelWriter.Increment() (multi-thread):
  ┌────────────────────────────────────┐
  │ [Safety] CheckWriteAndThrow()      │
  │ return Interlocked.Increment(      │
  │     ref *count                     │
  │ );                                 │
  │                                    │
  │ ⚡ 1 atomic RMW operation          │
  │ ⚡ Full memory barrier             │
  │ ⚡ Returns new value               │
  └────────────────────────────────────┘


COUNT PROPERTY
──────────────

  ┌──────────┐     ┌─────────────────────────┐
  │ Getter   │────►│ Safety check + *count    │
  └──────────┘     └─────────────────────────┘
  ┌──────────┐     ┌─────────────────────────┐
  │ Setter   │────►│ Safety check + *count = v│
  └──────────┘     └─────────────────────────┘


TYPICAL USAGE IN JOBS
──────────────────────

  ┌──────────────────────────────────────────────────────────┐
  │ var counter = new NativeCounter(Allocator.TempJob);      │
  │                                                          │
  │ // Single-thread increment                               │
  │ counter.Increment();                                     │
  │                                                          │
  │ // Pass to parallel jobs                                 │
  │ var writer = counter.AsParallelWriter();                 │
  │                                                          │
  │ // Inside IJobParallelFor:                               │
  │ writer.Increment();  // Thread-safe via Interlocked      │
  │                                                          │
  │ // Read result                                           │
  │ int total = counter.Count;                               │
  │ counter.Dispose();                                       │
  └──────────────────────────────────────────────────────────┘
