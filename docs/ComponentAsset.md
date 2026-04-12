# ComponentAsset — Standard Component Asset

## Overview

ComponentAsset is the simplest concrete implementation of ComponentAssetBase. It provides a
standard ScriptableObject asset for representing an ECS component type, with a StableTypeHash
selector filtered to IComponentData and IBufferElementData types.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ComponentAsset : ComponentAssetBase                                │
│                                                                     │
│  [CreateAssetMenu(menuName = "BovineLabs/Components/Component")]    │
│                                                                     │
│  Serialized:                                                        │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │  [StableTypeHash(                                         │     │
│  │    ComponentData | BufferData,                             │     │
│  │    AllowEditorAssemblies = false)]                         │     │
│  │  ulong component                                          │     │
│  └────────────────────────────────────────────────────────────┘     │
│                                                                     │
│  protected override ulong Component => this.component               │
└─────────────────────────────────────────────────────────────────────┘
```

## Create Asset Menu

- **Menu Path**: BovineLabs > Components > Component
- **File Name**: Component

## Type Filter

The `[StableTypeHash]` attribute restricts the type selector to:
- `TypeCategory.ComponentData` — standard IComponentData types
- `TypeCategory.BufferData` — IBufferElementData types
- `AllowEditorAssemblies = false` — excludes editor-only types

## Key Design Decisions

- **Minimal implementation**: Just a single serialized ulong field and the Component override.
- **No custom validation**: Uses the base class validation only (checks hash resolves to a type).

## Verified Data

```
ComponentAsset
  Kind: class, base=ComponentAssetBase
  Attributes: CreateAssetMenuAttribute

Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Component/ComponentAsset.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Component/ComponentAsset.cs)
