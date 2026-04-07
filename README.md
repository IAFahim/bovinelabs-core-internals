# StateInstanceUtil - Inner Workings

## Overview

`StateInstanceUtil` is a static utility class with a single method that queries the `EntityComponentStore` for all `StateInstance` components attached to system entities. This is the discovery mechanism used by `StateImpl` to build the `RegisteredStatesMap` during construction.

```
File: BovineLabs.Core/States/StateInstanceUtil.cs
Class: StateInstanceUtil (static)
Method: GetAllStateInstances(ref SystemState, Allocator) → NativeArray<StateInstance>
```

## Architecture

```
  ┌────────────────────────────────────────────────────────────────────┐
  │               StateInstanceUtil (static)                           │
  │                                                                    │
  │  ┌──────────────────────────────────────────────────────────────┐  │
  │  │  GetAllStateInstances(                                       │  │
  │  │      ref SystemState state,                                  │  │
  │  │      Allocator allocator = Allocator.Temp                    │  │
  │  │  ) → NativeArray<StateInstance>                              │  │
  │  │                                                              │  │
  │  │  1. Build query:                                             │  │
  │  │     EntityQueryBuilder(Allocator.Temp)                       │  │
  │  │       .WithAll<StateInstance>()                              │  │
  │  │       .WithOptions(EntityQueryOptions.IncludeSystems)        │  │
  │  │       .Build(state.EntityManager)                            │  │
  │  │                                                              │  │
  │  │  2. Return query.ToComponentDataArray<StateInstance>()       │  │
  │  └──────────────────────────────────────────────────────────────┘  │
  └────────────────────────────────────────────────────────────────────┘
```

## Query Construction Detail

```
  EntityQueryBuilder:
  ════════════════════

  ┌────────────────────────────────────────────────────────────────┐
  │  .WithAll<StateInstance>()                                     │
  │       │                                                        │
  │       ▼                                                        │
  │  Match any entity (including SYSTEM entities)                  │
  │  that has a StateInstance component                            │
  │                                                                │
  │  .WithOptions(EntityQueryOptions.IncludeSystems)               │
  │       │                                                        │
  │       ▼                                                        │
  │  CRITICAL: Without this flag, system entities are invisible    │
  │  to queries. StateInstance lives on system entities, so        │
  │  this flag MUST be included.                                   │
  │                                                                │
  │  .Build(state.EntityManager)                                   │
  │       │                                                        │
  │       ▼                                                        │
  │  Note: Built on EntityManager directly, NOT on ref SystemState │
  │  This is because we want ALL systems, not just our own.        │
  └────────────────────────────────────────────────────────────────┘
```

## ECS System Entity Layout

```
  EntityComponentStore
  ════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  Game Entities:                                                  │
  │  ┌──────────┐ ┌──────────┐ ┌──────────┐                       │
  │  │ Entity 0 │ │ Entity 1 │ │ Entity 2 │  ...                   │
  │  │ Position │ │ Position │ │ Position │                        │
  │  │ Velocity │ │ Health   │ │ AI       │                        │
  │  └──────────┘ └──────────┘ └──────────┘                        │
  │                                                                  │
  │  System Entities (IncludeSystems flag needed to see these):     │
  │  ┌──────────────────────────────┐                               │
  │  │ SystemHandle for SystemA     │                               │
  │  │  StateInstance {              │                               │
  │  │    State = MyState,           │                               │
  │  │    StateKey = 3,              │                               │
  │  │    Component = TagRunning     │                               │
  │  │  }                            │                               │
  │  └──────────────────────────────┘                               │
  │  ┌──────────────────────────────┐                               │
  │  │ SystemHandle for SystemB     │                               │
  │  │  StateInstance {              │                               │
  │  │    State = MyState,           │                               │
  │  │    StateKey = 1,              │                               │
  │  │    Component = TagIdle        │                               │
  │  │  }                            │                               │
  │  └──────────────────────────────┘                               │
  │  ┌──────────────────────────────┐                               │
  │  │ SystemHandle for SystemC     │                               │
  │  │  StateInstance {              │                               │
  │  │    State = OtherState,        │                               │
  │  │    StateKey = 2,              │                               │
  │  │    Component = TagX           │                               │
  │  │  }                            │                               │
  │  └──────────────────────────────┘                               │
  │                                                                  │
  │  GetAllStateInstances returns ALL of these StateInstances       │
  └─────────────────────────────────────────────────────────────────┘
```

