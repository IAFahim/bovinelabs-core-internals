# StateFlagModelTests

## Inner Workings Diagram

```
 StateFlagModelTests
 ======================================================================
 Defined as: StateFlagModelTests
 Namespace:  BovineLabs.Core.Tests.States

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ StateFlagModelTests                                                │
 ├────────────────────────────────────────────────────────────────────┤
 │ BitArray16               Value                                     │
 │ BitArray16               Value                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Flags()                                                            │
 │   → void                                                           │
 │ OnStartRunning(ref SystemState state)                              │
 │   → void                                                           │
 │ OnStopRunning(ref SystemState state)                               │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Tests/States/StateFlagModelTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/States/StateFlagModelTests.cs)
