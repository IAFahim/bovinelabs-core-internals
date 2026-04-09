# IBlobCurve

## Inner Workings Diagram

```
 IBlobCurve
 ======================================================================
 Defined as: IBlobCurve
 Namespace:  BovineLabs.Core.Collections

```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/IBlobCurve.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
IBlobCurve<T>
  Kind: interface, covariant T

  Methods:
    Single EvaluateIgnoreWrapMode(Single& time, BlobCurveCache& cache)
    Single EvaluateIgnoreWrapMode(Single& time)
    Single Evaluate(Single& time, BlobCurveCache& cache)
    Single Evaluate(Single& time)

  Known Implementors:
    BlobCurve implements IBlobCurve<float>: True
    BlobCurve2 implements IBlobCurve: True

Verified: 7 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/IBlobCurve.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/IBlobCurve.cs)
