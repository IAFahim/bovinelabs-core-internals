# PauseGame

## Inner Workings Diagram

```
 PauseGame
 ======================================================================
 Defined as: PauseGame
 Namespace:  BovineLabs.Core.Pause

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ PauseGame                                                          │
 ├────────────────────────────────────────────────────────────────────┤
 │ bool                     PauseAll                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ IsPaused(ref SystemState systemState)                              │
 │   → bool                                                           │
 │ Pause(ref SystemState systemState, bool pauseAll = false)          │
 │   → void                                                           │
 │ Unpause(ref SystemState systemState)                               │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/Pause/PauseGame.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Pause/PauseGame.cs)
