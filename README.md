# InitializeEntitySystem

## Inner Workings Diagram

```
 InitializeEntitySystem
 ======================================================================
 Defined as: InitializeEntitySystem
 Namespace:  BovineLabs.Core.LifeCycle

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ InitializeEntitySystem                                             │
 ├────────────────────────────────────────────────────────────────────┤
 │ ComponentTypeHandle<In   InitializeEntityHandle                    │
 │ ComponentTypeHandle<In   InitializeSubSceneEntityHandle            │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/LifeCycle/InitializeEntitySystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/LifeCycle/InitializeEntitySystem.cs)
