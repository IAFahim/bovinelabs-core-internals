# CollectEventsJob

## Inner Workings Diagram

```
 CollectEventsJob
 ======================================================================
 Defined as: ICollectsEventsImpl
 Namespace:  BovineLabs.Core.PhysicsStates

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ TC>                      CurrentEventMap                           │
 │ NativeHashSet<TC>        CurrentEvents                             │
 │ TI                       EventCollector                            │
 │ int                      EventsPerRead                             │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Execute(int startIndex, int count)                                 │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/PhysicsStates/Jobs/CollectEventsJob.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/PhysicsStates/Jobs/CollectEventsJob.cs)
