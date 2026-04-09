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

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Internal/ComponentSystemBaseInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/ComponentSystemBaseInternal.cs)
