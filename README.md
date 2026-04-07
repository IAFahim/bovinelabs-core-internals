# ISpatialPosition

**Interface for spatial position extraction**

Two simple interfaces that provide a standardized way to extract 2D and 3D
positions from arbitrary data structures. Used as generic constraints by all
spatial map types.

---

## Interface Definitions

```
  ┌──────────────────────────────────────────────────┐
  │  ISpatialPosition                                │
  │    float2 Position { get; }     ← 2D (XZ plane) │
  └──────────────────────────────────────────────────┘

  ┌──────────────────────────────────────────────────┐
  │  ISpatialPosition3                               │
  │    float3 Position { get; }     ← 3D (XYZ)      │
  └──────────────────────────────────────────────────┘
```

## Usage in Spatial Maps

```
  ┌──────────────────────────────────────────────────────────┐
  │                   Spatial Map Hierarchy                   │
  │                                                          │
  │  ISpatialPosition (float2)                               │
  │    ├── LocalSpatialMap<T>   where T : ISpatialPosition   │
  │    ├── SpatialKeyedMap<T>   where T : ISpatialPosition   │
  │    └── SpatialMap<T>        where T : ISpatialPosition   │
  │                                                          │
  │  ISpatialPosition3 (float3)                              │
  │    └── SpatialMap3<T>       where T : ISpatialPosition3  │
  └──────────────────────────────────────────────────────────┘
```

## SpatialPosition: Built-in Implementation

```
  ┌──────────────────────────────────────────────────────┐
  │  struct SpatialPosition                               │
  │    : ISpatialPosition, ISpatialPosition3              │
  │                                                      │
  │    float3 Position;  ← full 3D storage               │
  │                                                      │
  │    float2 ISpatialPosition.Position                  │
  │      => this.Position.xz    ← 2D projection          │
  │                                                      │
  │    float3 ISpatialPosition3.Position                 │
  │      => this.Position       ← full 3D               │
  │                                                      │
  │  Used by PositionBuilder to extract LocalTransform    │
  │  positions for consumption by any spatial map type   │
  └──────────────────────────────────────────────────────┘

  ┌───────────────────────────────────────────────┐
  │  PositionBuilder.Gather()                     │
  │       │                                       │
  │       ▼                                       │
  │  NativeArray<SpatialPosition>                 │
  │       │                                       │
  │       ├──▶ LocalSpatialMap<SpatialPosition>   │
  │       ├──▶ SpatialKeyedMap<SpatialPosition>   │
  │       ├──▶ SpatialMap<SpatialPosition>        │
  │       └──▶ SpatialMap3<SpatialPosition>       │
  │            (all accept ISpatialPosition types) │
  └───────────────────────────────────────────────┘
```

## Design Rationale

```
  Why interfaces instead of delegates/function pointers?

  ┌──────────────────────────────────────────────────────┐
  │  1. Generic constraints:                             │
  │     where T : ISpatialPosition                       │
  │     Burst can inline the property access             │
  │                                                      │
  │  2. Zero overhead:                                   │
  │     Interface methods on structs are resolved at      │
  │     compile time → direct field access in Burst       │
  │                                                      │
  │  3. Type safety:                                     │
  │     Can't accidentally pass 3D position to 2D map    │
  │                                                      │
  │  4. Custom data:                                     │
  │     Users can implement ISpatialPosition on their     │
  │     own component data to feed spatial maps           │
  └──────────────────────────────────────────────────────┘
```

## Source

`BovineLabs.Core/Spatial/ISpatialPosition.cs`
`BovineLabs.Core/Spatial/PositionBuilder.cs`
