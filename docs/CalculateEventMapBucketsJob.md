# CalculateEventMapBucketsJob

**Dynamically optimizes physics event hash maps in parallel**

`CalculateEventMapBucketsJob<T, TC>` is a single-threaded job that calls
`RecalculateBuckets()` on a `NativeMultiHashMap<Entity, TC>` after all concurrent
writes have completed. This is necessary because the multi-hash map's internal
bucket structure must be rebuilt before it can be queried efficiently.

---

## Architecture

```
  ┌──────────────────────────────────────────────────────────┐
  │  CalculateEventMapBucketsJob<T, TC> : IJob               │
  │                                                          │
  │  where T  : unmanaged, IBufferElementData                │
  │  where TC : unmanaged, IEventContainer<T, TC>            │
  │                                                          │
  │  NativeMultiHashMap<Entity, TC> CurrentEventMap           │
  │                                                          │
  │  Execute():                                              │
  │    CurrentEventMap.RecalculateBuckets()                   │
  └──────────────────────────────────────────────────────────┘
```

## Position in Event Pipeline

```
  Physics Simulation Output (NativeStream)
         │
         ▼
  ┌──────────────────────────┐
  │ EnsureCapacity jobs      │ ← parallel
  │  - events hashset        │
  │  - event map             │
  └───────────┬──────────────┘
              │
              ▼
  ┌──────────────────────────┐
  │ CollectEventsJob         │ ← parallel
  │  Writes to:              │
  │   currentEventMap (raw)  │
  │   currentEvents (set)    │
  └───────────┬──────────────┘
              │
              ▼
  ┌──────────────────────────────────────────────────────┐
  │ CalculateEventMapBucketsJob ← THIS JOB              │
  │ CalculateCurrentEventsBucketsJob                     │
  │                                                      │
  │  Both run in parallel (CombinedDependencies)         │
  │                                                      │
  │  ┌─────────────────────┐  ┌────────────────────────┐│
  │  │ currentEventMap     │  │ currentEvents          ││
  │  │ .RecalculateBuckets │  │ (NativeHashSet)        ││
  │  │                     │  │ rebuilds internal      ││
  │  │ Rebuilds bucket     │  │ bucket array for       ││
  │  │ linked lists from   │  │ fast Contains() calls  ││
  │  │ raw key/value data  │  │                        ││
  │  └─────────────────────┘  └────────────────────────┘│
  └──────────────────────────────────────────────────────┘
              │
              ▼
  ┌──────────────────────────┐
  │ WriteEventsJob           │ ← parallel IJobChunk
  │  Queries currentEventMap │
  │  Queries previousEventMap│
  │  Queries currentEvents   │
  │  Queries previousEvents  │
  │                          │
  │  All maps/sets must have │
  │  valid bucket structure! │
  └──────────────────────────┘
```

## Why RecalculateBuckets Is Needed

```
  NativeMultiHashMap internal structure:

  Before RecalculateBuckets():
  ┌────────┬────────┬────────┬────────┬────────┐
  │ Key[0] │ Key[1] │ Key[2] │ Key[3] │ Key[4] │  raw keys
  │  E1    │  E2    │  E1    │  E3    │  E2    │  (unsorted)
  ├────────┼────────┼────────┼────────┼────────┤
  │ Val[0] │ Val[1] │ Val[2] │ Val[3] │ Val[4] │  raw values
  │  evA   │  evB   │  evC   │  evD   │  evE   │
  └────────┴────────┴────────┴────────┴────────┘
  Buckets: INVALID / STALE
  TryGetFirstValue → UNDEFINED BEHAVIOR

  After RecalculateBuckets():
  ┌──────────────────────────────────────┐
  │ Bucket[hash(E1)] → [0] → [2] → end  │  E1: evA, evC
  │ Bucket[hash(E2)] → [1] → [4] → end  │  E2: evB, evE
  │ Bucket[hash(E3)] → [3] → end        │  E3: evD
  └──────────────────────────────────────┘
  TryGetFirstValue → CORRECT results
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateEventMapBucketsJob.cs`

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateEventMapBucketsJob.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateEventMapBucketsJob.cs)
- [BovineLabs.Core.Extensions/PhysicsStates/StatefulEventImpl.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulEventImpl.cs)
- [BovineLabs.Core.Extensions/AssemblyInfo.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/AssemblyInfo.cs)
