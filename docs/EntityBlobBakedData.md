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

## Source

- [BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakedData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Authoring/Blobs/EntityBlobBakedData.cs)
