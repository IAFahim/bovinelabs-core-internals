# SpatialMap

**High-performance parallel 2D grid hashing for fast proximity queries**

`SpatialMap<T>` is a parallel-safe 2D spatial hash map that uses
`NativeParallelMultiHashMap<int, int>` to store multiple entity indices per
grid cell. It supports concurrent writes during the quantize phase and
provides fast bucket-based proximity queries.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                        SpatialMap<T>                             │
│                                                                  │
│  where T : unmanaged, ISpatialPosition (float2)                 │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐     │
│  │  float  quantizeStep    ← cell size in world units      │     │
│  │  int    quantizeSize    ← cells per axis                │     │
│  │  int2   halfSize        ← world half-extent             │     │
│  │                                                        │     │
│  │  NativeParallelMultiHashMap<int, int> map               │     │
│  │    key   = hash(cell)                                   │     │
│  │    value = entity index                                 │     │
│  └────────────────────────────────────────────────────────┘     │
└──────────────────────────────────────────────────────────────────┘
```

## Build Pipeline (3-job chain, fully parallel writes)

```
  NativeArray<T> positions
         │
         ▼
 ┌─────────────────────────────────┐
 │ ResizeNativeParallelHashMapJob  │  IJob
 │                                 │
 │  if map.Capacity < count:       │
 │    map.Capacity = count         │
 │  map.Clear()                    │
 │  map.SetAllocatedIndexLength(n) │  ← pre-allocate slots
 └───────────────┬─────────────────┘
                 │
                 ▼
 ┌──────────────────────────────────────────────────────┐
 │ QuantizeJob                                          │
 │ IJobFor (ScheduleParallel over worker count)         │
 │                                                      │
 │  ┌────────────────────────────────────────────────┐  │
 │  │ Each worker writes directly to the              │  │
│  │ multi-hash-map's unsafe internal arrays:        │  │
│  │                                                │  │
│  │  keys*   = GetUnsafeBucketData().keys           │  │
│  │  values* = GetUnsafeBucketData().values         │  │
│  │                                                │  │
│  │  keys[i]   = Hash(Quantized(pos, step, half))  │  │
│  │  values[i] = i  (entity index)                 │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
│  Thread 0: ┃ █ █ █ █ █ █ █ █ ┃                      │
│  Thread 1: ┃ █ █ █ █ █ █ █   ┃                      │
│  Thread 2: ┃ █ █ █ █ █ █     ┃                      │
│  Thread 3: ┃ █ █ █ █ █ █ █ █ ┃ ← remainder          │
│            └──────────────────┘                      │
│            NativeDisableParallelForRestriction        │
│            allows parallel writes to shared buckets   │
└───────────────┬──────────────────────────────────────┘
                │
                ▼
 ┌─────────────────────────────────┐
 │ CalculateMap                    │  IJob
 │                                 │
 │  map.RecalculateBuckets()       │
 │    ← rebuilds hash bucket       │
 │      linked-list structure      │
│      from raw key/value arrays  │
└─────────────────────────────────┘
```

## MultiHashMap Grid Detail

```
  Grid Cell → Hash Key → Multiple Entities per Cell

  ┌─────────┬─────────┬─────────┐
  │ cell 0  │ cell 1  │ cell 2  │
  │ hash=0  │ hash=1  │ hash=2  │
  │         │         │  ●E2    │
  │         │         │  ●E7    │
  ├─────────┼─────────┼─────────┤
  │ cell 3  │ cell 4  │ cell 5  │
  │ hash=3  │ hash=4  │ hash=5  │
  │  ●E0    │  ●E1    │  ●E3    │
  │  ●E5    │         │  ●E8    │
  │         │         │  ●E9    │
  └─────────┴─────────┴─────────┘

  NativeParallelMultiHashMap<int,int> internals:
  ┌─────────────────────────────────────────────────┐
  │ Bucket[0] → [E0]                                │
  │ Bucket[1] → [E1]                                │
  │ Bucket[2] → [E2] → [E7]                        │
  │ Bucket[3] → [E0] → [E5]                        │
  │ Bucket[4] → [E1]                                │
  │ Bucket[5] → [E3] → [E8] → [E9]                │
  └─────────────────────────────────────────────────┘

  Query: hash = SpatialMap.Hash(Quantized(queryPos, ...))
  Result: TryGetFirstValue/TryGetNextValue iteration
```

## SpatialMap vs SpatialKeyedMap vs LocalSpatialMap

```
  ┌──────────────────┬────────────────────┬──────────────────┬──────────────────┐
  │                  │ LocalSpatialMap    │ SpatialKeyedMap   │ SpatialMap        │
  ├──────────────────┼────────────────────┼──────────────────┼──────────────────┤
  │ Collection       │ UnsafePartialKeyed │ NativeKeyedMap    │ NativeParallel    │
  │                  │ Map<T>             │ <int>             │ MultiHashMap      │
  │ Values per cell  │ 1 (T struct)       │ 1 (int index)     │ N (int indices)   │
  │ Parallel writes  │ Partial            │ Partial           │ Full parallel     │
  │ Bucket rebuild   │ map->Update()      │ RecalculateBuck.  │ RecalculateBuck.  │
  │ Use case         │ Lightweight local  │ Single-entity     │ Multi-entity      │
  │                  │ single-entity      │ spatial lookup    │ spatial queries   │
  └──────────────────┴────────────────────┴──────────────────┴──────────────────┘
```

## Query Pattern

```
  var ro = spatialMap.AsReadOnly();
  
  // Convert world position to grid cell
  int2 cell = ro.Quantized(worldPos);
  
  // Convert cell to hash
  int hash = ro.Hash(cell);
  
  // Iterate all entities in that cell
  if (ro.Map.TryGetFirstValue(hash, out int entityIdx, out var it))
  {
      do {
          // process entityIdx
      } while (ro.Map.TryGetNextValue(out entityIdx, ref it));
  }
```

## Source

`BovineLabs.Core/Spatial/SpatialMap.cs`
