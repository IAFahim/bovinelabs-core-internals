# SingletonInitializedSystem

## Inner Workings Diagram

```
 SingletonInitializedSystem
 ======================================================================
 Defined as: SingletonInitializedSystem
 Namespace:  BovineLabs.Core.Settings

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SingletonInitializedSystem                                         │
 ├────────────────────────────────────────────────────────────────────┤
 │ ComponentTypeHandle<Si   SingletonHandle                           │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
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

- [BovineLabs.Core.Extensions/Settings/SingletonInitializedSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Settings/SingletonInitializedSystem.cs)
