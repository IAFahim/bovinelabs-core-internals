# StateAPI - Inner Workings

## Overview

`StateAPI` is a static helper class that registers state components during system initialization. It maps a state component type to a state instance component via a string name (resolved to a byte key through `KSettingsBase`), storing the mapping as a `StateInstance` component on the system entity.

```
File: BovineLabs.Core/States/StateAPI.cs
Class: StateAPI (static)
Purpose: Register state-to-tag mappings during OnCreate
```

## Architecture

```
  ┌────────────────────────────────────────────────────────────────────┐
  │                      StateAPI (static)                             │
  │                                                                    │
  │  ┌──────────────────────────────────────────────────────────────┐  │
  │  │  Register<TState, TInstance, TSettings>(                     │  │
  │  │      ref SystemState systemState,                            │  │
  │  │      string stateName,                                       │  │
  │  │      bool queryDependency = true                             │  │
  │  │  )                                                           │  │
  │  │  → returns byte (state key)                                  │  │
  │  └──────────────────────────────────────────────────────────────┘  │
  │                                                                    │
  │  ┌──────────────────────────────────────────────────────────────┐  │
  │  │  Register<TState, TInstance>(                                │  │
  │  │      ref SystemState systemState,                            │  │
  │  │      byte stateKey,                                          │  │
  │  │      bool queryDependency = true                             │  │
  │  │  )                                                           │  │
  │  │  → void                                                       │  │
  │  └──────────────────────────────────────────────────────────────┘  │
  └────────────────────────────────────────────────────────────────────┘
```

## Registration Flow

```
  System OnCreate():
  ══════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  // In a system's OnCreate method:                              │
  │                                                                 │
  │  StateAPI.Register<                                             │
  │      MyStateComponent,        // TState: the state byte comp    │
  │      TagRunning,              // TInstance: tag to add/remove    │
  │      GameStateSettings        // TSettings: name→key resolver   │
  │  >(ref state, "Running");    // stateName: human-readable name  │
  │                                                                 │
  │  // Returns: byte key for "Running"                             │
  └─────────────────────────────────────────────────────────────────┘
          │
          ▼
  Step 1: Resolve name → key
  ┌─────────────────────────────────────────────────────────────────┐
  │  stateKey = KSettingsBase<TSettings, byte>.NameToKey("Running") │
  │                                                                 │
  │  ┌──────────────────────────────────────────────────────┐      │
  │  │  KSettingsBase<TSettings, byte>                       │      │
  │  │  ───────────────────────────────                      │      │
  │  │  Uses a pre-configured settings asset to map:         │      │
  │  │    "Running" → 3                                      │      │
  │  │    "Idle"    → 1                                      │      │
  │  │    "Jumping" → 5                                      │      │
  │  │    etc.                                               │      │
  │  └──────────────────────────────────────────────────────┘      │
  └─────────────────────────────────────────────────────────────────┘
          │
          ▼
  Step 2: Validate single-registration
  ┌─────────────────────────────────────────────────────────────────┐
  │  Check.Assume(                                                  │
  │    !systemState.EntityManager.HasComponent<StateInstance>(       │
  │        systemState.SystemHandle),                               │
  │    "Trying to register more than 1 state to a system."          │
  │  );                                                             │
  │                                                                 │
  │  Each system can only register ONE StateInstance!               │
  └─────────────────────────────────────────────────────────────────┘
          │
          ▼
  Step 3: Optional query dependency
  ┌─────────────────────────────────────────────────────────────────┐
  │  IF queryDependency:                                            │
  │    query = EntityQueryBuilder.WithAll<TInstance>.Build(state)   │
  │    systemState.RequireForUpdate(query)                          │
  │                                                                 │
  │  System won't update until at least one entity has TInstance    │
  └─────────────────────────────────────────────────────────────────┘
          │
          ▼
  Step 4: Create StateInstance on system entity
  ┌─────────────────────────────────────────────────────────────────┐
  │  systemState.EntityManager.AddComponentData(                    │
  │      systemState.SystemHandle,                                  │
  │      new StateInstance                                          │
  │      {                                                          │
  │          State = TypeManager.GetTypeIndex<TState>(),            │
  │          StateKey = stateKey,          // e.g. 3                │
  │          StateInstanceComponent = TypeManager.GetTypeIndex<TInstance>() │
  │      }                                                          │
  │  );                                                             │
  └─────────────────────────────────────────────────────────────────┘
```

## StateInstance Data Structure

