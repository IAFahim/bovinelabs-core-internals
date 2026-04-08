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

> [Run test snippet](../snippets/blob-system/BlobCurveSampler.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSampler.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSampler.cs)
