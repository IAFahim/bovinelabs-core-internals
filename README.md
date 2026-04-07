# StatefulCollisionEventClearSystem

## Inner Workings Diagram

```
 StatefulCollisionEventClearSystem
 ======================================================================
 Defined as: StatefulCollisionEventClearSystem
 Namespace:  BovineLabs.Core.PhysicsStates


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 │ Execute(ref DynamicBuffer<StatefulCollisionEvent> eventBuf)        │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventClearSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulCollisionEventClearSystem.cs)
