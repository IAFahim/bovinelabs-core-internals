# ConvexHullBuilder

**Native implementation of QuickHull for runtime mesh generation**

A Burst-compiled static class that computes the convex hull of a 3D point cloud
using the QuickHull algorithm. Outputs vertices and triangle indices as native
collections for direct mesh construction.

Based on Oskar Sigvardsson's unity-quickhull implementation.

---

## Algorithm Overview

```
  ┌──────────────────────────────────────────────────────────────┐
  │  ConvexHullBuilder.Generate(points, outVerts, outTris)       │
  │                                                              │
  │  Input:  NativeArray<float3> points (point cloud)            │
  │  Output: NativeList<float3> outVerts (hull vertices)         │
  │          NativeList<int> outTris  (triangle indices)         │
  │                                                              │
  │  Algorithm: QuickHull                                        │
  │    1. Find 4 non-coplanar points → seed tetrahedron          │
  │    2. Assign all points to faces (or mark Inside)            │
  │    3. Loop:                                                  │
  │       a. Find farthest point from any face                   │
  │       b. Find horizon (visible face boundary)                │
  │       c. Construct cone from horizon to point                │
  │       d. Reassign points to new faces                        │
  │    4. Export mesh from final hull                            │
  └──────────────────────────────────────────────────────────────┘
```

## Phase 1: Seed Tetrahedron

```
  Find 4 non-coplanar, non-collinear, non-coincident points:

  ┌──────────────────────────────────────────────────┐
  │  FindInitialHullIndices(points)                   │
  │                                                  │
  │  for i0:                             ──┐         │
  │    for i1: (not coincident with i0)    │ filter  │
  │      for i2: (not collinear w/ i0,i1) │ cascade │
  │        for i3: (not coplanar w/ rest)  ──┘       │
  │          return (i0, i1, i2, i3)                 │
  │                                                  │
  │  Tests:                                          │
  │   AreCoincident(p0,p1)  → distance ≈ 0           │
  │   AreCollinear(p0,p1,p2)→ cross product ≈ 0      │
  │   AreCoplanar(p0..p3)   → scalar triple ≈ 0      │
  └──────────────────────────────────────────────────┘

  Seed hull (tetrahedron):
         b3
         /│\
        / │ \
       /  │  \
      /   │   \
     /  b0│----\ b2
     \    │   /
      \   │  /
       \  │ /
        \ │/
         b1

  4 faces:
  F0: (b0,b2,b1)  F1: (b0,b1,b3)
  F2: (b0,b3,b2)  F3: (b1,b2,b3)
  (order depends on whether b3 is above/below base triangle)
```

## Phase 2: Point Assignment

```
  Open Set: all points not yet inside the hull
  ┌──────────────────────────────────────────────────┐
  │  OpenSet array layout:                            │
  │                                                  │
  │  [Active points...] [Inside points...]           │
  │  ← OpenSetTail ──→                               │
  │                                                  │
  │  PointFace { Point: int, Face: int, Distance: float }
  │                                                  │
  │  For each active point:                          │
  │    For each face of hull:                        │
  │      dist = PointFaceDistance(point, face)        │
  │      if dist > 0:                                │
  │        assign point to this face                  │
  │        break                                     │
  │    if no face assigned:                           │
  │      mark Inside → swap to tail, decrement tail  │
  └──────────────────────────────────────────────────┘
```

## Phase 3: Hull Growth (main loop)

```
  while OpenSetTail >= 0:
  ┌──────────────────────────────────────────────────────────┐
  │  GrowHull():                                             │
  │                                                          │
  │  1. Find farthest point from its assigned face            │
  │                                                          │
  │     face ────────●──── farthest point (max distance)      │
  │                                                          │
  │  2. FindHorizon (DFS from lit face):                     │
  │     ┌────────────────────────────────────────────┐       │
  │     │  Lit faces: faces visible from the point   │       │
  │     │  Horizon: boundary edges between lit/      │       │
  │     │          unlit faces                       │       │
  │     │                                            │       │
  │     │       ╲ Lit ╱                              │       │
  │     │        ╲   ╱  ── horizon edge              │       │
  │     │    ─────●──●──●─────                       │       │
  │     │        ╱   ╲                                │       │
  │     │       ╱ Unlit╲                              │       │
  │     └────────────────────────────────────────────┘       │
  │     Counter-clockwise DFS ensures ordered horizon        │
  │                                                          │
  │  3. ConstructCone():                                     │
  │     ┌────────────────────────────────────────────┐       │
  │     │  Remove all lit faces                      │       │
  │     │  Create new triangular faces:              │       │
  │     │    horizon edge [A,B] + farthest point F   │       │
  │     │    → new face (F, A, B)                    │       │
  │     │  Connect neighbors:                        │       │
  │     │    new face ←→ adjacent new face           │       │
  │     │    new face ←→ horizon face                │       │
  │     └────────────────────────────────────────────┘       │
  │                                                          │
  │  4. ReassignPoints():                                    │
  │     For each point from removed lit faces:               │
  │       Try assign to new faces                            │
  │       If inside all new faces → mark Inside              │
  └──────────────────────────────────────────────────────────┘
```

## Data Structure: Face

```
  ┌────────────────────────────────────────────────┐
  │  Face                                           │
  │    Vertex0, Vertex1, Vertex2: int   (point idx) │
  │    Opposite0, Opposite1, Opposite2: int (face)  │
  │    Normal: float3                                │
  │                                                  │
  │  Vertex-N → the vertex opposite edge N           │
  │  Opposite-N → the face opposite vertex N         │
  │                                                  │
  │        V1                                        │
  │       /│\                                        │
  │      / │ \                                       │
  │     /  │  \                                      │
  │   V2───┼──V0                                     │
  │     \  │  /                                      │
  │      \ │ /                                       │
  │       \│/                                        │
  │   Opp0=face opposite V0                          │
  │   Opp1=face opposite V1                          │
  │   Opp2=face opposite V2                          │
  └────────────────────────────────────────────────┘
```

## Source

`BovineLabs.Core/Utility/Mesh/ConvexHullBuilder.cs`
