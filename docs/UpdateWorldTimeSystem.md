# UpdateWorldTimeSystem

## Inner Workings Diagram

```
 UpdateWorldTimeSystem
 ======================================================================
 Defined as: UpdateWorldTimeSystem
 Namespace:  BovineLabs.Core


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnStartRunning(ref SystemState state)                              │
 │   → void                                                           │
 │ OnStopRunning(ref SystemState state)                               │
 │   → void                                                           │
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

- [BovineLabs.Core.Extensions/Time/UpdateWorldTimeSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Time/UpdateWorldTimeSystem.cs)
