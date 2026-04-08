# IJobForThread - Inner Workings

## Overview

`IJobForThread` divides a fixed range of iterations across a known number of worker threads. Unlike
`IJobParallelFor` which divides by item count with arbitrary batch sizes, `IJobForThread` explicitly
partitions work by thread count. Each thread receives a contiguous, evenly-sized slice of the total
range, with the last thread picking up any remainder.

This is ideal when you need exactly N threads doing work (e.g., one thread per physics island, one
thread per spatial partition), rather than letting the job scheduler decide the split.

---

## Source

`BovineLabs.Core/Jobs/IJobForThread.cs`

---

## Interface & Producer

```
+------------------------------------------------------------------+
|                     IJobForThread  (interface)                    |
+------------------------------------------------------------------+
|  void Execute(int index)                                          |
+------------------------------------------------------------------+
            |
            |  [JobProducerType(typeof(JobThreadStruct<>))]
            v
+------------------------------------------------------------------+
|                   JobForThread  (static helper)                   |
+------------------------------------------------------------------+
|  ScheduleParallel<T>(jobData, arrayLength, threadCount, dep)      |
+------------------------------------------------------------------+
            |
            v
+------------------------------------------------------------------+
|             JobThreadStruct<T>  (internal producer)               |
+------------------------------------------------------------------+
|  T JobData          user job instance                             |
|  int Length          total iteration count                        |
|  int Threads         number of logical threads to divide across   |
+------------------------------------------------------------------+
```

---

## Scheduling Flow

```
 User Code                                  Unity Job System
======================================================================

 job.ScheduleParallel(
     arrayLength: 1000,
     threadCount: 4,
     dependency: handle
 )
      |
      v
 threadCount = math.max(1, threadCount)    // guard against 0
      |
      v
 Build JobThreadStruct<T>
   { JobData, Length=1000, Threads=4 }
      |
      v
 JobsUtility.ScheduleParallelFor(
     ref scheduleParams,
     arrayLength = threadCount (4),      // <-- KEY: only 4 "work items"
     batchSize   = 1
 )
      |
      v
 +-----------------------------------------------------------+
 |           Unity Work-Stealing Scheduler                    |
 |                                                            |
 |   Work items:  [0]  [1]  [2]  [3]                         |
 |               (one per requested thread)                   |
 |                                                            |
 |   Thread A steals [0]                                      |
 |   Thread B steals [1]                                      |
 |   Thread C steals [2]                                      |
 |   Thread D steals [3]                                      |
 +-----------------------------------------------------------+
      |
      |  Execute() is called with workerIndex range
      v
 +-----------------------------------------------------------+
 |          JobThreadStruct.Execute()                         |
 |                                                           |
 |  while (true)                                             |
 |    GetWorkStealingRange -> beginWorkerIdx, endWorkerIdx   |
 |                                                           |
 |    perThread = Length / Threads   // 1000 / 4 = 250       |
 |                                                           |
 |    beginIndex = beginWorkerIdx * perThread                |
 |    endIndex   = endWorkerIdx   * perThread                |
 |                                                           |
 |    if (endWorkerIdx == Threads)                           |
 |       endIndex += Length % Threads   // remainder +0      |
 |                                                           |
 |    for index in [beginIndex, endIndex):                   |
 |      >>> JobData.Execute(index) <<<                       |
 +-----------------------------------------------------------+
```

---

## Work Division Diagram

### Example: 1000 items, 4 threads

```
 perThread = 1000 / 4 = 250
 remainder = 1000 % 4 = 0

 +--------+--------+--------+--------+--------+--------+---+------+
 |  idx 0 |  250   |  500   |  750   |        |        |        |
 +--------+--------+--------+--------+--------+--------+---+------+
      |         |         |         |
      v         v         v         v
 Thread 0    Thread 1   Thread 2   Thread 3
 [0..250)    [250..500) [500..750) [750..1000)
```

### Example: 103 items, 4 threads

