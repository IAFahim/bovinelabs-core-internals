# IEntityCommands — Structural Change Abstraction

## Overview

IEntityCommands provides a unified interface for performing ECS structural changes (create entities,
add/set components, manage buffers, toggle enableable components). Three implementations provide
different execution strategies: immediate (EntityManager), deferred (EntityCommandBuffer), and
deferred-parallel (EntityCommandBuffer.ParallelWriter).

```
┌──────────────────────────────────────────────────────────────────────────┐
│                      IEntityCommands Interface                           │
│                                                                          │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────┐  │
│  │ EntityManagerCommands│  │ CommandBufferCommands│  │  ParallelWriter │  │
│  │   (immediate)       │  │   (deferred)         │  │  (deferred+par) │  │
│  └──────────┬──────────┘  └──────────┬──────────┘  └────────┬────────┘  │
│             │                        │                       │           │
│             ▼                        ▼                       ▼           │
│     EntityManager           EntityCommandBuffer      ECB.ParallelWriter │
│                                                                          │
│  Common Operations:                                                      │
│    CreateEntity()         Instantiate(prefab)    SetName(name)          │
│    AddComponent<T>()      SetComponent<T>()      AddBuffer<T>()         │
│    SetBuffer<T>()         AppendToBuffer<T>()    AddBlobAsset<T>()      │
│    SetComponentEnabled<T>(bool)                                          │
│                                                                          │
│  Each method has overloads:                                              │
│    (no entity) → uses internal Entity property                           │
│    (Entity e)  → operates on explicit entity                             │
└──────────────────────────────────────────────────────────────────────────┘
```

## Interface Members

```
IEntityCommands
├── Entity Entity { get; set; }              ← tracked entity for overloads
│
├── Entity CreateEntity()
├── Entity Instantiate(Entity prefab)
├── void SetName(FixedString64Bytes name)
├── void SetName(Entity entity, FixedString64Bytes name)
│
├── void AddBlobAsset<T>(ref BlobAssetReference<T>, out Hash128)
│
├── void AddComponent<T>()                   where T : unmanaged, IComponentData
├── void AddComponent<T>(Entity entity)
├── void AddComponent<T>(in T component)
├── void AddComponent<T>(Entity entity, in T component)
├── void AddComponent(in ComponentTypeSet)
├── void AddComponent(Entity entity, in ComponentTypeSet)
│
├── void SetComponent<T>(in T component)
├── void SetComponent<T>(Entity entity, in T component)
│
├── DynamicBuffer<T> AddBuffer<T>()          where T : unmanaged, IBufferElementData
├── DynamicBuffer<T> AddBuffer<T>(Entity entity)
├── DynamicBuffer<T> SetBuffer<T>()
├── DynamicBuffer<T> SetBuffer<T>(Entity entity)
│
├── void AppendToBuffer<T>(in T element)
├── void AppendToBuffer<T>(Entity entity, in T element)
│
├── void SetComponentEnabled<T>(bool enabled)   where T : unmanaged, IEnableableComponent
├── void SetComponentEnabled<T>(Entity entity, bool enabled)
│
├── void AddSharedComponent<T>(Entity entity, in T component)    (discovered at runtime)
└── void SetSharedComponent<T>(Entity entity, in T component)    (discovered at runtime)
```

## Key Design Decisions

- **Strategy pattern**: Same interface, different execution timing. Write code once against
  IEntityCommands, choose immediate vs deferred at construction time.
- **Entity tracking**: The `Entity` property acts as a "current entity" cursor — CreateEntity()
  and Instantiate() update it automatically.
- **BlobAssetStore optional**: All three implementations accept an optional BlobAssetStore for
  blob asset management; if not provided, AddBlobAsset produces a default hash.
- **Buffer operations**: AddBuffer creates new, SetBuffer clears existing then returns,
  AppendToBuffer adds to existing.

## Verified Data

```
IEntityCommands
  Kind: interface
  Methods: AddBlobAsset, AddBuffer, AddComponent, AddSharedComponent, AppendToBuffer,
    CreateEntity, get_Entity, Instantiate, set_Entity, SetBuffer, SetComponent,
    SetComponentEnabled, SetName, SetSharedComponent
  Properties: Entity Entity
  NOTE: Also includes AddSharedComponent and SetSharedComponent (not in initial docs)

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/EntityCommands/IEntityCommands.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/EntityCommands/IEntityCommands.cs)
