# BlobAssetOwnerInspector

## Inner Workings Diagram

```
 BlobAssetOwnerInspector
 ======================================================================
 Namespace:  BovineLabs.Core.Editor.Inspectors

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ int                      TotalDataSize                             │
 │ int                      BlobAssetHeaderCount                      │
 │ int                      RefCount                                  │
 │ int                      Padding                                   │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Build()                                                            │
 │   → VisualElement                                                  │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobAssetOwnerInspector.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobAssetOwnerInspector
  Namespace: BovineLabs.Core.Editor.Inspectors
  Kind: class
  Base: PropertyInspector`1

  Fields:

  Methods:
    VisualElement Build()

Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Editor/Inspectors/BlobAssetOwnerInspector.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Inspectors/BlobAssetOwnerInspector.cs)
