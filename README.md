# EntityQuery.GetSingletonBufferNoSync

## Inner Workings Diagram

```
 EntityQuery.GetSingletonBufferNoSync<T>(isReadOnly)
 ══════════════════════════════════════════════════════════════════
 Reads singleton buffer data WITHOUT triggering dependency synchronization

 PURPOSE:
 ═════════
 The standard GetSingletonBuffer calls GetSingleton which syncs dependencies.
 This extension skips that sync and also uses GetUnsafe on the BufferAccessor
 to bypass per-element safety checks.


 ┌──────────────────────────────────────────────────────────────────────┐
 │  DynamicBuffer<T> GetSingletonBufferNoSync<T>(                       │
 │      this EntityQuery query, bool isReadOnly)                        │
 └───────────────────────────┬──────────────────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Get impl + validate                                        │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var impl = query._GetImpl();                                │  │
 │  │  if (TypeManager.IsEnableable(typeIndex))                    │  │
 │  │      throw: "Can't use with enableable components"           │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 2: Resolve singleton chunk and entity                         │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  impl->GetSingletonChunkAndEntity(                           │  │
 │  │      typeIndex,                                              │  │
 │  │      out indexInArchetype,                                   │  │
 │  │      out chunk,                                              │  │
 │  │      out entityIndexInChunk);                                │  │
 │  │                                                              │  │
 │  │  Gets the ONE chunk/entity matching the singleton query      │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 3: Get buffer accessor (NO DEPENDENCY SYNC)                   │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var archetype = ECS->GetArchetype(chunk);                   │  │
 │  │                                                              │  │
 │  │  var bufferAccessor =                                       │  │
 │  │      ChunkIterationUtility.GetChunkBufferAccessor<T>(        │  │
 │  │          archetype, chunk,                                   │  │
 │  │          !isReadOnly, indexInArchetype,                      │  │
 │  │          ECS->GlobalSystemVersion,                           │  │
 │  │          safetyHandles...);  // safety handles set but       │  │
 │  │                              // NO CompleteDependency() call │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 4: Return via GetUnsafe (bypass safety per-element check)     │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  return bufferAccessor.GetUnsafe(entityIndexInChunk);        │  │
 │  │                                                              │  │
 │  │  GetUnsafe does raw pointer math:                            │  │
 │  │  hdr = (BufferHeader*)(BasePointer + entityIndex * Stride)   │  │
 │  │  return new DynamicBuffer<T>(hdr, internalCapacity)          │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 DEPENDENCY SYNC COMPARISON
 ┌──────────────────────────────────────────────────────────────────┐
 │  Standard GetSingletonBuffer:                                     │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  1. query.CompleteDependency()  ← WAITS for all writers    │  │
 │  │  2. GetSingletonChunkAndEntity(...)                         │  │
 │  │  3. bufferAccessor[index]  ← safety-checked access          │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  GetSingletonBufferNoSync:                                       │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  1. GetSingletonChunkAndEntity(...)  ← direct, no sync     │  │
 │  │  2. bufferAccessor.GetUnsafe(index)  ← no per-element check│  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 SINGLETON LAYOUT IN CHUNK
 ┌──────────────────────────────────────────────────────────────────┐
 │  Singleton Query (matches exactly 1 entity)                      │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Matching Archetype                                        │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │  Chunk (only 1 chunk, 1 entity)                      │ │  │
 │  │  │  ┌──────────────────────────────────────────────┐    │ │  │
 │  │  │  │ Entity[0]: E_singleton                        │    │ │  │
 │  │  │  │ BufferHeader[0]: [ptr→data, len, cap]         │    │ │  │
 │  │  │  └──────────────────────────────────────────────┘    │ │  │
 │  │  │                                                       │ │  │
 │  │  │  entityIndexInChunk = 0                               │ │  │
 │  │  │  indexInArchetype = slot of buffer type in archetype  │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘

 WARNING:
 ═════════
 The caller is responsible for ensuring no concurrent writes are
 happening. Only use when you KNOW the singleton is safe to read
 (e.g. from main thread, or after manual dependency management).
```
