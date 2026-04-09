# SystemStateExtensions.GetUnsafeEntityDataAccess

## Inner Workings Diagram

```
 SystemStateExtensions.GetUnsafeEntityDataAccess
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetInternalDependency(ref this SystemState system)                 │
 │   → JobHandle                                                      │
 │ GetUnsafeEntityDataAccess(ref this SystemState system)             │
 │   → UnsafeEntityDat                                                │
 │ GetUnsafeEnableableLookup(ref this SystemState system)             │
 │   → UnsafeEnableabl                                                │
 │ AddDependency(ref this SystemState state, TypeIndex typeIndex, b)  │
 │   → void                                                           │
 │ AddDependency(ref this SystemState state, ComponentType componen)  │
 │   → void                                                           │
 │ GetAllSystemDependencies(ref this SystemState state)               │
 │   → JobHandle                                                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/SystemStateExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/SystemStateExtensions.cs)
