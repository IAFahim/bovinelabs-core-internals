# BlobCurveHeader

## Inner Workings Diagram

```
 BlobCurveHeader
 ======================================================================
 Defined as: BlobCurveHeader
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobCurveHeader                                                    │
 ├────────────────────────────────────────────────────────────────────┤
 │ WrapMode                 WrapModePrev                              │
 │ WrapMode                 WrapModePost                              │
 │ int                      SegmentCount                              │
 │ float                    StartTime                                 │
 │ float                    EndTime                                   │
 │ BlobArray<float>         Times                                     │
 │ float                    Duration                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SearchIgnoreWrapMode(in float time, [NoAlias] ref BlobCurveCache cach
 │   → int                                                            │
 │ SearchIgnoreWrapMode(in float time, [NoAlias] out float t)         │
 │   → int                                                            │
 │ Search(in float time, [NoAlias] ref BlobCurveCache cache,)         │
 │   → int                                                            │
 │ Search(in float time, [NoAlias] out float t)                       │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobCurveHeader.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobCurveHeader
  Kind: struct, 24 bytes

  Fields:
    [0] WrapMode WrapModePrev (public)
    [2] WrapMode WrapModePost (public)
    [4] Int32 SegmentCount (public)
    [8] Single StartTime (public)
    [12] Single EndTime (public)
    [16] BlobArray`1 Times (private)

  Properties:
    Single Duration

  Nested Enum: WrapMode (underlying: Int16)
    Clamp = 0
    Loop = 1
    PingPong = 2

  Methods:
    Int32 SearchIgnoreWrapMode(Single& time, BlobCurveCache& cache, Single& t)
    Int32 SearchIgnoreWrapMode(Single& time, Single& t)
    Int32 Search(Single& time, BlobCurveCache& cache, Single& t)
    Int32 Search(Single& time, Single& t)

Verified: 15 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveHeader.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveHeader.cs)
