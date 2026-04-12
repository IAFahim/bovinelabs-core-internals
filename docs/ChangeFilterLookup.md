# ChangeFilterLookup — Manual Change Version Control

## Overview

ChangeFilterLookup<T> provides manual control over Unity's chunk-level change versioning system.
It can check whether a component has changed (DidChange) and, uniquely, can SET the change
version on a chunk (SetChangeFilter), allowing custom systems to mark data as changed.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ChangeFilterLookup<T>  where T : unmanaged                        │
│                                                                     │
│  Fields:                                                            │
│    TypeIndex typeIndex                                              │
│    EntityDataAccess* access                                         │
│    LookupCache cache                                                │
│    uint globalSystemVersion                                         │
│    bool isReadOnly (checks-only)                                    │
│                                                                     │
│  Query Methods:                                                     │
│    Update(SystemBase) / Update(ref SystemState)                     │
│    DidChange(Entity entity, uint version) → bool                    │
│                                                                     │
│  Mutation Methods:                                                  │
│    SetChangeFilter(Entity entity)                                   │
│      → Sets chunk change version to globalSystemVersion             │
│    SetChangeFilter(int chunkIndex)                                  │
│      → Same but by raw chunk index                                 │
└─────────────────────────────────────────────────────────────────────┘
```

## SetChangeFilter Flow

```
SetChangeFilter(Entity entity)
       │
       ▼
  chunk = ecs->GetChunk(entity)
  SetChangeFilter(chunk, globalSystemVersion)
       │
       ▼
  CheckWriteAndThrow (checks-only)
  archetype = ecs->GetArchetype(chunk)
  cache.Update if archetype changed
  typeIndexInArchetype = cache.IndexInArchetype
       │
       ├─ -1 → return (type not in archetype)
       │
       └─ archetype->Chunks.SetChangeVersion(typeIndexInArchetype, listIndex, systemVersion)
```

## Key Design Decisions

- **Manual change marking**: This is the only lookup type that lets you SET change versions,
  not just read them. Useful for custom systems that modify component data through raw pointers
  and need to signal the change to downstream systems.
- **T is unmanaged, not IComponentData**: The constraint is just `unmanaged`, not
  `IComponentData`, because ChangeFilterLookup works with TypeIndex directly — it doesn't
  need to know the generic type for data access.
- **Read-only guard**: SetChangeFilter throws if the lookup was created as read-only (in
  collections checks builds).

## Verified Data

```
ChangeFilterLookup<T>
  Kind: struct, generic (1 param)
  Methods: Update (x2 overloads), Boolean DidChange, Void SetChangeFilter (x2 overloads)

Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Iterators/ChangeFilterLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/ChangeFilterLookup.cs)
