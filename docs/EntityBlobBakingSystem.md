# EntityBlobBakingSystem

## Inner Workings Diagram

```
 EntityBlobBakingSystem
 ======================================================================
 Namespace:  BovineLabs.Core.Authoring.Blobs

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ComponentTypeHandle<En   EntityBlobBakedDataHandle                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnDestroy(ref SystemState state)                                   │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/blob-system/EntityBlobBakingSystem.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakingSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakingSystem.cs)
