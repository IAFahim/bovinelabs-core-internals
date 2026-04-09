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

## Verified Data

```
LifeCycle: TYPE NOT FOUND
Authoring: TYPE NOT FOUND
Verified: 0 checks, 2 failures
```

## Source

- [BovineLabs.Core.Extensions.Authoring/LifeCycle/LifeCycleAuthoring.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/LifeCycle/LifeCycleAuthoring.cs)
