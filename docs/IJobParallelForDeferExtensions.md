# IJobParallelForDeferExtensions - Inner Workings

## Overview

`IJobParallelForDeferExtensions` is a small but powerful extension class that bridges Unity's
`IJobParallelForDefer` with ECS `DynamicBuffer<T>`. It provides a single `Schedule` overload
that reads the iteration count from a `DynamicBuffer<U>`'s internal length field at execution
time, enabling deferred parallel processing of dynamic buffer contents without the main thread
needing to know the buffer's length.

---

## Source

`BovineLabs.Core/Extensions/IJobParallelForDeferExtensions.cs`

---

## Class Structure

```
+------------------------------------------------------------------+
|         IJobParallelForDeferExtensions  (static class)            |
+------------------------------------------------------------------+
|  Schedule<T,U>(T job, DynamicBuffer<U> list,                     |
|                int innerloopBatchCount, JobHandle dependsOn)      |
|    where T : struct, IJobParallelForDefer                        |
|    where U : unmanaged                                            |
+------------------------------------------------------------------+
            |
            |  uses
            v
+------------------------------------------------------------------+
|           DynamicBufferInternal  (unsafe struct)                  |
+------------------------------------------------------------------+
|  BufferHeader* Buffer   (readonly, pointer to buffer header)     |
+------------------------------------------------------------------+
            |
            |  accesses
            v
+------------------------------------------------------------------+
|              BufferHeader (Unity internal)                         |
+------------------------------------------------------------------+
|  ...                                                              |
|  int Length          <-- THIS is the deferred count                |
|  int Capacity                                                       |
|  void* Pointer       <-- data pointer                              |
|  ...                                                              |
+------------------------------------------------------------------+
```

---

## Scheduling Flow

```
 User Code (inside SystemBase/ISystem)
 ================================================================

 Entities.ForEach((DynamicBuffer<MyElement> buffer) =>
 {
     job.Schedule(buffer, innerloopBatchCount: 32, dep)
 })
       |
       v
 IJobParallelForDeferExtensions.Schedule<T, U>()
       |
       +---> Reinterpret DynamicBuffer as DynamicBufferInternal
       |     (unsafe struct cast via UnsafeUtility.As)
       |
       |     ref var intern = ref UnsafeUtility.As<
       |         DynamicBuffer<U>, DynamicBufferInternal>(ref list);
       |
       |     var header = intern.Buffer;  // BufferHeader*
       |
       +---> Delegate to standard IJobParallelForDefer.Schedule:
       |
       |     job.Schedule(
       |         &header->Length,          // int* to length field
       |         innerloopBatchCount: 32,
       |         dependsOn
       |     )
       |
       v
 Unity's IJobParallelForDefer.Schedule(int* lengthPtr, batch, dep)
       |
       +---> JobsUtility.ScheduleParallelForDeferArraySize(
       |       ref scheduleParams,
       |       innerloopBatchCount,
       |       lengthPtr = &header->Length,   // READ AT EXEC TIME
       |       atomicSafetyHandlePtr
       |     )
              |
              v
 +--------------------------------------------------------------+
 |              DEFERRED EXECUTION                               |
 |                                                              |
 |  SCHEDULE TIME:                                              |
 |    Stores pointer to header->Length                          |
 |    Does NOT read the value                                   |
 |                                                              |
 |  EXECUTION TIME:                                             |
 |    *lengthPtr dereferenced -> actual buffer.Length           |
 |    Work divided into batches across threads                  |
 |                                                              |
 |  Result: prior jobs can resize the DynamicBuffer,           |
 |  and this job sees the updated length automatically.         |
 +--------------------------------------------------------------+
```

---

## Memory Layout & Pointer Chase

```
 DynamicBuffer<U> (managed wrapper)
   |
   |  UnsafeUtility.As cast
   v
 DynamicBufferInternal (unsafe)
   +------------------+
   | BufferHeader* ----|-------> BufferHeader (native memory)
   +------------------+          +-------------------+
                                 | Length            | <--- deferred length pointer
                                 | Capacity          |
                                 | Pointer ----------|--> [elem0, elem1, ..., elemN]
                                 | ...               |
                                 +-------------------+

 The Schedule call passes &header->Length (address of the Length int field).
 Unity reads this at execution time to determine iteration count.
```

---

## Execution Timeline

```
 Job A (produces items into DynamicBuffer)
   |
   |  Dep chain:  JobA ---> IJobParallelForDefer
   v
 IJobParallelForDefer job
   |
   |  At exec time: reads *(&header->Length) = 500
   |  Divides 500 items into batches of 32
   |
   |  Result: ~16 work-stealing ranges
   |
   v
 +----------------------------------------------------------+
 | Thread 0: Execute(0)    Execute(1)  ...  Execute(31)     |
 | Thread 1: Execute(32)   Execute(33) ...  Execute(63)     |
 | Thread 2: Execute(64)   ...                               |
 | ...                                                      |
 | Thread N: (steals remaining ranges)                      |
 +----------------------------------------------------------+
```

---

## Why Not Just Use IJobParallelForDefer Directly?

```
 Standard IJobParallelForDefer.Schedule overloads accept:
   - NativeList<T>       (length deferred via list header)
   - int*                (raw pointer to count)

 DynamicBuffer<T> is NOT a NativeList<T>.
 Its length lives in a BufferHeader, not in NativeList's internal struct.

 Without this extension, users would need:
   unsafe {
       ref var intern = ref UnsafeUtility.As<...>(ref buffer);
       job.Schedule(&intern.Buffer->Length, batch, dep);
   }

 This extension wraps that unsafe pointer chase into a clean API.
```

---

## Relationship to Other BovineLabs Jobs

```
 +---------------------------+------------------------+------------------+
 | Job Type                  | Deferred From          | Execute Signature |
 +---------------------------+------------------------+------------------+
 | IJobParallelForDefer      | NativeList, int*       | Execute(int i)   |
 |   + DeferExtensions       | DynamicBuffer          | Execute(int i)   |
 | IJobParallelForDeferBatch | NativeList, int*       | Execute(int,int) |
 | IJobHashMapDefer          | HashMap bucket count   | ExecuteNext(int) |
 | IJobForThread             | Nothing (eager)        | Execute(int)     |
 +---------------------------+------------------------+------------------+

 This extension specifically bridges DynamicBuffer -> IJobParallelForDefer.
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| `UnsafeUtility.As` cast to `DynamicBufferInternal` | Accesses the internal `BufferHeader*` without reflection; zero-allocation reinterpretation |
| Only one `Schedule` overload (no `ScheduleParallel`) | `IJobParallelForDefer.Schedule(int*)` is already parallel by default |
| No `ScheduleByRef` variant | Job structs are typically small; DynamicBuffer is already a thin wrapper |
| Targets `IJobParallelForDefer`, not `IJobParallelForDeferBatch` | Per-item execution; use `IJobParallelForDeferBatch` with manual length pointer if batch API needed |
| `DynamicBufferInternal` is private nested struct | Encapsulates the unsafe layout knowledge; users never see it |
| Uses existing `IJobParallelForDefer.Schedule(int*, ...)` | Delegates all job registration and deferred mechanics to Unity's built-in implementation |

## Source

- [BovineLabs.Core/Extensions/IJobParallelForDeferExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/IJobParallelForDeferExtensions.cs)
