```
╔══════════════════════════════════════════════════════════════════════════════╗
║        mathex.FromToRotation — Shortest Quaternion Between Vectors          ║
║              Source: BovineLabs.Core/Utility/mathex.cs                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  Computes the shortest-arc quaternion that rotates vector 'from' onto
  vector 'to'. Handles all edge cases: zero vectors, parallel, and
  anti-parallel directions.

  Result satisfies: q * from_normalized == to_normalized

SIGNATURE
═════════

  quaternion FromToRotation(float3 from, float3 to)

DECISION TREE
══════════════

                    ┌──────────────────┐
                    │  Input: from, to │
                    └────────┬─────────┘
                             │
                    ┌────────▼─────────┐
                    │  lenSq(from) <=  │──── Yes ──▶ return IDENTITY
                    │  1e-20 OR        │             (degenerate input)
                    │  lenSq(to) <=    │
                    │  1e-20 OR        │
                    │  !isfinite?      │
                    └────────┬─────────┘
                             │ No
                    ┌────────▼─────────┐
                    │  Normalize both  │
                    │  f = from × rsqrt│
                    │  t = to   × rsqrt│
                    └────────┬─────────┘
                             │
                    ┌────────▼─────────┐
                    │  dot = dot(f, t) │
                    └────────┬─────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
     dot > 1 - 1e-6    normal case    dot < -1 + 1e-6
     (same direction)                   (opposite direction)
              │              │              │
              ▼              ▼              ▼
     ┌────────────┐  ┌────────────┐  ┌──────────────────────┐
     │ return     │  │ GENERAL    │  │ 180° ROTATION        │
     │ IDENTITY   │  │ CASE       │  │ Find perpendicular   │
     └────────────┘  └────────────┘  │ axis via cross       │
                                    └──────────────────────┘

GENERAL CASE — QUATERNION CONSTRUCTION
══════════════════════════════════════

  Given normalized f and t with -1 < dot < 1:

  ┌──────────────────────────────────────────────────────────────────┐
  │  v = cross(f, t)               ← rotation axis (perpendicular)  │
  │  q = quaternion(v.x, v.y, v.z, 1 + dot)                       │
  │  return normalize(q)           ← unit quaternion                │
  └──────────────────────────────────────────────────────────────────┘

  Quaternion layout:
  ┌────────┬────────┬────────┬────────┐
  │   q.x  │   q.y  │   q.z  │   q.w  │
  │  v.x   │  v.y   │  v.z   │ 1+dot  │
  │ (axis) │ (axis) │ (axis) │ (real) │
  └────────┴────────┴────────┴────────┘

  Why (v, 1+dot)?
  ● cross(f,t) gives the axis perpendicular to both vectors
  ● 1+dot gives a real component that encodes the half-angle
  ● Normalization converts to unit quaternion

  Geometric proof:
    If f·t = cos(θ), then ||cross(f,t)|| = sin(θ)
    The unnormalized quaternion has magnitude √((1+cos θ)² + sin²θ)
    After normalization, it correctly represents rotation by 2×half-angle = θ

OPPOSITE DIRECTION — 180° CASE
══════════════════════════════

  When from and to point in exactly opposite directions (dot ≈ -1),
  the cross product degenerates to zero. We must find ANY perpendicular
  axis to rotate 180° around.

  ┌──────────────────────────────────────────────────────────────────┐
  │  // Pick the most stable perpendicular axis                      │
  │  if |f.x| < 0.5:                                               │
  │    axis = cross(f, (1,0,0))     ← f is NOT near X-axis          │
  │  else:                                                          │
  │    axis = cross(f, (0,1,0))     ← f is near X-axis              │
  │                                                                  │
  │  // Fallback if that cross is still near-zero                    │
  │  if lenSq(axis) <= 1e-12:                                      │
  │    axis = cross(f, (0,0,1))     ← last resort                   │
  │                                                                  │
  │  return AxisAngle(normalize(axis), π)                           │
  └──────────────────────────────────────────────────────────────────┘

  Why the |f.x| < 0.5 check?
  ● cross(f, axis) has magnitude = |f| × |axis| × sin(angle between)
  ● If f is parallel to axis, cross = 0 (bad!)
  ● We pick whichever axis is FURTHEST from f for maximum sin()

  ┌──────────────────────────────────────┐
  │         Y-axis                       │
  │          ↑                           │
  │          │  If f is near Y-axis:     │
  │          │  cross(f, (1,0,0)) is big │
  │          ● ─ ─ ─ ─▶                 │
  │         ╱    f vector                │
  │        ╱                             │
  │       ╱  If f is near X-axis:       │
  │      ╱   cross(f, (0,1,0)) is big   │
  │     ╱                                │
  │    ●──────────▶ X-axis               │
  └──────────────────────────────────────┘

VISUAL EXAMPLE
══════════════

  FromToRotation( (1,0,0), (0,1,0) ):

      Y                          Y
      │                          │
      │           →              ●───▶ rotated (0,1,0)
      │          ╱               │ ╱
      │         ╱     ====>     │╱  90° rotation around Z
      │        ╱                ●
      └──────●───▶ X      └───────▶ X
      from=(1,0,0)         identity for from, rotated to

  dot = 0 → perpendicular vectors → 90° rotation
  v = cross((1,0,0), (0,1,0)) = (0, 0, 1)
  q = quaternion(0, 0, 1, 1) → normalized = (0, 0, 0.707, 0.707)
  This is a 90° rotation around Z-axis ✓

EDGE CASE SUMMARY
═════════════════

  ┌────────────────────┬───────────────────────────────────────────┐
  │ Condition          │ Result                                    │
  ├────────────────────┼───────────────────────────────────────────┤
  │ Zero-length vector │ quaternion.identity                       │
  │ Non-finite input   │ quaternion.identity                       │
  │ from == to         │ quaternion.identity (0° rotation)         │
  │ from == -to        │ 180° around most-stable perpendicular    │
  │ General case       │ normalize(cross(f,t), 1+dot(f,t))        │
  └────────────────────┴───────────────────────────────────────────┘
```
