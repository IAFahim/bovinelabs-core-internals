# PhysicsExtensions.Raycast

**Calculates mathematical ray-plane intersections inside Burst quickly**

Extension methods on `Unity.Physics.Plane` and `Unity.Physics.Ray` that compute
ray-plane intersection using pure math. No physics scene queries needed.

---

## Ray-Plane Intersection Algorithm

```
  ┌──────────────────────────────────────────────────────────┐
  │  Plane.Raycast(Ray ray, out float enter) → bool          │
  │                                                          │
  │  Given:                                                  │
  │    Plane: Normal, Distance  (plane equation: N·P + D = 0)│
  │    Ray:   Origin, Displacement                            │
  │                                                          │
  │  Math:                                                   │
  │    direction = normalize(ray.Displacement)                │
  │    a = dot(direction, plane.Normal)                       │
  │    num = -dot(ray.Origin, plane.Normal) - plane.Distance  │
  │                                                          │
  │    if a == 0: ray parallel to plane → false               │
  │    enter = num / a                                        │
  │    return enter > 0  (hit is in front of ray origin)      │
  └──────────────────────────────────────────────────────────┘
```

## Visual Geometry

```
           Plane (N·P + D = 0)
          ╲ ╱╲ ╱╲ ╱╲ ╱╲ ╱╲ ╱
           ╲  ╲  ╲  ╲  ╲  ╲
    N ────→ ╲  ╲  ╲  ╲  ╲  ╲
             ╲  ╲  ╲  ╲  ╲  ╲
              ╲  ╲  ╲  ╲  ╲  ╲
     ────────●───────────────→  Ray direction (normalized)
     Origin   ╲  ╲  ╲  ╲  ╲
               ╲  ╲  ╲  ╲  ╲
                ● ← intersection point
                enter = distance along ray

     enter = -dot(Origin, Normal) - Distance
             ─────────────────────────────────
                    dot(Direction, Normal)

  Special cases:
  ┌────────────────────────────────────────────────┐
  │  a == 0 (ray parallel to plane)                │
  │    → return false, enter = 0                   │
  │                                                │
  │  enter <= 0 (intersection behind origin)       │
  │    → return false                              │
  │                                                │
  │  enter > 0                                     │
  │    → return true, caller gets intersection dist│
  └────────────────────────────────────────────────┘
```

## GetPoint Helper

```
  ┌──────────────────────────────────────────────┐
  │  Ray.GetPoint(float distance) → float3       │
  │                                              │
  │  point = ray.Origin                          │
  │        + normalize(ray.Displacement) * dist  │
  │                                              │
  │  Usage:                                      │
  │    if (plane.Raycast(ray, out float enter))  │
  │        hitPoint = ray.GetPoint(enter);       │
  └──────────────────────────────────────────────┘
```

## Source

`BovineLabs.Core/Extensions/PhysicsExtensions.cs`
