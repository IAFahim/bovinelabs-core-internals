# CalculateCurrentEventsBucketsJob

## Inner Workings Diagram

```
 CalculateCurrentEventsBucketsJob
 ======================================================================
 Defined as: CalculateCurrentEventsBucketsJob
 Namespace:  BovineLabs.Core.PhysicsStates

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CalculateCurrentEventsBucketsJob                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ NativeHashSet<TC>        CurrentEvents                             │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Execute()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateCurrentEventsBucketsJob.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateCurrentEventsBucketsJob.cs)
