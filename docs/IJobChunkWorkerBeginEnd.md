# IJobChunkWorkerBeginEnd - Inner Workings

## Overview

`IJobChunkWorkerBeginEnd` extends Unity's `IJobChunk` pattern with per-thread setup and teardown hooks.
Each worker thread that picks up work calls `OnWorkerBegin()` exactly once before processing any chunks,
and `OnWorkerEnd()` exactly once after all its stolen work ranges are exhausted. This enables thread-local
allocations, cached NativeContainer views, or per-thread accumulator state without a separate coordination job.

---

## Verified Data

```
default: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

`BovineLabs.Core/Jobs/IJobChunkWorkerBeginEnd.cs`

---

## Class Hierarchy

```
+------------------------------------------------------------------+
|                    IJobChunkWorkerBeginEnd                        |
|                      (user interface)                             |
+------------------------------------------------------------------+
|  void OnWorkerBegin()            <- default empty impl            |
|  void OnWorkerEnd()              <- default empty impl            |
|  void Execute(in ArchetypeChunk,  int, bool, in v128)            |
+------------------------------------------------------------------+
            |
            |  [JobProducerType(typeof(JobChunkProducer<>))]
            v
+------------------------------------------------------------------+
|          JobChunkWorkerBeginEndExtensions                         |
|                      (scheduling layer)                           |
+------------------------------------------------------------------+
|  Schedule()            -> ScheduleInternal(Single)                |
|  ScheduleByRef()       -> ScheduleInternal(Single)                |
|  ScheduleParallel()    -> ScheduleInternal(Parallel)              |
|  ScheduleParallelByRef()                                         |
|  Run()                 -> ScheduleInternal(Run)                   |
|  RunByRef()                                                    |
|  RunByRefWithoutJobs() <- bypasses job system entirely            |
+------------------------------------------------------------------+
            |
            v
+------------------------------------------------------------------+
|          JobChunkWrapper<T>  (internal state bag)                 |
+------------------------------------------------------------------+
|  T JobData                           user job struct              |
|  UnsafeMatchingArchetypePtrList      archetype match list         |
|  UnsafeCachedChunkList               cached chunk list            |
|  EntityQueryFilter                   query filter                 |
|  int IsParallel                      1=parallel, 0=single         |
|  int QueryHasEnableableComponents    flag for enableables         |
+------------------------------------------------------------------+
```

---

## Scheduling Flow

```
 User Code                          BovineLabs                     Unity Job System
===========================================================================

 var job = new MyJob();
 job.ScheduleParallel(query, dep)
        |
        v
 ScheduleInternal(ref jobData, query, dep, Parallel, default)
        |
        +---> queryImpl->GetMatchingChunkCache()
        |         |
        |         v
        |     cachedChunks  (length = totalChunkCount)
        |
        +---> Build JobChunkWrapper<T>
        |         |
        |         v
        |     { JobData, CachedChunks, Filter, IsParallel=1, ... }
        |
        +---> JobsUtility.ScheduleParallelFor(params, totalChunkCount, batchSize=1)
                  |
                  v
          +-------------------------------------------+
          |     Unity Work-Stealing Scheduler         |
          |                                           |
          |  Chunk 0  Chunk 1  ...  Chunk N-1         |
          |    |        |              |               |
          |    v        v              v               |
          | Thread0  Thread1  ...  ThreadK             |
          +-------------------------------------------+
                  |
                  |  Each thread calls Execute(ref wrapper, ..., ref ranges, jobIndex)
                  v
          +-------------------------------------------+
          |          Execute() Producer Method         |
          |                                           |
          |  executed = false                         |
          |                                           |
          |  while (true)                             |
          |    +------------------------------------+ |
          |    | GetWorkStealingRange(ranges, idx)  | |
          |    |   -> beginChunkIndex, endChunkIndex| |
          |    |   if no range -> BREAK             | |
          |    +------------------------------------+ |
          |         |                                 |
          |         v                                 |
          |    if (!executed)                         |
          |       executed = true                     |
          |       >>> OnWorkerBegin() <<<             |
          |         |                                 |
          |         v                                 |
          |    +-------------------------------+      |
          |    | for chunkIndex in [begin, end) |      |
          |    |   >>> Execute(chunk, ...) <<<  |      |
          |    +-------------------------------+      |
          |         |                                 |
          |         +---> steal more work? loop back   |
          |                                           |
          |  if (executed)                            |
          |    >>> OnWorkerEnd() <<<                  |
          +-------------------------------------------+
