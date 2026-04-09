# ConfigVarPanel

## Inner Workings Diagram

```
 ConfigVarPanel
 ======================================================================
 Defined as: ConfigVarPanel
 Namespace:  BovineLabs.Core.Editor.ConfigVars

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ConfigVarPanel                                                     │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   DisplayName                               │
 │ string                   GroupName                                 │
 │ bool                     IsEmpty                                   │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnActivate(string searchContext, VisualElement rootElement)        │
 │   → void                                                           │
 │ OnDeactivate()                                                     │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
ConfigVars: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Editor/ConfigVars/ConfigVarPanel.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/ConfigVars/ConfigVarPanel.cs)
