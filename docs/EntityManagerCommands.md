# EntityManagerCommands — Immediate Execution

## Overview

EntityManagerCommands implements IEntityCommands by forwarding all operations directly to an
EntityManager. Every operation takes effect immediately — no deferral, no playback step.

```
┌───────────────────────────────────────────────────────────────┐
│  EntityManagerCommands : IEntityCommands                      │
│                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                  │
│  │ EntityManager    │  │ BlobAssetStore   │                  │
│  │   (required)     │  │   (optional)     │                  │
│  └────────┬─────────┘  └────────┬─────────┘                  │
│           │                      │                             │
│           ▼                      ▼                             │
│  All AddComponent, SetComponent,                               │
│  AddBuffer, SetBuffer, etc.                                    │
│  execute immediately on EntityManager                          │
│                                                               │
│  Entity Entity { get; set; }  ← tracked entity                │
└───────────────────────────────────────────────────────────────┘
```

## Constructor

```
EntityManagerCommands(
    EntityManager entityManager,
    Entity localEntity = default,
    BlobAssetStore blobAssetStore = default
)
```

## Implementation Details

| Method | Behavior |
|--------|----------|
| `CreateEntity()` | `entityManager.CreateEntity()`, updates Entity |
| `Instantiate(prefab)` | `entityManager.Instantiate(prefab)`, updates Entity |
| `SetName(name)` | `entityManager.SetName(Entity, name)` |
| `AddComponent<T>(entity)` | `entityManager.AddComponent<T>(entity)` |
| `AddComponent<T>(entity, comp)` | `entityManager.AddComponentData(entity, comp)` |
| `SetComponent<T>(entity, comp)` | `entityManager.SetComponentData(entity, comp)` |
| `AddBuffer<T>(entity)` | `entityManager.AddBuffer<T>(entity)` |
| `SetBuffer<T>(entity)` | `entityManager.GetBuffer<T>(entity)` then `Clear()` |
| `AppendToBuffer<T>(entity, el)` | `entityManager.GetBuffer<T>(entity).Add(el)` |
| `SetComponentEnabled<T>(entity, val)` | `entityManager.SetComponentEnabled<T>(entity, val)` |
| `AddBlobAsset<T>` | `blobAssetStore?.TryAdd(...)` if store is created |

## Key Design Decisions

- **Immediate side effects**: All changes happen synchronously. Use when you need components to
  be available immediately after creation (e.g., in baking, editor scripts).
- **SetBuffer clears first**: Unlike AddBuffer (which adds a new buffer), SetBuffer gets the
  existing buffer and clears it before returning.
- **BlobAssetStore guard**: AddBlobAsset checks `blobAssetStore.IsCreated` before accessing it,
  returning a default hash if no store is available.

## Verified Data

```
EntityManagerCommands
  Kind: struct, 112 bytes
  Implements: IEntityCommands
  Fields: EntityManager entityManager (private), BlobAssetStore blobAssetStore (private),
    Entity <Entity>k__BackingField

Verified: 2 checks, 0 failures
```


> Tested example: [Example/EntityCommandsExample.cs](../Example/EntityCommandsExample.cs)

## Source

- [BovineLabs.Core/EntityCommands/EntityManagerCommands.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/EntityCommands/EntityManagerCommands.cs)
