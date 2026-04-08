# ConfigVarManager

## Inner Workings Diagram

```
 ConfigVarManager
 ======================================================================
 Namespace:  BovineLabs.Core.ConfigVars

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ IConfigVarContainer>     All                                       │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Initialize()                                                       │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/ConfigVars/ConfigVarManager.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/ConfigVars/ConfigVarManager.cs)
