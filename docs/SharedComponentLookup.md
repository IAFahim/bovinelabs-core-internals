# SharedComponentLookup — Shared Component Data Access

## Overview

SharedComponentLookup<T> and SharedComponentDataFromIndex<T> provide access to ISharedComponentData
values indexed by either Entity or shared component index. They are [NativeContainer] structs
with proper AtomicSafetyHandle integration.

```
┌─────────────────────────────────────────────────────────────────────┐
│  SharedComponentLookup<T>                                           │
│    where T : unmanaged, ISharedComponentData                        │
│  [NativeContainer]                                                  │
│                                                                     │
│  Fields:                                                            │
│    AtomicSafetyHandle m_Safety (checks-only)                        │
│    EntityDataAccess* m_Access                                       │
│    TypeIndex m_TypeIndex                                            │
│                                                                     │
│  Indexers:                                                          │
│    T this[int index]       → by shared component table index        │
│    T this[Entity entity]   → resolve entity to shared component     │
│                                                                     │
│  Methods:                                                           │
│    HasComponent(Entity) → bool                                      │
│    TryGetComponent(Entity, out T) → bool                            │
│    Update(SystemBase) / Update(ref SystemState) → refreshes safety  │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│  SharedComponentDataFromIndex<T>                                    │
│    where T : struct, ISharedComponentData                           │
│  [NativeContainer]                                                  │
│                                                                     │
│  Indexer:                                                           │
│    T this[int index]       → by shared component table index only   │
│                                                                     │
│  Methods:                                                           │
│    Update(SystemBase) / Update(ref SystemState)                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Differences

| Feature | SharedComponentLookup<T> | SharedComponentDataFromIndex<T> |
|---------|--------------------------|----------------------------------|
| Constraint | `unmanaged` + ISCD | `struct` + ISCD |
| Entity indexer | Yes | No |
| Index indexer | Yes | Yes |
| HasComponent | Yes | No |
| TryGetComponent | Yes | No |

## Key Design Decisions

- **Two variants**: SharedComponentLookup is the full-featured version with Entity lookup;
  SharedComponentDataFromIndex is a minimal version for when you only need index-based access.
- **AtomicSafetyHandle**: Both use [NativeContainer] with safety handles, unlike the Unsafe*
  variants.
- **Safety refresh in Update**: The Update method refreshes the safety handle from the
  DependencyManager, ensuring it's valid for the current frame.

## Verified Data

```
SharedComponentLookup<T>
  Kind: struct, generic (1 param)
  Methods: Boolean HasComponent, Boolean TryGetComponent, Update (x2 overloads)

SharedComponentDataFromIndex<T>
  Kind: struct, generic (1 param)

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Iterators/SharedComponentLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/SharedComponentLookup.cs)
- [BovineLabs.Core/Iterators/SharedComponentDataFromIndex.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/SharedComponentDataFromIndex.cs)
