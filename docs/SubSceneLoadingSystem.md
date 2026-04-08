# SubSceneLoadingSystem

## Inner Workings Diagram

```
 SubSceneLoadingSystem
 ======================================================================
 Namespace:  BovineLabs.Core.SubScenes


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnStartRunning(ref SystemState state)                              │
 │   → void                                                           │
 │ OnStopRunning(ref SystemState state)                               │
 │   → void                                                           │
 │ OnDestroy(ref SystemState state)                                   │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/SubScenes/SubSceneLoadingSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/SubScenes/SubSceneLoadingSystem.cs)
