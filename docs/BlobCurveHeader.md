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

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveHeader.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveHeader.cs)
