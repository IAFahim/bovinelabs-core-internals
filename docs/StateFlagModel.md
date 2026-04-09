# StateFlagModel - Inner Workings

## Overview

`StateFlagModel` manages a bitmask-based state machine that adds or removes tag components based on individual bit changes. Unlike `StateModel` (which uses a single byte to represent one-of-N states), `StateFlagModel` supports multi-byte state values where **each bit** independently maps to a tag component, enabling concurrent multi-state representation.

```
File: BovineLabs.Core/States/StateFlagModel.cs
Constructor: StateFlagModel(ref SystemState, ComponentType stateComponent, ComponentType previousStateComponent)
State Size: Any byte width (1, 2, 4, 8 bytes) → 8 to 64 independent state bits
Key: Bit-level diffing → add/remove tag components per-bit
```

## Architecture

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │                          StateFlagModel                                 │
 │                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  Constructor                                                      │  │
 │  │                                                                   │  │
 │  │  stateSize = TypeManager.GetTypeInfo(stateComponent).ElementSize │  │
 │  │  Assert: stateSize == previousStateComponent.ElementSize         │  │
 │  │                                                                   │  │
 │  │  impl = new StateImpl(ref state, stateComponent, prevComponent)  │  │
 │  │       │                                                           │  │
 │  │       ▼                                                           │  │
 │  │  ┌─────────────────────────────────────────────────────────┐     │  │
 │  │  │  StateImpl:                                              │     │  │
 │  │  │  • Query: [stateComponent] + RW[previousStateComponent] │     │  │
 │  │  │  • Change filter on stateComponent                      │     │  │
 │  │  │  • RegisteredStatesMap: HashMap<byte, ComponentType>    │     │  │
 │  │  │    populated from StateInstanceUtil.GetAllStateInstances │     │  │
 │  │  └─────────────────────────────────────────────────────────┘     │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 │                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  Update Modes                                                     │  │
 │  │                                                                   │  │
 │  │  Run()            → Complete dependency, RunByRef (sync)         │  │
 │  │  Update()         → ScheduleByRef (single thread)                │  │
 │  │  UpdateParallel() → ScheduleParallelByRef (parallel chunks)      │  │
 │  │                                                                   │  │
 │  │  All require an EntityCommandBuffer for add/remove ops           │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────┘
```

## The Core Bit-Diffing Algorithm

```
  For each byte j in [0..stateSize):
    For each bit r in [0..7):
      bit = (j * 8) + r
      state[j]    = current bitmask byte
      previous[j] = previous bitmask byte

  Truth Table:
  ┌─────────┬─────────┬─────────────┬────────────┐
  │ State S │ Prev  P │ Remove R    │ Add A      │
  │ (new)   │ (old)   │ ~S & P      │ S & ~P     │
  ├─────────┼─────────┼─────────────┼────────────┤
  │    0    │    0    │     0       │     0      │  No change
  │    0    │    1    │     1       │     0      │  REMOVE tag
  │    1    │    0    │     0       │     1      │  ADD tag
  │    1    │    1    │     0       │     0      │  No change
  └─────────┴─────────┴─────────────┴────────────┘

  R = ~state & previous   (was ON, now OFF → remove)
  A = state & ~previous   (was OFF, now ON → add)
```

## StateJob Processing Pipeline

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │                    StateJob : IJobChunk                                 │
 │                                                                         │
 │  INPUTS:                                                                │
 │  ┌───────────────────┐  ┌────────────────────┐  ┌──────────────────┐  │
 │  │ RegisteredStates  │  │ StateType (dynamic) │  │ PreviousState    │  │
 │  │ HashMap<byte,     │  │ current state bytes │  │ (dynamic, RW)    │  │
 │  │ ComponentType>    │  │ (RO)               │  │ previous bytes   │  │
 │  └─────────┬─────────┘  └─────────┬──────────┘  └────────┬─────────┘  │
 │            │                      │                      │            │
 │            ▼                      ▼                      ▼            │
 │  ┌─────────────────────────────────────────────────────────────────┐  │
 │  │  For each entity in chunk:                                     │  │
 │  │                                                                │  │
 │  │    For byte j in 0..stateSize:                                │  │
 │  │      toRemove = ~state[j] & previous[j]                       │  │
 │  │      toAdd    =  state[j] & ~previous[j]                      │  │
 │  │                                                                │  │
 │  │      For bit r in 0..7:                                       │  │
 │  │        mask = (1 << r)                                        │  │
 │  │        bit = (j * 8) + r                                      │  │
 │  │                                                                │  │
 │  │        ┌─────────────────────────────────────────────────┐     │  │
 │  │        │  if (mask & toRemove):                           │     │  │
 │  │        │    lookup bit in RegisteredStates                │     │  │
 │  │        │    CommandBuffer.RemoveComponent(entity, tag)   │     │  │
 │  │        │                                                 │     │  │
 │  │        │  else if (mask & toAdd):                        │     │  │
 │  │        │    lookup bit in RegisteredStates                │     │  │
 │  │        │    CommandBuffer.AddComponent(entity, tag)      │     │  │
 │  │        │    (warn if bit not registered)                  │     │  │
 │  │        └─────────────────────────────────────────────────┘     │  │
 │  │                                                                │  │
 │  │    previous = state  (copy current to previous)               │  │
 │  └─────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────┘
```

