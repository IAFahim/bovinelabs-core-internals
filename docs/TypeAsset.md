# TypeAsset — Generic C# Type Reference

## Overview

TypeAsset is a ScriptableObject that stores a C# type name as a string and provides a method
to resolve it to a System.Type at runtime via Type.GetType.

```
┌─────────────────────────────────────────────────────────────────────┐
│  TypeAsset : ScriptableObject                                       │
│                                                                     │
│  [CreateAssetMenu(menuName = "BovineLabs/Components/Type")]         │
│                                                                     │
│  Constants:                                                         │
│    SearchProviderType = "types"                                     │
│                                                                     │
│  Serialized:                                                        │
│  ┌────────────────────────────────────────┐                         │
│  │  string typeName                       │  ← assembly-qualified   │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Public API:                                                        │
│    Type ResolveType()                                               │
│      └─ null/empty → return null                                   │
│      └─ Type.GetType(typeName) → Type or null                       │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Type.GetType resolution**: Uses the standard .NET Type.GetType which requires
  assembly-qualified names for types outside the calling assembly.
- **Null-safe**: Returns null for empty/missing type names rather than throwing.
- **Search integration**: SearchProviderType constant enables the type search provider in
  editor UI elements.

## Verified Data

```
TypeAsset
  Kind: class, base=ScriptableObject
  Constants: String SearchProviderType = "types"
  Methods: Type ResolveType()

Verified: 1 checks, 0 failures
```


> Tested example: [Example/ComponentAssetsExample.cs](../Example/ComponentAssetsExample.cs)

## Source

- [BovineLabs.Core/Component/TypeAsset.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Component/TypeAsset.cs)
