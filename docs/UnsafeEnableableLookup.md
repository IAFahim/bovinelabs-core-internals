# UnsafeEnableableLookup — Non-Generic Enableable Access

## Overview

UnsafeEnableableLookup is a non-generic, unmanaged struct that provides enableable component
checking and toggling by ComponentType instead of by generic type parameter. It wraps a raw
EntityDataAccess pointer.

```
┌─────────────────────────────────────────────────────────────────────┐
│  UnsafeEnableableLookup (struct)                                    │
│                                                                     │
│  Fields:                                                            │
│    EntityDataAccess* access (readonly)                              │
│                                                                     │
│  Methods:                                                           │
│    HasComponent(Entity, ComponentType) → bool                       │
│    IsComponentEnabled(Entity, ComponentType) → bool                 │
│    SetComponentEnabled(Entity, ComponentType, bool)                 │
│                                                                     │
│  All methods delegate to access->IsComponentEnabled /               │
│  access->SetComponentEnabled with the TypeIndex extracted           │
│  from ComponentType.TypeIndex                                       │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Non-generic**: Takes ComponentType as a parameter at runtime, useful when the component type
  is not known at compile time (e.g., iterating over multiple component types).
- **No safety handles**: Wraps EntityDataAccess directly without AtomicSafetyHandle injection.
- **Minimal surface area**: Only 3 methods — this is intentionally lean, providing just the
  enableable operations that aren't already covered by other lookup types.

## Verified Data

```
UnsafeEnableableLookup
  Kind: struct, 8 bytes
  Methods: Boolean HasComponent, Boolean IsComponentEnabled, Void SetComponentEnabled

Verified: 2 checks, 0 failures
```


> Tested example: [Example/UnsafeLookupsExample.cs](../Example/UnsafeLookupsExample.cs)

## Source

- [BovineLabs.Core/Iterators/UnsafeEnableableLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/UnsafeEnableableLookup.cs)
