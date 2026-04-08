# StripLocalSystem

## Inner Workings Diagram

```
 StripLocalSystem
 ======================================================================
 Defined as: StripLocalSystem
 Namespace:  BovineLabs.Core.Stripping


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnDestroy(ref SystemState state)                                   │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/Stripping/StripLocalSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Stripping/StripLocalSystem.cs)
