# MathematicsExtensions.Encapsulate

## Inner Workings Diagram

```
 AABB.Encapsulate(bounds)
 ══════════════════════════════════════════════════════════════════
 Merges two AABBs mathematically without Unity Bounds overhead

 PURPOSE:
 ═════════
 Unity's Bounds.Encapsulate() operates on the managed Bounds class
 with unnecessary overhead. This extension works directly on the
 value-type AABB (MinMaxAABB) using pure math.min/math.max.


 ┌──────────────────────────────────────────────────────────────────┐
 │  AABB Encapsulate(this AABB aabb, AABB bounds)                   │
 │                                                                   │
 │  Input:  aabb  = base bounding box                               │
 │          bounds = box to merge in                                 │
 │  Output: new AABB containing both inputs                          │
 └───────────────────────────┬──────────────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  IMPLEMENTATION                                                     │
 │                                                                     │
 │  var min = bounds.Min;   // float3 (x,y,z)                         │
 │  var max = bounds.Max;   // float3 (x,y,z)                         │
 │                                                                     │
 │  // Pass 1: merge bounds.Min into aabb                             │
 │  aabb = new MinMaxAABB                                              │
 │  {                                                                  │
 │      Min = math.min(aabb.Min, min),   // take smallest corner      │
 │      Max = math.max(aabb.Max, min),   // expand max for min pt     │
 │  };                                                                 │
 │                                                                     │
 │  // Pass 2: merge bounds.Max into aabb                             │
 │  aabb = new MinMaxAABB                                              │
 │  {                                                                  │
 │      Min = math.min(aabb.Min, max),   // expand min for max pt     │
 │      Max = math.max(aabb.Max, max),   // take largest corner       │
 │  };                                                                 │
 │                                                                     │
 │  return aabb;                                                       │
 └─────────────────────────────────────────────────────────────────────┘


 WHY TWO PASSES?
 ┌──────────────────────────────────────────────────────────────────┐
 │  The source code processes bounds.Min and bounds.Max in          │
 │  separate passes. For a valid AABB (Min <= Max), the result     │
 │  is mathematically identical to the straightforward approach:   │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Straightforward: result.Min = min(a.Min, b.Min);          │  │
 │  │                   result.Max = max(a.Max, b.Max);          │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  Proof (trace through the 2 passes for valid AABB):             │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  After Pass 1:                                             │  │
 │  │    Min = min(aabb.Min, bounds.Min)                         │  │
 │  │    Max = max(aabb.Max, bounds.Min)                         │  │
 │  │         ↑ may shrink if bounds.Min < aabb.Max              │  │
 │  │                                                            │  │
 │  │  After Pass 2:                                             │  │
 │  │    Min = min(min(aabb.Min, bounds.Min), bounds.Max)       │  │
 │  │       = min(aabb.Min, bounds.Min)  [b.Max >= b.Min]       │  │
 │  │    Max = max(max(aabb.Max, bounds.Min), bounds.Max)       │  │
 │  │       = max(aabb.Max, bounds.Max)  [b.Max >= b.Min]       │  │
 │  │                                                            │  │
 │  │  Result is identical to straightforward version.           │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  NOTE: If bounds is degenerate (Min > Max), the intermediate    │
 │  state after Pass 1 may be invalid, but Pass 2 corrects it.     │
 │  This is likely the author's intent with the 2-pass structure.  │
 └──────────────────────────────────────────────────────────────────┘


 GEOMETRIC VISUALIZATION
 ┌──────────────────────────────────────────────────────────────────┐
 │  3D Space (showing XY plane, Z implied)                          │
 │                                                                  │
 │  Before:                                                         │
 │  ┌────────────────┐                                              │
 │  │  AABB "aabb"   │         ┌─────────┐                          │
 │  │  Min=(0,0,0)   │         │ AABB     │                          │
 │  │  Max=(4,3,2)   │         │"bounds" │                          │
 │  └────────────────┘         │Min=(3,2)│                          │
 │                             │Max=(7,5)│                          │
 │                             └─────────┘                          │
 │                                                                  │
 │  After Encapsulate:                                              │
 │  ┌──────────────────────────────────┐                            │
 │  │  Result AABB                     │                            │
 │  │  Min = min((0,0,0), (3,2,1))     │                            │
 │  │      = (0, 0, 0)                 │                            │
 │  │  Max = max((4,3,2), (7,5,3))     │                            │
 │  │      = (7, 5, 3)                 │                            │
 │  └──────────────────────────────────┘                            │
 │                                                                  │
 │  The result AABB fully contains BOTH input AABBs.                │
 └──────────────────────────────────────────────────────────────────┘


 AABB INTERNAL STRUCTURE
 ┌──────────────────────────────────────────────────────────────────┐
 │  MinMaxAABB / AABB (Unity.Mathematics)                           │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  float3 Min;  ─→ corner with smallest x,y,z coordinates   │  │
 │  │  float3 Max;  ─→ corner with largest x,y,z coordinates    │  │
 │  │                                                            │  │
 │  │  Properties:                                               │  │
 │  │    Center = (Min + Max) * 0.5f                             │  │
 │  │    Extents = (Max - Min) * 0.5f                            │  │
 │  │    Size   = Max - Min                                      │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  This is a VALUE TYPE — no heap allocation, no GC pressure.     │
 └──────────────────────────────────────────────────────────────────┘


 PERFORMANCE COMPARISON
 ┌──────────────────────────────────┬─────────────────────────────────┐
 │  Unity Bounds.Encapsulate()      │  AABB.Encapsulate()             │
 │  ────────────────────────────────│  ───────────────────────────    │
 │  Bounds (managed class)          │  AABB (value type struct)       │
 │  Heap allocation                 │  Stack only                     │
 │  Method call overhead            │  Inlined by Burst compiler      │
 │  Not Burst-compatible            │  Full Burst-compatible          │
 │  Uses float3 internally          │  Uses math.min/max (SIMD)       │
 │  Includes validation checks      │  Zero branching                 │
 └──────────────────────────────────┴─────────────────────────────────┘

 SIMD OPTIMIZATION:
 ═════════════════
 math.min(float3, float3) and math.max(float3, float3) compile to
 single SIMD instructions on supported hardware:
   • x86: VMINPS / VMAXPS (AVX)
   • ARM: FMIN / FMAX (NEON)
 This means all 3 components (x,y,z) are compared in parallel.

## Verified Data

```
with: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/MathematicsExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/MathematicsExtensions.cs)
- [BovineLabs.Core.Tests/Extensions/MathematicsExtensionsTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Extensions/MathematicsExtensionsTests.cs)
