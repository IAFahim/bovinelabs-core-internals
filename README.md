# LocalSpatialMap

**Non-parallel partial spatial map for lightweight fast proximity checks**

`LocalSpatialMap<T>` is a generic spatial hash structure that maps 2D positions
into quantized grid cells using an `UnsafePartialKeyedMap`. It is designed for
scenarios where you need fast local proximity queries but do not require a
full parallel-safe multi-map.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                   LocalSpatialMap<T>                         │
│                                                              │
│  where T : unmanaged, ISpatialPosition                      │
│                                                              │
│  ┌─────────────────┐     ┌──────────────────────┐           │
│  │ quantizeStep     │     │ quantizeSize          │           │
│  │ (cell resolution)│     │ = ceil(size/step)     │           │
│  └────────┬────────┘     └──────────┬───────────┘           │
│           │                         │                        │
│           ▼                         ▼                        │
│  ┌──────────────────────────────────────────┐               │
│  │         Spatial Quantization             │               │
│  │                                          │               │
│  │  World: [-halfSize .. +halfSize]²        │               │
│  │  Grid : [0 .. quantizeSize)²             │               │
│  │                                          │               │
│  │  quantized = floor((pos + halfSize)/step)│               │
│  │  hash      = q.x + (q.y * width)        │               │
│  └──────────────┬───────────────────────────┘               │
│                 │                                            │
│                 ▼                                            │
│  ┌──────────────────────────────────────┐                   │
│  │  UnsafePartialKeyedMap<T>*  map      │ ◄─ dense storage  │
│  │  UnsafeList<int>*          keys      │ ◄─ hash keys      │
│  └──────────────────────────────────────┘                   │
│                                                              │
│  Capacity: quantizeSize * quantizeSize (full grid)           │
└──────────────────────────────────────────────────────────────┘
```

## Build Pipeline (3-job chain)

```
  NativeArray<T> positions
         │
         ▼
 ┌───────────────────┐
 │   ResizeKeys      │  IJob
 │   (single-thread) │  keys->Resize(count)
 └────────┬──────────┘
          │ dependency
          ▼
 ┌───────────────────┐
 │   QuantizeJob     │  IJobFor (ScheduleParallel)
 │                   │  Workers = max(1, JobWorkerCount)
 │  For each worker: │
 │   start = idx*len │
 │   end = start+len │  (last worker gets remainder)
 │                   │
 │   pos = Positions[i].Position (float2)
 │   q   = Quantized(pos, step, halfSize)
 │   h   = Hash(q, width)
 │   keys[i] = h
 └────────┬──────────┘
          │ dependency
          ▼
 ┌───────────────────┐
 │   UpdateMap       │  IJob (single-thread)
 │                   │
 │   Assert: keys.Length == values.Length
 │   map->Update(keys, values, length)
 │     ↳ UnsafePartialKeyedMap rebuilds
 │       internal data from key+value arrays
 └────────┬──────────┘
          │
          ▼
     map is ready for queries via AsReadOnly()
```

## Quantization Visualized (2D Grid)

```
   World Coordinate Space (e.g. size=100, step=10)
   ┌─────────────────────────────────────────────────┐
   │ (-50,50)                          (50,50)       │
   │   ┌───┬───┬───┬───┬───┬───┬───┬───┬───┬───┐    │
   │   │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │ 8 │ 9 │    │
   │   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤    │
   │   │10 │11 │12 │13 │14 │15 │16 │17 │18 │19 │    │
   │   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤    │
   │   │20 │21 │22 ◎│23 │24 │25 │26 │27 │28 │29 │    │
   │   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤    │
   │   │...│quantizeSize = 10 (=ceil(100/10))       │
   │   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤    │
   │   │90 │91 │92 │93 │94 │95 │96 │97 │98 │99 │    │
   │   └───┴───┴───┴───┴───┴───┴───┴───┴───┴───┘    │
   │ (-50,-50)                         (50,-50)      │
   └─────────────────────────────────────────────────┘

   ◎ Entity at world pos (5,30)
     quantized = floor((5+50, 30+50)/10) = (5,8)
     hash = 5 + 8*10 = 85
```

## Querying via ReadOnly

```
  ┌────────────────────────────────────────┐
  │       LocalSpatialMap<T>.ReadOnly      │
  │                                        │
  │  quantizeStep : float                  │
  │  quantizeWidth: int                    │
  │  halfSize     : int2                   │
  │  Map          : UnsafePartialKeyedMap* │ ◄─ direct ptr access
  │                                        │
  │  Quantized(float2 pos) → int2          │
  │  Hash(int2 q)         → int            │
  └────────────────────────────────────────┘

  Usage pattern:
    var ro = spatialMap.AsReadOnly();
    var q  = ro.Quantized(queryPos);
    var h  = ro.Hash(q);
    // look up in ro.Map by hash key
```

## Key Design Decisions

| Aspect | Decision | Reason |
|--------|----------|--------|
| Storage | `UnsafePartialKeyedMap<T>` | One value per cell; no multi-value support |
| Quantization | `floor((pos + halfSize) / step)` | Shifts world to positive coordinates |
| Hash | `q.x + q.y * width` | Simple row-major linearization |
| Parallelism | QuantizeJob parallel, UpdateMap single | Map rebuild is not thread-safe |
| Generic constraint | `ISpatialPosition` (float2) | 2D spatial only; see SpatialMap3 for 3D |

## Source

`BovineLabs.Core/Spatial/LocalSpatialMap.cs`
