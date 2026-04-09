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

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/LifeCycle/DestroyOnSubSceneUnloadSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/LifeCycle/DestroyOnSubSceneUnloadSystem.cs)
