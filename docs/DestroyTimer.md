# DestroyTimer

## Inner Workings Diagram

```
 DestroyTimer
 ======================================================================
 Defined as: DestroyTimer
 Namespace:  BovineLabs.Core.LifeCycle

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ DestroyTimer                                                       │
 ├────────────────────────────────────────────────────────────────────┤
 │ ComponentTypeHandle<De   EntityDestroyHandle                       │
 │ ComponentTypeHandle<T>   RemainingHandle                           │
 │ float                    DeltaTime                                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state, UpdateTimeJob job = default)       │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/LifeCycle/DestroyTimer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/LifeCycle/DestroyTimer.cs)
