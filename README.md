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

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobShared.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobShared.cs)
