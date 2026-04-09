# ChangeFilterTrackingSystem

## Inner Workings Diagram

```
 ChangeFilterTrackingSystem
 ======================================================================
 Defined as: ChangeFilterTrackingSystem
 Namespace:  BovineLabs.Core.Editor.ChangeFilterTracking

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ChangeFilterTrackingSystem                                         │
 ├────────────────────────────────────────────────────────────────────┤
 │ SharedStatic<bool>       IsEnabled                                 │
 │ SharedStatic<float>      WarningLevel                              │
 │ NativeArray<TypeTrack>   TypeTracks                                │
 │ FixedString128Bytes      TypeName                                  │
 │ DynamicComponentTypeHa   DynamicTypeHandle                         │
 │ EntityQuery              Query                                     │
 │ NativeArray<int>         Changed                                   │
 │ NativeArray<int>         Chunks                                    │
 │ NativeArray<float>       Result                                    │
 │ NativeReference<bool>    HasWarned                                 │
 │ NativeReference<float>   Short                                     │
 │ NativeReference<float>   Long                                      │
 │ FakeDynamicComponentTy   DynamicTypeHandle                         │
 │ NativeArray<int>         Changed                                   │
 │ NativeArray<int>         Chunks                                    │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnDestroy(ref SystemState state)                                   │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
ChangeFilterTracking: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Editor/ChangeFilterTracking/ChangeFilterTrackingSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/ChangeFilterTracking/ChangeFilterTrackingSystem.cs)
