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

## Source

- [BovineLabs.Core.Extensions/Settings/SingletonInitializedSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Settings/SingletonInitializedSystem.cs)
