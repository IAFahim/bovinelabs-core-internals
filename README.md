# SyncEnableStateUtil

## Inner Workings Diagram

```
 SyncEnableStateUtil
 ======================================================================
 Defined as: SyncEnableStateUtil
 Namespace:  BovineLabs.Core.Utility

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SyncEnableStateUtil                                                │
 ├────────────────────────────────────────────────────────────────────┤
 │ ComponentTypeHandle<TP   ActivePreviousHandle                      │
 │ ComponentTypeHandle<T>   ActiveHandle                              │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state, bool includeDisabled = fals)       │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state, SetPreviousJob job = defaul)       │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Utility/SyncEnableStateUtil.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/SyncEnableStateUtil.cs)
