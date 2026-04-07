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

## Source

- [BovineLabs.Core.Editor/Inspectors/BlobAssetOwnerInspector.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Inspectors/BlobAssetOwnerInspector.cs)
