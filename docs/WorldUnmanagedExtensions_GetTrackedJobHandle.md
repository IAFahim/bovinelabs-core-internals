# WorldUnmanagedExtensions.GetTrackedJobHandle

## Inner Workings Diagram

```
 WorldUnmanagedExtensions.GetTrackedJobHandle
 ======================================================================
 Defined as: DependencyHandle
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ DependencyHandle                                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ struct                   ComponentDependencyManagerClone           │
 │ int                      MaxWriteJobHandles                        │
 │ int                      MaxReadJobHandles                         │
 │ ushort*                  TypeArrayIndices                          │
 │ DependencyHandle*        DependencyHandles                         │
 │ ushort                   DependencyHandlesCount                    │
 │ JobHandle*               ReadJobFences                             │
 │ TypeIndex                EntityTypeIndex                           │
 │ JobHandle                ExclusiveTransactionDependency            │
 │ byte                     IsInTransaction                           │
 │ ProfilerMarker           Marker                                    │
 │ WorldUnmanaged           World                                     │
 │ ComponentSafetyHandles   Safety                                    │
 │ ForEachDisallowStructu   ForEachStructuralChange                   │
 │ JobHandle                WriteFence                                │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetAllSystemDependencies(this WorldUnmanaged world, NativeList<JobHan
 │   → void                                                           │
 │ GetTrackedJobHandle(this WorldUnmanaged world)                     │
 │   → JobHandle                                                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
ComponentDependencyManagerClone: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/WorldUnmanagedExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/WorldUnmanagedExtensions.cs)
