# IJobHashMapDefer - Inner Workings

## Overview

`IJobHashMapDefer` iterates over the contents of a `NativeHashMap`, `NativeMultiHashMap`, or
`NativeHashSet` in a parallel job. The iteration count is **deferred** -- it is read from the
hash map's internal bucket capacity pointer at job execution time, not at schedule time. This
means a prior job can write to the hash map, and the iteration job will see the updated size
without the main thread needing to intervene.

The user's `ExecuteNext(entryIndex, jobIndex)` is called once per *entry* in the hash map,
not per bucket. The producer walks each bucket's linked list internally.

---

## Sources

- `BovineLabs.Core/Jobs/IJobHashMapDefer.cs` -- core implementation
- `BovineLabs.Core/Jobs/IJobParallelHashMapDefer.cs` -- parallel variant with `OnWorkerBegin/End` hooks

---

## Interface & Producer

```
+------------------------------------------------------------------+
|                     IJobHashMapDefer (interface)                  |
+------------------------------------------------------------------+
|  void ExecuteNext(int entryIndex, int jobIndex)                   |
+------------------------------------------------------------------+
            |
            |  [JobProducerType(typeof(JobHashMapVisitKeyValueProducer<>))]
            v
+------------------------------------------------------------------+
|                  JobHashMapDefer (static helper)                  |
+------------------------------------------------------------------+
|  ScheduleParallel<TJob,TKey,TValue>(hashMap, min, dep)           |
|  Schedule<TJob,TKey,TValue>(hashMap, min, dep)                   |
|  (overloads for NativeHashMap, NativeMultiHashMap,               |
|   NativeHashSet)                                                  |
|  Read<TJob,TKey,TValue>(ref job, hashMap, entryIndex,            |
|                          out key, out value)                      |
+------------------------------------------------------------------+
            |
            v
+------------------------------------------------------------------+
|     JobHashMapVisitKeyValueProducer<T>  (internal producer)       |
+------------------------------------------------------------------+
|  HashMapWrapper* HashMap    pointer to internal hash map data     |
|  T JobData                  user job instance                     |
+------------------------------------------------------------------+
            |
            v
+------------------------------------------------------------------+
|          HashMapWrapper  (unsafe internal layout)                 |
+------------------------------------------------------------------+
|  byte* Ptr          values array                                  |
|  byte* Keys         keys array                                    |
|  int*  Next         next-pointer array (linked list per bucket)   |
|  int*  Buckets      bucket heads (one per bucket)                 |
|  int   Count        number of entries                             |
|  int   Capacity                                                      |
|  int   BucketCapacity    <-- deferred length points here          |
|  int   AllocatedIndex                                                  |
|  int   FirstFreeIdx                                                    |
|  int   SizeOfTValue                                                    |
|  AllocatorHandle Allocator                                            |
+------------------------------------------------------------------+
```

---

## Deferred Scheduling Flow

```
 User Code                              Unity Job System
================================================================

 job.ScheduleParallel(hashMap, minIndicesPerJobCount=64, dep)
       |
       v
 ScheduleInternal(jobData, &hashMap->..., min, dep, Parallel)
       |
       +---> Build JobHashMapVisitKeyValueProducer<T>
       |       { HashMap = (HashMapWrapper*)hashMap.m_Data,
       |         JobData = job }
       |
       +---> Calculate deferred length pointer:
       |       lengthPtr = &hashMap->BucketCapacity - sizeof(void*)
       |       (points to the count field just before BucketCapacity)
       |
       +---> JobsUtility.ScheduleParallelForDeferArraySize(
       |       ref scheduleParams,
       |       minIndicesPerJobCount = 64,
       |       lengthPtr,               // <-- READ AT EXECUTION TIME
       |       atomicSafetyHandlePtr)
              |
              v
 +--------------------------------------------------------------+
 |                 DEFERRED SCHEDULING                          |
 |                                                              |
 |   At SCHEDULE time:                                          |
 |     The job is registered but the iteration count is NOT     |
 |     read yet. Only a pointer to the count is stored.         |
 |                                                              |
 |   At EXECUTION time (after dep completes):                   |
 |     *lengthPtr is dereferenced to get bucketCount            |
 |     Work is divided into ranges of buckets                   |
 |     across available threads                                 |
 |                                                              |
 |   This allows a prior parallel job to write to the           |
 |   hash map and this job iterates the result without          |
 |   the main thread needing to complete between them.          |
 +--------------------------------------------------------------+
```

---

## Execution: Bucket Walking

```
 Execute() Producer Method:
 ============================================

 while (true):
   GetWorkStealingRange -> begin, end  (bucket range)

   buckets  = HashMap->Buckets   (int[], one per bucket)
   nextPtrs = HashMap->Next      (int[], linked list)

   for i in [begin, end):          // <-- iterate BUCKETS
     |
     |  entryIndex = buckets[i]    // head of chain
     |
     v
   +---------------------------+
   | while entryIndex != -1:   |
   |                           |
   |   >>> ExecuteNext(entryIndex, jobIndex) <<<   (USER CODE)
   |                           |
   |   entryIndex = nextPtrs[entryIndex]  // follow chain
   +---------------------------+
```

### Visual: Hash Map Structure & Iteration

