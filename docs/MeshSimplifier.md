# MeshSimplifier

**Provides runtime mesh decimation entirely using Burst**

A Burst-compiled mesh simplification system based on quadric error metrics.
Collapses edges iteratively based on error thresholds to reduce triangle count
while preserving mesh shape. Operates entirely on native collections.

---

## Architecture

```
  ┌──────────────────────────────────────────────────────────────┐
  │  MeshSimplifier                                               │
  │                                                              │
  │  Input:  Vector3[] verts, int[] tris, Options opts           │
  │  Output: Result { NativeList<Vertex>, NativeList<Triangle> } │
  │                                                              │
  │  Options:                                                     │
  │    float Quality    ← target ratio (0-1, e.g. 0.5 = halve)  │
  │    float Agressiveness ← threshold exponent (default ~2)     │
  │    int   MaxIterationCount                                   │
  └──────────────────────────────────────────────────────────────┘
```

## Simplification Pipeline

```
  Input Mesh (verts + triangles)
         │
         ▼
  ┌──────────────────────────────────────────────────────┐
  │  Initialize                                          │
  │                                                      │
  │  targetTris = round(triCount * quality)              │
  │  For each triangle:                                  │
  │    Triangle(index, v0, v1, v2)                       │
  │  For each vertex:                                    │
  │    Vertex(index, position)                            │
  └──────────────────────┬───────────────────────────────┘
                         │
                         ▼
  ┌──────────────────────────────────────────────────────┐
  │  Iteration Loop                                      │
  │                                                      │
  │  for iteration in 0..MaxIterationCount:              │
  │    if tris deleted enough → break                    │
  │                                                      │
  │    every 5 iterations: UpdateMesh()                  │
  │      - compact deleted triangles                     │
  │      - rebuild reference lists                       │
  │      - (iter 0) identify boundary vertices           │
  │      - (iter 0) compute quadric error matrices       │
  │      - (iter 0) compute per-edge errors              │
  │                                                      │
  │    threshold = 0.000000001 * (iter+3)^agressiveness  │
  │                                                      │
  │    RemoveVertexPass():                                │
  │      for each triangle (not dirty/deleted):           │
  │        for each edge:                                 │
  │          if error > threshold → skip                  │
  │          if border mismatch → skip                    │
  │          compute collapse target position             │
  │          if flip check fails → skip                   │
  │          collapse edge → merge vertices               │
  │          update adjacent triangles                    │
  │          break (one collapse per triangle per pass)   │
  └──────────────────────┬───────────────────────────────┘
                         │
                         ▼
  ┌──────────────────────────────────────────────────────┐
  │  CompactMesh()                                       │
  │                                                      │
  │  Remove deleted triangles                            │
  │  Remap vertex attribute indices                      │
  │  Compact vertex list                                 │
  │  Remap triangle indices to new vertex positions      │
  └──────────────────────────────────────────────────────┘
```

## Edge Collapse Visualization

```
  Before collapse (edge V0-V1):
       V0 ─────────── V1
      / │ \          / │ \
     /  │  \        /  │  \
    T1  T2  T3    T4  T5  T6

  After collapse (V1 → V0 at optimal position P):
            P (merged)
           /│\
          / │ \
        T1' T2' remaining
          \ │ /
           \|/
        deleted: T3, T4, T5, T6
        (triangles using collapsed edge)

  ┌───────────────────────────────────────────────────┐
  │  Collapse validation:                              │
  │                                                   │
  │  1. Flipped() check for both vertices             │
  │     - For each adjacent triangle of vi:           │
  │       - Compute new normal after moving to P      │
  │       - If normal flips (dot < 0) → REJECT       │
  │                                                   │
  │  2. Border edge check                             │
  │     - Both vertices must be border or both not    │
  │                                                   │
  │  3. UV seam/foldover checks                       │
  │     - Seam and foldover states must match         │
  └───────────────────────────────────────────────────┘
```

## Quadric Error Metric

```
  ┌────────────────────────────────────────────────────┐
  │  SymmetricMatrix (quadric) per vertex              │
  │                                                    │
  │  Q accumulates error from adjacent face planes:    │
  │                                                    │
  │  For each triangle with normal n, point p:         │
  │    plane = (n.x, n.y, n.z, -dot(n,p))             │
  │    Q[v0] += SymmetricMatrix(plane)                 │
  │    Q[v1] += SymmetricMatrix(plane)                 │
  │    Q[v2] += SymmetricMatrix(plane)                 │
  │                                                    │
  │  Edge collapse error for (v0, v1):                 │
  │    Q_combined = Q[v0] + Q[v1]                      │
  │    optimal_pos = minimize(Q_combined, position)     │
  │    error = Q_combined.evaluate(optimal_pos)         │
  └────────────────────────────────────────────────────┘
```

## Source

`BovineLabs.Core/Utility/Mesh/MeshSimplifier.cs`

## Source

- [BovineLabs.Core/Utility/Mesh/MeshSimplifier.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Mesh/MeshSimplifier.cs)
