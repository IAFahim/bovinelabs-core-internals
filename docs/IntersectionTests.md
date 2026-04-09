# IntersectionTests

**Provides fast AABB-Triangle intersection math strictly for Burst**

Implements the separating axis theorem (SAT) for AABB-vs-Triangle intersection
testing. Based on MathGeoLib's Triangle.cpp algorithm. Pure math, no allocations,
fully Burst-compatible.

---

## Algorithm: AABB vs Triangle (SAT)

```
  ┌──────────────────────────────────────────────────────────┐
  │  IntersectionTests.AABBTriangle(aabb, a, b, c)          │
  │                                                          │
  │  Based on: MathGeoLib Triangle.cpp#L630                  │
  │  Method: 13 separating axis tests (SAT)                  │
  │                                                          │
  │  Test axes:                                              │
  │   1-3. AABB face normals (3 axes: X, Y, Z)              │
  │   4.   Triangle face normal (1 axis)                     │
  │   5-13. AABB edges × Triangle edges (9 cross products)   │
  │         eX × t[0], eX × t[1], eX × t[2]                 │
  │         eY × t[0], eY × t[1], eY × t[2]                 │
  │         eZ × t[0], eZ × t[1], eZ × t[2]                 │
  │                                                          │
  │  If ANY axis separates → no intersection                 │
  │  If ALL 13 pass → intersection exists                    │
  └──────────────────────────────────────────────────────────┘
```

## Early Exit Flow

```
  ┌─────────────────────────────────────────────────────┐
  │  Step 1: AABB bounding box test (cheap)             │
  │                                                     │
  │  tMin = min(a, min(b, c))                           │
  │  tMax = max(a, max(b, c))                           │
  │                                                     │
  │  if tMin >= aabb.Max → NO HIT (any axis)            │
  │  if tMax <= aabb.Min → NO HIT (any axis)            │
  │                                                     │
  │  ┌─────────────────────────────────┐                │
  │  │    AABB                         │                │
  │  │  ┌──────────┐                   │                │
  │  │  │          │     ▲c            │                │
  │  │  │   ┌─●b───┤    /             │                │
  │  │  │   │      │   /              │                │
  │  │  │   └─●a───┤  /               │                │
  │  │  │          │                   │                │
  │  │  └──────────┘                   │                │
  │  │  tMax < aabb.Min → REJECT      │                │
  │  └─────────────────────────────────┘                │
  └────────────────────┬────────────────────────────────┘
                       │ passes
                       ▼
  ┌─────────────────────────────────────────────────────┐
  │  Step 2: Triangle normal test                       │
  │                                                     │
  │  n = cross(b-a, c-a)     (triangle face normal)    │
  │  s = dot(n, a-center)    (signed distance)          │
  │  r = abs(dot(halfExtents, abs(n)))                  │
  │                                                     │
  │  if |s| >= r → NO HIT (triangle plane too far)     │
  │                                                     │
  │      │n                                            │
  │      │  ┌───────┐                                  │
  │      │  │  AABB │                                  │
  │      │  │       │ ← r = projected half-extent      │
  │      │  └───────┘                                  │
  │      │                                              │
  │      ───●────────  triangle plane                   │
  │      |s| = distance from AABB center to plane       │
  └────────────────────┬────────────────────────────────┘
                       │ passes
                       ▼
  ┌─────────────────────────────────────────────────────┐
  │  Step 3: 9 edge cross-product tests                 │
  │                                                     │
  │  For each AABB axis e ∈ {X, Y, Z}:                 │
  │    For each triangle edge t ∈ {b-a, c-a, c-b}:     │
  │                                                     │
  │      axis = e × t  (cross product)                  │
  │      d1 = proj(vertex_a, axis)                      │
  │      d2 = proj(vertex_b/c, axis)                    │
  │      tc = (d1 + d2) / 2                             │
  │      r  = projected AABB half-extent                │
  │                                                     │
  │      if r + |tc - d1| < |tc| → NO HIT               │
  │                                                     │
  │  ┌──────────────────────────────┐                   │
  │  │    AABB edge eY              │                   │
  │  │    │                         │                   │
  │  │    │    ●a───●b              │                   │
  │  │    │     ╲                    │                   │
  │  │    │       ●c                │                   │
  │  │    │   triangle edge t       │                   │
  │  │    │                         │                   │
  │  │    │ eY × t → separating axis│                   │
  │  └──────────────────────────────┘                   │
  └────────────────────┬────────────────────────────────┘
                       │ all 13 pass
                       ▼
               return true (intersection!)
```

## Performance Notes

- All 13 tests use early exit (return false immediately on separation)
- Uses `stackalloc` for triangle edge arrays (no heap allocation)
- Pure float math, fully Burst-compatible
- No epsilon used in comparisons (exact math)

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core/Utility/IntersectionTests.cs`

## Source

- [BovineLabs.Core/Utility/IntersectionTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/IntersectionTests.cs)
- [BovineLabs.Core.Tests/Utility/IntersectionTestsTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/IntersectionTestsTests.cs)
