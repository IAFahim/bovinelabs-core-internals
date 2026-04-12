# ComponentFieldAsset — Component Field Offset Resolver

## Overview

ComponentFieldAsset is a ScriptableObject that resolves the memory offset of a specific field
within an ECS component type. It uses reflection combined with UnsafeUtility.GetFieldOffset to
get the exact byte offset, with a non-serialized cache for performance.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ComponentFieldAsset : ScriptableObject                             │
│                                                                     │
│  [CreateAssetMenu(menuName = "BovineLabs/Components/Component Field")]│
│                                                                     │
│  Serialized:                                                        │
│  ┌────────────────────────────────────────┐                         │
│  │  ComponentAssetBase component          │  ← which component      │
│  │  string fieldName = string.Empty       │  ← which field          │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Non-Serialized Cache:                                              │
│  ┌────────────────────────────────────────┐                         │
│  │  Cache (readonly struct)               │                         │
│  │  ├── ComponentAssetBase Component      │                         │
│  │  ├── string FieldName                  │                         │
│  │  └── ushort Offset                     │                         │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Public API:                                                        │
│    ushort GetOffset()                                               │
│      1. Check cache (Component + FieldName match?)                  │
│      2. Resolve type via component.GetComponentType()               │
│      3. GetField(fieldName, Instance | Public)                      │
│      4. UnsafeUtility.GetFieldOffset(field) → ushort                │
│      5. Cache and return                                            │
└─────────────────────────────────────────────────────────────────────┘
```

## GetOffset Flow

```
GetOffset()
    │
    ├─ Cache hit (same component + fieldName)?
    │    └─ return cached offset
    │
    └─ Cache miss:
         ├─ component null?    → NullReferenceException
         ├─ fieldName empty?   → NullReferenceException
         ├─ type.GetField(fieldName, Instance|Public) → null?
         │    └─ InvalidOperationException
         ├─ offset = UnsafeUtility.GetFieldOffset(field)
         ├─ cache = new Cache(this, offset)
         └─ return offset
```

## Key Design Decisions

- **Non-serialized cache**: Avoids repeated reflection calls for the same field. The cache is
  invalidated if the component reference or field name changes.
- **Public fields only**: Uses BindingFlags.Instance | BindingFlags.Public — only publicly
  accessible fields can be targeted.
- **ushort offset**: Offsets fit in 16 bits since components are typically small structs.

## Verified Data

```
ComponentFieldAsset
  Kind: class, base=ScriptableObject
  Uses UnsafeUtility.GetFieldOffset for field offset resolution
  Has non-serialized Cache struct for memoization

Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Component/ComponentFieldAsset.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Component/ComponentFieldAsset.cs)