```
  ┌────────────────────────────────────────────────────────────────┐
  │  StateInstance : IComponentData                                │
  │                                                                │
  │  ┌─────────────────────────────────────────────────────────┐  │
  │  │  State : TypeIndex                                      │  │
  │  │  Which state component this registration belongs to      │  │
  │  │  e.g. TypeIndex of MyStateComponent                     │  │
  │  └─────────────────────────────────────────────────────────┘  │
  │                                                                │
  │  ┌─────────────────────────────────────────────────────────┐  │
  │  │  StateKey : byte                                        │  │
  │  │  The byte key / bit position for this state             │  │
  │  │  e.g. 3 (mapped from "Running")                         │  │
  │  └─────────────────────────────────────────────────────────┘  │
  │                                                                │
  │  ┌─────────────────────────────────────────────────────────┐  │
  │  │  StateInstanceComponent : TypeIndex                     │  │
  │  │  The tag component to add/remove when this state is     │  │
  │  │  activated/deactivated                                  │  │
  │  │  e.g. TypeIndex of TagRunning                           │  │
  │  └─────────────────────────────────────────────────────────┘  │
  └────────────────────────────────────────────────────────────────┘
```

## Multi-System Registration Pattern

```
  System A (RunningStateSystem):
  ┌────────────────────────────────────────────────────────────┐
  │  OnCreate:                                                 │
  │    StateAPI.Register<MyState, TagRunning, Settings>(       │
  │        ref state, "Running");                              │
  │                                                            │
  │  Result: SystemHandle A → StateInstance {                  │
  │    State = MyState, StateKey = 3,                          │
  │    StateInstanceComponent = TagRunning                     │
  │  }                                                         │
  └────────────────────────────────────────────────────────────┘

  System B (IdleStateSystem):
  ┌────────────────────────────────────────────────────────────┐
  │  OnCreate:                                                 │
  │    StateAPI.Register<MyState, TagIdle, Settings>(          │
  │        ref state, "Idle");                                 │
  │                                                            │
  │  Result: SystemHandle B → StateInstance {                  │
  │    State = MyState, StateKey = 1,                          │
  │    StateInstanceComponent = TagIdle                        │
  │  }                                                         │
  └────────────────────────────────────────────────────────────┘

  System C (JumpStateSystem):
  ┌────────────────────────────────────────────────────────────┐
  │  OnCreate:                                                 │
  │    StateAPI.Register<MyState, TagJumping, Settings>(       │
  │        ref state, "Jumping");                              │
  │                                                            │
  │  Result: SystemHandle C → StateInstance {                  │
  │    State = MyState, StateKey = 5,                          │
  │    StateInstanceComponent = TagJumping                     │
  │  }                                                         │
  └────────────────────────────────────────────────────────────┘
```

## How StateImpl Consumes StateInstances

```
  StateImpl constructor (used by StateModel, StateFlagModel, etc.):
  ════════════════════════════════════════════════════════════════

  ┌──────────────────────────────────────────────────────────────────┐
  │  1. GetAllStateInstances(ref state)                              │
  │     → Queries ALL systems for StateInstance components           │
  │     → Includes system entities (IncludeSystems query option)     │
  │                                                                  │
  │  2. Filter: only keep entries where State matches our component  │
  │                                                                  │
  │  3. Build RegisteredStatesMap:                                   │
  │     ┌─────────────────────────────────────────────────────┐     │
  │     │  key=1 (Idle)    → ComponentType(TagIdle)           │     │
  │     │  key=3 (Running) → ComponentType(TagRunning)        │     │
  │     │  key=5 (Jumping) → ComponentType(TagJumping)        │     │
  │     └─────────────────────────────────────────────────────┘     │
  │                                                                  │
  │  4. StateModel/StateFlagModel uses this map to know which        │
  │     tag component corresponds to each state key                   │
  └──────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

1. **String-Based Registration**: Uses `KSettingsBase` to resolve human-readable names to byte keys, enabling data-driven state configuration from settings assets.

2. **System Entity Storage**: `StateInstance` is stored on the system's entity handle, not on game entities. This keeps registration metadata separate from game data.

3. **Single Registration Per System**: Asserts that each system registers at most one `StateInstance`, preventing ambiguous mappings.

4. **Optional Query Dependency**: The `queryDependency` parameter lets systems opt out of `RequireForUpdate` if they handle their own update conditions.

5. **Decoupled Design**: `StateAPI` only handles registration. `StateImpl` reads all registrations to build the map. This allows registration to happen across multiple systems independently.

6. **TypeIndex-Based Matching**: Uses `TypeManager.GetTypeIndex<TState>()` for type identity, ensuring the same state component type is used consistently.

## Verified Data

```
that: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/States/StateAPI.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/StateAPI.cs)
