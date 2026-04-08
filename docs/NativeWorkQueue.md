NativeWorkQueue — Inner Workings
==================================

Thread-safe lock-free queue for deferred job scheduling and load balancing.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────────────────┐
  │                     NativeWorkQueue<T>                           │
  │                                                                  │
  │  ┌───────────────┐  ┌───────────────┐  ┌───────────────────┐   │
  │  │ T* queue      │  │ int*          │  │ int*              │   │
  │  │ (data array)  │  │ queueWriteHead│  │ queueReadHead     │   │
  │  │               │  │ (write index) │  │ (read index)      │   │
  │  │ T[0] T[1]..   │  │               │  │                   │   │
  │  └───────────────┘  └───────────────┘  └───────────────────┘   │
  │  ┌───────────────┐  ┌───────────────────────────────────────┐  │
  │  │ int*          │  │ int Capacity                          │  │
  │  │ currentRef    │  │ (max queue size, fixed at creation)   │  │
  │  │ (unique IDs)  │  └───────────────────────────────────────┘  │
  │  └───────────────┘                                              │
  │                                                                  │
  │  [Safety: AtomicSafetyHandle m_Safety]                          │
  └──────────────────────────────────────────────────────────────────┘


MEMORY LAYOUT
─────────────

  Four separate heap allocations:
  ┌──────────────────────────────────────────────────────────────────┐
  │                                                                  │
  │  1. queue: T[Capacity]                                          │
  │     ┌──────┬──────┬──────┬──────┬──────┬──────┬───┬──────┐      │
  │     │ T[0] │ T[1] │ T[2] │ T[3] │ T[4] │ T[5] │...│T[Cap]│      │
  │     └──────┴──────┴──────┴──────┴──────┴──────┴───┴──────┘      │
  │                                                                  │
  │  2. queueWriteHead: int (atomic counter)                        │
  │     ┌───────────┐                                                │
  │     │  writeIdx │  ← Interlocked.Increment for parallel writes  │
  │     └───────────┘                                                │
  │                                                                  │
  │  3. queueReadHead: int (atomic counter)                         │
  │     ┌──────────┐                                                 │
  │     │  readIdx │  ← Interlocked.Increment for parallel reads    │
  │     └──────────┘                                                 │
  │                                                                  │
  │  4. currentRef: int (unique ID generator)                       │
  │     ┌────────────┐                                               │
  │     │  nextRefId │  ← Interlocked.Increment, never 0            │
  │     └────────────┘                                               │
  └──────────────────────────────────────────────────────────────────┘


PRODUCER-CONSUMER PATTERN
─────────────────────────

  Writers (ParallelWriter):               Readers (ParallelReader):
  ┌─────────┐ ┌─────────┐ ┌─────────┐   ┌─────────┐ ┌─────────┐
  │ Worker 0│ │ Worker 1│ │ Worker 2│   │ Worker 0│ │ Worker 1│
  │  Add()  │ │  Add()  │ │  Add()  │   │ GetNext │ │ GetNext │
  └────┬────┘ └────┬────┘ └────┬────┘   └────┬────┘ └────┬────┘
       │           │           │              │           │
       ▼           ▼           ▼              ▼           ▼
  ┌────────────────────────────────┐  ┌───────────────────────────┐
  │ Interlocked.Increment(         │  │ Interlocked.Increment(    │
  │   ref *queueWriteHead)         │  │   ref *queueReadHead)     │
  │ Returns unique slot index      │  │ Returns unique read index │
  └─────────────┬──────────────────┘  └─────────────┬─────────────┘
                │                                   │
                ▼                                   ▼
  ┌───────────────────────────────────────────────────────────────┐
  │                          T[] queue                            │
  │  ┌──────┬──────┬──────┬──────┬──────┬──────┬───┬──────┐      │
  │  │ T[0] │ T[1] │ T[2] │ T[3] │ T[4] │ T[5] │...│T[Cap]│      │
  │  └──────┴──────┴──────┴──────┴──────┴──────┴───┴──────┘      │
  │  ▲                                         ▲                  │
  │  │ Writers claim slots via atomic inc      │ Capacity limit   │
  └───────────────────────────────────────────────────────────────┘


PARALLEL WRITER — TryAdd FLOW
─────────────────────────────

  ┌───────────────────────────────────────────────────────────────┐
  │  ParallelWriter.TryAdd(out T* ptr):                           │
  │                                                               │
  │  1. Claim a slot atomically:                                  │
  │     idx = Interlocked.Increment(ref *queueWriteHead) - 1      │
  │                            ↑                                   │
  │     Thread-safe! No two threads get same index.               │
  │                                                               │
  │  2. Capacity check:                                           │
  │     if (idx >= Capacity):                                     │
  │         ptr = null                                            │
  │         return 0  ← Queue full, try again next frame         │
  │                                                               │
  │  3. Generate unique reference ID:                             │
  │     do {                                                      │
  │         queueRef = Interlocked.Increment(ref *currentRef)     │
  │     } while (queueRef == 0);  ← 0 is reserved sentinel       │
  │                                                               │
  │  4. Return:                                                   │
  │     ptr = queue + idx  ← Pointer to claimed slot              │
  │     return queueRef    ← Unique ID for tracking               │
  │                                                               │
  │  ⚡ Lock-free: only atomic increments, no CAS loops           │
  └───────────────────────────────────────────────────────────────┘


