# GhostComponentAttribute

## Inner Workings Diagram

```
 GhostComponentAttribute
 ======================================================================
 Defined as: GhostComponentAttribute
 Namespace:  Unity.NetCode

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GhostComponentAttribute                                            │
 ├────────────────────────────────────────────────────────────────────┤
 │ GhostPrefabType          PrefabType                                │
 │ GhostSendType            SendTypeOptimization                      │
 │ SendToOwnerType          OwnerSendType                             │
 │ bool                     SendDataForChildEntity                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Wrappers/GhostComponentAttribute.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Wrappers/GhostComponentAttribute.cs)
