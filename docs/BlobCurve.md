BlobCurve - Bakes Unity AnimationCurves Into BlobAssets for Fast Burst Evaluation
===================================================================================

Overview
--------

BlobCurve converts a Unity AnimationCurve (managed object, not Burst-safe)
into a frozen BlobAsset that can be evaluated in Burst-compiled jobs.  Each
pair of adjacent keyframes becomes a pre-computed cubic polynomial segment
stored as a float4 of coefficients.  At runtime, evaluation reduces to a
binary search over time stamps followed by a single dot product.

The system supports variants for float (BlobCurve), float2 (BlobCurve2),
float3 (BlobCurve3), and float4 (BlobCurve4), all sharing the same header
and search infrastructure.

Architecture Diagram
--------------------

  BlobAssetReference<BlobCurve>
  ┌────────────────────────────────────────────────────────────────────┐
  │ BlobAssetHeader (Unity internal)                                    │
  ├────────────────────────────────────────────────────────────────────┤
  │ BlobCurve                                                           │
  │                                                                     │
  │  ┌─ header: BlobCurveHeader ─────────────────────────────────────┐ │
  │  │                                                                │ │
  │  │  WrapModePrev : short  (Clamp=0, Loop=1, PingPong=2)          │ │
  │  │  WrapModePost : short                                         │ │
  │  │  SegmentCount : int    (= keyframeCount - 1, min 1)           │ │
  │  │  StartTime    : float                                         │ │
  │  │  EndTime      : float                                         │ │
  │  │                                                                │ │
  │  │  ┌─ Times: BlobArray<float> ──────────────────────────────┐   │ │
  │  │  │  length = keyframeCount + 2                             │   │ │
  │  │  │                                                         │   │ │
  │  │  │  [0]        [1]    [2]    ...  [n-1]   [n]       [n+1] │   │ │
  │  │  │  +MaxValue  t₀     t₁    ...  tₙ₋₂    tₙ₋₁  -MinValue │   │ │
  │  │  │  (sentinel)  ↑ real timestamps start ↑    (sentinel)    │   │ │
  │  │  └─────────────────────────────────────────────────────────┘   │ │
  │  └────────────────────────────────────────────────────────────────┘ │
  │                                                                     │
  │  ┌─ segments: BlobArray<BlobCurveSegment> ──────────────────────┐  │
  │  │  length = SegmentCount                                       │  │
  │  │                                                               │  │
  │  │  Each BlobCurveSegment stores:                                │  │
  │  │    factors: float4   = [a₃, a₂, a₁, a₀]                     │  │
  │  │                                                               │  │
  │  │  Evaluate(t) = dot(factors, [t³, t², t, 1])                  │  │
  │  │             = a₃t³ + a₂t² + a₁t + a₀                        │  │
  │  │                                                               │  │
  │  │  [0]: factors for key₀→key₁                                  │  │
  │  │  [1]: factors for key₁→key₂                                  │  │
  │  │  ...                                                          │  │
  │  │  [n-2]: factors for keyₙ₋₂→keyₙ₋₁                           │  │
  │  └───────────────────────────────────────────────────────────────┘  │
  └────────────────────────────────────────────────────────────────────┘


Segment Factor Computation (Hermite → Polynomial)
--------------------------------------------------

  Given two keyframes: k₀(time₀, val₀, outTangent₀) and k₁(time₁, val₁, inTangent₁)

  ┌─────────────────────────────────────────────────────────────────┐
  │  duration = time₁ - time₀                                      │
  │  m₀ = outTangent₀ × duration                                   │
  │  m₁ = inTangent₁ × duration                                    │
  │                                                                 │
  │  Hermite basis → Cubic coefficients via matrix multiply:        │
  │                                                                 │
  │  ┌                    ┐ ┌    ┐     ┌     ┐                      │
  │  │  2   1   1  -2     │ │ v₀ │     │  a₃  │                    │
  │  │ -3  -2  -1   3     │ │ m₀ │  =  │  a₂  │                    │
  │  │  0   1   0   0     │ │ m₁ │     │  a₁  │                    │
  │  │  1   0   0   0     │ │ v₁ │     │  a₀  │                    │
  │  └                    ┘ └    ┘     └     ┘                      │
  │                                                                 │
  │  Special case: if either tangent is infinity → constant segment │
  │  (BezierFactor with all identical values = val₀)                │
  └─────────────────────────────────────────────────────────────────┘


