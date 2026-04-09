# EntityBlobBakedData

## Inner Workings Diagram

```
 EntityBlobBakedData
 ======================================================================
 Defined as: EntityBlobBakedData
 Namespace:  BovineLabs.Core.Authoring.Blobs

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ EntityBlobBakedData                                                │
 ├────────────────────────────────────────────────────────────────────┤
 │ Entity                   Target                                    │
 │ int                      Key                                       │
 │ BlobAssetReference<byt   Blob                                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/EntityBlobBakedData.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
EntityBlobBakedData
  Namespace: BovineLabs.Core.Authoring.Blobs
  Kind: struct, 24 bytes
  Implements: IQueryTypeParameter, IComponentData
  Attributes: BakingTypeAttribute

  Fields:
    Entity Target (public)
    Int32 Key (public)
    BlobAssetReference`1 Blob (public)

Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakedData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakedData.cs)
