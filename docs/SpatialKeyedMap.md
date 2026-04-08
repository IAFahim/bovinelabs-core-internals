# SpatialKeyedMap

**Maps spatial coordinates to IDs using tight integer-keyed buckets**

`SpatialKeyedMap<T>` quantizes 2D positions into a grid and stores them in a
`NativeKeyedMap<int>`, where each entry maps a spatial hash key to the original
index of the position. Unlike `LocalSpatialMap`, this uses native collections
rather than unsafe pointers and supports proper keyed bucket lookup.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                    SpatialKeyedMap<T>                         │
│                                                              │
│  where T : unmanaged, ISpatialPosition (float2)             │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Construction:                                       │   │
│  │    quantizeStep = step (cell size in world units)     │   │
│  │    quantizeSize = ceil(worldSize / step)              │   │
│  │    halfSize     = int2(worldSize / 2)                 │   │
│  │    map capacity = quantizeSize²                       │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  Storage:                                                    │
│  ┌────────────────────────────────────────────┐             │
│  │  NativeKeyedMap<int>   map                 │             │
│  │                                          │             │
│  │  ┌──────────┬──────────┐                 │             │
│  │  │ Key: int │ Val: int │                 │             │
│  │  │ (hash)   │ (index)  │                 │             │
│  │  ├──────────┼──────────┤                 │             │
│  │  │  85      │    0     │  ← entity 0    │             │
│  │  │  12      │    1     │  ← entity 1    │             │
│  │  │  85      │    3     │  ← entity 3    │             │
│  │  │  ...     │  ...     │                 │             │
│  │  └──────────┴──────────┘                 │             │
│  └────────────────────────────────────────────┘             │
└──────────────────────────────────────────────────────────────┘
```

## Build Pipeline (3-job chain)

```
  NativeArray<T> positions  (or NativeList<T> → deferred)
         │
         ▼
 ┌───────────────────────────┐
 │ ResizeNativeKeyedMapJob   │  IJob (single)
 │                           │
 │  if map.Capacity < count: │
 │    map.Capacity = count   │
 │  map.Clear()              │
 │  map.SetLength(count)     │
 └───────────┬───────────────┘
             │
             ▼
 ┌───────────────────────────┐
 │ QuantizeJob               │  IJobFor (parallel by worker count)
 │                           │
 │  Workers split the array: │
 │                            │
 │  Worker 0: [0 .. N/W)    │
 │  Worker 1: [N/W .. 2N/W) │
 │  ...                      │
 │  Last worker: remainder   │
 │                           │
 │  For each position:       │
 │    q = Quantized(pos)     │
 │    h = Hash(q)            │
 │    keys[i] = h            │
 │    values[i] = i          │  ← stores original index!
 │  (via GetUnsafeKeysPtr /  │
 │   GetUnsafeValuesPtr)     │
 └───────────┬───────────────┘
             │
             ▼
 ┌───────────────────────────┐
 │ CalculateMap              │  IJob (single)
 │                           │
 │  map.RecalculateBuckets() │  ← rebuilds internal bucket
 │                           │     structure for fast lookup
 └───────────────────────────┘
```

## Grid Hashing Detail

```
  Quantization:
    quantized.x = floor((position.x + halfSize.x) / step)
    quantized.y = floor((position.y + halfSize.y) / step)

  Hash (row-major):
    hash = quantized.x + quantized.y * quantizeSize

  Example (size=100, step=10, quantizeSize=10):
  ┌────┬────┬────┬────┬────┬────┬────┬────┬────┬────┐
  │ 0  │ 1  │ 2  │ 3  │ 4  │ 5  │ 6  │ 7  │ 8  │ 9  │ y=0
  ├────┼────┼────┼────┼────┼────┼────┼────┼────┼────┤
  │ 10 │ 11 │ 12 │ 13 │ 14 │ 15 │ 16 │ 17 │ 18 │ 19 │ y=1
  ├────┼────┼────┼────┼────┼────┼────┼────┼────┼────┤
  │ 20 │ 21 │ 22 │ 23 │ 24 │ 25 │ 26 │ 27 │ 28 │ 29 │ y=2
  ├────┼────┼────┼────┼────┼────┼────┼────┼────┼────┤
  │...                                              │
  └────┴────┴────┴────┴────┴────┴────┴────┴────┴────┘

  Two entities in cell 22 → keys=[22,22], values=[0,3]
  Query cell 22 → get indices [0, 3]
```

## SpatialKeyedMap vs LocalSpatialMap

```
  ┌───────────────────┬─────────────────────┬────────────────────┐
  │ Aspect            │ LocalSpatialMap     │ SpatialKeyedMap     │
  ├───────────────────┼─────────────────────┼────────────────────┤
  │ Storage           │ UnsafePartialKeyedMap│ NativeKeyedMap<int>│
  │ Value per entry   │ Full T struct       │ int (index only)    │
  │ Multi-value/cell  │ Partial map (single)│ Keyed map (single)  │
  │ Safety            │ Unsafe pointers     │ Native collections  │
  │ Bucket rebuild    │ map->Update()       │ RecalculateBuckets()│
  │ Dispose w/ Job    │ Manual              │ .Dispose(handle)    │
  └───────────────────┴─────────────────────┴────────────────────┘
```

## Source

`BovineLabs.Core/Spatial/SpatialKeyedMap.cs`

## Source

- [BovineLabs.Core/Spatial/SpatialKeyedMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Spatial/SpatialKeyedMap.cs)
