# BovineLabsBootstrap

## Inner Workings Diagram

```
 BovineLabsBootstrap
 ======================================================================
 Defined as: BovineLabsBootstrap
 Namespace:  BovineLabs.Core

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BovineLabsBootstrap                                                │
 ├────────────────────────────────────────────────────────────────────┤
 │ Action<World>            GameWorldCreated                          │
 │ World                    ServiceWorld                              │
 │ World                    GameWorld                                 │
 │ BovineLabsBootstrap      Instance                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Initialize(string defaultWorldName)                                │
 │   → bool                                                           │
 │ CreateGameWorld()                                                  │
 │   → void                                                           │
 │ DestroyGameWorld()                                                 │
 │   → void                                                           │
 │ CreateMenuWorld()                                                  │
 │   → void                                                           │
 │ DestroyMenuWorld()                                                 │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/Utility/BovineLabsBootstrap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Utility/BovineLabsBootstrap.cs)
