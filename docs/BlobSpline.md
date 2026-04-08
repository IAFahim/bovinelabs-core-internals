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

## Source

- [BovineLabs.Core/Collections/Blobs/Splines/BlobSpline.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Splines/BlobSpline.cs)
