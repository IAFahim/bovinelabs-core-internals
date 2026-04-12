# ComponentAssetBase — ECS Component ScriptableObject

## Overview

ComponentAssetBase is an abstract ScriptableObject that represents an ECS component type as an
asset. It stores a stable type hash and provides methods to resolve the runtime TypeIndex and
System.Type from that hash via TypeManager.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ComponentAssetBase : ScriptableObject (abstract)                  │
│                                                                     │
│  Serialized Fields:                                                 │
│  ┌────────────────────────────────────────┐                         │
│  │  [InspectorReadOnly]                    │                         │
│  │  string componentName                  │  ← display name         │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Abstract Property:                                                 │
│  ┌────────────────────────────────────────┐                         │
│  │  protected abstract ulong Component    │  ← stable type hash     │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Public Methods:                                                    │
│    GetStableTypeHash() → ulong                                      │
│      └─ Validates in editor, returns Component hash                 │
│                                                                     │
│    GetComponentType() → Type                                        │
│      └─ TypeManager.GetTypeIndexFromStableTypeHash → TypeIndex      │
│      └─ TypeManager.GetType(typeIndex) → Type                       │
│                                                                     │
│  Virtual:                                                           │
│    CustomValidation(TypeIndex)  ← override for extra checks         │
└─────────────────────────────────────────────────────────────────────┘
```

## Validation Flow

```
GetTypeIndexWithValidation()
       │
       ▼
  TypeManager.GetTypeIndexFromStableTypeHash(Component)
       │
       ├─ TypeIndex.Null → throw InvalidCastException
       │                    "Type not found for stable type hash {hash} on {name}"
       │
       └─ Valid TypeIndex
            │
            ▼
          CustomValidation(typeIndex)   ← subclass hook
            │
            ▼
          return typeIndex
```

## Key Design Decisions

- **Stable type hash**: Uses Unity's StableTypeHash system so the asset survives domain reloads
  and package version changes.
- **Editor-only validation**: GetTypeIndexWithValidation only runs full validation in #if UNITY_EDITOR
  builds; GetStableTypeHash returns the raw hash at runtime.
- **InvalidCastException**: Throws when the hash doesn't resolve to a valid type — this means the
  component was removed or the package changed.
- **CustomValidation hook**: EnableableComponentAsset overrides this to verify the type still
  implements IEnableableComponent.

## Verified Data

```
ComponentAssetBase
  Kind: abstract class, base=ScriptableObject
  Methods: UInt64 GetStableTypeHash(), Type GetComponentType()

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Component/ComponentAssetBase.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Component/ComponentAssetBase.cs)
