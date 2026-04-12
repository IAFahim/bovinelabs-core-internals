# EntityCache — Fast Entity-to-Component Pointer Cache

## Overview

EntityCache is a readonly unsafe struct that caches archetype and chunk lookups for a specific
entity, enabling rapid retrieval of multiple components via raw pointers. It bypasses Unity's
safety system (no AtomicSafetyHandle) for maximum performance in Burst-compiled jobs.

```
┌─────────────────────────────────────────────────────────────────────┐
│  EntityCache (readonly unsafe struct)                               │
│                                                                     │
│  Public:                                                            │
│    readonly Entity Entity                                           │
│    int Chunk { get; }  ← EntityInChunk.Chunk                        │
│                                                                     │
│  Internal:                                                          │
│    bool Exists                                                      │
│    Archetype* Archetype                                             │
│    EntityInChunk EntityInChunk                                      │
│                                                                     │
│  Construction:                                                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  static Create<T>(ComponentLookup<T> lookup, Entity entity)  │  │
│  │    └─ UnsafeUtility.As to extract EntityComponentStore*      │  │
│  │                                                               │  │
│  │  static Create<T>(BufferLookup<T> lookup, Entity entity)     │  │
│  │    └─ UnsafeUtility.As to extract EntityComponentStore*      │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  Internal Access Methods (raw pointers):                            │
│    HasComponent(ref LookupCache, TypeIndex)                         │
│    GetOptionalComponentDataWithTypeRO(TypeIndex, ref LookupCache)   │
│    GetOptionalComponentDataWithTypeRW(TypeIndex, uint, ref cache)   │
│    GetComponentDataWithTypeRO(TypeIndex, ref LookupCache)           │
│    GetComponentDataWithTypeRW(TypeIndex, uint, ref cache)           │
└─────────────────────────────────────────────────────────────────────┘
```

## Construction Flow

```
Create<T>(ComponentLookup<T> lookup, Entity entity)
       │
       ▼
  ref lookupInternal = ref UnsafeUtility.As<ComponentLookup<T>, ComponentLookupInternal>(ref lookup)
       │
       ▼
  new EntityCache(lookupInternal.m_Access->EntityComponentStore, entity)
       │
       ▼
  EntityComponentStore.s_entityStore.Data.Exists(entity)?
       │
       ├─ NO → Exists = false, Archetype = null, EntityInChunk = default
       │
       └─ YES:
            chunk = ecs->GetChunk(entity)
            Archetype = ecs->GetArchetype(chunk)
            Exists = (WorldSequenceNumber matches)
            if Exists: EntityInChunk = ecs->GetEntityInChunk(entity)
```

## Key Design Decisions

- **No safety handles**: Deliberately bypasses AtomicSafetyHandle for zero-overhead access in
  Burst jobs. The user is responsible for ensuring valid data.
- **LookupCache optimization**: Internal methods use a LookupCache struct that caches the
  archetype-to-type-index mapping, avoiding repeated binary searches.
- **Optional vs Required**: GetOptional returns null if the component doesn't exist; GetRequired
  asserts and returns the pointer directly.
- **Factory via UnsafeUtility.As**: Reinterprets ComponentLookup/BufferLookup internals to
  extract the EntityComponentStore pointer without going through public API.

## Verified Data

```
EntityCache
  Kind: struct, 32 bytes
  Fields: Entity Entity, Boolean Exists (internal), Archetype* Archetype (internal),
    EntityInChunk EntityInChunk (internal)
  Properties: Int32 Chunk

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/EntityCache.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityCache.cs)
