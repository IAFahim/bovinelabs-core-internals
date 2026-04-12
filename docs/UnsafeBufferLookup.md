# UnsafeBufferLookup — Unmanaged Buffer Access

## Overview

UnsafeBufferLookup<T> is an unmanaged alternative to BufferLookup<T> that returns
UnsafeDynamicBuffer<T> instead of DynamicBuffer<T>. It bypasses AtomicSafetyHandle for
zero-overhead buffer access in Burst-compiled jobs.

```
┌─────────────────────────────────────────────────────────────────────┐
│  UnsafeBufferLookup<T>  where T : unmanaged, IBufferElementData    │
│  [StructLayout(LayoutKind.Sequential)]                              │
│                                                                     │
│  Fields:                                                            │
│  ┌────────────────────────────────────────┐                         │
│  │  EntityDataAccess* access              │                         │
│  │  TypeIndex typeIndex                   │                         │
│  │  byte isReadOnly                       │                         │
│  │  LookupCache cache                     │                         │
│  │  uint globalSystemVersion              │                         │
│  │  int internalCapacity                  │  ← TypeManager buffer   │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Indexer:                                                           │
│    UnsafeDynamicBuffer<T> this[Entity] { get; }                     │
│      └─ Gets BufferHeader*, creates UnsafeDynamicBuffer             │
│      └─ RO: GetComponentDataWithTypeRO → BufferHeader*              │
│      └─ RW: GetComponentDataWithTypeRW → BufferHeader*              │
│                                                                     │
│  Methods:                                                           │
│    TryGetBuffer(Entity, out UnsafeDynamicBuffer<T>) → bool          │
│    HasBuffer(Entity) → bool                                         │
│    DidChange(Entity, uint version) → bool                           │
│    IsBufferEnabled(Entity) → bool                                   │
│    SetBufferEnabled(Entity, bool)                                   │
│    Update(SystemBase) / Update(ref SystemState)                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **UnsafeDynamicBuffer return**: Returns UnsafeDynamicBuffer<T> which wraps a BufferHeader*
  directly, avoiding the managed overhead of DynamicBuffer<T>.
- **Sequential layout**: Explicit [StructLayout(LayoutKind.Sequential)] ensures deterministic
  field ordering for unsafe reinterpretation.
- **internalCapacity**: Fetched once from TypeManager.GetTypeInfo<T>().BufferCapacity and cached
  in the struct for creating UnsafeDynamicBuffer instances.
- **Null-safe TryGetBuffer**: Returns false with default buffer for non-existent entities or
  missing buffer components, using GetOptionalComponentDataWithTypeRO.

## Verified Data

```
UnsafeBufferLookup<T>
  Kind: struct, generic (1 param)
  Methods: TryGetBuffer, HasBuffer, DidChange, IsBufferEnabled, SetBufferEnabled,
    Update (x2 overloads)
  Returns UnsafeDynamicBuffer<T> from indexer

Verified: 1 checks, 0 failures
```


> Tested example: [Example/UnsafeLookupsExample.cs](../Example/UnsafeLookupsExample.cs)

## Source

- [BovineLabs.Core/Iterators/UnsafeBufferLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/UnsafeBufferLookup.cs)