Times Array Layout (Sentinel Pattern)
--------------------------------------

  For N keyframes, the Times array has N+2 entries:

  Index:   0          1      2      3     ...   N-1     N        N+1
  Value:  +INF     time₀  time₁  time₂  ...  timeₙ₋₂  timeₙ₋₁  -INF

  Purpose of sentinels:
    [0] = +MaxValue  →  binary search lower bound (prevents underflow)
    [N+1] = -MinValue →  binary search upper bound (prevents overflow)

  This allows the Search() function to read *(float2*)(times + i + 1)
  as the [start, end] time range for segment i without bounds checking.


Evaluation Pipeline
-------------------

  ┌─────────────────────────────────────────────────────────────────┐
  │  Evaluate(float time, ref BlobCurveCache cache)                 │
  │                                                                 │
  │  STEP 1: Wrap time                                              │
  │  ┌──────────────────────────────────────────────────────────┐   │
  │  │  if Clamp:  clamp(time, Start, End)                      │   │
  │  │  if Loop:   (time - Start) % Duration + Start            │   │
  │  │  if PingPong: mirror on odd loop counts                  │   │
  │  └──────────────────────────────────────────────────────────┘   │
  │                        │                                        │
  │                        ▼ wrappedTime                            │
  │  STEP 2: Binary Search (with cache)                             │
  │  ┌──────────────────────────────────────────────────────────┐   │
  │  │  Cache: { Index, NeighborhoodTimes: float2 }             │   │
  │  │                                                           │   │
  │  │  if cached range contains wrappedTime:                    │   │
  │  │    → return cached index (HIT, no search needed)         │   │
  │  │                                                           │   │
  │  │  Check neighbors (cache.Index ± 1):                      │   │
  │  │    → if hit, update cache, return                        │   │
  │  │                                                           │   │
  │  │  Full binary search:                                      │   │
  │  │    lo=0, hi=SegmentCount-1                                │   │
  │  │    read *(float2*)(times + i + 1) as [t_i, t_{i+1}]     │   │
  │  │    branchless: lo/hi adjust via math.select               │   │
  │  │    until wrappedTime in [t_i, t_{i+1}]                   │   │
  │  └──────────────────────────────────────────────────────────┘   │
  │                        │                                        │
  │                        ▼ segment index i, local t               │
  │  STEP 3: Evaluate polynomial                                    │
  │  ┌──────────────────────────────────────────────────────────┐   │
  │  │  t_serial = (t³, t², t, 1)                               │   │
  │  │  result = dot(segments[i].factors, t_serial)              │   │
  │  └──────────────────────────────────────────────────────────┘   │
  └─────────────────────────────────────────────────────────────────┘


Binary Search Detail (Branchless Core)
---------------------------------------

  The search loop uses no branches for the comparison -- only for the
  loop continuation condition:

  ┌──────────────────────────────────────────────────────────┐
  │  do {                                                    │
  │    timeRange = *(float2*)(times + (i + 1));              │
  │    //  timeRange.x = start of segment i                  │
  │    //  timeRange.y = end of segment i (= start of i+1)   │
  │                                                          │
  │    goLow  = wrappedTime < timeRange.x;                   │
  │    goHigh = wrappedTime > timeRange.y;                   │
  │    notFound = goLow | goHigh;                            │
  │                                                          │
  │    lo = select(lo,    i + 1, goHigh);   // move right    │
  │    hi = select(hi,    i - 1, goLow);    // move left     │
  │    i  = select(i, lo + ((hi-lo)>>1), notFound);          │
  │  }                                                       │
  │  while (notFound & (lo <= hi));                          │
  │                                                          │
  │  // When notFound becomes false, i is the segment index  │
  │  // and timeRange contains [t_i, t_{i+1}]                │
  │  // t = (wrappedTime - t_i) / (t_{i+1} - t_i)           │
  └──────────────────────────────────────────────────────────┘


