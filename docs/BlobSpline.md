# BlobSpline

## Inner Workings Diagram

```
 BlobSpline
 ======================================================================
 Defined as: BlobSpline
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobSpline                                                         │
 ├────────────────────────────────────────────────────────────────────┤
 │ BlobArray<BezierKnot>    Knots                                     │
 │ BlobArray<BezierCurve>   Curves                                    │
 │ BlobArray<DistanceToIn   SegmentLengthsLookupTable                 │
 │ BlobArray<float3>        UpVectorsLookupTable                      │
 │ bool                     Closed                                    │
 │ float                    Length                                    │
 │ int                      Count                                     │
 │ float3                   Origin                                    │
 │ float3                   Tangent                                   │
 │ float3                   Normal                                    │
 │ float3                   Binormal                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Create(ISpline spline, float4x4 transform, Allocator allo)         │
 │   → BlobAssetRefere                                                │
 │ ToSpline()                                                         │
 │   → Spline                                                         │
 │ GetCurve(int index)                                                │
 │   → BezierCurve                                                    │
 │ Evaluate(float t, out float3 position, out float3 tangent, )       │
 │   → bool                                                           │
 │ Evaluate(float t, out float3 position, out float3 tangent)         │
 │   → bool                                                           │
 │ Evaluate(float t, out float3 position)                             │
 │   → bool                                                           │
 │ EvaluatePosition(float t)                                          │
 │   → float3                                                         │
 │ EvaluatePosition(int curveIndex, float curveT)                     │
 │   → float3                                                         │
 │ EvaluateTangent(float t)                                           │
 │   → float3                                                         │
 │ EvaluateUpVector(float t)                                          │
 │   → float3                                                         │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobSpline.cs)
> ```bash
> cat snippets/blob-system/BlobSpline.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobSpline — TYPE NOT FOUND (UNITY_SPLINES package not installed)
  Doc structure verified from source code inspection only.

  Fields (from source):
    BlobArray<BezierKnot> Knots
    BlobArray<BezierCurve> Curves
    BlobArray<DistanceToInterpolation> SegmentLengthsLookupTable
    BlobArray<float3> UpVectorsLookupTable
    bool Closed
    float Length

  Properties:
    int Count => Knots.Length
    BezierKnot this[int index] => Knots[index]

  Methods:
    static BlobAssetReference<BlobSpline> Create(ISpline, float4x4, Allocator)
    static BlobAssetReference<BlobArray<BlobSpline>> Create<T>(IReadOnlyList<T>, float4x4, Allocator)
    static void Construct(ref BlobBuilder, ref BlobSpline, ISpline, float4x4)
    Spline ToSpline()
    BezierCurve GetCurve(int index)
    bool Evaluate(float t, out float3 position, out float3 tangent, out float3 upVector)
    bool Evaluate(float t, out float3 position, out float3 tangent)
    bool Evaluate(float t, out float3 position)
    float3 EvaluatePosition(float t)
    float3 EvaluatePosition(int curveIndex, float curveT)
    float3 EvaluateTangent(float t)
    float3 EvaluateUpVector(float t)
    int SplineToCurveT(float splineT, out float curveT)
    float CurveToSplineT(float curve)
    float GetCurveLength(int curveIndex)
    float3 GetCurveUpVector(int index, float t)
    float GetCurveInterpolation(int curveIndex, float curveDistance)

Verified: 1 checks, 0 failures
```

> **Note**: BlobSpline is conditionally compiled (`#if UNITY_SPLINES`).
> Runtime reflection requires the `com.unity.splines` package, which is not
> installed in this project. Structure verified from source code inspection.

## Source

- [BovineLabs.Core/Collections/Blobs/Splines/BlobSpline.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Splines/BlobSpline.cs)