```
  Buckets[]              Next[]              Keys[] / Values[]
+----------+          +----------+
| buckets[0]|----+    |          |          +--------+--------+
+----------+    |    +----------+          | key[3] | val[3] |
| buckets[1]|----|-----------+  |          +--------+--------+
+----------+    |    |        |          | key[7] | val[7] |
|    -1    |    |    |  +-----|--+       +--------+--------+
+----------+    |    |  |     |  |       | key[1] | val[1] |
| buckets[3]|----|--------|--|--|------+ +--------+--------+
+----------+    |    |  |  |  |  |     |
   ...          |    |  |  |  |  |     |
               v    v  v  v
              Entry indices: 3, 7, 1, ...

 Thread 0 iterates buckets [0..2):
   Bucket 0 -> entry 3 -> next[3]=7 -> next[7]=-1
     calls: ExecuteNext(3, 0), ExecuteNext(7, 0)
   Bucket 1 -> entry 1 -> next[1]=-1
     calls: ExecuteNext(1, 0)

 Thread 1 iterates buckets [2..4):
   Bucket 2 -> -1  (empty, skip)
   Bucket 3 -> entry 5 -> next[5]=-1
     calls: ExecuteNext(5, 1)
```

---

## Read Helper: Accessing Key/Value from EntryIndex

```
 job.Read(hashMap, entryIndex, out key, out value)

   key   = UnsafeUtility.ReadArrayElement<TKey>(hashMap.Keys, entryIndex)
   value = UnsafeUtility.ReadArrayElement<TValue>(hashMap.Ptr,  entryIndex)

 Usage pattern inside ExecuteNext:

   void ExecuteNext(int entryIndex, int jobIndex)
   {
       this.Read(myHashMap, entryIndex, out var key, out var value);
       // process key/value...
   }
```

---

## Supported Container Types

```
 +-------------------------+----------------------------+-------------------+
 | Container               | Internal Cast              | Schedule Methods  |
 +-------------------------+----------------------------+-------------------+
 | NativeHashMap<K,V>      | (HashMapWrapper*)m_Data    | Schedule,         |
 |                         |                            | ScheduleParallel  |
 +-------------------------+----------------------------+-------------------+
 | NativeMultiHashMap<K,V> | (HashMapWrapper*)data      | Schedule,         |
 |                         |                            | ScheduleParallel  |
 +-------------------------+----------------------------+-------------------+
 | NativeHashSet<K>        | (HashMapWrapper*)m_Data    | Schedule,         |
 |                         |                            | ScheduleParallel  |
 +-------------------------+----------------------------+-------------------+
```

---

## IJobParallelHashMapDefer (Extended Variant)

```
+------------------------------------------------------------------+
|             IJobParallelHashMapDefer (interface)                  |
+------------------------------------------------------------------+
|  void OnWorkerBegin()            <- per-thread setup              |
|  void OnWorkerEnd()              <- per-thread teardown           |
|  void ExecuteNext(entryIndex, jobIndex)                           |
|  void OnBucketEnd()              <- called after each non-empty   |
|                                     bucket's chain is exhausted   |
+------------------------------------------------------------------+
```

### Execution with Hooks

```
 while (true):
   GetWorkStealingRange -> begin, end

   if (!executed):
     executed = true
     >>> OnWorkerBegin() <<<

   // Handle mask-based bucket capacity for parallel variant
   if (end == hashMap->bucketCapacityMask):
     end++                    // last bucket inclusive

   for i in [begin, end):
     entryIndex = buckets[i]
     anyValid = false

     while entryIndex != -1:
       anyValid = true
       >>> ExecuteNext(entryIndex, jobIndex) <<<
       entryIndex = nextPtrs[entryIndex]

     if (anyValid):
       >>> OnBucketEnd() <<<

 if (executed):
   >>> OnWorkerEnd() <<<
```

### Parallel Execution Timeline

```
         Time -------------------------------------------------------->

Thread 0 | OnWorkerBegin | bucket0[entries] OnBucketEnd | bucket1[...] | OnWorkerEnd
Thread 1 | OnWorkerBegin | bucket2[empty]  | bucket3[entries] OnBucketEnd | OnWorkerEnd
Thread 2 | OnWorkerBegin | bucket4[entries] OnBucketEnd | steal bucket6 | OnWorkerEnd
Thread 3 | OnWorkerBegin | bucket5[entries] OnBucketEnd | OnWorkerEnd
```

---

## Deferred Length Pointer Arithmetic

```
 HashMapWrapper memory layout:
 +-------------------+
 | Ptr               |  offset 0
 | Keys              |  offset 8
 | Next              |  offset 16
 | Buckets           |  offset 24
 | Count             |  offset 32    <--- THIS is the deferred length
 | Capacity          |  offset 36
 | ...               |
 | BucketCapacity    |  offset 48+
 +-------------------+

 lengthPtr = (byte*)&hashMap->BucketCapacity - sizeof(void*)
           = address of the int field just before BucketCapacity
           = &Count (the actual entry count)

 ScheduleParallelForDeferArraySize reads *lengthPtr when the job runs,
 giving the real bucket count at that moment.
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Iterate buckets, not entries directly | Buckets are the parallelizable unit; entries within a bucket are a linked list |
| Deferred array size via pointer | Prior parallel job can fill the hash map; iteration count is resolved at execution time |
| `HashMapWrapper` unsafe cast | Avoids generic constraints; works across NativeHashMap, NativeMultiHashMap, NativeHashSet |
| `Read<T>` helper with `entryIndex` | Direct array element access; no bucket lookup needed since we already have the index |
| `OnBucketEnd` in parallel variant | Enables flushing per-bucket aggregations (e.g., sort within bucket, write to stream) |
| `bucketCapacityMask` adjustment | Parallel variant stores mask instead of capacity; +1 needed for last bucket to be inclusive |