PARALLEL READER — TryGetNext FLOW
─────────────────────────────────

  ┌───────────────────────────────────────────────────────────────┐
  │  ParallelReader.TryGetNext(out T* value):                     │
  │                                                               │
  │  1. Claim a read position atomically:                         │
  │     idx = Interlocked.Increment(ref *queueReadHead) - 1       │
  │                            ↑                                   │
  │     Each reader gets a unique index.                          │
  │                                                               │
  │  2. Bounds check:                                             │
  │     effectiveLength = min(Capacity, *queueWriteHead)          │
  │                                                               │
  │     if (idx >= effectiveLength):                               │
  │         value = null                                          │
  │         return false  ← All work consumed                     │
  │                                                               │
  │  3. Return:                                                   │
  │     value = queue + idx  ← Pointer to work item              │
  │     return true                                               │
  │                                                               │
  │  ⚡ Natural load balancing: fast threads get more work        │
  └───────────────────────────────────────────────────────────────┘


UPDATE / RESET CYCLE
────────────────────

  The queue is designed for frame-based usage:

  Frame N:                          Frame N+1:
  ┌──────────────────────┐         ┌──────────────────────┐
  │ Update():            │         │ Update():            │
  │  *writeHead = 0      │────────►│  *writeHead = 0      │
  │  *readHead  = 0      │         │  *readHead  = 0      │
  │                      │         │                      │
  │ Workers Add() items  │         │ Workers Add() items  │
  │ Workers GetNext()    │         │ Workers GetNext()    │
  │                      │         │                      │
  │ [some items may be   │         │ Items from Frame N   │
  │  overflowed if full] │         │ that didn't fit are  │
  └──────────────────────┘         │ lost! (by design)    │
                                   └──────────────────────┘

  ┌───────────────────────────────────────────────────────────────┐
  │  Update can be done via job:                                   │
  │                                                               │
  │  new UpdateNativeWorkQueueJob {                               │
  │      QueueWriteHead,                                          │
  │      QueueReadHead                                            │
  │  }.Schedule(handle)                                           │
  │                                                               │
  │  Execute(): *writeHead = 0; *readHead = 0;                    │
  └───────────────────────────────────────────────────────────────┘


UNIQUE ID GENERATION
────────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  currentRef is a monotonically increasing counter:           │
  │                                                              │
  │  Each Add() generates a unique queueRef:                     │
  │    do {                                                      │
  │        queueRef = Interlocked.Increment(ref *currentRef)     │
  │    } while (queueRef == 0);                                  │
  │                                                              │
  │  Why avoid 0?                                                │
  │    0 is used as "invalid" sentinel in TryAdd return value    │
  │    (returns 0 when queue is full)                            │
  │                                                              │
  │  The counter wraps around at int.MaxValue → int.MinValue,   │
  │  and skips 0 on the way. Practical limit: ~2 billion items  │
  │  before wrap.                                                │
  └──────────────────────────────────────────────────────────────┘


SINGLE-THREAD vs PARALLEL MODES
───────────────────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  NativeWorkQueue (single-thread):                            │
  │    *writeHead += 1              (non-atomic increment)       │
  │    currentRef: ++*currentRef    (non-atomic increment)       │
  │                                                              │
  │  ParallelWriter (multi-thread):                              │
  │    Interlocked.Increment(writeHead)  (atomic)                │
  │    Interlocked.Increment(currentRef) (atomic)                │
  │                                                              │
  │  ParallelReader (multi-thread):                              │
  │    Interlocked.Increment(readHead)   (atomic)                │
  └──────────────────────────────────────────────────────────────┘


LENGTH SEMANTICS
────────────────

  ┌──────────────────────────────────────────────────────────────┐
  │  Length = min(Capacity, *queueWriteHead)                     │
  │                                                              │
  │  Why min()?                                                  │
  │  When queue overflows, writeHead > Capacity.                 │
  │  Those overflow items are NOT written (ptr = null).          │
  │  So effective length is clamped to Capacity.                 │
  │                                                              │
  │  Example:                                                    │
  │    Capacity = 10                                             │
  │    Workers try to add 15 items                               │
  │    writeHead = 15 (atomic inc happened)                     │
  │    But only items 0..9 were actually written                 │
  │    Length = min(10, 15) = 10                                 │
  └──────────────────────────────────────────────────────────────┘


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌──────────────────────┬─────────────────────────────────────────┐
  │ Operation            │ Cost                                    │
  ├──────────────────────┼─────────────────────────────────────────┤
  │ TryAdd (parallel)    │ O(1) — 2 Interlocked.Increment          │
  │ TryGetNext (parallel)│ O(1) — 1 Interlocked.Increment          │
  │ Update (reset)       │ O(1) — zero 2 counters                  │
  │ Lock-free            │ YES — no CAS loops, no spinlocks        │
  │ Memory overhead      │ 3 ints + T[Capacity]                    │
  │ Load balancing       │ Automatic — work-stealing via atomics   │
  └──────────────────────┴─────────────────────────────────────────┘

## Source

- [BovineLabs.Core/Collections/NativeWorkQueue.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/NativeWorkQueue.cs)
