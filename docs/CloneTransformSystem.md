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

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/Clone/CloneTransformSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Clone/CloneTransformSystem.cs)
