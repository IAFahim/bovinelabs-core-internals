# CommandBufferParallelCommands — Deferred Parallel Execution

## Overview

CommandBufferParallelCommands implements IEntityCommands by recording operations into an
EntityCommandBuffer.ParallelWriter with a sort key. This ensures thread-safe recording from
parallel jobs — the sort key determines playback order.

```
┌───────────────────────────────────────────────────────────────────┐
│  CommandBufferParallelCommands : IEntityCommands                  │
│                                                                   │
│  ┌────────────────────────────────┐  ┌──────────────────┐        │
│  │ EntityCommandBuffer.ParallelWriter│ │ BlobAssetStore   │        │
│  │   (required)                   │  │   (optional)     │        │
│  └──────────────┬─────────────────┘  └────────┬─────────┘        │
│                 │                             │                    │
│  int sortKey ───┘                             │                    │
│  (thread-safe ordering)                       │                    │
│                 │                             │                    │
│                 ▼                             ▼                    │
│  All operations include sortKey as first arg                      │
│                                                                   │
│  Entity Entity { get; set; }  ← tracked entity                    │
└───────────────────────────────────────────────────────────────────┘
```

## Constructor

```
CommandBufferParallelCommands(
    EntityCommandBuffer.ParallelWriter commandBuffer,
    int sortKey,
    Entity localEntity = default,
    BlobAssetStore blobAssetStore = default
)
```

## Implementation Details

Every ECB call prepends `this.sortKey` as the first argument:

| Method | ParallelWriter Call |
|--------|---------------------|
| `CreateEntity()` | `commandBuffer.CreateEntity(sortKey)` |
| `Instantiate(prefab)` | `commandBuffer.Instantiate(sortKey, prefab)` |
| `AddComponent<T>(entity)` | `commandBuffer.AddComponent<T>(sortKey, entity)` |
| `SetComponent<T>(entity, comp)` | `commandBuffer.SetComponent(sortKey, entity, comp)` |
| `AddBuffer<T>(entity)` | `commandBuffer.AddBuffer<T>(sortKey, entity)` |
| `AppendToBuffer<T>(entity, el)` | `commandBuffer.AppendToBuffer(sortKey, entity, el)` |
| `SetComponentEnabled<T>(entity, val)` | `commandBuffer.SetComponentEnabled<T>(sortKey, entity, val)` |

## Key Design Decisions

- **Sort key for deterministic playback**: Each parallel job thread uses a unique sort key
  (typically chunk index or entity index) so commands are replayed in a deterministic order.
- **No safety handles**: The ParallelWriter already handles thread safety internally.
- **Same interface**: Implements the exact same IEntityCommands interface, allowing code to be
  shared between sequential and parallel contexts.

## Verified Data

```
CommandBufferParallelCommands
  Kind: struct, 160 bytes
  Implements: IEntityCommands
  Fields: Int32 sortKey (private), ParallelWriter commandBuffer (private),
    BlobAssetStore blobAssetStore (private), Entity <Entity>k__BackingField

Verified: 3 checks, 0 failures
```


> Tested example: [Example/EntityCommandsExample.cs](../Example/EntityCommandsExample.cs)

## Source

- [BovineLabs.Core/EntityCommands/CommandBufferParallelCommands.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/EntityCommands/CommandBufferParallelCommands.cs)
