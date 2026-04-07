# StateModelEnableable - Inner Workings

## Overview

`StateModelEnableable` is a variant of `StateModel` that toggles **enableable components** instead of adding/removing tag components via `EntityCommandBuffer`. This avoids expensive structural changes (archetype changes) by simply enabling/disabling existing components on entities.

```
File: BovineLabs.Core/States/StateModelEnableable.cs
State Size: 1 byte (single state value, not bitmask)
Key: Uses UnsafeEnableableLookup to toggle components in-place
     No EntityCommandBuffer needed, no structural changes
```

## Architecture

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │                     StateModelEnableable                                 │
 │                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  Constructor                                                      │  │
 │  │                                                                   │  │
 │  │  Assert: stateSize == 1 byte                                      │  │
 │  │  Assert: previousStateSize == 1 byte                              │  │
 │  │                                                                   │  │
 │  │  impl = new StateImpl(ref state, stateComponent, prevComponent)  │  │
 │  │                                                                   │  │
 │  │  VALIDATION:                                                      │  │
 │  │  for each (key, componentType) in RegisteredStatesMap:            │  │
 │  │    ┌─────────────────────────────────────────────────────────┐    │  │
 │  │    │  Assert: componentType must be EnableableType           │    │  │
 │  │    │  (all registered states must be enableable components)  │    │  │
 │  │    │                                                         │    │  │
 │  │    │  state.AddDependency(componentType.TypeIndex)           │    │  │
 │  │    │  (declare dependency on each enableable component)      │    │  │
 │  │    └─────────────────────────────────────────────────────────┘    │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 │                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  Update Modes (NO EntityCommandBuffer needed!)                    │  │
 │  │                                                                   │  │
 │  │  Run()            → Complete dep, RunByRef (synchronous)         │  │
 │  │  Update()         → ScheduleByRef (single thread)                │  │
 │  │  UpdateParallel() → ScheduleParallel (parallel chunks)           │  │
 │  │                                                                   │  │
 │  │  All 3 modes: NO ECB parameter!                                  │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────┘
```

## StateJob Processing Pipeline

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │                    StateJob : IJobChunk                                 │
 │                                                                         │
 │  INPUTS:                                                                │
 │  ┌───────────────────┐  ┌────────────────────┐  ┌──────────────────┐  │
 │  │ RegisteredStates  │  │ StateType (dynamic) │  │ UnsafeEnableable │  │
 │  │ HashMap<byte,     │  │ current state byte  │  │ Lookup           │  │
 │  │ ComponentType>    │  │ (RO, 1 byte)        │  │ (direct enable/  │  │
 │  └─────────┬─────────┘  └─────────┬──────────┘  │  disable access) │  │
 │            │                      │              └────────┬─────────┘  │
 │            ▼                      ▼                       ▼            │
 │  ┌─────────────────────────────────────────────────────────────────┐  │
 │  │  For each entity in chunk:                                     │  │
 │  │                                                                │  │
 │  │    state    = states[i]     (current state byte)              │  │
 │  │    previous = previous[i]   (previous state byte)             │  │
 │  │                                                                │  │
 │  │    IF state == previous: SKIP (no change)                     │  │
 │  │                                                                │  │
 │  │    ┌───────────────────────────────────────────────────────┐   │  │
 │  │    │  IF previous != 0:                                    │   │  │
 │  │    │    lookup RegisteredStates[previous]                  │   │  │
 │  │    │    UnsafeEnableableLookup.SetComponentEnabled(        │   │  │
 │  │    │        entity, oldComponent, false  ◄── DISABLE OLD   │   │  │
 │  │    │    )                                                 │   │  │
 │  │    └───────────────────────────────────────────────────────┘   │  │
 │  │                                                                │  │
 │  │    ┌───────────────────────────────────────────────────────┐   │  │
 │  │    │  IF state != 0:                                       │   │  │
 │  │    │    lookup RegisteredStates[state]                     │   │  │
 │  │    │    UnsafeEnableableLookup.SetComponentEnabled(         │   │  │
 │  │    │        entity, newComponent, true   ◄── ENABLE NEW    │   │  │
 │  │    │    )                                                 │   │  │
 │  │    │    ELSE: warn if state not registered                 │   │  │
 │  │    └───────────────────────────────────────────────────────┘   │  │
 │  │                                                                │  │
 │  │    previous = state  (update previous for next frame)         │  │
 │  └─────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────┘
```

## State Transition Diagram

