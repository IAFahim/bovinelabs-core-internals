# UnsafeEntityDataAccess — Raw Entity Data Wrapper

## Overview

UnsafeEntityDataAccess wraps a raw EntityDataAccess pointer, providing low-level methods for
component data access by pointer, untyped buffer access, chunk-level operations, and enabled-bit
retrieval. Designed for advanced users who need direct memory manipulation in Burst jobs.

```
┌─────────────────────────────────────────────────────────────────────┐
│  UnsafeEntityDataAccess (struct)                                    │
│                                                                     │
│  Fields:                                                            │
│    EntityDataAccess* access (readonly)                              │
│    uint GlobalSystemVersion { get; private set; }                   │
│                                                                     │
│  Instance Methods:                                                  │
│    Update(ref SystemState)  → refreshes GlobalSystemVersion         │
│    HasComponent(Entity, ComponentType/TypeIndex) → bool             │
│    Exists(Entity) → bool                                            │
│    GetEntityStorageInfo(Entity) → EntityStorageInfo                 │
│    GetRequiredComponentDataPtrRO/RW(Entity, TypeIndex) → byte*      │
│    GetComponentDataPtrRO/RW(Entity, ComponentType) → byte*          │
│    GetUntypedBufferRO/RW(Entity, ComponentType) → UnsafeUntypedDB   │
│                                                                     │
│  Static Methods:                                                    │
│    GetComponentDataPtrRW(ArchetypeChunk, ComponentType, uint)       │
│      → direct chunk buffer + offset access                          │
│    GetChunkComponentDataPtrRW/RO(ArchetypeChunk, ComponentType)     │
│    GetDynamicBufferAccessor(ArchetypeChunk, ComponentType, uint)    │
│      → UnsafeUntypedDynamicBufferAccessor                           │
│    GetRequiredEnabledBitsRO(ArchetypeChunk, ComponentType)          │
│      → ref readonly v128                                            │
└─────────────────────────────────────────────────────────────────────┘
```

## Static Chunk-Level Access

```
GetComponentDataPtrRW(chunk, componentType, globalVersion)
       │
       ▼
  archetype = store->GetArchetype(chunk.m_Chunk)
  indexInTypeArray = ChunkDataUtility.GetIndexInTypeArray(archetype, typeIndex)
  offset = archetype->Offsets[indexInTypeArray]
  SetChangeVersion(indexInTypeArray, listIndex, globalVersion)
  return chunk.m_Chunk.Buffer + offset
```

## Key Design Decisions

- **Non-generic API**: All methods take ComponentType or TypeIndex instead of being generic,
  allowing runtime-determined type access.
- **byte* returns**: Returns raw byte pointers that callers can reinterpret as needed.
- **Required vs Optional**: GetRequired* asserts component exists; Get* returns null if missing.
- **Chunk-level static methods**: Operate directly on ArchetypeChunk for batch processing without
  per-entity overhead.

## Verified Data

```
UnsafeEntityDataAccess
  Kind: struct, 16 bytes
  Methods: Update, GetComponentDataPtrRW, GetChunkComponentDataPtrRW,
    GetChunkComponentDataPtrRO, GetDynamicBufferAccessor, GetRequiredEnabledBitsRO,
    HasComponent (x2), Exists, GetEntityStorageInfo, GetRequiredComponentDataPtrRO (x2),
    GetRequiredComponentDataPtrRW (x2), GetComponentDataPtrRO, GetComponentDataPtrRW,
    GetUntypedBufferRO, GetUntypedBufferRW

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Iterators/UnsafeEntityDataAccess.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/UnsafeEntityDataAccess.cs)
