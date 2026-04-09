# StatefulTriggerEvent

**Converts immediate physics triggers into persistent enter/stay/exit states**

`StatefulTriggerEvent` is the trigger counterpart to `StatefulCollisionEvent`.
It stores trigger overlap events with Enter/Stay/Exit state tracking, using the
same double-buffered event comparison pattern.

---

## Event State Machine

```
  ┌──────────────────────────────────────────────────────────┐
  │  Identical state machine to collision events:            │
  │                                                          │
  │  Trigger this frame & previous → Stay                    │
  │  Trigger this frame & NOT prev  → Enter                  │
  │  Trigger NOT this & previous    → Exit                   │
  │                                                          │
  │   Previous ╲  Current │  Yes   │  No     │               │
  │  ───────────╲─────────┼────────┼─────────│               │
  │              │  Yes    │ STAY   │ EXIT    │               │
  │              │  No     │ ENTER  │ (none)  │               │
  └──────────────────────────────────────────────────────────┘
```

## StatefulTriggerEvent Buffer Element

```
  ┌──────────────────────────────────────────────────────────┐
  │  StatefulTriggerEvent : IBufferElementData               │
  │  [InternalBufferCapacity(0)]                             │
  │                                                          │
  │  ┌──────────┬───────────┬───────────┬──────────┬────────┐│
  │  │ EntityB  │ BodyIdxA  │ BodyIdxB  │ ColKeyA  │ColKeyB ││
  │  │ (Entity) │   (int)   │   (int)   │(ColKey)  │(ColKey)││
  │  └──────────┴───────────┴───────────┴──────────┴────────┘│
  │  ┌──────────────────┐                                    │
  │  │ State            │  StatefulEventState (byte)         │
  │  │ (Enter/Stay/Exit)│                                    │
  │  └──────────────────┘                                    │
  │                                                          │
  │  NOTE: Simpler than collision events:                    │
  │    - No Normal field                                     │
  │    - No CollisionDetails                                 │
  │    - Triggers are overlap-only, no contact data          │
  └──────────────────────────────────────────────────────────┘
```

## System Pipeline

```
  ┌──────────────────────────────────────────────────────────────┐
  │  StatefulTriggerEventSystem : ISystem                         │
  │                                                              │
  │  [UpdateInGroup(PhysicsSystemGroup)]                         │
  │  [UpdateAfter(PhysicsSimulationGroup)]                       │
  │                                                              │
  │  OnUpdate:                                                   │
  │  ┌───────────────────────────────────────────────────────┐  │
  │  │ 1. Get SimulationSingleton                           │  │
  │  │    - Skip if NoPhysics or not ReadyForEventScheduling│  │
  │  │                                                     │  │
  │  │ 2. Get trigger events                                │  │
  │  │    - simulation.TriggerEvents                        │  │
  │  │    - Reinterpret as NativeStream                     │  │
  │  │    - CollectTriggerEvents reads TriggerEventData     │  │
  │  │      and creates StatefulTriggerEventContainer       │  │
  │  │                                                     │  │
  │  │ 3. Delegate to StatefulEventImpl.OnUpdate()          │  │
  │  │    (same shared infrastructure as collision events)  │  │
  │  └───────────────────────────────────────────────────────┘  │
  └──────────────────────────────────────────────────────────────┘
```

## Double-Buffer Comparison

```
  Frame N:

  ┌──────────────────────────────────────────────────────┐
  │ previousEvents (from frame N-1)                      │
  │ ┌─────────┬─────────┬─────────┐                      │
  │ │ (E1,E3) │ (E1,E5) │ (E2,E5) │                      │
  │ └─────────┴─────────┴─────────┘                      │
  │                                                      │
  │ currentEvents (from frame N physics)                 │
  │ ┌─────────┬─────────┬─────────┬─────────┐           │
  │ │ (E1,E3) │ (E1,E7) │ (E2,E5) │ (E4,E5) │           │
  │ └─────────┴─────────┴─────────┴─────────┘           │
  │                                                      │
  │ Result for Entity E1:                               │
  │   (E1,E3): was prev, still curr → Stay              │
  │   (E1,E5): was prev, NOT curr  → Exit               │
  │   (E1,E7): NOT prev, now curr  → Enter              │
  │                                                      │
  │ Result for Entity E2:                               │
  │   (E2,E5): was prev, still curr → Stay              │
  │                                                      │
  │ Result for Entity E4:                               │
  │   (E4,E5): NOT prev, now curr → Enter               │
  └──────────────────────────────────────────────────────┘
  
  After write → swap current/previous buffers for frame N+1
```

## Collision vs Trigger Comparison

```
  ┌───────────────────┬──────────────────┬──────────────────┐
  │ Aspect            │ CollisionEvent   │ TriggerEvent      │
  ├───────────────────┼──────────────────┼──────────────────┤
  │ Normal            │ Yes (float3)     │ No                │
  │ ContactDetails    │ Optional         │ No                │
  │ Events per read   │ 2 (bidirectionl) │ 1                 │
  │ Raw data source   │ CollisionEvents  │ TriggerEvents     │
  │                   │ (CollisionData)  │ (TriggerEventData)│
  │ Contact points    │ Yes              │ No                │
  │ Impulse           │ Yes              │ No                │
  └───────────────────┴──────────────────┴──────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core.Extensions/PhysicsStates/Data/StatefulTriggerEvent.cs`
`BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventSystem.cs`
`BovineLabs.Core.Extensions/PhysicsStates/StatefulEventImpl.cs`

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/Data/StatefulTriggerEvent.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Data/StatefulTriggerEvent.cs)
- [BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventSystem.cs)
- [BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventClearSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventClearSystem.cs)
