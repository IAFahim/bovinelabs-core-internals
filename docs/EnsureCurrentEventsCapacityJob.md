# EnsureCurrentEventsCapacityJob

## Inner Workings Diagram

```
 EnsureCurrentEventsCapacityJob
 ======================================================================
 Defined as: EnsureCurrentEventsCapacityJob
 Namespace:  BovineLabs.Core.PhysicsStates

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ EnsureCurrentEventsCapacityJob                                     │
 ├────────────────────────────────────────────────────────────────────┤
 │ NativeHashSet<TC>        CurrentEvents                             │
 │ NativeReference<int>     ForeachCount                              │
 │ int                      EventsPerRead                             │
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

- [BovineLabs.Core.Extensions/PhysicsStates/Jobs/EnsureCurrentEventsCapacityJob.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Jobs/EnsureCurrentEventsCapacityJob.cs)