## Bitmask-to-Component Mapping

```
  State Byte:  0b11010010
               │││ │ │└── bit 0 → key=0  → RegisteredStates[0] = TagIdle
               │││ │ └─── bit 1 → key=1  → RegisteredStates[1] = TagRunning
               │││ └───── bit 2 → key=2  → (not registered, skip)
               ││└─────── bit 3 → key=3  → RegisteredStates[3] = TagCrouching
               │└──────── bit 4 → key=4  → (not registered, skip)
               └───────── bit 5 → key=5  → RegisteredStates[5] = TagAttacking
                     bit 6 → key=6  → RegisteredStates[6] = TagBlocking
                     bit 7 → key=7  → RegisteredStates[7] = TagStunned

  Multi-byte state (e.g. 2 bytes = 16 bits):
  Byte 0: bits 0-7  → keys 0-7
  Byte 1: bits 0-7  → keys 8-15

  Example transition:
  Previous: 0b11010010 → Tags: Idle, Running, Crouching, Attacking
  State:    0b10011010 → Tags: Idle, Running, Blocking, Stunned

  Diff:
    toRemove = ~State & Prev = ~10011010 & 11010010 = 01100101 & 11010010 = 01010000
               bit 4 and bit 6 → Remove TagAttacking, Remove TagCrouching

    toAdd    = State & ~Prev  = 10011010 & 00101101 = 00001000
               bit 3 and bit 6 → Add TagBlocking, Add TagStunned
```

## Registration Flow

```
  ┌──────────────────────────────────────────────────────────────────┐
 │  StateImpl Constructor                                          │
 │                                                                  │
 │  1. StateInstanceUtil.GetAllStateInstances(ref state)            │
 │     Queries ALL systems for StateInstance components             │
 │                                                                  │
 │  2. For each StateInstance:                                      │
 │     ┌─────────────────────────────────────────────────────────┐  │
 │     │  IF component.State.Index matches our StateType:        │  │
 │     │    key = component.StateKey (byte)                      │  │
 │     │    tag = ComponentType.FromTypeIndex(                    │  │
 │     │            component.StateInstanceComponent)             │  │
 │     │    RegisteredStatesMap.TryAdd(key, tag)                 │  │
 │     │    (log error if duplicate key)                          │  │
 │     └─────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  Result: HashMap<byte, ComponentType> mapping bit positions      │
 │  to tag components. Up to 256 entries (byte key space).          │
 └──────────────────────────────────────────────────────────────────┘
```

## Entity State Transition Example

```
  Entity E:  State=0b00000101, Previous=0b00000000

  Step 1: Compute diffs
  ┌────────────────────────────────────────────────────────────┐
  │  toRemove = ~0b00000101 & 0b00000000 = 0b00000000         │
  │  toAdd    =  0b00000101 & ~0b00000000 = 0b00000101        │
  │                                                             │
  │  bit 0 → ADD  → ECB.AddComponent(TagWalking)               │
  │  bit 2 → ADD  → ECB.AddComponent(TagArmed)                 │
  └────────────────────────────────────────────────────────────┘

  Step 2: Update previous
  Previous = State = 0b00000101

  ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─

  Next frame: State changes to 0b00000001

  Step 1: Compute diffs
  ┌────────────────────────────────────────────────────────────┐
  │  toRemove = ~0b00000001 & 0b00000101 = 0b00000100         │
  │  toAdd    =  0b00000001 & ~0b00000101 = 0b00000000        │
  │                                                             │
  │  bit 2 → REMOVE → ECB.RemoveComponent(TagArmed)            │
  └────────────────────────────────────────────────────────────┘

  Step 2: Update previous
  Previous = State = 0b00000001
```

## Key Design Decisions

1. **Multi-byte State**: Unlike StateModel (single byte), StateFlagModel supports arbitrary byte widths, enabling up to 64 concurrent state flags (8 bytes × 8 bits).

2. **Bit-Level Independence**: Each bit is treated as an independent on/off flag mapping to a tag component. Multiple states can be active simultaneously.

3. **EntityCommandBuffer**: Tag add/remove operations are deferred via ECB to avoid structural changes during iteration.

4. **Change Version Filter**: The query only processes chunks where the state component has changed, skipping unchanged entities entirely.

5. **StateImpl Reuse**: Shares the `StateImpl` infrastructure with `StateModel`, including the `RegisteredStatesMap` populated from `StateInstance` components on systems.

6. **Dynamic Component Handles**: Uses `DynamicComponentTypeHandle` to support any state component type regardless of size, reinterpreted as byte arrays.

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/States/StateFlagModel.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/StateFlagModel.cs)
- [BovineLabs.Core.Tests/States/StateFlagModelTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/States/StateFlagModelTests.cs)
- [BovineLabs.Core/States/StateFlagModelWithHistory.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/StateFlagModelWithHistory.cs)
