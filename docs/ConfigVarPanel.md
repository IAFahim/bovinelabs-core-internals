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

## Source

- [BovineLabs.Core.Editor/ConfigVars/ConfigVarPanel.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/ConfigVars/ConfigVarPanel.cs)