Cache Structure (BlobCurveCache)
---------------------------------

  ┌────────────────────────────────────────────┐
  │  BlobCurveCache                             │
  │  ┌────────────────────────────────────────┐ │
  │  │  NeighborhoodTimes: float2             │ │
  │  │    .x = start time of cached segment   │ │
  │  │    .y = end time of cached segment     │ │
  │  │                                        │ │
  │  │  Index: int                            │ │
  │  │    segment index from last evaluation  │ │
  │  └────────────────────────────────────────┘ │
  │                                             │
  │  Empty sentinel: Index = int.MinValue       │
  │                  NeighborhoodTimes = NaN    │
  │                                             │
  │  On sequential evaluation (typical):        │
  │    - Cache hit rate approaches 100%         │
  │    - Skips binary search entirely           │
  │    - Falls back to neighbor check           │
  └────────────────────────────────────────────┘


Multidimensional Variants
--------------------------

  ┌─────────────┐   ┌──────────────────┐   ┌───────────────────────────┐
  │ BlobCurve   │   │ BlobCurveSegment │   │ Segment stores            │
  │ (float)     │──▶│ float4 factors   │   │ a₃,a₂,a₁,a₀ per axis     │
  ├─────────────┤   ├──────────────────┤   │                           │
  │ BlobCurve2  │──▶│ float4x2 factors │   │ X axis: float4            │
  │ (float2)    │   │                  │   │ Y axis: float4            │
  ├─────────────┤   ├──────────────────┤   ├───────────────────────────┤
  │ BlobCurve3  │──▶│ float4x3 factors │   │ X,Y,Z axes each: float4  │
  │ (float3)    │   │                  │   │                           │
  ├─────────────┤   ├──────────────────┤   ├───────────────────────────┤
  │ BlobCurve4  │──▶│ float4x4 factors │   │ X,Y,Z,W axes each:float4 │
  │ (float4)    │   │                  │   │                           │
  └─────────────┘   └──────────────────┘   └───────────────────────────┘

  All variants share the same BlobCurveHeader and Times array.
  Evaluation: math.dot(timeSerial, factors) or math.mul(timeSerial, factors)


Construction Flow
------------------

  ┌───────────────────────────────────────┐
  │  AnimationCurve (managed Unity obj)   │
  │    keys: [k0, k1, k2, k3, k4]        │
  └──────────────────┬────────────────────┘
                     │
                     ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  BlobCurve.Construct(ref builder, ref blobCurve, curve)     │
  │                                                             │
  │  1. Validate curve (non-null, non-empty, no weights)        │
  │  2. segmentCount = keys.Length - 1 (or 1 if single key)    │
  │  3. Set wrap modes                                          │
  │  4. Allocate Times[keys.Length + 2]                         │
  │     Times[0] = +MaxValue  (left sentinel)                   │
  │     Times[keys.Length + 1] = -MinValue  (right sentinel)    │
  │  5. Allocate Segments[segmentCount]                         │
  │     for each key pair (kᵢ, kᵢ₊₁):                         │
  │       Times[i+1] = kᵢ.time                                 │
  │       Segments[i] = BlobCurveSegment(kᵢ, kᵢ₊₁)            │
  │         └─ factors = HermiteMat × [v₀, m₀, m₁, v₁]         │
  │  6. Set StartTime, EndTime                                  │
  └──────────────────┬──────────────────────────────────────────┘
                     │
                     ▼  CreateBlobAssetReference()
         BlobAssetReference<BlobCurve>


