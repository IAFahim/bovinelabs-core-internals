# PauseUtility

## Inner Workings Diagram

```
 PauseUtility
 ======================================================================
 Namespace:  BovineLabs.Core.Pause

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ HashSet<Type>            UpdateWhilePaused                         │
 │ HashSet<Type>            DisableWhilePaused                        │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ UpdateAlwaysSystems(ComponentSystemGroup group)                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/Pause/PauseUtility.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Pause/PauseUtility.cs)
