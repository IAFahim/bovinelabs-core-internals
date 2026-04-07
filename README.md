# CurveRemapUtility — Inner Workings

## Overview

CurveRemapUtility normalizes an AnimationCurve's keyframe times from their original
range to a new clip-local range [clipIn, clipIn + clipDuration], adjusting tangent
slopes accordingly. Only works with Clamp wrap modes.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   CurveRemapUtility (static class)                          │
│                                                                             │
│   TryRemapToClipLength(curve, clipIn, clipDuration, out remappedCurve)      │
│                                                                             │
│   IsClampWrapMode(curve) → validates pre/post wrap mode                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Time Remapping Algorithm

```
  SOURCE CURVE                         REMAPPED CURVE
  ════════════                         ══════════════

  firstTime=0.5                         clipIn=2.0
  lastTime=3.0                          clipDuration=4.0
  sourceDuration=2.5

  ┌────────────────────┐               ┌────────────────────────────┐
  │     *              │               │              *             │
  │    / \             │    remap      │             / \            │
  │   /   *            │  ──────────►  │            /   *          │
  │  *     \           │               │           *     \         │
  │         *          │               │                  *        │
  └─┬──────────┬───────┘               └─┬────────────────────┬───┘
   0.5        3.0                        2.0                  6.0
   firstT     lastT                      clipIn    clipIn + clipDur

  Formula:
    timeScale = clipDuration / sourceDuration
    new_time  = clipIn + (old_time - firstTime) * timeScale
    new_tangent = old_tangent / timeScale
```

## Keyframe Transform Detail

```
  For each keyframe:
  ┌─────────────────────────────────────────────────────────────────┐
  │                                                                 │
  │  ORIGINAL KEYFRAME              REMAPPED KEYFRAME               │
  │  ┌─────────────────┐            ┌─────────────────┐             │
  │  │ time = t         │            │ time = clipIn +  │            │
  │  │ value = v        │  ──────►  │   (t - firstT) * │            │
  │  │ inTangent = i    │            │   timeScale      │            │
  │  │ outTangent = o   │            │                  │            │
  │  └─────────────────┘            │ inTangent =      │            │
  │                                  │   i / timeScale  │            │
  │  timeScale = clipDur / srcDur   │ outTangent =     │            │
  │                                  │   o / timeScale  │            │
  │                                  │ value = v        │            │
  │                                  └─────────────────┘             │
  │                                                                 │
  │  NOTE: Infinite tangents (broken/stepped) are preserved as-is   │
  └─────────────────────────────────────────────────────────────────┘
```

## Full Flow

```
  TryRemapToClipLength(curve, clipIn, clipDuration, out remappedCurve)
         │
         ▼
  ┌─────────────────────────────────────────────────────────┐
  │  GUARDS:                                                │
  │  ┌───────────────────────────────────────────────────┐  │
  │  │ curve == null?          → return false            │  │
  │  │ clipDuration <= ε?     → return false            │  │
  │  │ !IsClampWrapMode?      → return false            │  │
  │  │ sourceKeys.Length==0?  → return false            │  │
  │  └───────────────────────────────────────────────────┘  │
  │                                                         │
  │  EXTRACT RANGE:                                         │
  │  firstTime = sourceKeys[0].time                         │
  │  lastTime  = sourceKeys[^1].time                        │
  │  sourceDuration = lastTime - firstTime                  │
  │                                                         │
  │  BRANCH:                                                │
  │  ┌─────────────────────────────────────────────────┐    │
  │  │ sourceDuration ≈ 0 ?                             │    │
  │  │   All keys collapse to clipIn:                   │    │
  │  │     key.time = clipIn (for every key)            │    │
  │  └─────────────────────────────────────────────────┘    │
  │  ┌─────────────────────────────────────────────────┐    │
  │  │ sourceDuration > 0 ?                             │    │
  │  │   timeScale = clipDuration / sourceDuration      │    │
  │  │   for each key:                                  │    │
  │  │     key.time = clipIn + (key.time-first)*scale   │    │
  │  │     if !inf(key.inTangent):  key.inTangent/=s    │    │
  │  │     if !inf(key.outTangent): key.outTangent/=s   │    │
  │  └─────────────────────────────────────────────────┘    │
  │                                                         │
  │  OUTPUT:                                                │
  │  remappedCurve = new AnimationCurve(remappedKeys)       │
  │  remappedCurve.preWrapMode  = curve.preWrapMode         │
  │  remappedCurve.postWrapMode = curve.postWrapMode        │
  │  return true                                            │
  └─────────────────────────────────────────────────────────┘
```

## IsClamp Wrap Mode Check

```
  IsClampWrapMode(curve)
         │
         ▼
  ┌─────────────────────────────────────────────────────┐
  │  return curve != null                                │
  │      && IsClamp(curve.preWrapMode)                   │
  │      && IsClamp(curve.postWrapMode)                  │
  │                                                      │
  │  IsClamp(mode):                                      │
  │    mode == Clamp        ✓                            │
  │    mode == ClampForever ✓                            │
  │    mode == Default      ✓                            │
  │    (Loop, PingPong)     ✗  → not remappable          │
  └─────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Clamp-only**: Looping curves can't be meaningfully remapped to a clip range
  because they extend beyond their keyframe bounds — only clamp modes are safe.
- **Tangent scaling**: Tangents represent dy/dx; when x-scale changes, tangents
  must be divided by `timeScale` to preserve the visual slope.
- **Infinite tangent preservation**: `float.IsInfinity` checks ensure broken/stepped
  keyframes (used for constant transitions) aren't corrupted.
- **Zero-duration edge case**: When all keys have the same time, they collapse to
  `clipIn` — prevents division by zero.

## Source File

- `BovineLabs.Core/Utility/CurveRemapUtility.cs`
