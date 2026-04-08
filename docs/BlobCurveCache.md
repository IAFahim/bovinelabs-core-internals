# BlobCurveCache

## Inner Workings Diagram

```
 BlobCurveCache
 ======================================================================
 Defined as: BlobCurveCache
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobCurveCache                                                     │
 ├────────────────────────────────────────────────────────────────────┤
 │ BlobCurveCache           Empty                                     │
 │ float2                   NeighborhoodTimes                         │
 │ int                      Index                                     │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobCurveCache.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveCache.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveCache.cs)