```
  Entity E has components: State(byte), PreviousState(byte),
                           TagIdle(enableable), TagRunning(enableable),
                           TagJumping(enableable)

  State value: 0 = none, 1 = Idle, 2 = Running, 3 = Jumping

  ┌─────────────────────────────────────────────────────────────────┐
  │  Frame 1: State=1 (Idle)                                       │
  │                                                                 │
  │  Previous=0 → State=1                                          │
  │    Disable previous (0 = none, skip)                            │
  │    Enable TagIdle (state=1) ✓                                   │
  │                                                                 │
  │  Result: TagIdle=ENABLED, TagRunning=DISABLED, TagJumping=DIS  │
  └─────────────────────────────────────────────────────────────────┘
                    │
                    ▼
  ┌─────────────────────────────────────────────────────────────────┐
  │  Frame 2: State=2 (Running)                                    │
  │                                                                 │
  │  Previous=1 → State=2                                          │
  │    Disable TagIdle (previous=1) ✓                               │
  │    Enable TagRunning (state=2) ✓                                │
  │                                                                 │
  │  Result: TagIdle=DISABLED, TagRunning=ENABLED, TagJumping=DIS  │
  └─────────────────────────────────────────────────────────────────┘
                    │
                    ▼
  ┌─────────────────────────────────────────────────────────────────┐
  │  Frame 3: State=3 (Jumping)                                    │
  │                                                                 │
  │  Previous=2 → State=3                                          │
  │    Disable TagRunning (previous=2) ✓                            │
  │    Enable TagJumping (state=3) ✓                                │
  │                                                                 │
  │  Result: TagIdle=DISABLED, TagRunning=DISABLED, TagJumping=EN  │
  └─────────────────────────────────────────────────────────────────┘
                    │
                    ▼
  ┌─────────────────────────────────────────────────────────────────┐
  │  Frame 4: State=3 (Jumping) - no change                        │
  │                                                                 │
  │  Previous=3 == State=3 → SKIP (no work done)                   │
  └─────────────────────────────────────────────────────────────────┘
```

## Comparison: StateModel vs StateModelEnableable

```
  StateModel (structural):
  ┌───────────────────────────────────────────────────────────────┐
  │  • Adds/removes tag components via EntityCommandBuffer        │
  │  • Causes archetype changes (expensive)                       │
  │  • Entity moves between archetypes                            │
  │  • ECB must be Playback()'d                                    │
  │  • 1-frame delay before changes take effect                   │
  │                                                               │
  │  Entity Archetype: [State, Prev]                              │
  │        + TagRunning  ← ECB.AddComponent                       │
  │  → moves to archetype [State, Prev, TagRunning]               │
  │                                                               │
  │  Entity Archetype: [State, Prev, TagRunning]                  │
  │        + TagJumping  ← ECB.AddComponent                       │
  │        - TagRunning  ← ECB.RemoveComponent                    │
  │  → moves to archetype [State, Prev, TagJumping]               │
  └───────────────────────────────────────────────────────────────┘

  StateModelEnableable (non-structural):
  ┌───────────────────────────────────────────────────────────────┐
  │  • Enables/disables existing components via UnsafeLookup      │
  │  • NO archetype changes (fast!)                               │
  │  • Entity stays in same archetype                             │
  │  • Changes take effect immediately (no ECB)                   │
  │  • All state tags must be pre-added to entity                 │
  │                                                               │
  │  Entity Archetype: [State, Prev, TagIdle, TagRun, TagJump]   │
  │  Always: TagIdle(disabled), TagRun(disabled), TagJump(disabled)│
  │                                                               │
  │  State change → just flip enable bits:                        │
  │    TagRun: disabled → enabled                                 │
  │    TagIdle: enabled → disabled                                │
  │  Same archetype, different enable mask!                       │
  └───────────────────────────────────────────────────────────────┘
```

## UnsafeEnableableLookup Detail

```
  SetComponentEnabled(entity, componentType, enabled):

  ┌─────────────────────────────────────────────────────────┐
  │  1. Find entity's chunk                                  │
  │  2. Locate component's enable bit within chunk           │
  │  3. IF enabled:                                          │
  │       Set bit to 1 in chunk's enable mask                │
  │       Decrement chunk's disabled count                   │
  │  4. IF disabled:                                         │
  │       Clear bit to 0 in chunk's enable mask              │
  │       Increment chunk's disabled count                   │
  │                                                          │
  │  NO archetype change!                                    │
  │  NO structural change!                                   │
  │  O(1) operation per component per entity                 │
  └─────────────────────────────────────────────────────────┘

  Chunk enable mask before:
  ┌──────────────────────────────────────────────┐
  │  TagIdle:   [1 0 1 1 0 1 0 1 ...]           │
  │  TagRun:    [0 0 0 0 0 0 0 0 ...]           │
  │  TagJump:   [0 0 0 0 0 0 0 0 ...]           │
  └──────────────────────────────────────────────┘

  After State=2 (Running) for entity[3]:
  ┌──────────────────────────────────────────────┐
  │  TagIdle:   [1 0 1 0 0 1 0 1 ...]  ← bit 3  │
  │  TagRun:    [0 0 0 1 0 0 0 0 ...]  ← bit 3  │
  │  TagJump:   [0 0 0 0 0 0 0 0 ...]            │
  └──────────────────────────────────────────────┘
```

## Key Design Decisions

1. **No Structural Changes**: The primary advantage - no archetype moves means no chunk fragmentation, no entity relocation, instant state transitions.

2. **Single Byte State**: Unlike StateFlagModel's multi-byte bitmask, this uses a single byte for one-of-N state selection (mutually exclusive states).

3. **Constructor Validation**: Asserts that ALL registered state components are `EnableableType`, catching misconfiguration early.

4. **Dependency Registration**: Calls `state.AddDependency()` for each registered component, ensuring the system's query includes these types.

5. **UnsafeEnableableLookup**: Uses the unsafe variant for burst-compatible, direct chunk enable bit manipulation without safety checks.

6. **No EntityCommandBuffer**: Since we're only toggling enable bits (not adding/removing), no ECB is needed. Changes apply immediately within the job.
