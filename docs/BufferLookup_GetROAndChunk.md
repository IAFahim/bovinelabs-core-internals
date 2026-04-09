# BufferLookup.GetROAndChunk

## Inner Workings Diagram

```
 BufferLookup<T>.GetROAndChunk(ref this BufferLookup<T> lookup, Entity entity)
 ========================================================================
 Retrieves both a read-only DynamicBuffer<T> AND the chunk index for an
 entity in a single lookup. This avoids a separate chunk query when you
 need the chunk index for ordering or filtering purposes.
```

### Call Flow

```
┌────────────────── Step 1: Reinterpret as internal ───────────────────┐
│ UnsafeUtility.As<BufferLookup<T>, BufferLookupInternal<T>>           │
│ Reinterpret the public struct to access private fields               │
└──────────────────────────────────────────────────────────────────────┘

                                   │
                                   ▼

┌───────────────── Step 2: Safety check (debug only) ──────────────────┐
│ AtomicSafetyHandle.CheckReadAndThrow(m_Safety0)                      │
│ ecs->AssertEntityHasComponent(entity, typeIndex, ref cache)          │
└──────────────────────────────────────────────────────────────────────┘

                                   │
                                   ▼

┌───────────────────── Step 3: Get EntityInChunk ──────────────────────┐
│ ecs->GetEntityInChunk(entity)                                        │
│ Returns: { ArchetypeChunk Chunk, int IndexInChunk }                  │
└──────────────────────────────────────────────────────────────────────┘

                                   │
                                   ▼

┌─────────────────── Step 4: Resolve buffer header ────────────────────┐
│ ChunkDataUtility.GetComponentDataWithTypeRO(                         │
│   entityInChunk.Chunk, archetype,                                    │
│   entityInChunk.IndexInChunk, typeIndex, ref cache)                  │
└──────────────────────────────────────────────────────────────────────┘

                                   │
                                   ▼

┌───────────────── Step 5: Construct and return tuple ─────────────────┐
│ return (new DynamicBuffer<T>(header, ...),                           │
│         (int)entityInChunk.Chunk)                                    │
│ Returns: (DynamicBuffer<T> Buffer, int ChunkIndex)                   │
└──────────────────────────────────────────────────────────────────────┘

```

### BufferLookupInternal Layout (mirrors BufferLookup<T>)

```
┌────────────────────── BufferLookupInternal<T> ───────────────────────┐
│ struct BufferLookupInternal<T> {                                     │
│     AtomicSafetyHandle  m_Safety0                  // safety ha...   │
│     AtomicSafetyHandle  m_ArrayInvalidationSafety  // array saf...   │
│     int                 m_SafetyReadOnlyCount      // read coun...   │
│     int                 m_SafetyReadWriteCount     // write cou...   │
│     EntityDataAccess*   m_Access                   // entity da...   │
│     LookupCache         m_Cache                    // type inde...   │
│     TypeIndex           m_TypeIndex                // component...   │
│     uint                m_GlobalSystemVersion      // for chang...   │
│     int                 m_InternalCapacity         // buffer in...   │
│     byte                m_IsReadOnly               // 1 = read-...   │
│ }                                                                    │
└──────────────────────────────────────────────────────────────────────┘

```

### Return Value Structure

```
Return Value: (DynamicBuffer<T>, int)
├── DynamicBuffer<T> Buffer
│   ├── BufferHeader* (length, capacity, ptr)
│   └── T* Pointer to element data
└── int ChunkIndex (from ArchetypeChunk)
    └── Chunk-based filtering/ordering

```

### Comparison: Standard vs GetROAndChunk

```
Standard vs GetROAndChunk                                       
┌───────────────────────────────┬──────────────────────────────┐
│ Two Separate Calls            │ GetROAndChunk (one call)     │
├───────────────────────────────┼──────────────────────────────┤
│ lookup.GetBuffer(entity)      │ lookup.GetROAndChunk(entity) │
│ ecs->GetEntityInChunk(entity) │ Returns (buffer, chunkIndex) │
│ Two pointer dereferences      │ Single unified lookup path   │
│ Two safety checks             │ One combined safety check    │
└───────────────────────────────┴──────────────────────────────┘

```

### Key Design Details

```
 WHY ref this?
 ──────────────
 BufferLookup<T> is a struct. The extension method uses 'ref this' to
 avoid copying the entire struct on each call. This is essential for
 performance since BufferLookup contains pointers and safety handles.

 WHY EntityInChunk instead of direct lookup?
 ────────────────────────────────────────────
 GetROAndChunk uses ecs->GetEntityInChunk(entity) to get both the chunk
 pointer AND the entity's index within that chunk. This is then used by
 ChunkDataUtility.GetComponentDataWithTypeRO() which takes chunk+archetype+
 index to directly compute the buffer header address.

 The standard GetBuffer(entity) path does a separate entity-to-component
 resolution. GetROAndChunk combines this with chunk retrieval.

 RETURN TYPE NOTE:
 ─────────────────
 Returns (DynamicBuffer<T> Buffer, int ChunkIndex).
 The ChunkIndex comes from entityInChunk.Chunk which is an ArchetypeChunk.
 Unity's ArchetypeChunk has an implicit conversion to int.
 EntityCache also exposes: public int Chunk => EntityInChunk.Chunk;
 This int is used for chunk-based filtering, ordering, and indexing.
```

## Verified Data

```
to: TYPE NOT FOUND
and: TYPE NOT FOUND
BufferLookupInternal: TYPE NOT FOUND
Verified: 0 checks, 3 failures
```

## Source

- [BovineLabs.Core/Extensions/BufferLookupExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/BufferLookupExtensions.cs)
- [BovineLabs.Core/Iterators/UnsafeBufferLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/UnsafeBufferLookup.cs)
- [SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.Semantics.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.Semantics.cs)
