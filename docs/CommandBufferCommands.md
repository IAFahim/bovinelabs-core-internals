# CommandBufferCommands — Deferred Execution

## Overview

CommandBufferCommands implements IEntityCommands by recording all operations into an
EntityCommandBuffer. Changes are deferred until the ECB is played back by the
EntityCommandBufferSystem that owns it.

```
┌───────────────────────────────────────────────────────────────┐
│  CommandBufferCommands : IEntityCommands                      │
│                                                               │
│  ┌──────────────────────┐  ┌──────────────────┐              │
│  │ EntityCommandBuffer  │  │ BlobAssetStore   │              │
│  │   (required)         │  │   (optional)     │              │
│  └────────┬─────────────┘  └────────┬─────────┘              │
│           │                          │                         │
│           ▼                          ▼                         │
│  All operations are RECORDED                                  │
│  (not executed until ECB playback)                            │
│                                                               │
│  Entity Entity { get; set; }  ← tracked entity                │
└───────────────────────────────────────────────────────────────┘
```

## Constructor

```
CommandBufferCommands(
    EntityCommandBuffer commandBuffer,
    Entity localEntity = default,
    BlobAssetStore blobAssetStore = default
)
```

## Implementation Details

| Method | ECB Call |
|--------|---------|
| `CreateEntity()` | `commandBuffer.CreateEntity()`, updates Entity |
| `Instantiate(prefab)` | `commandBuffer.Instantiate(prefab)`, updates Entity |
| `AddComponent<T>(entity)` | `commandBuffer.AddComponent<T>(entity)` |
| `AddComponent<T>(entity, comp)` | `commandBuffer.AddComponent(entity, comp)` |
| `SetComponent<T>(entity, comp)` | `commandBuffer.SetComponent(entity, comp)` |
| `AddBuffer<T>(entity)` | `commandBuffer.AddBuffer<T>(entity)` |
| `SetBuffer<T>(entity)` | `commandBuffer.SetBuffer<T>(entity)` |
| `AppendToBuffer<T>(entity, el)` | `commandBuffer.AppendToBuffer(entity, el)` |
| `SetComponentEnabled<T>(entity, val)` | `commandBuffer.SetComponentEnabled<T>(entity, val)` |

## Key Design Decisions

- **Deferred execution**: All structural changes are recorded and played back later. This avoids
  sync points and enables batched structural changes.
- **SetBuffer delegates to ECB**: Unlike EntityManagerCommands which clears the buffer, the ECB's
  SetBuffer handles this at playback time.
- **BlobAssetStore same as EntityManagerCommands**: Checked at record time, not playback.

## Verified Data

```
CommandBufferCommands
  Kind: struct, 168 bytes
  Implements: IEntityCommands
  Wraps: EntityCommandBuffer + BlobAssetStore

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/EntityCommands/CommandBufferCommands.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/EntityCommands/CommandBufferCommands.cs)
