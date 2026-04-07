# AlwaysUpdatePhysicsWorld

**Forces spatial map updates independently of the fixed simulation tick**

When the game runs at a higher FPS than the fixed-step simulation rate (default 50Hz),
the physics world only updates during fixed ticks. `AlwaysUpdatePhysicsWorld` and its
companion system ensure `BuildPhysicsWorld` runs every frame so spatial queries
remain accurate even between fixed steps.

---

## Architecture

```
  ┌──────────────────────────────────────────────────────────────┐
  │  AlwaysUpdatePhysicsWorld : IComponentData                   │
  │                                                              │
  │    bool FixedStepUpdatedThisFrame                            │
  │      └─ Set true by FixedStepUpdatedSystem after fixed step │
  │      └─ Checked by AlwaysUpdatePhysicsWorldSystem           │
  └──────────────────────────────────────────────────────────────┘

  ┌──────────────────────────────────────────────────────────────┐
  │  AlwaysUpdatePhysicsWorldSystem : SystemBase                 │
  │                                                              │
  │  [UpdateInGroup(BeforeTransformSystemGroup, OrderFirst)]     │
  │  [CreateAfter(typeof(BuildPhysicsWorld))]                    │
  │                                                              │
  │  Cached handles:                                             │
  │    SystemHandle buildPhysicsWorld                            │
  │    SystemHandle buildPhysicsWorldDependencyResolver          │
  │    SystemState* buildPhysicsWorldSystemState                 │
  └──────────────────────────────────────────────────────────────┘
```

## Update Decision Flow

```
  ┌───────────────────────────────────────────────────────────┐
  │  OnUpdate()                                               │
  │                                                           │
  │  ┌─────────────────────────────────────────────────────┐ │
  │  │  physicsUpdated = GetSingletonRW<AlwaysUpdatePhysics │ │
  │  │                                World>()              │ │
  │  └─────────────────────┬───────────────────────────────┘ │
  │                        │                                  │
  │                        ▼                                  │
  │                FixedStepUpdatedThisFrame?                  │
  │                   ╱           ╲                           │
  │                 YES             NO                         │
  │                  │              │                          │
  │                  ▼              ▼                          │
  │  ┌─────────────────────┐  ┌───────────────────────────┐  │
  │  │ Reset flag to false │  │ Manually update:          │  │
  │  │ return early        │  │                           │  │
  │  │                     │  │ 1. ResolveDepResolver()   │  │
  │  │ (physics already    │  │ 2. BuildPhysicsWorld      │  │
  │  │  updated this frame │  │    .Update(world)         │  │
  │  │  via fixed step)    │  │                           │  │
  │  └─────────────────────┘  │ 3. Combine dependencies  │  │
  │                           └───────────────────────────┘  │
  └───────────────────────────────────────────────────────────┘
```

## Timeline Visualization

```
  Without AlwaysUpdatePhysicsWorld:

  Frame:  1    2    3    4    5    6    7    8
  Fixed:  ▼         ▼         ▼         ▼
  Physics:█         █         █         █
  Spatial:░         ░         ░         ░   ← stale between fixed steps!
          ├─────────┼─────────┼─────────┤
          stale    stale     stale     stale

  With AlwaysUpdatePhysicsWorld:

  Frame:  1    2    3    4    5    6    7    8
  Fixed:  ▼         ▼         ▼         ▼
  Physics:█         █         █         █
  AlwaysU:   █    █    █    █    █    █    █    ← forces rebuild
  Spatial:█    █    █    █    █    █    █    █   ← always fresh!
          ├──────────────────────────────────┤
          spatial map always up-to-date
```

## Key Design Points

```
  ┌──────────────────────────────────────────────────────┐
  │  DOES NOT simulate physics                           │
  │  Only runs BuildPhysicsWorld (copies transforms to   │
  │  physics world), NOT the simulation step             │
  │                                                      │
  │  Purpose: keep PhysicsWorldSingleton spatial data     │
  │  (broadphase tree, collision world) current for      │
  │  queries like OverlapQuery, Raycast, etc.            │
  │                                                      │
  │  The fixed step still runs the full simulation       │
  │  (collision detection, solving, integration)         │
  └──────────────────────────────────────────────────────┘
```

## Source

`BovineLabs.Core.Extensions/PhysicsUpdate/AlwaysUpdatePhysicsWorld.cs`
`BovineLabs.Core.Extensions/PhysicsUpdate/AlwaysUpdatePhysicsWorldSystem.cs`
