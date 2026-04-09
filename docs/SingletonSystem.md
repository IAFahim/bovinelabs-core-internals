# SingletonSystem

## Inner Workings Diagram

```
 SingletonSystem
 ======================================================================
 Namespace:  BovineLabs.Core.Settings

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ComponentType            ComponentType                             │
 │ Entity                   Entity                                    │
 │ EntityQuery              Query                                     │
 │ DynamicComponentTypeHa   TypeHandle                                │
 │ List<ComponentType>      Components                                │
 └────────────────────────────────────────────────────────────────────┘

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

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/Settings/SingletonSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Settings/SingletonSystem.cs)
