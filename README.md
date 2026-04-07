# ObjectDefinition

## Inner Workings Diagram

```
 ObjectDefinition
 ======================================================================
 Defined as: ObjectDefinition
 Namespace:  BovineLabs.Core.Authoring.ObjectManagement

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ObjectDefinition                                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ GameObject               Prefab                                    │
 │ int                      ID                                        │
 │ ObjectCategory           Categories                                │
 │ string                   FriendlyName                              │
 │ string                   Description                               │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions.Authoring/ObjectManagement/ObjectDefinition.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/ObjectManagement/ObjectDefinition.cs)
