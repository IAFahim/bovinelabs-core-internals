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

> [Run test snippet](../snippets/blob-system/EntityBlobBakedData.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakedData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakedData.cs)
