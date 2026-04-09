# CopyEnableable - Inner Workings

## Overview

`CopyEnableable<TTo, TFrom>` is a generic utility struct that synchronizes the **enableable state** (enabled/disabled bits) of one component type (`TTo`) to match another component type (`TFrom`) across all entities in a query. It uses change filtering to only process chunks where `TFrom` has actually changed.

```
File: BovineLabs.Core/Model/CopyEnableable.cs
Generic Parameters:
  TTo   - Target enableable component (IComponentData + IEnableableComponent)
  TFrom - Source enableable component (IComponentData + IEnableableComponent)
```

## Architecture

```
 ┌─────────────────────────────────────────────────────────────────┐
 │                     CopyEnableable<TTo, TFrom>                  │
 │                                                                 │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  OnCreate(ref SystemState)                                │  │
 │  │                                                           │  │
 │  │  EntityQueryBuilder                                       │  │
 │  │    .WithPresentRW<TTo>()        ◄── TTo must be present   │  │
 │  │    .WithPresent<TFrom>()        ◄── TFrom must be present  │  │
 │  │    .Build(ref state)                                      │  │
 │  │                                                           │  │
 │  │  query.AddChangedVersionFilter(ReadOnly<TFrom>())         │  │
 │  │       │                                                   │  │
 │  │       ▼                                                   │  │
 │  │  ┌───────────────────────────────────────────────────┐    │  │
 │  │  │  Change Filter: Only processes chunks where       │    │  │
 │  │  │  TFrom's enable mask was modified since last      │    │  │
 │  │  │  system version.                                  │    │  │
 │  │  └───────────────────────────────────────────────────┘    │  │
 │  └───────────────────────────────────────────────────────────┘  │
 │                                                                 │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  OnUpdate(ref SystemState, SetPreviousJob)                │  │
 │  │                                                           │  │
 │  │  1. Update type handles (toHandle, fromHandle)            │  │
 │  │  2. Assign handles to job                                 │  │
 │  │  3. ScheduleParallel(query, dependency)                   │  │
 │  └───────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────┘
```

## The SetPreviousJob (IJobChunk)

```
 ┌─────────────────────────────────────────────────────────┐
 │              SetPreviousJob : IJobChunk                  │
 │                                                         │
 │  INPUTS:                                                │
 │  ┌──────────────────┐  ┌──────────────────┐            │
 │  │ ToHandle (RW)    │  │ FromHandle (RO)  │            │
 │  │ ComponentType-   │  │ ComponentType-   │            │
 │  │ Handle<TTo>      │  │ Handle<TFrom>    │            │
 │  └────────┬─────────┘  └────────┬─────────┘            │
 │           │                     │                       │
 │           ▼                     ▼                       │
 │  ┌──────────────────────────────────────────────────┐   │
 │  │  Execute(in ArchetypeChunk chunk, ...)           │   │
 │  │                                                  │   │
 │  │  chunk.CopyEnableMaskFrom(                       │   │
 │  │      ref ToHandle,      ◄── TARGET (modified)    │   │
 │  │      ref FromHandle     ◄── SOURCE (read-only)   │   │
 │  │  );                                             │   │
 │  └──────────────────────────────────────────────────┘   │
 └─────────────────────────────────────────────────────────┘
```

## Enable Mask Copy Operation

```
  Entity Chunk (128 entities max)
 ┌──────────────────────────────────────────────────────────┐
 │  TFrom Enable Bits (source)                              │
 │  ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐     │
 │  │ 1│ 0│ 1│ 1│ 0│ 0│ 1│ 0│ 1│ 1│ 0│ 0│ 1│ 0│ 1│ 1│ ... │
 │  └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘     │
 │         │ COPY OPERATION (bitwise)                        │
 │         ▼                                                │
 │  TTo Enable Bits (target)                                │
 │  ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐     │
 │  │ 1│ 0│ 1│ 1│ 0│ 0│ 1│ 0│ 1│ 1│ 0│ 0│ 1│ 0│ 1│ 1│ ... │
 │  └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘     │
 └──────────────────────────────────────────────────────────┘

  Before:  TTo=[1,1,0,1,...]   TFrom=[1,0,1,1,...]
                   ┌──────┐            ┌──────┐
  Operation:       │COPY  │◄───────────│SOURCE│
                   │ TO   │            │MASK  │
                   └──────┘            └──────┘
  After:   TTo=[1,0,1,1,...]   TFrom=[1,0,1,1,...]  (identical)
```

## Change Filtering Flow

```
  Frame N                                Frame N+1
  ═══════                                ═════════

  TFrom mask modified?                    TFrom mask modified?
  ┌──────────┐                           ┌──────────┐
  │  Chunk A │──YES──► Process chunk     │  Chunk A │──NO───► Skip chunk
  │  v=5     │           │               │  v=5     │
  └──────────┘           ▼               └──────────┘
                   CopyEnableMask()
                   TTo ← TFrom bits
                   
  ┌──────────┐                           ┌──────────┐
  │  Chunk B │──NO───► Skip chunk        │  Chunk B │──YES──► Process chunk
  │  v=3     │                           │  v=8     │           │
  └──────────┘                           └──────────┘           ▼
                                                                CopyEnableMask()
                                                                TTo ← TFrom bits
```

## Lifecycle

```
 ┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
 │   OnCreate()     │────►│   OnUpdate()     │────►│  Every Frame     │
 │                  │     │                  │     │  (if changes)    │
 │ • Build query    │     │ • Update handles │     │                  │
 │ • Set change     │     │ • Schedule job   │     │ • Copy masks     │
 │   filter on      │     │   in parallel    │     │   chunk-by-chunk │
 │   TFrom          │     │                  │     │                  │
 └──────────────────┘     └──────────────────┘     └──────────────────┘
```

## Key Design Decisions

1. **Change Version Filter** on `TFrom`: The query skips entire chunks where `TFrom`'s enable bits haven't changed, avoiding unnecessary work.

2. **CopyEnableMaskFrom()** extension method: performs a direct bitwise copy of the enable mask from one component type handle to another within the same chunk, avoiding per-entity iteration.

3. **ScheduleParallel**: The job runs in parallel across chunks for maximum throughput.

4. **TFrom is ReadOnly**: The source component's enable mask is only read, never written to.

5. **TTo is ReadWrite**: Only the target's enable bits are modified.

## Verified Data

```
that: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Model/CopyEnableable.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Model/CopyEnableable.cs)
