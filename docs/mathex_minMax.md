```
╔══════════════════════════════════════════════════════════════════════════════╗
║              mathex.minMax — SIMD Accelerated Bounds Calculation            ║
║              Source: BovineLabs.Core/Utility/mathex.cs                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  Computes min/max bounds of float2 arrays using float4 SIMD intrinsics.
  Packs pairs of float2 into a single float4 register, processing 2 points
  per SIMD instruction — effectively 2x throughput vs scalar.

ARCHITECTURE — float2* minMax
══════════════════════════════

  Memory Layout (float2 array):
  ┌────────┬────────┬────────┬────────┬────────┬────────┬────────┬────────┐
  │ f2[0]  │ f2[1]  │ f2[2]  │ f2[3]  │ f2[4]  │ f2[5]  │ f2[6]  │ f2[7]  │
  │ x0  y0 │ x1  y1 │ x2  y2 │ x3  y3 │ x4  y4 │ x5  y5 │ x6  y6 │ x7  y7 │
  └────┬───┴────┬───┴────┬───┴────┬───┴────┬───┴────┬───┴────┬───┴────┬───┘
       │        │        │        │        │        │        │        │
       ▼        ▼        ▼        ▼        ▼        ▼        ▼        ▼
  Reinterpret as float4 (cast float2* → float4*):
  ┌─────────────────────┐ ┌─────────────────────┐ ┌─────────────────────┐ ┌─────────────────────┐
  │     f4[0]           │ │     f4[1]           │ │     f4[2]           │ │     f4[3]           │
  │  x0  y0  x1  y1    │ │  x2  y2  x3  y3    │ │  x4  y4  x5  y5    │ │  x6  y6  x7  y7    │
  └─────────────────────┘ └─────────────────────┘ └─────────────────────┘ └─────────────────────┘
          │                       │                       │                       │
          ▼                       ▼                       ▼                       ▼
  ┌─────────────────────────────────────────────────────────────────────────────────────┐
  │                       SIMD PROCESSING LOOP                                          │
  │                                                                                     │
  │   numSamples4 = length >> 1  (divide by 2 — each float4 = 2 float2s)                │
  │                                                                                     │
  │   for (i = 0..numSamples4):                                                         │
  │     value4    = ((float4*)values)[i]     ← load 128 bits = 2 points                 │
  │     minValue4 = min(minValue4, value4)   ← SIMD min per lane                       │
  │     maxValue4 = max(maxValue4, value4)   ← SIMD max per lane                       │
  └─────────────────────────────────────────────────────────────────────────────────────┘
          │
          ▼
  ┌──────────────────────────────────────────────────────────────────┐
  │                    REDUCTION PHASE                                │
  │                                                                  │
  │  minValue4 layout after loop:        maxValue4 layout after loop: │
  │  ┌─────┬─────┬─────┬─────┐          ┌─────┬─────┬─────┬─────┐   │
  │  │minX0│minY0│minX1│minY1│          │maxX0│maxY0│maxX1│maxY1│   │
  │  └──┬──┴──┬──┴──┬──┴──┬──┘          └──┬──┴──┬──┴──┬──┴──┬──┘   │
  │     │     │     │     │                │     │     │     │      │
  │  cmin(.xz)  cmin(.yw)              cmax(.xz)  cmax(.yw)         │
  │     │     │     │     │                │     │     │     │      │
  │     ▼     ▼     │     │                ▼     ▼     │     │      │
  │  minX = min(minX0, minX1)         maxX = max(maxX0, maxX1)      │
  │  minY = min(minY0, minY1)         maxY = max(maxY0, maxY1)      │
  │                                                                  │
  │  → float2(minX, minY)            → float2(maxX, maxY)           │
  └──────────────────────────────────────────────────────────────────┘
          │
          ▼
  ┌──────────────────────────────────────────────────────────────────┐
  │                    TAIL HANDLING                                  │
  │                                                                  │
  │  for (i = numSamples4*2..length):                                │
  │    minValue = min(minValue, values[i])   ← scalar fallback      │
  │    maxValue = max(maxValue, values[i])   ← for odd-length arrays │
  └──────────────────────────────────────────────────────────────────┘
          │
          ▼
  Output: Rect.MinMaxRect(min.x, min.y, max.x, max.y)

SIMD REGISTER DETAIL — float4 Lane Mapping
═══════════════════════════════════════════

  When you cast float2* → float4* and load one element:

  Memory (2 × float2 = 16 bytes):
  ┌────────┬────────┬────────┬────────┐
  │  x[0]  │  y[0]  │  x[1]  │  y[1]  │    4 × float32 = 128 bits
  └────────┴────────┴────────┴────────┘

  float4 Register:
  ┌────────┬────────┬────────┬────────┐
  │  .x    │  .y    │  .z    │  .w    │
  │  x[0]  │  y[0]  │  x[1]  │  y[1]  │
  └────────┴────────┴────────┴────────┘

  math.min(float4, float4) operates per-lane:
  ┌────────┬────────┬────────┬────────┐
  │ min(X0)│ min(Y0)│ min(X1)│ min(Y1)│    ← 4 comparisons in 1 op
  └────────┴────────┴────────┴────────┘

  Reduction via swizzle:
    .xz → [minX0, minX1] → cmin → overall minX
    .yw → [minY0, minY1] → cmin → overall minY

SCALAR VARIANTS (float3*, int*, float* max/min)
═════════════════════════════════════════════════

  max(float* values, int length) — processes 4 floats at a time:

  ┌────────────┬────────────┬────────────┬────────────┐
  │  values[0] │  values[1] │  values[2] │  values[3] │  ← 1 float4 load
  └────────────┴────────────┴────────────┴────────────┘
       ↓ math.max per lane ↓
  ┌────────────┬────────────┬────────────┬────────────┐
  │  maxVal.x  │  maxVal.y  │  maxVal.z  │  maxVal.w  │  ← accumulate
  └────────────┴────────────┴────────────┴────────────┘
       ↓ after loop ↓
       math.cmax(maxValue4) → single scalar maximum

  Same pattern for: int max, float min, int min, float sum, int sum

PERFORMANCE ANALYSIS
════════════════════

  ┌───────────────────┬────────────────┬─────────────────────────────┐
  │ Method            │ Elements/cycle │ Technique                   │
  ├───────────────────┼────────────────┼─────────────────────────────┤
  │ float2 minMax     │ 2 float2/cycle │ float4 reinterpret cast     │
  │ float max         │ 4 float/cycle  │ float4 stride-4 loop        │
  │ int max           │ 4 int/cycle    │ int4 stride-4 loop          │
  │ float3 minMax     │ 1 float3/cycle │ scalar (no clean SIMD map)  │
  └───────────────────┴────────────────┴─────────────────────────────┘

  Tail loop handles remainder: numSamples4 << 2 (or << 1) to length
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Utility/mathex.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/mathex.cs)
- [BovineLabs.Core.Tests/Utility/mathexTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/mathexTests.cs)
- [BovineLabs.Core.Tests/Utility/MathExPerformanceTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/MathExPerformanceTests.cs)
