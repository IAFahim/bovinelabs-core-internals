# EnableableComponentAsset — Validated Enableable Component Asset

## Overview

EnableableComponentAsset extends ComponentAssetBase to represent an ECS component that implements
IEnableableComponent. It adds runtime validation to ensure the target type remains enableable.

```
┌─────────────────────────────────────────────────────────────────────┐
│  EnableableComponentAsset : ComponentAssetBase                      │
│                                                                     │
│  [CreateAssetMenu(menuName = "BovineLabs/Components/Enableable")]   │
│                                                                     │
│  Serialized:                                                        │
│  ┌────────────────────────────────────────────────────────────┐     │
│  │  [StableTypeHash(                                         │     │
│  │    ComponentData | BufferData,                             │     │
│  │    AllowEditorAssemblies = false,                          │     │
│  │    OnlyEnableable = true)]                                 │     │
│  │  ulong component                                          │     │
│  └────────────────────────────────────────────────────────────┘     │
│                                                                     │
│  protected override ulong Component => this.component               │
│                                                                     │
│  CustomValidation(TypeIndex typeIndex):                             │
│    if !TypeManager.IsEnableable(typeIndex):                         │
│      throw InvalidCastException                                     │
│        "Type {hash} on {name} is no longer enableable"          │
└─────────────────────────────────────────────────────────────────────┘
```

## Validation Flow

```
GetComponentType() or GetStableTypeHash()
       │
       ▼
  GetTypeIndexWithValidation()
       │
       ▼
  TypeManager.GetTypeIndexFromStableTypeHash(component)
       │
       ▼
  CustomValidation(typeIndex)
       │
       ├─ TypeManager.IsEnableable(typeIndex) == false
       │    └─ throw: "Type is no longer enableable"
       │
       └─ IsEnableable == true
            └─ return (valid)
```

## Key Design Decisions

- **OnlyEnableable filter**: The StableTypeHash attribute restricts the type picker to only show
  types implementing IEnableableComponent.
- **Runtime safety**: If a component is refactored to remove IEnableableComponent, the asset will
  throw at validation time rather than silently failing.
- **Create Asset Menu**: BovineLabs > Components > Enableable

## Verified Data

```
EnableableComponentAsset
  Kind: class, base=ComponentAssetBase
  Overrides CustomValidation to check TypeManager.IsEnableable

Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Component/EnableableComponentAsset.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Component/EnableableComponentAsset.cs)