## Data Flow: Registration → Discovery → StateImpl

```
  ┌─────────────┐     ┌──────────────────┐     ┌──────────────────────┐
  │ StateAPI.   │     │ StateInstance    │     │ StateImpl            │
  │ Register()  │     │ (on system       │     │ Constructor          │
  │             │     │  entities)       │     │                      │
  │ Called in   │────►│                  │────►│ GetAllStateInstances │
  │ OnCreate    │     │ Stores:          │     │     │                │
  │ of each     │     │ • State TypeIdx  │     │     ▼                │
  │ state       │     │ • StateKey       │     │ Filter by our State  │
  │ system      │     │ • Component      │     │ TypeIndex            │
  │             │     │   TypeIdx        │     │     │                │
  └─────────────┘     └──────────────────┘     │     ▼                │
                                               │ Build                │
                                               │ RegisteredStatesMap  │
                                               │ {key → ComponentType}│
                                               └──────────────────────┘
```

## Filtering in StateImpl

```
  GetAllStateInstances returns ALL StateInstances from ALL systems:
  ┌────────────────────────────────────────────────────────────────┐
  │  [0] { State=MyState, Key=3, Comp=TagRunning }                │
  │  [1] { State=MyState, Key=1, Comp=TagIdle }                   │
  │  [2] { State=OtherState, Key=2, Comp=TagX }  ← different!    │
  │  [3] { State=MyState, Key=5, Comp=TagJumping }                │
  └────────────────────────────────────────────────────────────────┘
          │
          ▼
  StateImpl filters by StateType.m_TypeIndex:
  ┌────────────────────────────────────────────────────────────────┐
  │  IF component.State.Index != this.StateType.m_TypeIndex.Index  │
  │      CONTINUE  // Skip, belongs to different state system      │
  │                                                                │
  │  Results for MyState:                                          │
  │  Key=1 → TagIdle                                               │
  │  Key=3 → TagRunning                                            │
  │  Key=5 → TagJumping                                            │
  │                                                                │
  │  OtherState entries are ignored                                │
  └────────────────────────────────────────────────────────────────┘
```

## Allocation

```
  NativeArray<StateInstance> with Allocator.Temp:
  ┌────────────────────────────────────────────────────────────────┐
  │  • Temp allocation: valid for one frame, no explicit dispose   │
  │    needed (disposed automatically)                              │
  │  • Used immediately in StateImpl constructor                   │
  │  • Query is "using var" → disposed after enumeration           │
  │  • Typical size: 1-64 entries depending on project complexity  │
  └────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

1. **IncludeSystems Flag**: The `EntityQueryOptions.IncludeSystems` flag is essential because `StateInstance` components live on system entities, not game entities. Without this flag, the query returns nothing.

2. **EntityManager (not SystemState) Build**: The query is built on `state.EntityManager` rather than `ref state` because we want to query across ALL systems, not create a system-local query.

3. **Allocator.Temp Default**: The returned `NativeArray` uses `Allocator.Temp` since it's only needed during `StateImpl` construction (same frame, same call stack).

4. **Single Responsibility**: This utility does ONE thing - find all `StateInstance` components. The filtering and map-building logic lives in `StateImpl`.

5. **Static Class**: No state, no allocation, no disposal concerns for the utility itself. Pure function over `SystemState`.
