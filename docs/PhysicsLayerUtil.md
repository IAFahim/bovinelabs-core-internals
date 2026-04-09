# PhysicsLayerUtil

**Converts Collider masks accurately into ECS CollisionFilters**

`PhysicsLayerUtil` translates Unity's GameObject-based physics layer system
(including the global layer collision matrix and per-collider/rigidbody layer
overrides) into an ECS `CollisionFilter` for use with Unity Physics.

---

## Architecture

```
  ┌─────────────────────────────────────────────────────────────┐
  │  PhysicsLayerUtil.ProduceCollisionFilter(Collider, GameObject)│
  │                                                             │
  │  Input:                                                     │
  │    UnityEngine.Collider collider                            │
  │    GameObject body (the rigidbody root)                     │
  │                                                             │
  │  Output:                                                    │
  │    Unity.Physics.CollisionFilter                            │
  │      .BelongsTo  = 1 << layer                               │
  │      .CollidesWith = computed include mask                  │
  └─────────────────────────────────────────────────────────────┘
```

## Layer Mask Computation Pipeline

```
  Step 1: Base Layer
  ┌──────────────────────────────────────────────────┐
  │  layer = body.layer                               │
  │  filter.BelongsTo = 1u << layer                   │
  │                                                   │
  │  Layer 3 → BelongsTo = 0b00000000_00000000_00000000_00001000
  └──────────────────────┬───────────────────────────┘
                         │
                         ▼
  Step 2: Global Collision Matrix
  ┌──────────────────────────────────────────────────┐
  │  includeMask = 0                                  │
  │  for i in 0..31:                                  │
  │    if !Physics.GetIgnoreLayerCollision(layer, i): │
  │      includeMask |= 1u << i                       │
  │                                                   │
  │  Unity Layer Collision Matrix:                    │
  │       0  1  2  3  4  5  ...                      │
  │  0  [ ✓  ✗  ✓  ✓  ✗  ✓ ]                       │
  │  1  [ ✗  ✓  ✓  ✗  ✗  ✓ ]                       │
  │  2  [ ✓  ✓  ✓  ✓  ✓  ✓ ]                       │
  │  3  [ ✓  ✗  ✓  ✓  ✗  ✓ ]  ← layer 3           │
  │  ...                                              │
  │                                                   │
  │  includeMask starts with all layer interactions   │
  │  that are NOT ignored                             │
  └──────────────────────┬───────────────────────────┘
                         │
                         ▼
  Step 3: Collider Include/Exclude Layers
  ┌──────────────────────────────────────────────────┐
  │  includeMask |= collider.includeLayers.value      │
  │  excludeMask  = collider.excludeLayers.value      │
  └──────────────────────┬───────────────────────────┘
                         │
                         ▼
  Step 4: Rigidbody Include/Exclude Layers
  ┌──────────────────────────────────────────────────┐
  │  if rigidBody exists on body:                     │
  │    includeMask |= rigidBody.includeLayers.value   │
  │    excludeMask  |= rigidBody.excludeLayers.value  │
  └──────────────────────┬───────────────────────────┘
                         │
                         ▼
  Step 5: Final Merge
  ┌──────────────────────────────────────────────────┐
  │  includeMask &= ~excludeMask                      │
  │  filter.CollidesWith = includeMask                │
  │                                                   │
  │  Exclude takes priority over include              │
  │  Final bitfield:                                  │
  │  0b...00010110_10101010                           │
  │     │││└─ layer 1 included                        │
  │     ││└── layer 2 included                        │
  │     │└─── layer 4 excluded                        │
  │     └──── layer 6 included                        │
  └──────────────────────────────────────────────────┘
```

## Layer Override Priority

```
  Priority (highest to lowest):
  ┌────────────────────────────────────────┐
  │  1. Rigidbody.excludeLayers            │
  │  2. Collider.excludeLayers             │
  │  ─── exclude always wins ───           │
  │  3. Rigidbody.includeLayers            │
  │  4. Collider.includeLayers             │
  │  5. Global collision matrix            │
  └────────────────────────────────────────┘

  includeMask = global_matrix | collider.include | rigidbody.include
  excludeMask = collider.exclude | rigidbody.exclude
  final = includeMask & ~excludeMask
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core/Utility/PhysicsLayerUtil.cs`

## Source

- [BovineLabs.Core/Utility/PhysicsLayerUtil.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/PhysicsLayerUtil.cs)
