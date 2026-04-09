# BlobCurveSampler

## Inner Workings Diagram

```
 BlobCurveSampler
 ======================================================================
 Defined as: BlobCurveSampler
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobCurveSampler                                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ BlobAssetReference<Blo   Curve                                     │
 │ bool                     IsCreated                                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Evaluate(in float time)                                            │
 │   → float                                                          │
 │ EvaluateIgnoreWrapMode(in float time)                              │
 │   → float                                                          │
 │ EvaluateWithoutCache(in float time)                                │
 │   → float                                                          │
 │ EvaluateIgnoreWrapModeWithoutCache(in float time)                  │
 │   → float                                                          │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobCurveSampler.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobCurveSampler
  Kind: struct, 24 bytes

  Fields:
    BlobAssetReference`1 Curve (public)
    BlobCurveCache cache (private)

  Properties:
    Boolean IsCreated

  Methods:
    Single Evaluate(Single& time)
    Single EvaluateIgnoreWrapMode(Single& time)
    Single EvaluateWithoutCache(Single& time)
    Single EvaluateIgnoreWrapModeWithoutCache(Single& time)

Verified: 7 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSampler.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSampler.cs)
