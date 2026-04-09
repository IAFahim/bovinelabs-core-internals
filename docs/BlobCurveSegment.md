# BlobCurveSegment

## Inner Workings Diagram

```
 BlobCurveSegment
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ struct                   BlobCurveSegment                          │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Sample(in float4 timeSerial)                                       │
 │   → float                                                          │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobCurveSegment.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobCurveSegment
  Kind: readonly struct, 16 bytes

  Fields:
    float4 factors (initonly) (private)

  Constructors:
    BlobCurveSegment(float4 factors)
    BlobCurveSegment(Keyframe k0, Keyframe k1)

  Methods:
    Single Sample(float4& timeSerial)

Verified: 4 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSegment.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSegment.cs)
