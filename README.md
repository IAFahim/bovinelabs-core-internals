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

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventClearSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/StatefulTriggerEventClearSystem.cs)
