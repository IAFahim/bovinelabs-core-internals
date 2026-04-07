# EntityQuery.GetFirstEntity

## Inner Workings Diagram

```
 EntityQuery.GetFirstEntity()
 ══════════════════════════════════════════════════════════════════
 Unsafe fast path for fetching the first matched entity from a query

 PURPOSE:
 ═════════
 Avoids allocating a NativeArray and calculating entity count when
 you only need the first entity. Directly reads from the matching
 chunk cache.


 ┌──────────────────────────────────────────────────────────────────┐
 │  Entity GetFirstEntity(this EntityQuery query)                    │
 └───────────────────────┬──────────────────────────────────────────┘
                        │
                        ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Get impl pointer                                          │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  EntityQueryImpl* impl = query._GetImpl();                   │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                        │                                            │
 │                        ▼                                            │
 │  Step 2: Validate (debug only)                                     │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  if (impl->_QueryData->HasEnableableComponents != 0)         │  │
 │  │      throw: "Can't call GetFirstEntity() on queries          │  │
 │  │             containing enableable component types."           │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                        │                                            │
 │                        ▼                                            │
 │  Step 3: Resolve first chunk and entity (FAST or SLOW path)       │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  impl->GetFirstChunkAndEntity(TypeIndex<Entity>(),           │  │
 │  │      out _, out chunk, out entityIndexInChunk);              │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                        │                                            │
 │                        ▼                                            │
 │  Step 4: Read entity directly from chunk memory                    │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var archetype = ECS->GetArchetype(chunk);                   │  │
 │  │  Entity* chunkEntities = (Entity*)                           │  │
 │  │      ChunkIterationUtility                                   │  │
 │  │          .GetChunkComponentDataROPtr(archetype, chunk, 0);   │  │
 │  │                                                              │  │
 │  │  return UnsafeUtility.AsRef<Entity>(                         │  │
 │  │      chunkEntities + entityIndexInChunk);                    │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 FAST PATH vs SLOW PATH
 ┌──────────────────────────────────────────────────────────────────┐
 │  FAST PATH (common case):                                        │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Conditions:                                               │  │
 │  │    • No requires-matches filter                            │  │
 │  │    • No enableable components                              │  │
 │  │    • RequiredComponentsCount <= 2                          │  │
 │  │    • RequiredComponents[1] is Entity type                  │  │
 │  │                                                             │  │
 │  │  Action:                                                   │  │
 │  │    matchingChunkCache = impl.GetMatchingChunkCache()       │  │
 │  │    outChunk = matchingChunkCache.ChunkIndices[0]           │  │
 │  │    outEntityIndexInChunk = 0  ← always first entity        │  │
 │  │                                                             │  │
 │  │  Zero iteration, direct index into first chunk             │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  SLOW PATH (filtered queries):                                   │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  When fast-path conditions aren't met:                     │  │
 │  │                                                             │  │
 │  │  1. SyncFilterTypes()                                      │  │
 │  │  2. CalculateEntityCountAndSingleton(...)                  │  │
 │  │     → scans matching archetypes + chunks for first match   │  │
 │  │  3. Uses firstMatchArchetype->IndexInArchetype mapping     │  │
 │  │                                                             │  │
 │  │  More expensive but handles all filter scenarios           │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 CHUNK MEMORY — DIRECT ENTITY READ
 ┌──────────────────────────────────────────────────────────────────┐
 │  Archetype matching the query                                     │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Matching Chunk Cache                                       │  │
 │  │  ChunkIndices[0] ──→ First matching chunk                  │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  First Matching Chunk                                            │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  Entity Array (component index 0)                         │  │
 │  │  ┌─────────┬─────────┬─────────┬─────────┐                │  │
 │  │  │ Entity0 │ Entity1 │ Entity2 │ Entity3 │ ...            │  │
 │  │  │  ↑      │         │         │         │                │  │
 │  │  │  │      │         │         │         │                │  │
 │  │  │  entityIndexInChunk = 0      │         │                │  │
 │  │  │  GetFirstEntity returns THIS ONE        │                │  │
 │  │  └─────────┴─────────┴─────────┴─────────┘                │  │
 │  └───────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 COMPARISON
 ┌──────────────────────────────────┬─────────────────────────────────┐
 │  Standard: query.ToEntityArray() │  Fast: query.GetFirstEntity()   │
 │  ────────────────────────────────│  ───────────────────────────    │
 │  Allocates NativeArray           │  No allocation                  │
 │  Calculates ALL matched entities │  Only reads first chunk         │
 │  Must dispose array              │  Returns Entity directly        │
 │  Safe for all query types        │  No enableable components       │
 └──────────────────────────────────┴─────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Extensions/EntityQueryExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityQueryExtensions.cs)
- [BovineLabs.Core/Internal/EntityQueryInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/EntityQueryInternal.cs)
- [BovineLabs.Core/Extensions/EntityQueryBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityQueryBuilderExtensions.cs)
