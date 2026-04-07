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

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateCurrentEventsBucketsJob.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Jobs/CalculateCurrentEventsBucketsJob.cs)
