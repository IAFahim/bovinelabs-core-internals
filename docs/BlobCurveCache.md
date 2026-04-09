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

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobCurveCache.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobCurveCache
  Kind: struct, 12 bytes

  Fields:
    float2 NeighborhoodTimes (public)
    Int32 Index (public)

  Static Fields:
    BlobCurveCache Empty (static)

  Runtime Constants:
    Empty.Index = -2147483648 (expect int.MinValue = -2147483648)
    Empty.NeighborhoodTimes = (NaN, NaN) (expect NaN, NaN)

Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveCache.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveCache.cs)
