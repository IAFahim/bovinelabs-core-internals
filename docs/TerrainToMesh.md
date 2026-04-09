# TerrainToMesh

**Converts Unity Terrains to native meshes asynchronously using jobs**

A Burst-compiled system that reads Unity `TerrainData` heightmaps and holes,
and generates a complete mesh (positions, UVs, normals, indices) using parallel
job scheduling. Modified from Unity's rendering light-transport implementation.

---

## Architecture

```
  ┌──────────────────────────────────────────────────────────────┐
  │  TerrainToMesh                                                │
  │                                                              │
  │  Convert(terrainData) → synchronous mesh                     │
  │  ConvertAsync(terrainData) → Result (job handle)             │
  │                                                              │
  │  Input:  TerrainData                                         │
  │           .heightmapTexture.width/height                     │
  │           .heightmapScale (Vector3)                          │
  │           .GetHeights(0, 0, w, h) → float[,]                  │
  │           .GetHoles(0, 0, w-1, h-1) → bool[,]                 │
  │                                                              │
  │  Output: Result                                               │
  │           .GetMesh()  → UnityEngine.Mesh                     │
  │           .GetVerts() → NativeArray<float3>                  │
  │           .GetTris()  → NativeList<int>                      │
  └──────────────────────────────────────────────────────────────┘
```

## Conversion Pipeline

```
  TerrainData
       │
       ▼
  ┌──────────────────────────────────────────────────┐
  │  Extract heightmap & holes data                  │
  │                                                  │
  │  width  = heightmapTexture.width                 │
  │  height = heightmapTexture.height                │
  │  vertexCount = width * height                    │
  │  indexCount  = (width-1) * (height-1) * 6        │
  │                                                  │
  │  Flatten 2D arrays to 1D:                        │
  │    Heightmap[i] = heightmap[i/w, i%w]            │
  │    Holes[i]     = holes[i/(w-1), i%(w-1)]        │
  └──────────────────┬───────────────────────────────┘
                     │
                     ▼
  ┌──────────────────────────────────────────────────┐
  │  ComputeTerrainMeshJob : IJobFor                 │
  │  ScheduleParallel(vertexCount, max(width, 128))  │
  │                                                  │
  │  For each vertex index i:                        │
  │    x = i % width                                 │
  │    y = i / height                                │
  │                                                  │
  │    ┌───────────────────────────────────────┐     │
  │    │ Position:                              │     │
  │    │   v = float3(x, heightmap[y*w+x], y)  │     │
  │    │   positions[i] = v * heightmapScale    │     │
  │    │                                       │     │
  │    │ UV:                                    │     │
  │    │   uvs[i] = v.xz / float2(width,height)│     │
  │    │                                       │     │
  │    │ Normal:                                │     │
  │    │   Sobel filter (3×3 kernel)            │     │
  │    │   normalize(float3(-dX, 8, -dY))       │     │
  │    └───────────────────────────────────────┘     │
  │                                                  │
  │    If x < width-1 AND y < height-1:              │
  │    ┌───────────────────────────────────────┐     │
  │    │ Triangle indices:                      │     │
  │    │                                       │     │
  │    │   i1 = y*width + x                    │     │
  │    │   i2 = i1 + 1                         │     │
  │    │   i3 = i1 + width                     │     │
  │    │   i4 = i3 + 1                         │     │
  │    │                                       │     │
  │    │   if hole at (x,y): i1=i2=i3=i4=0    │     │
  │    │                                       │     │
  │    │   faceIndex = x + y*(width-1)         │     │
  │    │                                       │     │
  │    │   Indices[6*fi + 0] = i1   ┌──i4     │     │
  │    │   Indices[6*fi + 1] = i4   │╲ │      │     │
  │    │   Indices[6*fi + 2] = i2   │ ╲│      │     │
  │    │   Indices[6*fi + 3] = i1   i1──i2     │     │
  │    │   Indices[6*fi + 4] = i3   │╱         │     │
  │    │   Indices[6*fi + 5] = i4   i3         │     │
  │    └───────────────────────────────────────┘     │
  └──────────────────┬───────────────────────────────┘
                     │
                     ▼
  ┌──────────────────────────────────────────────────┐
  │  Result.TriangleIndicesWithoutHoles()            │
  │                                                  │
  │  Filters out degenerate triangles (all idx = 0)  │
  │  from hole regions:                              │
  │                                                  │
  │  for i in 0..Indices.Length step 3:              │
  │    if i1 != 0 && i2 != 0 && i3 != 0:            │
  │      add to output                               │
  │                                                  │
  │  Ensures empty output gets at least 1 degenerate │
  │  triangle (required by Mesh API)                 │
  └──────────────────────────────────────────────────┘
```

## Normal Calculation (Sobel Filter)

```
  CalculateTerrainNormal using 3×3 Sobel kernel:

  ┌────┬────┬────┐
  │ -1 │ -2 │ -1 │   → dX contribution
  ├────┼────┼────┤
  │  0 │  ● │  0 │   ← center pixel (x,y)
  ├────┼────┼────┤
  │ +1 │ +2 │ +1 │
  └────┴────┴────┘

  dX = -h(x-1,y-1) - 2*h(x-1,y) - h(x-1,y+1)
     + h(x+1,y-1) + 2*h(x+1,y) + h(x+1,y+1)
  dX /= scale.x

  dY = -h(x-1,y-1) - 2*h(x,y-1) - h(x+1,y-1)
     + h(x-1,y+1) + 2*h(x,y+1) + h(x+1,y+1)
  dY /= scale.z

  normal = normalize(float3(-dX, 8, -dY))
             ↑ cross product of gradient components
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core/Utility/Mesh/TerrainToMesh.cs`

## Source

- [BovineLabs.Core/Utility/Mesh/TerrainToMesh.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Mesh/TerrainToMesh.cs)
