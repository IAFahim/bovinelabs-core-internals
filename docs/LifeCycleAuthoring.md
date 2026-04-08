# LifeCycleAuthoring

## Inner Workings Diagram

```
 LifeCycleAuthoring
 ======================================================================
 Defined as: LifeCycleAuthoring
 Namespace:  BovineLabs.Core.Authoring.LifeCycle


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ AddComponents(IBaker baker, Entity entity, bool isPrefab)          │
 │   → void                                                           │
 │ Bake(LifeCycleAuthoring authoring)                                 │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions.Authoring/LifeCycle/LifeCycleAuthoring.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/LifeCycle/LifeCycleAuthoring.cs)
