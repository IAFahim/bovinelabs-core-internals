# PrefabInstance

## Inner Workings Diagram

```
 PrefabInstance
 ======================================================================
 Defined as: PrefabInstanceBake
 Namespace:  BovineLabs.Core.Authoring.BakeFast

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ PrefabInstanceBake                                                 │
 ├────────────────────────────────────────────────────────────────────┤
 │ Entity                   Prefab                                    │
 │ float4x4                 Transform                                 │
 │ bool                     IsStatic                                  │
 │ GameObject?              Prefab                                    │
 │ EntityGuid               PrefabGUID                                │
 │ Entity                   Value                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CreatePrefabInstances()                                            │
 │   → void                                                           │
 │ Bake(PrefabInstance authoring)                                     │
 │   → void                                                           │
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnDestroy(ref SystemState state)                                   │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
BakeFast: TYPE NOT FOUND
Authoring: TYPE NOT FOUND
Verified: 0 checks, 2 failures
```

## Source

- [BovineLabs.Core.Extensions.Authoring/BakeFast/PrefabInstance.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/BakeFast/PrefabInstance.cs)
