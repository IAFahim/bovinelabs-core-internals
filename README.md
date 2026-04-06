# IJobParallelForDeferBatch - Inner Workings

## Overview

`IJobParallelForDeferBatch` is a batch-oriented parallel job where the total iteration count is
**not known at schedule time**. Instead, the count is read from a deferred pointer (typically a
`NativeList<T>.Length` or an `int*`) when the job actually begins execution. This enables a
pipeline where one job produces items into a list, and a subsequent batch job processes them
without the main thread needing to read the count in between.

Unlike `IJobParallelForDefer` which calls `Execute(int index)` per item, this interface calls
`Execute(int startIndex, int count)` with a contiguous batch of indices, giving the user control
over inner-loop optimization.

---

## Source

`BovineLabs.Core/Jobs/IJobParallelForDeferBatch.cs`

---

## Interface & Producer

```
+------------------------------------------------------------------+
|              IJobParallelForDeferBatch (interface)                |
+------------------------------------------------------------------+
|  void Execute(int startIndex, int count)                          |
+------------------------------------------------------------------+
            |
            |  [JobProducerType(typeof(IJobParallelForDeferBatchProducer<>))]
            v
+------------------------------------------------------------------+
|        IJobParallelForDeferBatchExtensions (scheduling)           |
+------------------------------------------------------------------+
|  ScheduleParallel<T,U>(job, NativeList<U>, batch, dep)           |
|  Schedule<T,U>(job, NativeList<U>, batch, dep)                   |
|  ScheduleParallelByRef<T,U>(...)                                  |
|  ScheduleParallel<T>(job, int* forEachCount, batch, dep)         |
|  ScheduleParallel<T>(job, NativeReference<int>, batch, dep)      |
|  ScheduleParallelByRef<T>(job, int*, batch, dep)                  |
+------------------------------------------------------------------+
            |
            v
+------------------------------------------------------------------+
|     IJobParallelForDeferBatchProducer<T>  (internal)              |
+------------------------------------------------------------------+
|  SharedStatic<IntPtr> JobReflectionData                          |
|  Execute(ref T jobData, ..., ref JobRanges, jobIndex)            |
+------------------------------------------------------------------+
```

---

## Deferred Scheduling Flow

```
 User Code                            BovineLabs                Unity Job System
===========================================================================

 var job = new ProcessBatchJob();
 job.ScheduleParallel(myList, innerloopBatchCount: 64, dep)
       |
       v
 +--------------------------------------------------------------+
 |  Extract deferred length pointer from NativeList:            |
 |                                                              |
 |  #if UNITY_6000_5_OR_NEWER                                   |
 |    list.GetUnsafeList()         -> forEachListPtr            |
 |  #else                                                       |
 |    NativeListUnsafeUtility.                                   |
 |      GetInternalListDataPtrUnchecked(ref list)               |
 |      -> forEachListPtr                                       |
 |                                                              |
 |  #if ENABLE_UNITY_COLLECTIONS_CHECKS                         |
 |    atomicSafetyHandlePtr = &list.m_Safety                    |
 |  #endif                                                      |
 +--------------------------------------------------------------+
       |
       v
 ScheduleParallelBatchInternal(ref jobData, 64, forEachListPtr, ...)
       |
       v
 JobsUtility.ScheduleParallelForDeferArraySize(
     ref scheduleParams,
     innerloopBatchCount = 64,
     forEachListPtr,           // <-- pointer to list header; Length read at exec time
     atomicSafetyHandlePtr
 )
       |
       v
 +--------------------------------------------------------------+
 |              DEFERRED EXECUTION MECHANISM                     |
 |                                                              |
 |  SCHEDULE TIME:                                              |
 |    Job is registered with a POINTER to the length.           |
 |    The actual count is NOT read.                             |
 |                                                              |
 |  EXECUTION TIME (after dep chain completes):                 |
 |    *forEachListPtr.Length is dereferenced                    |
 |    Unity divides: totalLength / innerloopBatchCount          |
 |    -> determines number of work-stealing ranges              |
 |    -> distributes ranges across worker threads               |
 +--------------------------------------------------------------+
```

---

## Execution: Batch Processing

```
 Execute() Producer Method:
 ============================================

 while (true):
   |
   +---> GetWorkStealingRange -> begin, end
   |     if no range -> BREAK
   |
   +---> PatchBufferMinMaxRanges(begin, end - begin)
   |     (safety checks for parallel writes)
   |
   +---> >>> jobData.Execute(begin, end - begin) <<<
   |         USER CODE receives:
   |           startIndex = begin
   |           count      = end - begin
   |
   +---> loop back, try to steal more work
```

