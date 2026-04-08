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

> [Run test snippet](../snippets/blob-system/BlobAssetOwnerInspector.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core.Editor/Inspectors/BlobAssetOwnerInspector.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Inspectors/BlobAssetOwnerInspector.cs)
