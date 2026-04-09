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

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/EntityBlobBakingSystem.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
EntityBlobBakingSystem
  Namespace: BovineLabs.Core.Authoring.Blobs
  Kind: partial struct
  Implements: ISystem, ISystemCompilerGenerated
  Attributes: WorldSystemFilterAttribute, CompilerGeneratedAttribute, BurstCompileAttribute

  Lifecycle Methods:
    + OnCreate
    + OnDestroy
    + OnUpdate

  Fields:
    NativeHashMap`2 tempBlobMap
    BlobAssetStore sceneBlobStore
    BlobAssetStore localBlobAssetStore
    TypeHandle __TypeHandle
    EntityQuery __query_451793246_0
    EntityQuery __query_451793246_1

Verified: 10 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakingSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakingSystem.cs)
