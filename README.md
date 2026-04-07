# CloneTransformSystem

## Inner Workings Diagram

```
 CloneTransformSystem
 ======================================================================
 Defined as: CloneTransformSystem
 Namespace:  BovineLabs.Core.Clone

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CloneTransformSystem                                               │
 ├────────────────────────────────────────────────────────────────────┤
 │ ComponentLookup<LocalT   LocalTransforms                           │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/Clone/CloneTransformSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Clone/CloneTransformSystem.cs)
