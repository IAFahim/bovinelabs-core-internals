# UnsafeComponentLookup — Unmanaged Component Access

## Overview

UnsafeComponentLookup<T> is an unmanaged alternative to ComponentLookup<T> that bypasses
AtomicSafetyHandle. It provides direct pointer-based access to component data indexed by Entity,
suitable for use inside Burst-compiled jobs where safety handles would add overhead.

```
┌─────────────────────────────────────────────────────────────────────┐
│  UnsafeComponentLookup<T>  where T : unmanaged, IComponentData     │
│                                                                     │
│  Fields:                                                            │
│  ┌────────────────────────────────────────┐                         │
│  │  EntityDataAccess* access              │  ← raw data access      │
│  │  TypeIndex typeIndex                   │                         │
│  │  byte isZeroSized                      │                         │
│  │  LookupCache cache                     │  ← archetype cache      │
│  │  uint globalSystemVersion              │                         │
│  │  byte isReadOnly (checks-only)         │                         │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Indexers:                                                          │
│    T this[Entity] { get; set; }                                     │
│    T this[SystemHandle] { get; set; }  → delegates to entity        │
│                                                                     │
│  Methods:                                                           │
│    Update(SystemBase) / Update(ref SystemState)                     │
│    HasComponent(Entity) / HasComponent(SystemHandle) → bool         │
│    TryGetComponent(Entity, out T) → bool                            │
│    DidChange(Entity, uint version) → bool                           │
│    IsComponentEnabled(Entity) → bool                                │
│    SetComponentEnabled(Entity, bool)                                │
│    IsComponentEnabled(SystemHandle) → bool                          │
│    SetComponentEnabled(SystemHandle, bool)                          │
└─────────────────────────────────────────────────────────────────────┘
```

## Access Pattern

```
this[entity] (get)
       │
       ▼
  ecs->AssertEntityHasComponent(entity, typeIndex, ref cache)
       │
       ├─ isZeroSized? → return default(T)
       │
       └─ void* ptr = ecs->GetComponentDataWithTypeRO(entity, typeIndex, ref cache)
            └─ UnsafeUtility.CopyPtrToStructure(ptr, out T data)
            └─ return data

this[entity] (set)
       │
       ▼
  CheckWriteAndThrow (checks-only)
       │
       ▼
  ecs->AssertEntityHasComponent(...)
       │
       ├─ isZeroSized? → return
       │
       └─ void* ptr = ecs->GetComponentDataWithTypeRW(entity, typeIndex, globalSystemVersion)
            └─ UnsafeUtility.CopyStructureToPtr(ref value, ptr)
```

## Key Design Decisions

- **No AtomicSafetyHandle**: Unlike ComponentLookup<T>, this struct has no safety handle injection.
  This removes job system safety checks but reduces overhead.
- **LookupCache**: Caches the archetype-to-type-index mapping internally, reusing it across
  accesses to the same archetype for faster lookups.
- **Zero-sized handling**: Returns default(T) for tag components (zero-sized structs) instead
  of dereferencing a null pointer.
- **SystemHandle support**: Both indexers and query methods work with SystemHandle, mapping to
  the underlying system entity.

## Verified Data

```
UnsafeComponentLookup<T>
  Kind: struct, generic (1 param)
  Fields: EntityDataAccess* access, TypeIndex typeIndex, Byte isZeroSized, Byte isReadOnly,
    LookupCache cache, UInt32 globalSystemVersion
  Methods: Update (x2 overloads), HasComponent (x2), TryGetComponent, DidChange,
    IsComponentEnabled (x2), SetComponentEnabled (x2)

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Iterators/UnsafeComponentLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/UnsafeComponentLookup.cs)
