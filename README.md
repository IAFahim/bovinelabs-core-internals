# ComponentSystemBaseInternal.RequireSingletonForUpdate

## Inner Workings Diagram

```
 ComponentSystemBaseInternal.RequireSingletonForUpdate
 ======================================================================
 Namespace:  BovineLabs.Core.Internal


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetEntityQuery(this ComponentSystemBase system, params ComponentT) │
 │   → EntityQuery                                                    │
 │ GetEntityQuery(this ComponentSystemBase system, params EntityQuer) │
 │   → EntityQuery                                                    │
 │ RequireSingletonForUpdate(this ComponentSystemBase system, ComponentT
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Internal/ComponentSystemBaseInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/ComponentSystemBaseInternal.cs)