### Visual: Batch Division

```
 Total deferred length: 256 items (resolved at exec time)
 innerloopBatchCount: 64

 Unity creates 4 work-stealing ranges:
   Range 0: [0, 64)
   Range 1: [64, 128)
   Range 2: [128, 192)
   Range 3: [192, 256)

 Thread 0 steals Range 0:
   Execute(startIndex=0, count=64)

 Thread 1 steals Range 1:
   Execute(startIndex=64, count=64)

 Thread 2 steals Range 2:
   Execute(startIndex=128, count=64)

 Thread 3 steals Range 3:
   Execute(startIndex=192, count=64)
```

### Uneven Distribution with Work Stealing

```
 Total: 200 items, batch=64

 Unity creates 4 ranges:
   [0,64)  [64,128)  [128,192)  [192,200)  <-- last batch is smaller

 Timeline:
                      Time ------------------------------------>

 Thread 0: | Execute(0, 64)    | Execute(192, 8)  | (stole last batch)
 Thread 1: | Execute(64, 64)   |
 Thread 2: | Execute(128, 64)  |
 Thread 3: | (idle - all work taken)             |
```

---

## Comparison: DeferBatch vs Defer (per-item)

```
 IJobParallelForDefer              IJobParallelForDeferBatch
 ========================          ============================

 Execute(int index)                Execute(int startIndex, int count)

 Called ONCE per item              Called ONCE per BATCH

 User does:                        User does:
   for (int i = 0; i < 1; i++)      for (int i = 0; i < count; i++)
     process(index);                  process(startIndex + i);

 No inner loop control             Full inner loop control
 (scheduler decides granularity)   (scheduler gives batch, user iterates)

 Better for:                       Better for:
   Uniform per-item work             Variable work per item
   Small tasks                       Expensive tasks needing
                                      custom SIMD / loop unrolling
```

---

## Schedule Variants

```
 +--------------------------------------------+------------------+
 | Method                                     | Execution Mode   |
 +--------------------------------------------+------------------+
 | ScheduleParallel<T,U>(NativeList<U>)       | Parallel         |
 | Schedule<T,U>(NativeList<U>)               | Single (serial)  |
 | ScheduleParallelByRef<T,U>(NativeList<U>)  | Parallel         |
 | ScheduleParallel<T>(int*)                  | Parallel         |
 | ScheduleParallel<T>(NativeReference<int>)  | Parallel         |
 | ScheduleParallelByRef<T>(int*)             | Parallel         |
 +--------------------------------------------+------------------+

 Single mode:
   ScheduleBatchInternal -> ScheduleMode.Single
   -> JobsUtility.ScheduleParallelForDeferArraySize with Single mode
   -> entire array processed on one thread

 Parallel mode:
   ScheduleParallelBatchInternal -> ScheduleMode.Parallel
   -> work-stealing across multiple threads
```

---

## NativeReference<int> Variant

```
 job.ScheduleParallel(nativeRef, batch=32, dep)

   forEachListPtr = (byte*)nativeRef.GetUnsafePtrWithoutChecks() - sizeof(void*)

   This points to the "length" field of the internal buffer header,
   similar to how NativeList exposes its length.

   Use case: a prior job writes the count to a NativeReference<int>,
   and this job uses that count as the iteration count.
```

---

## Unsafe int* Variant

```
 job.ScheduleParallel(&myCount, batch=32, dep)

   forEachListPtr = (byte*)forEachCount - sizeof(void*)

   WARNING: "This API is unsafe, it is recommended to use
    the NativeList based Schedule method instead."

   Use case: custom data structures where you control the memory
   layout and guarantee the int* points to a valid count field.
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| `Execute(start, count)` instead of `Execute(index)` | Gives user control over inner loop for SIMD, unrolling, or batch-level optimizations |
| Deferred length via `ScheduleParallelForDeferArraySize` | Prior job determines count; no main-thread sync point needed |
| `innerloopBatchCount` user-configurable | Trade-off: large batches = less stealing overhead, small batches = better load balancing |
| NativeList, NativeReference, and raw `int*` overloads | Flexibility from safe to unsafe APIs |
| No job wrapper struct (unlike IJobChunk) | Simpler; `ref T jobData` is passed directly to Execute |
| `ScheduleByRef` variants | Support for unusually large job structs that shouldn't be copied |