```
 perThread = 103 / 4 = 25
 remainder = 103 % 4  = 3

 +-----+-----+-----+-----+---+---+---+
 |  0  | 25  | 50  | 75  |   |   |   |
 +-----+-----+-----+-----+---+---+---+
    |      |      |      |
    v      v      v      v
 Thread0  Thread1 Thread2 Thread3
 [0..25)  [25..50)[50..75)[75..103)
                          +-------+
                          | +3 extra
                          | (last thread
                          |  gets remainder)
                          +-------+
```

### Edge case: 5 items, 8 threads

```
 perThread = 5 / 8 = 0 (integer division)
 threadCount clamped to max(1, ...) = 8

 But perThread = 0 means:
   beginIndex = beginWorkerIdx * 0 = 0
   endIndex   = endWorkerIdx   * 0 = 0

 Only the LAST worker (endWorkerIdx == Threads) gets:
   endIndex += 5 % 8 = 5
   => processes [0..5)

 Other threads: [0..0) -> NO WORK
```

---

## Comparison: IJobForThread vs IJobParallelFor

```
 IJobParallelFor                        IJobForThread
 =================                      =================

 ScheduleParallelFor(                   ScheduleParallelFor(
   arrayLength: 1000,                     arrayLength: 1000,
   batchSize:   64,                       threadCount: 4,
   dependsOn)                             dependsOn)

 Scheduler sees:                         Scheduler sees:
   1000 items, batch=64                  4 "items", batch=1

 Work split by:                          Work split by:
   Batch count (dynamic)                  Thread count (explicit)

 64 jobs of ~16 items                   4 jobs of 250 items each
 (approximately)                        (exactly, per-thread math)

 Thread assignment:                      Thread assignment:
   Auto by scheduler                      Auto by scheduler,
                                          but 1:1 with requested count

 Guaranteed thread count: NO             Guaranteed thread count: YES
 (depends on available workers)
```

---

## Work Stealing Behavior

```
 Work items submitted to scheduler: threadCount items (NOT arrayLength)

 +-----------+-----------+-----------+-----------+
 | Worker 0  | Worker 1  | Worker 2  | Worker 3  |
 +-----------+-----------+-----------+-----------+

 GetWorkStealingRange maps:
   Worker 0 -> begin=0, end=1
   Worker 1 -> begin=1, end=2
   Worker 2 -> begin=2, end=3
   Worker 3 -> begin=3, end=4

 Inside Execute, the worker index is translated to array index:
   beginWorkerIdx * perThread  ->  real array start
   endWorkerIdx   * perThread  ->  real array end

 If a thread finishes early and steals another worker's range:
   Thread A finishes [0..1), steals [2..3)
   => processes [0..250) AND [500..750) in same Execute call
   => OnWorkerBegin/End NOT part of this interface (no hooks)
```

---

## Buffer Range Patching

```
 JobsUtility.PatchBufferMinMaxRanges(
     bufferRangePatchData,
     &fullData,
     beginIndex,         // start of this thread's slice
     endIndex - beginIndex  // count of items
 )

 This ensures safety checks on [NativeDisableContainerSafetyRestriction]
 containers are scoped to the correct range per thread.
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| `ScheduleParallelFor(threadCount, 1)` | The "array" the scheduler sees has threadCount elements; each maps to a contiguous real slice |
| `perThread = Length / Threads` integer division | Truncation is intentional; remainder goes to last thread via modulo check |
| `endWorkerIdx == Threads` check for remainder | Last worker always picks up slack, ensuring all items are processed exactly once |
| No `OnWorkerBegin/End` hooks | Simpler interface; use `IJobChunkWorkerBeginEnd` or `IJobParallelHashMapDefer` if you need per-thread setup |
| `threadCount = math.max(1, ...)` | Prevents zero-thread degenerate case when jobs preference disables worker threads |

## Source

- [BovineLabs.Core/Jobs/IJobForThread.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Jobs/IJobForThread.cs)
