# SpatialMap3

**High-performance parallel 3D grid hashing for fast proximity queries**

`SpatialMap3<T>` extends the spatial map concept into 3D, using `int3` quantized
coordinates and a `long` hash key to support the much larger key space of a 3D grid.
Uses `NativeParallelMultiHashMap<long, int>` for storage.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                       SpatialMap3<T>                         │
│                                                              │
│  where T : unmanaged, ISpatialPosition3 (float3)            │
│                                                              │
│  ┌────────────────────────────────────────────────────┐     │
│  │  float quantizeStep                                 │     │
│  │  int   quantizeSize  (= quantizeWidth = quantDepth) │     │
│  │  int3  halfSize                                     │     │
│  │                                                    │     │
│  │  NativeParallelMultiHashMap<long, int> map          │     │
│  │    key   = long hash of int3 cell                   │     │
│  │    value = entity index                             │     │
│  └────────────────────────────────────────────────────┘     │
└──────────────────────────────────────────────────────────────┘
```

## 3D Quantization and Hashing

```
  Quantization (3D):
    quantized.x = floor((position.x + halfSize.x) / step)
    quantized.y = floor((position.y + halfSize.y) / step)
    quantized.z = floor((position.z + halfSize.z) / step)

  Hash (3D row-major → long):
    hash = quantized.x
         + quantized.y * width
         + quantized.z * width * depth

  ┌───────────────────────────────────────────┐
  │         3D Grid Visualization             │
  │                                           │
  │        z                                  │
  │       /│                                  │
  │      / │   Layer z=1:                     │
  │     /  │   ┌───┬───┬───┐                │
  │    /   │   │ 36│ 37│ 38│                │
  │   /    │   ├───┼───┼───┤                │
  │  /     │   │ 39│ 40│ 41│                │
  │ /______│   ├───┼───┼───┤                │
  │  y     x   │ 42│ 43│ 44│                │
  │            └───┴───┴───┘                │
  │                                           │
  │            Layer z=0:                     │
  │            ┌───┬───┬───┐                 │
  │            │ 0 │ 1 │ 2 │                 │
  │            ├───┼───┼───┤                 │
  │            │ 3 │ 4 │ 5 │                 │
  │            ├───┼───┼───┤                 │
  │            │ 6 │ 7 │ 8 │                 │
  │            └───┴───┴───┘                 │
  │                                           │
  │  Example: width=3, depth=3               │
  │  Cell (1,1,1) → 1 + 1*3 + 1*3*3 = 13   │
  │  Cell (2,0,1) → 2 + 0*3 + 1*3*3 = 11   │
  └───────────────────────────────────────────┘
```

## Build Pipeline

```
  NativeArray<T> positions (ISpatialPosition3 → float3)
         │
         ▼
 ┌───────────────────────────────────┐
 │ ResizeNativeParallelHashMapJob    │  IJob
 │   map.Capacity = max(cap, count)  │
 │   map.Clear()                     │
 │   map.SetAllocatedIndexLength(n)  │
 └───────────────┬───────────────────┘
                 │
                 ▼
 ┌────────────────────────────────────────────┐
 │ QuantizeJob  (IJobFor, parallel)          │
 │                                            │
 │  Writes to unsafe bucket data arrays:      │
 │   long* keys   = (long*)buckets.keys       │
 │   int*  values = (int*)buckets.values      │
 │                                            │
 │  For each position:                        │
 │    int3 q = Quantized(pos3, step, halfSz3) │
 │    long h = Hash(q, width, depth)          │
 │    keys[i]   = h                           │
 │    values[i] = i                           │
 └───────────────┬────────────────────────────┘
                 │
                 ▼
 ┌───────────────────────────────────┐
 │ CalculateMap                      │  IJob
 │   map.RecalculateBuckets()        │
 └───────────────────────────────────┘
```

## SpatialMap3 vs SpatialMap (2D)

```
  ┌───────────────┬──────────────────┬──────────────────────┐
  │ Aspect        │ SpatialMap (2D)  │ SpatialMap3 (3D)     │
  ├───────────────┼──────────────────┼──────────────────────┤
  │ Position type │ float2 (ISpatial │ float3 (ISpatialPos3)│
  │               │ Position)        │                      │
  │ Quantized     │ int2             │ int3                 │
  │ Hash key      │ int              │ long (64-bit)        │
  │ Hash formula  │ x + y*w         │ x + y*w + z*w*d      │
  │ Grid capacity │ size²            │ size³                │
  │ Half-size     │ int2             │ int3                 │
  │ Map type      │ MultiHashMap     │ MultiHashMap         │
  │               │ <int,int>        │ <long,int>           │
  └───────────────┴──────────────────┴──────────────────────┘
```

## Source

`BovineLabs.Core/Spatial/SpatialMap3.cs`

## Source

- [BovineLabs.Core/Spatial/SpatialMap3.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Spatial/SpatialMap3.cs)
