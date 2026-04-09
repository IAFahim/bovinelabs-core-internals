# StatefulCollisionEvent

**Converts immediate physics collisions into persistent enter/stay/exit states**

`StatefulCollisionEvent` is an `IBufferElementData` that stores collision events
with a state machine (Enter/Stay/Exit). The `StatefulCollisionEventSystem` reads
raw collision events from the physics simulation and tracks which collisions are
new, ongoing, or ended relative to the previous frame.

---

## Event State Machine

```
  ┌──────────────────────────────────────────────────────────────┐
  │              Collision Event State Machine                    │
  │                                                              │
  │         Collision exists   ──── this frame?                  │
  │              │                                               │
  │     ┌────────┴────────┐                                     │
  │     │                 │                                     │
  │     ▼                 ▼                                     │
  │  Previous frame?   Previous frame?                          │
  │     YES               NO                                    │
  │     │                 │                                     │
  │     ▼                 ▼                                     │
  │ ┌────────┐       ┌────────┐                                 │
  │ │ STAY   │       │ ENTER  │                                 │
  │ │ (was & │       │ (new   │                                 │
  │ │  still)│       │  collision)                               │
  │ └────────┘       └────────┘                                 │
  │                                                              │
  │         No collision this frame                             │
  │              │                                               │
  │     Was previous?                                           │
  │     YES │                                                   │
  │         ▼                                                   │
  │    ┌────────┐                                               │
  │    │  EXIT  │                                               │
  │    │ (was,  │                                               │
  │    │  ended)│                                               │
  │    └────────┘                                               │
  └──────────────────────────────────────────────────────────────┘
```

## StatefulEventState Enum

```
  ┌────────────────────────────────────────┐
  │  enum StatefulEventState : byte        │
  │  {                                     │
  │    Undefined = 0,  // Unknown/N/A      │
  │    Enter     = 1,  // New this frame   │
  │    Stay      = 2,  // Continuing       │
  │    Exit      = 3,  // Ended this frame │
  │  }                                     │
  └────────────────────────────────────────┘
```

## System Pipeline

```
  ┌──────────────────────────────────────────────────────────────┐
  │  StatefulCollisionEventSystem : ISystem                       │
  │  [UpdateInGroup(PhysicsSystemGroup)]                         │
  │  [UpdateAfter(PhysicsSimulationGroup)]                       │
  │                                                              │
  │  OnUpdate:                                                   │
  │  ┌─────────────────────────────────────────────────────────┐│
  │  │ 1. Get SimulationSingleton                             ││
  │  │    - Skip if NoPhysics or not ReadyForEventScheduling  ││
  │  │                                                       ││
  │  │ 2. Get collision events from simulation                ││
  │  │    - Reinterpret CollisionEvents as CollisionEventWrapper│
  │  │    - Extract EventDataStream (NativeStream)             ││
  │  │    - Extract InputVelocities, TimeStep                  ││
  │  │                                                       ││
  │  │ 3. Delegate to StatefulEventImpl.OnUpdate()            ││
  │  └─────────────────────────────────────────────────────────┘│
  └──────────────────────────────────────────────────────────────┘
```

## StatefulEventImpl Flow

```
  Frame N:
  ┌────────────────────────────────────────────────────────────┐
  │                                                            │
  │  NativeStream (from physics sim)                           │
  │    ┌──────┬──────┬──────┬──────┬──────┐                   │
  │    │ Ev A │ Ev B │ Ev C │ Ev D │ Ev E │  raw events       │
  │    └──────┴──────┴──────┴──────┴──────┘                   │
  │         │                                                  │
  │         ▼                                                  │
  │  ┌──────────────────────────────────────────────────┐      │
  │  │ EnsureCapacity → CollectEventsJob (parallel)     │      │
  │  │                                                  │      │
  │  │  Builds:                                         │      │
  │  │   currentEventMap  [Entity → EventContainer]     │      │
  │  │   currentEvents    HashSet<EventContainer>       │      │
  │  └──────────────────┬───────────────────────────────┘      │
  │                     │                                      │
  │                     ▼                                      │
  │  ┌──────────────────────────────────────────────────┐      │
  │  │ CalculateEventMapBucketsJob                      │      │
  │  │ CalculateCurrentEventsBucketsJob                 │      │
  │  │  → RecalculateBuckets() for parallel lookup      │      │
  │  └──────────────────┬───────────────────────────────┘      │
  │                     │                                      │
  │                     ▼                                      │
  │  ┌──────────────────────────────────────────────────┐      │
  │  │ WriteEventsJob : IJobChunk                      │      │
  │  │                                                  │      │
  │  │ For each entity with StatefulCollisionEvent buf: │      │
  │  │                                                  │      │
  │  │  currentEventMap[entity] → new events:           │      │
  │  │    if previousEvents.Contains(event):            │      │
  │  │      state = Stay                                │      │
  │  │    else:                                         │      │
  │  │      state = Enter                               │      │
  │  │                                                  │      │
  │  │  previousEventMap[entity] → removed events:      │      │
  │  │    if !currentEvents.Contains(event):            │      │
  │  │      state = Exit                                │      │
  │  └──────────────────┬───────────────────────────────┘      │
  │                     │                                      │
  │                     ▼                                      │
  │  Swap: (current, previous) = (previous, current)           │
  │        ← double-buffer for next frame                      │
  └────────────────────────────────────────────────────────────┘
```

## Collision Event Buffer

```
  Entity A:
  ┌──────────────────────────────────────────────────────────┐
  │  DynamicBuffer<StatefulCollisionEvent>                    │
  │  [InternalBufferCapacity(0)]  ← always heap allocated    │
  │                                                          │
  │  ┌─────────────────────────────────────────────────────┐ │
  │  │ EntityB │ BodyIdxA │ BodyIdxB │ ColKeyA │ ColKeyB  │ │
  │  │  Entity │    int   │   int    │  CKey   │  CKey    │ │
  │  │    5    │    0     │    0     │   -     │   -      │ │
  │  ├─────────┼──────────┼──────────┼─────────┼──────────┤ │
  │  │ State   │ Normal   │ CollisionDetails                   │ │
  │  │ Enter   │ (0,1,0)  │ (2 contacts, 0.5 impulse, avg)  │ │
  │  ├─────────┼──────────┼──────────┼────────────────────────┤ │
  │  │  Entity 7 │ Stay  │ (-1,0,0) │ default (no details)  │ │
  │  ├───────────┼───────┼──────────┼───────────────────────┤ │
  │  │  Entity 3 │ Exit  │ (0,0,1)  │ default              │ │
  │  └───────────┴───────┴──────────┴───────────────────────┘ │
  └──────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core.Extensions/PhysicsStates/Data/StatefulCollisionEvent.cs`
`BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventSystem.cs`
`BovineLabs.Core.Extensions/PhysicsStates/StatefulEventImpl.cs`

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/Data/StatefulCollisionEvent.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Data/StatefulCollisionEvent.cs)
- [BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventSystem.cs)
- [BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventClearSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventClearSystem.cs)
