# QueryEntityEnumerator — Inner Workings

## Overview

QueryEntityEnumerator provides manual chunk-level iteration over EntityQuery results
using `UnsafeChunkCacheIterator` and v128 bitmask masks, giving fine-grained control
over entity iteration inside Burst jobs.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│              QueryEntityEnumerator (unmanaged struct)                        │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  UnsafeChunkCacheIterator chunkCacheIterator                          │  │
│  │  int chunkIndex                                                       │  │
│  │  v128 chunkEnabledMask                                                │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  MoveNextChunk(out chunk, out chunkEnumerator) → bool                       │
│  Reset()                                                                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Construction — Internal Query Access

```
  new QueryEntityEnumerator(query)
         │
         ▼
  ┌─────────────────────────────────────────────────────────────────────────┐
  │  1. chunkIndex = -1                                                     │
  │  2. chunkEnabledMask = default                                          │
  │                                                                         │
  │  3. Access query internals:                                             │
  │     var queryImpl = query._GetImpl();                                   │
  │     ┌───────────────────────────────────────────────────────────────┐   │
  │     │  queryImpl->_Filter           → entity query filter           │   │
  │     │  queryImpl->_QueryData->      → query metadata                │   │
  │     │    HasEnableableComponents                                       │   │
  │     │  queryImpl->GetMatchingChunkCache() → chunk matching cache    │   │
  │     │  queryImpl->_QueryData->MatchingArchetypes.Ptr → archetype list│  │
  │     └───────────────────────────────────────────────────────────────┘   │
  │                                                                         │
  │  4. new UnsafeChunkCacheIterator(filter, hasEnableable, cache, archs)   │
  └─────────────────────────────────────────────────────────────────────────┘
```

## Chunk Iteration Flow

```
  while (enumerator.MoveNextChunk(out var chunk, out var ce))
  {
      // Process chunk...
      for (int i = 0; i < chunk.Count; i++)
      {
          // Use ce to iterate with enabled-mask awareness
      }
  }

  MoveNextChunk internals:
  ┌─────────────────────────────────────────────────────────────────────┐
  │  chunkCacheIterator.MoveNextChunk(                                  │
  │      ref chunkIndex,     ← incremented internally                  │
  │      out chunk,          ← the ArchetypeChunk                      │
  │      out _,              ← unused filter output                    │
  │      out useEnabledMaskBit,                                        │
  │      ref chunkEnabledMask ← v128 bitmask for enabled components   │
  │  )                                                                  │
  │                                                                     │
  │  ┌───────────────────────────────────────────────────────────┐      │
  │  │  ECS Chunk Architecture:                                  │      │
  │  │                                                           │      │
  │  │  ArchetypeChunk                                           │      │
  │  │  ┌─────────────────────────────────────────────────────┐  │      │
  │  │  │ Entity[0] Entity[1] Entity[2] ... Entity[N]         │  │      │
  │  │  │ CompA[0]  CompA[1]  CompA[2]  ... CompA[N]         │  │      │
  │  │  │ CompB[0]  CompB[1]  CompB[2]  ... CompB[N]         │  │      │
  │  │  └─────────────────────────────────────────────────────┘  │      │
  │  │                                                           │      │
  │  │  chunkEnabledMask (v128 = 128-bit mask):                  │      │
  │  │  ┌───┬───┬───┬───┬───┬───┬───┬───┬─── ... ───┐          │      │
  │  │  │ 1 │ 1 │ 0 │ 1 │ 1 │ 0 │ 1 │ 1 │   ...     │          │      │
  │  │  └───┴───┴───┴───┴───┴───┴───┴───┴─── ... ───┘          │      │
  │  │    ↑   ↑       ↑   ↑       ↑   ↑                       │      │
  │  │   en  en  dis  en  en  dis en  en                       │      │
  │  │  (1=enabled, 0=disabled for enableable components)       │      │
  │  └───────────────────────────────────────────────────────────┘      │
  │                                                                     │
  │  ChunkEntityEnumerator:                                             │
  │    new ChunkEntityEnumerator(                                       │
  │        useEnabledMask: useEnabledMaskBit != 0,                      │
  │        enabledMask: chunkEnabledMask,                               │
  │        entityCount: chunk.Count                                     │
  │    )                                                                │
  │    → Handles per-entity iteration respecting enableable masks       │
  └─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Manual iteration**: Unlike `query.ToEntityArray()` or `Entities.ForEach`, this
  gives chunk-by-chunk control without allocating intermediate arrays.
- **v128 mask support**: Handles enableable components (IEnableableComponent) where
  individual entities within a chunk can be disabled.
- **Internal API access**: Uses `_GetImpl()` to reach Unity's internal query data —
  this is necessary for Burst-compatible manual iteration that the public API doesn't expose.
- **Stateful cursor**: `chunkIndex = -1` starts before the first chunk; each
  `MoveNextChunk` call advances it.

## Source File

- `BovineLabs.Core/Utility/QueryEntityEnumerator.cs`
