# DestroyOnSubSceneUnloadSystem

## Inner Workings Diagram

```
 DestroyOnSubSceneUnloadSystem
 ======================================================================
 Defined as: DestroyOnSubSceneUnloadSystem
 Namespace:  BovineLabs.Core.LifeCycle

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ DestroyOnSubSceneUnloadSystem                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ ComponentTypeHandle<De   DestroyEntityHandle                       │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/LifeCycle/DestroyOnSubSceneUnloadSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/LifeCycle/DestroyOnSubSceneUnloadSystem.cs)
