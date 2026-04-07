# EntityStorageInfoLookupExtensions.GetNameUnsafe

## Inner Workings Diagram

```
 EntityStorageInfoLookupExtensions.GetNameUnsafe
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    EntityStorageInfoLookupExtensions         │
 │ AtomicSafetyHandle       Safety                                    │
 │ EntityDataAccess*        EntityDataAccess                          │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetNameUnsafe(this EntityStorageInfoLookup lookup, Entity entity)  │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Extensions/EntityStorageInfoLookupExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityStorageInfoLookupExtensions.cs)
