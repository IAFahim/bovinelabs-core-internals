# BlobCurve2_3_4

## Inner Workings Diagram

```
 BlobCurve2_3_4
 ======================================================================
 Defined as: BlobCurve2
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobCurve2                                                         │
 ├────────────────────────────────────────────────────────────────────┤
 │ BlobCurveHeader          Header                                    │
 │ BlobArray<float>         Times                                     │
 │ int                      SegmentCount                              │
 │ float                    StartTime                                 │
 │ float                    EndTime                                   │
 │ float                    Duration                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ EvaluateIgnoreWrapMode(in float time, [NoAlias] ref BlobCurveCache ca
 │   → float2                                                         │
 │ EvaluateIgnoreWrapMode(in float time)                              │
 │   → float2                                                         │
 │ Evaluate(in float time, [NoAlias] ref BlobCurveCache cache)        │
 │   → float2                                                         │
 │ Evaluate(in float time)                                            │
 │   → float2                                                         │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobCurve2_3_4.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobCurve2
  Kind: struct, 32 bytes
  Implements: IBlobCurve`1
  Properties:
    BlobCurveHeader& Header { get }
    BlobArray`1& Times { get }
    WrapMode WrapModePrev { get }
    WrapMode WrapModePost { get }
    Int32 SegmentCount { get }
    Single StartTime { get }
    Single EndTime { get }
    Single Duration { get }
  Methods:
    float2 EvaluateIgnoreWrapMode(Single&, BlobCurveCache&)
    float2 EvaluateIgnoreWrapMode(Single&)
    float2 Evaluate(Single&, BlobCurveCache&)
    float2 Evaluate(Single&)

BlobCurve3
  Kind: struct, 32 bytes
  Implements: IBlobCurve`1
  Properties:
    BlobCurveHeader& Header { get }
    BlobArray`1& Times { get }
    WrapMode WrapModePrev { get }
    WrapMode WrapModePost { get }
    Int32 SegmentCount { get }
    Single StartTime { get }
    Single EndTime { get }
    Single Duration { get }
  Methods:
    float3 EvaluateIgnoreWrapMode(Single&, BlobCurveCache&)
    float3 EvaluateIgnoreWrapMode(Single&)
    float3 Evaluate(Single&, BlobCurveCache&)
    float3 Evaluate(Single&)

BlobCurve4
  Kind: struct, 32 bytes
  Implements: IBlobCurve`1
  Properties:
    BlobCurveHeader& Header { get }
    BlobArray`1& Times { get }
    WrapMode WrapModePrev { get }
    WrapMode WrapModePost { get }
    Int32 SegmentCount { get }
    Single StartTime { get }
    Single EndTime { get }
    Single Duration { get }
  Methods:
    float4 EvaluateIgnoreWrapMode(Single&, BlobCurveCache&)
    float4 EvaluateIgnoreWrapMode(Single&)
    float4 Evaluate(Single&, BlobCurveCache&)
    float4 Evaluate(Single&)

Verified: 12 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurve2.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurve2.cs)
