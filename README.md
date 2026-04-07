# BufferLookup.GetROAndChunk

## Inner Workings Diagram

```
 BufferLookup<T>.GetROAndChunk(entity)
 ═════════════════════════════════════════════════════════════════════
 Retrieves both a read-only DynamicBuffer AND the chunk index in one call

 PURPOSE:
 ═════════
 Avoids a separate entity-to-chunk lookup when you need both the buffer
 data and the chunk index (e.g. for parallel job scheduling, change
 filtering, or chunk-based batching).


 ┌──────────────────────────────────────────────────────────────────┐
 │  (DynamicBuffer<T> Buffer, int ChunkIndex)                       │
 │      GetROAndChunk(ref this BufferLookup<T> lookup, Entity e)    │
 └───────────────────────┬──────────────────────────────────────────┘
                        │
                        ▼
 ┌────────────────────────────────────────────────────────────────────┐
 │  INTERNALS                                                        │
 │                                                                    │
 │  BufferLookupInternal<T> layout (mirrors BufferLookup<T>):       │
 │  ┌─────────────────────────────────────────────────────────────┐  │
 │  │  EntityDataAccess* m_Access  ─→ entity data access layer    │  │
 │  │  LookupCache        m_Cache   ─→ archetype/type lookup cache│  │
 │  │  TypeIndex          m_TypeIndex                              │  │
 │  │  uint               m_GlobalSystemVersion                   │  │
 │  │  int                m_InternalCapacity                       │  │
 │  │  byte               m_IsReadOnly                             │  │
 │  └─────────────────────────────────────────────────────────────┘  │
 │                                                                    │
 │  Step 1: Get EntityComponentStore                                  │
 │  ┌────────────────────────────────────────────────────────────┐   │
 │  │  var ecs = lookupInternal.m_Access->EntityComponentStore;  │   │
 │  │  Safety check: CheckReadAndThrow(m_Safety0)                │   │
 │  │  Assert entity has component                               │   │
 │  └────────────────────────────────────────────────────────────┘   │
 │                         │                                          │
 │                         ▼                                          │
 │  Step 2: Get EntityInChunk (CRITICAL — single ECS lookup)         │
 │  ┌────────────────────────────────────────────────────────────┐   │
 │  │  var entityInChunk = ecs->GetEntityInChunk(entity);        │   │
 │  │                                                            │   │
 │  │  EntityInChunk:                                            │   │
 │  │  ┌──────────────────────────────────────────────────────┐ │   │
 │  │  │  ChunkIndex Chunk;         ← THIS IS THE CHUNK INDEX │ │   │
 │  │  │  int        IndexInChunk;  ← entity's slot in chunk  │ │   │
 │  │  └──────────────────────────────────────────────────────┘ │   │
 │  └────────────────────────────────────────────────────────────┘   │
 │                         │                                          │
 │                         ▼                                          │
 │  Step 3: Resolve buffer header from chunk directly                │
 │  ┌────────────────────────────────────────────────────────────┐   │
 │  │  var archetype = ecs->GetArchetype(entityInChunk.Chunk);   │   │
 │  │                                                            │   │
 │  │  var header = (BufferHeader*)ChunkDataUtility              │   │
 │  │      .GetComponentDataWithTypeRO(                          │   │
 │  │          entityInChunk.Chunk,                              │   │
 │  │          archetype,                                        │   │
 │  │          entityInChunk.IndexInChunk,                       │   │
 │  │          lookupInternal.m_TypeIndex,                       │   │
 │  │          ref lookupInternal.m_Cache);                      │   │
 │  └────────────────────────────────────────────────────────────┘   │
 │                         │                                          │
 │                         ▼                                          │
 │  Step 4: Return tuple                                              │
 │  ┌────────────────────────────────────────────────────────────┐   │
 │  │  var buffer = new DynamicBuffer<T>(header, ...);           │   │
 │  │  return (buffer, entityInChunk.Chunk);  ← chunk index!     │   │
 │  └────────────────────────────────────────────────────────────┘   │
 └────────────────────────────────────────────────────────────────────┘


 ENTITY LOOKUP PATH COMPARISON
 ┌─────────────────────────────────────────────────────────────────┐
 │  Standard approach (TWO lookups):                                │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  buffer = lookup[entity];     // lookup 1: entity→chunk   │  │
 │  │  chunkIndex = entity.ToChunkIndex(); // lookup 2: again!  │  │
 │  └───────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  GetROAndChunk (ONE lookup):                                     │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  (buffer, chunk) = lookup.GetROAndChunk(entity);           │  │
 │  │  // Single GetEntityInChunk call provides both             │  │
 │  └───────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────┘


 ECS ENTITY RESOLUTION
 ┌──────────────────────────────────────────────────────────────┐
 │  EntityComponentStore                                        │
 │  ┌────────────────────────────────────────────────────────┐  │
 │  │  Entity → EntityInChunk lookup table                   │  │
 │  │  ┌──────────────────────────────────────────────────┐  │  │
 │  │  │  Entity(3) → { Chunk: ChunkIndex(7),              │  │  │
 │  │  │                 IndexInChunk: 42 }                 │  │  │
 │  │  └──────────────────────────────────────────────────┘  │  │
 │  │                                                        │  │
 │  │  ChunkIndex(7)                                         │  │
 │  │  ┌──────────────────────────────────────────────────┐  │  │
 │  │  │  [E0][E1]...[E42]...[E127]                       │  │  │
 │  │  │  [B0][B1]...[B42]...[B127]  ← BufferHeaders      │  │  │
 │  │  │                ↑                                  │  │  │
 │  │  │    IndexInChunk=42                                │  │  │
 │  │  └──────────────────────────────────────────────────┘  │  │
 │  └────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────┘
```
