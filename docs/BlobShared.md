# BlobShared

## Inner Workings Diagram

```
 BlobShared
 ======================================================================
 Namespace:  BovineLabs.Core.Collections


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ PowerSerial(float t)                                               │
 │   → float4                                                         │
 │ UnityFactor(float v0, float t0, float t1, float v1, float dura)    │
 │   → float4                                                         │
 │ HermiteFactor(float v0, float m0, float m1, float v1)              │
 │   → float4                                                         │
 │ BezierFactor(float p0, float p1, float p2, float p3)               │
 │   → float4                                                         │
 │ LinearFactor(float p0, float p3)                                   │
 │   → float4                                                         │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobShared.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobShared
  Kind: static class

  Static Methods:
    float4 PowerSerial(Single t)
    WrapMode ConvertWrapMode(WrapMode mode)
    float4 UnityFactor(Single v0, Single t0, Single t1, Single v1, Single duration)
    float4 HermiteFactor(Single v0, Single m0, Single m1, Single v1)
    float4 BezierFactor(Single p0, Single p1, Single p2, Single p3)
    float4 LinearFactor(Single p0, Single p3)

  Runtime Verification:
    PowerSerial(0.5) = (0.125, 0.25, 0.5, 1)
    PowerSerial(0.0) = (0, 0, 0, 1)
    PowerSerial(1.0) = (1, 1, 1, 1)

Verified: 12 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobShared.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobShared.cs)
