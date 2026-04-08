# GlobalRandom — Inner Workings

## Overview

GlobalRandom provides globally accessible per-thread random number generators that
work from both Burst-compiled jobs and managed code. It uses `SharedStatic` to store
a `ThreadRandom` which allocates one `Unity.Mathematics.Random` per CPU cache line
to prevent false sharing between threads.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     GlobalRandom (static class)                             │
│                                                                             │
│  SharedStatic<ThreadRandom> ThreadRandoms                                   │
│         │                                                                   │
│         ▼                                                                   │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  ThreadRandom                                                        │   │
│  │  ┌────────────────────────────────────────────────────────────────┐  │   │
│  │  │  Randoms* buffer  → [Random₀][Random₁][Random₂]...[Randomₙ]  │  │   │
│  │  │                     ────────────────────────────────────────    │  │   │
│  │  │                     Each padded to cache line size             │  │   │
│  │  └────────────────────────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  Thread → ref Random GetRandomRef()                                        │
│           uses JobsUtility.ThreadIndex to select correct slot              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## ThreadLocal Memory Layout

```
  ThreadRandom.buffer  (heap allocated)
  ═══════════════════════════════════════════

  Each slot = JobsUtility.CacheLineSize bytes (typically 64 bytes)
  ┌─────────────────────────────────────────────────────────────────┐
  │ Cache Line 0 (64 bytes)                                         │
  │ ┌──────────────────────────────────┐ ┌────────────────────────┐ │
  │ │ Random struct (20 bytes)         │ │ Padding (44 bytes)     │ │
  │ │ [state: uint4 × 4 + index: int] │ │ (unused, prevents      │ │
  │ │                                  │ │  false sharing)        │ │
  │ └──────────────────────────────────┘ └────────────────────────┘ │
  ├─────────────────────────────────────────────────────────────────┤
  │ Cache Line 1 (64 bytes)                                         │
  │ ┌──────────────────────────────────┐ ┌────────────────────────┐ │
  │ │ Random struct                     │ │ Padding                │ │
  │ └──────────────────────────────────┘ └────────────────────────┘ │
  ├─────────────────────────────────────────────────────────────────┤
  │ ...                                                             │
  │ Cache Line N-1 (where N = JobsUtility.ThreadIndexCount)         │
  └─────────────────────────────────────────────────────────────────┘

  False sharing prevention:
  ┌───────────────────────────────────────────────────────────┐
  │  Thread 0 writes to Random[0] (cache line 0)              │
  │  Thread 1 writes to Random[1] (cache line 1)              │
  │  → NO cache line invalidation between threads!            │
  └───────────────────────────────────────────────────────────┘
```

## SharedStatic Storage

```
  SharedStatic<ThreadRandom>
  ┌───────────────────────────────────────────────────────────────────┐
  │  Unique key: typeof(RandomType) (private nested struct)           │
  │                                                                   │
  │  Data: ThreadRandom struct                                        │
  │  ┌─────────────────────────────────────────────────────────────┐  │
  │  │  AllocatorManager.AllocatorHandle allocator                  │  │
  │  │  Randoms* buffer → heap array of cache-line-padded Randoms  │  │
  │  └─────────────────────────────────────────────────────────────┘  │
  │                                                                   │
  │  Shared across ALL Burst jobs + main thread in same process       │
  │  Persists for domain lifetime (Domain Reload in Editor resets)    │
  └───────────────────────────────────────────────────────────────────┘
```

## Initialization

```
  [RuntimeInitializeOnLoadMethod(SubsystemRegistration)]
  GlobalRandom.Initialize()
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  if ThreadRandoms.Data.IsCreated: return (skip if exists)     │
  │                                                               │
  │  ThreadRandoms.Data = new ThreadRandom(                       │
  │      seed: Random.Range(0, int.MaxValue),                     │
  │      allocator: Allocator.Domain                              │
  │  );                                                            │
  │                                                               │
  │  Inside ThreadRandom constructor:                             │
  │  ┌─────────────────────────────────────────────────────────┐  │
  │  │  buffer = Allocate(sizeof(Randoms) * ThreadIndexCount)  │  │
  │  │                                                         │  │
  │  │  for i in 0..ThreadIndexCount:                         │  │
  │  │    buffer[i].Random =                                   │  │
  │  │      Random.CreateFromIndex(seed + i)                   │  │
  │  │    // Each thread gets unique seed                      │  │
  │  └─────────────────────────────────────────────────────────┘  │
  └───────────────────────────────────────────────────────────────┘
```

## Thread Access Pattern

```
  GlobalRandom.NextInt()
         │
         ▼
  Thread property:
    ref Random GetRandomRef()
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  JobsUtility.ThreadIndex → which thread am I?                 │
  │                                                               │
  │  Thread 0: buffer[0].Random    Thread 2: buffer[2].Random    │
  │  Thread 1: buffer[1].Random    Thread N: buffer[N].Random    │
  │                                                               │
  │  return ref UnsafeUtility.ArrayElementAsRef<Randoms>(         │
  │      buffer, JobsUtility.ThreadIndex)                         │
  │                                                               │
  │  Each thread gets its OWN Random instance                     │
  │  No synchronization needed!                                   │
  └───────────────────────────────────────────────────────────────┘
```

## API Surface

```
  GlobalRandom exposes all Unity.Mathematics.Random methods statically:
  ┌─────────────────────────────────────────────────────────────────┐
  │  NextBool()    NextBool2()   NextBool3()   NextBool4()          │
  │  NextInt()     NextInt2()    NextInt3()    NextInt4()           │
  │  NextInt(max)  NextInt(min,max)                                │
  │  NextUInt()    NextUInt2()   NextUInt3()   NextUInt4()          │
  │  NextFloat()   NextFloat2()  NextFloat3()  NextFloat4()         │
  │  NextDouble()  NextDouble2() NextDouble3() NextDouble4()         │
  │  NextFloat2Direction()  NextFloat3Direction()                   │
  │  NextDouble2Direction() NextDouble3Direction()                  │
  │  NextQuaternionRotation()                                       │
  │                                                                 │
  │  Also: ref Random Thread → direct access to underlying Random  │
  └─────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **SharedStatic**: The only way to share mutable state with Burst-compiled code
  without passing pointers through job data. Keyed by private nested type.
- **Cache-line padding**: `Randoms` struct is `StructLayout(LayoutKind.Explicit, Size=CacheLineSize)`
  to prevent false sharing between threads writing to adjacent array slots.
- **Per-thread seeding**: Each thread gets `Random.CreateFromIndex(seed + i)` so
  sequences don't overlap. Not deterministic across runs due to random seed.
- **Not deterministic**: Explicitly documented — thread scheduling is nondeterministic,
  so global random is only for gameplay variety, not replay/serialization.

## Source Files

- `BovineLabs.Core/Utility/GlobalRandom.cs`
- `BovineLabs.Core/Collections/ThreadRandom.cs` (underlying per-thread storage)

## Source

- [BovineLabs.Core/Utility/GlobalRandom.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/GlobalRandom.cs)
- [BovineLabs.Core.Editor/InitializeAllOnLoad.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/InitializeAllOnLoad.cs)
