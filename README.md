# AabbExtensions

**Provides fast mathematical expansion and shrinking of bounding boxes**

Static extension methods for Unity Physics `Aabb` (Axis-Aligned Bounding Box)
that allow uniform and per-axis expansion, as well as safe shrinking that
prevents negative extents (inverted boxes).

---

## Architecture

```
  ┌──────────────────────────────────────────────────────────┐
  │  AabbExtensions (static)                                 │
  │                                                          │
  │  ┌────────────────┐  ┌────────────────┐                 │
  │  │ Shrink(ref,    │  │ ShrinkSafe(ref,│                 │
  │  │   float amount)│  │   float amount)│                 │
  │  │                │  │                │                 │
  │  │ Min += amount  │  │ amount=abs(a)  │                 │
  │  │ Max -= amount  │  │ Shrink(amount) │                 │
  │  └────────────────┘  │ EnsureSafe()   │                 │
  │                      └────────────────┘                 │
  │  ┌────────────────┐  ┌────────────────┐                 │
  │  │ ExpandX/Y/Z(   │  │ EnsureSafe(    │  (private)      │
  │  │  ref, float a) │  │  ref Aabb)     │                 │
  │  │                │  │                │                 │
  │  │ axis-specific  │  │ extents=max(   │                 │
  │  │ expansion      │  │   Extents, 0)  │                 │
  │  └────────────────┘  │ recalc from    │                 │
  │                      │ Center         │                 │
  │                      └────────────────┘                 │
  └──────────────────────────────────────────────────────────┘
```

## Bounding Box Operations Visualized

```
  Original AABB:
  ┌──────────────────────────────┐
  │          Max                 │
  │    ┌───────────────┐        │
  │    │               │        │
  │    │   Center ●    │ Extents│
  │    │               │        │
  │    └───────────────┘        │
  │          Min                 │
  └──────────────────────────────┘

  ExpandX(amount):
  ├──────────────────────────────────┤
  │  ┌───────────────────────┐      │
  │  │                       │      │
  │  │       Center ●        │      │
  │  │                       │      │
  │  └───────────────────────┘      │
  ├──.Min.x -= amt    Max.x += amt─┤

  ExpandY(amount):
  ┌──────────────────────────┐
  │                          │ ← Min.y -= amount
  │    ┌───────────────┐     │
  │    │   Center ●    │     │
  │    └───────────────┘     │
  │                          │ ← Max.y += amount
  └──────────────────────────┘

  Shrink(amount):
       ┌─────────────┐
       │             │
       │  Center ●   │  Min += amount
       │             │  Max -= amount
       └─────────────┘

  ShrinkSafe(large_amount):
       ┌─┐
       │●│  ← collapsed to center, no negative extents
       └─┘
       If amount > half-extent:
         clamp extents to 0
         rebuild from center
```

## EnsureSafe Logic

```
  ┌────────────────────────────────────────────┐
  │  EnsureSafe(ref Aabb)                      │
  │                                            │
  │  1. extents = max(aabb.Extents, float3(0)) │
  │     ← prevents negative extent values      │
  │                                            │
  │  2. center = aabb.Center                   │
  │                                            │
  │  3. aabb.Min = center - (extents / 2)      │
  │     aabb.Max = center + (extents / 2)      │
  │     ← reconstruct symmetrically            │
  └────────────────────────────────────────────┘
```

## All Methods

| Method | Axis | Direction | Safe? |
|--------|------|-----------|-------|
| `Shrink(amount)` | All XYZ | Inward | No (can invert) |
| `ShrinkSafe(amount)` | All XYZ | Inward | Yes |
| `ExpandX(amount)` | X only | Outward | N/A |
| `ExpandY(amount)` | Y only | Outward | N/A |
| `ExpandZ(amount)` | Z only | Outward | N/A |

All methods are `AggressiveInlining` for zero overhead in Burst-compiled code.

## Source

`BovineLabs.Core/Extensions/AabbExtensions.cs`

## Source

- [BovineLabs.Core/Extensions/AabbExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/AabbExtensions.cs)
- [BovineLabs.Core/Extensions/MinMaxAABBExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/MinMaxAABBExtensions.cs)
