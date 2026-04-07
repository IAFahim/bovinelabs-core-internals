# PositionBuilder

**Extracts and caches LocalTransform positions into native arrays efficiently**

`PositionBuilder` is a helper struct that queries ECS entities with `LocalTransform`
and copies their float3 positions into a `NativeArray<SpatialPosition>` via a
burst-compiled `IJobChunk` job with stride-aware `MemCpy`.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                      PositionBuilder                         │
│                                                              │
│  ┌───────────────────────────┐  ┌──────────────────────────┐│
│  │ EntityQuery query         │  │ ComponentTypeHandle<      ││
│  │ (entities with transforms)│  │   LocalTransform> (RO)    ││
│  └───────────┬───────────────┘  └────────────┬─────────────┘│
│              │                                │              │
│              └────────────┬───────────────────┘              │
│                           ▼                                  │
│               Gather(ref SystemState,                        │
│                     JobHandle dependency,                    │
│                     out NativeArray<SpatialPosition>)        │
└──────────────────────────────────────────────────────────────┘
```

## SpatialPosition Data Layout

```
  ┌────────────────────────────────────────┐
  │  struct SpatialPosition                │
  │    ISpatialPosition  → .Position xz    │
  │    ISpatialPosition3 → .Position xyz   │
  │                                        │
  │    float3 Position;  ← actual storage  │
  │                                        │
  │    float2 ISpatialPosition.Position     │
  │      => this.Position.xz               │
  │    float3 ISpatialPosition3.Position    │
  │      => this.Position                  │
  └────────────────────────────────────────┘
  Satisfies BOTH 2D and 3D spatial interfaces!
```

## Gather Pipeline

```
  SystemState + EntityQuery + JobHandle dependency
                    │
                    ▼
  ┌──────────────────────────────────────────────────────┐
  │  1. Allocate output array                            │
  │     positions = WorldRewindableAllocator             │
  │         .AllocateNativeArray<SpatialPosition>(       │
  │             query.CalculateEntityCount())            │
  │                                                      │
  │  2. Calculate base entity indices (async)            │
  │     firstEntityIndices = query                       │
  │         .CalculateBaseEntityIndexArrayAsync(...)     │
  └──────────────────────┬───────────────────────────────┘
                         │
                         ▼
  ┌──────────────────────────────────────────────────────┐
  │  3. GatherPositionsJob : IJobChunk                  │
  │                                                      │
  │  For each chunk:                                     │
  │                                                      │
  │  ┌───────────────────────────────────────────────┐  │
  │  │ chunk: [E0][E1][E2][E3][E4][E5][E6][E7]...  │  │
  │  │         │     LocalTransform data             │  │
  │  │         │  (float3 position + quaternion rot  │  │
  │  │         │   + float scale)                    │  │
  │  │         │     stride = sizeof(LocalTransform) │  │
  │  │         ▼                                     │  │
  │  │  dst = outputPtr + firstEntityIndices[idx]    │  │
  │  │                                              │  │
  │  │  MemCpyStride(                               │  │
  │  │    dst,        dstStride = sizeof(float3),    │  │
  │  │    src,        srcStride = sizeof(LocalTrans),│  │
  │  │    elemSize,   count                         │  │
  │  │  )                                           │  │
  │  │                                              │  │
  │  │  Extracts ONLY the float3 position field     │  │
  │  │  from each LocalTransform, skipping rotation │  │
  │  │  and scale.                                  │  │
  │  └───────────────────────────────────────────────┘  │
  └──────────────────────┬───────────────────────────────┘
                         │
                         ▼
              NativeArray<SpatialPosition> ready
              for spatial map consumption
```

## Stride-aware Memory Copy Detail

```
  LocalTransform in memory (chunk):
  ┌──────────────────────────────────────────────────┐
  │ float3 pos │ quaternion rot │ float scale │ pad  │
  │  12 bytes  │   16 bytes     │  4 bytes    │      │
  └──────────────────────────────────────────────────┘
  ↑ offset 0   stride = sizeof(LocalTransform) = 32+ bytes

  Output NativeArray<SpatialPosition>:
  ┌──────────┬──────────┬──────────┬──────────┐
  │ float3 P │ float3 P │ float3 P │ float3 P │
  │  12 bytes│  12 bytes│  12 bytes│  12 bytes│
  └──────────┴──────────┴──────────┴──────────┘
  stride = sizeof(float3) = 12 bytes (tightly packed)

  MemCpyStride:
    copies sizeof(float3) bytes from each LocalTransform
    at stride offset 0 into tightly packed output array
```

## Key Design Decisions

| Aspect | Decision | Reason |
|--------|----------|--------|
| Allocator | `WorldRewindableAllocator` | No manual deallocation; auto-cleared each frame |
| Chunk job | `IJobChunk` | Process entities archetype-chunk at a time |
| Enableable components | NOT supported | Asserts if `useEnabledMask` is true |
| Copy method | `MemCpyStride` | Extracts position field from strided chunk data |
| Dual interface | `ISpatialPosition` + `ISpatialPosition3` | Same data works for both 2D and 3D spatial maps |

## Source

`BovineLabs.Core/Spatial/PositionBuilder.cs`
