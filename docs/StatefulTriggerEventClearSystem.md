# StatefulTriggerEventClearSystem

## Inner Workings Diagram

```
 StatefulTriggerEventClearSystem
 ======================================================================
 Defined as: StatefulTriggerEventClearSystem
 Namespace:  BovineLabs.Core.PhysicsStates


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 │ Execute(ref DynamicBuffer<StatefulTriggerEvent> eventBuffe)        │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventClearSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventClearSystem.cs)
