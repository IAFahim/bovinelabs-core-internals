# PolygonUtility — Inner Workings

## Overview

PolygonUtility provides Burst-compatible signed area and winding-order calculations
for 2D polygons using the shoelace formula, operating directly on NativeArrays of
float2 or float3 points.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                  PolygonUtility (static class)                               │
│                                                                             │
│  SignedArea(NativeArray<float2>) → float                                    │
│  SignedArea(NativeArray<float3>) → float  (uses x,z as x,y)                │
│  IsClockwise(...)        → bool                                             │
│  IsCounterClockwise(...) → bool                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Shoelace Formula Algorithm

```
  Signed area = Σ (x₂ - x₁)(y₂ + y₁)  over all edges (including closing edge)

  ┌───────────────────────────────────────────────────────────────────────┐
  │                                                                       │
  │  For N vertices [p₀, p₁, ..., pₙ₋₁]:                                │
  │                                                                       │
  │  sum = 0                                                             │
  │  for i = 0 to N-2:                                                   │
  │    sum += (p[i+1].x - p[i].x) * (p[i+1].y + p[i].y)                 │
  │                                                                       │
  │  // Closing edge: last → first                                        │
  │  sum += (p[0].x - p[N-1].x) * (p[0].y + p[N-1].y)                   │
  │                                                                       │
  │  return sum  (= 2 × signed area)                                      │
  └───────────────────────────────────────────────────────────────────────┘
```

## Visual Example — Clockwise Polygon

```
  Points (clockwise):
  p₀=(0,0)  p₁=(4,0)  p₂=(4,3)  p₃=(0,3)

      p₃ ──────────── p₂
      │               │
      │               │  3
      │               │
      p₀ ──────────── p₁
            4

  Edge calculation:
  ┌──────────┬───────────────────────────────────┬───────────┐
  │  Edge    │  (x₂-x₁) × (y₂+y₁)              │  Result   │
  ├──────────┼───────────────────────────────────┼───────────┤
  │ p₀→p₁   │  (4-0) × (0+0)                    │    0      │
  │ p₁→p₂   │  (4-4) × (3+0)                    │    0      │
  │ p₂→p₃   │  (0-4) × (3+3)                    │  -24      │
  │ p₃→p₀   │  (0-0) × (0+3)                    │    0      │
  ├──────────┼───────────────────────────────────┼───────────┤
  │  TOTAL   │                                   │  -24      │
  └──────────┴───────────────────────────────────┴───────────┘

  Wait — this is actually COUNTER-clockwise! (negative = CCW)
  Result: -24 → IsCounterClockwise = true, IsClockwise = false
```

## Winding Order Rules

```
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  SignedArea > 0  →  CLOCKWISE                                │
  │  SignedArea < 0  →  COUNTER-CLOCKWISE                        │
  │  SignedArea = 0  →  NEITHER (degenerate/colinear)            │
  │                                                               │
  │  CLOCKWISE:           COUNTER-CLOCKWISE:                      │
  │       p₂                    p₀                                │
  │      / \                   / \                                │
  │     /   \                 /   \                               │
  │    p₁───p₀              p₁───p₂                              │
  │    (positive area)       (negative area)                      │
  │                                                               │
  └───────────────────────────────────────────────────────────────┘
```

## float3 Variant (XZ Plane)

```
  The float3 overload uses X and Z components (ignoring Y),
  treating the polygon as lying on the XZ ground plane:

  ┌────────────────────────────────────────────────────────────┐
  │  float3 version:                                           │
  │                                                            │
  │  sum += (p[i+1].x - p[i].x) * (p[i+1].z + p[i].z)       │
  │                       ─                       ─            │
  │                    uses .z instead of .y                   │
  │                                                            │
  │  Top-down view:                                            │
  │                                                            │
  │  Z ↑                                                       │
  │    │  p₃ ──── p₂                                          │
  │    │  │        │                                           │
  │    │  p₀ ──── p₁                                          │
  │    └──────────────► X                                      │
  └────────────────────────────────────────────────────────────┘
```

## Edge Cases

```
  ┌───────────────────────────────────────────────────────────────┐
  │  Points.Length <= 1:  return 0                               │
  │  // Not enough points to form a polygon                       │
  │                                                               │
  │  Closing edge:                                               │
  │  // The formula REQUIRES the edge from last→first vertex     │
  │  // This is computed separately after the loop               │
  │  p_last = points[^1]                                         │
  │  p_first = points[0]                                         │
  │  sum += (p_first.x - p_last.x) * (p_first.y + p_last.y)    │
  └───────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Shoelace formula**: O(n) single-pass algorithm that computes twice the signed
  area — fast and numerically stable for convex and concave polygons.
- **NativeArray input**: Burst-compatible collections, no managed array allocations.
- **float3 XZ variant**: Essential for Unity's Y-up coordinate system where ground
  polygons lie on the XZ plane.
- **Sign convention**: Follows the "positive = clockwise" convention from the
  shoelace formula, consistent with Unity's coordinate system.
- **No normalization**: Returns raw signed area (2×actual), which is sufficient for
  winding order checks. Divide by 2 for actual area.

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source File

- `BovineLabs.Core/Utility/PolygonUtility.cs`

## Source

- [BovineLabs.Core/Utility/PolygonUtility.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/PolygonUtility.cs)