BlobCurveSampler (Stateful Cache Wrapper)
------------------------------------------

  ┌─────────────────────────────────────────────────────┐
  │  BlobCurveSampler                                    │
  │                                                      │
  │  Curve: BlobAssetReference<BlobCurve>  (read-only)  │
  │  cache: BlobCurveCache  (mutable, per-instance)      │
  │                                                      │
  │  Evaluate(time):                                     │
  │    → Curve.Value.Evaluate(time, ref cache)           │
  │    (automatically maintains cache across calls)      │
  │                                                      │
  │  EvaluateWithoutCache(time):                         │
  │    → Curve.Value.Evaluate(time)                      │
  │    (no cache, binary search every time)              │
  └─────────────────────────────────────────────────────┘

  Usage in jobs:
    var sampler = new BlobCurveSampler(blobCurveRef);
    for (int i = 0; i < count; i++)
        values[i] = sampler.Evaluate(times[i]);
    // Sequential access → cache hits → near-free lookups


Key Design Decisions
--------------------

1. **Pre-baked polynomial coefficients.**  Each segment stores 4 floats
   (the cubic coefficients a₃..a₀) so runtime evaluation is just
   `dot(factors, [t³,t²,t,1])` -- one SIMD dot product, no Hermite
   basis matrix multiply needed at runtime.

2. **Sentinel-bounded times array.**  Adding +INF and -INF sentinels at
   the edges eliminates bounds checks in the binary search.  The search
   can safely read `*(float2*)(times + i + 1)` for any valid segment
   index without special-casing the first and last segments.

3. **Cache with neighborhood check.**  The BlobCurveCache stores the time
   range of the last-evaluated segment.  On the next call, it checks:
   is the new time still within this range?  If not, check ±1 neighbor.
   Only if both miss does it fall back to binary search.  This makes
   sequential evaluation (the common case) O(1) amortized.

4. **Branchless binary search.**  The inner loop uses `math.select` for
   all direction decisions, avoiding branch mispredictions on the hot
   path.  Only the loop continuation `while (notFound & (lo <= hi))`
   involves a branch.

5. **Single-keyframe special case.**  When an AnimationCurve has only one
   key, the segment is a constant polynomial (a₀ = value, all others 0)
   and the times array is filled with the same time repeated 4 times.

6. **No weighted tangent support.**  The Unity `WeightedMode` is checked
   at construction time and a warning is logged.  Only the default
   Hermite interpolation (non-weighted) is supported.


Performance Characteristics
---------------------------

| Operation                | Time Complexity     | Notes                        |
|--------------------------|---------------------|------------------------------|
| Evaluate (cache hit)     | O(1)                | No search, just dot product  |
| Evaluate (neighbor hit)  | O(1)                | Check ±1 segment             |
| Evaluate (cache miss)    | O(log n)            | Binary search + dot product  |
| EvaluateWithoutCache     | O(log n)            | Always binary search         |
| Construction             | O(n)                | Per-segment factor compute   |

Memory per curve (BlobCurve with N keyframes):
  - Header:     ~24 bytes (wrap modes, segment count, start/end time)
  - Times:      (N + 2) × 4 bytes
  - Segments:   (N - 1) × 16 bytes  (float4 each)
  - Total:      ~24 + (N+2)×4 + (N-1)×16 ≈ 20N + 16 bytes

BlobCurve2:  segments are float4x2 = 32 bytes each
BlobCurve3:  segments are float4x3 = 48 bytes each
BlobCurve4:  segments are float4x4 = 64 bytes each

Burst-compatible:   Yes (100% unmanaged, no managed references)
Thread-safe reads:  Yes (blob is immutable; cache is per-thread)
SIMD-friendly:      Yes (float4 dot product, float2 reads)

## Source

- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurve.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurve.cs)
- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSamplerT.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSamplerT.cs)
- [BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSampler.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/Curve/BlobCurveSampler.cs)