```

---

## Parallel Execution Timeline (4 threads, 12 chunks)

```
         Time ---------------------------------------------------->

Thread 0 | OnWorkerBegin() | C0 | C1 | C2 | C3 | C4 | steal C9 | OnWorkerEnd()
Thread 1 | OnWorkerBegin() | C5 | C6 | C7 |    steal C10       | OnWorkerEnd()
Thread 2 | OnWorkerBegin() | C8  | (idle, no work left)         | OnWorkerEnd()
Thread 3 | (no work stolen initially)
                  |
                  +---> later steals range [C10..C11)
                  | OnWorkerBegin() | C10 | C11 | OnWorkerEnd()

Key Insight:
  OnWorkerBegin/End fires ONCE PER THREAD that actually does work.
  Thread 3 might never fire them if it never steals a range.
  The `executed` flag guards this.
```

---

## Single (Non-Parallel) Mode

```
 +-----------------------------------------+
 |          Execute() - Single Mode        |
 |                                         |
 |  executed = false                       |
 |                                         |
 |  No work-stealing loop:                 |
 |    beginChunkIndex = 0                  |
 |    endChunkIndex   = chunks.Length      |
 |                                         |
 |    OnWorkerBegin()                      |
 |                                         |
 |    for i in [0..N):                     |
 |      Execute(chunk[i], i, ...)          |
 |                                         |
 |    OnWorkerEnd()                        |
 |                                         |
 |    break  (exit while loop immediately) |
 +-----------------------------------------+
```

---

## Chunk Iteration Paths

```
                   QueryHasEnableableComponents == 0
                   AND no filtering active?
                          /           \
                        YES            NO
                        /               \
                       v                 v
            +-----------------+   +--------------------+
            |    FAST PATH    |   |   ITERATOR PATH    |
            |                 |   |                    |
            | Direct index    |   | UnsafeChunkCache   |
            | into cached     |   | Iterator walks     |
            | chunkIndices[]  |   | chunks applying    |
            | array.          |   | filter + enabled   |
            |                 |   | mask per chunk.    |
            | v128 = default  |   | v128 = actual mask |
            +-----------------+   +--------------------+
```

---

## Safety / Range Patching (Parallel Mode)

```
#if ENABLE_UNITY_COLLECTIONS_CHECKS

  For each chunk in a parallel job:

  +-- if ChunkBaseEntityIndices != null --+
  |   PatchBufferMinMaxRanges(            |
  |     buffer, wrapper,                  |
  |     chunkBaseEntityIndex,             |
  |     chunk.Count)                      |
  |   => limits NativeContainer writes    |
  |      to the entity range in chunk     |
  +---------------------------------------+

  +-- else (standard parallel) -----------+
  |   PatchBufferMinMaxRanges(            |
  |     buffer, wrapper,                  |
  |     chunkIndex, 1)                    |
  |   => limits writes to 1 element       |
  |      per chunk index                  |
  +---------------------------------------+

  [NativeDisableParallelForRestriction]
  on a container field opts out of this.
#endif
```

---

## RunByRefWithoutJobs (Bypass Path)

```
 +-------------------------------------------+
 |  No job scheduling at all.                |
 |  Runs synchronously on the calling thread.|
 |                                           |
 |  If query has filter or enableables:      |
 |    Use UnsafeChunkCacheIterator           |
 |    -> MoveNextChunk loop                  |
 |    -> Execute(chunk, ...) per chunk       |
 |                                           |
 |  Otherwise (fast path):                   |
 |    Direct for loop over chunkIndices[]    |
 |    -> Execute(chunk, ...) per chunk       |
 |                                           |
 |  Note: OnWorkerBegin/End are NOT called   |
 |  (single thread, no worker concept).      |
 +-------------------------------------------+
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| `executed` flag prevents `OnWorkerEnd` if thread never got work | Avoids false setup/teardown when thread is idle |
| Batch size = 1 in `ScheduleParallelFor` | Chunks are the natural granularity; 1 chunk per steal |
| `OnWorkerBegin/End` are virtual (interface default) | Opt-in: most jobs don't need them, zero cost if unused |
| Work-stealing loop wraps entire begin/end cycle | One thread can steal multiple ranges with one init/cleanup |
| Fast path skips iterator when no filtering | Eliminates per-chunk filter checks for simple queries |

## Source

- [BovineLabs.Core/Jobs/IJobChunkWorkerBeginEnd.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Jobs/IJobChunkWorkerBeginEnd.cs)
