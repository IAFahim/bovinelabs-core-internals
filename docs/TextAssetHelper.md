# TextAssetHelper

## Inner Workings Diagram

```
 TextAssetHelper
 ======================================================================
 Namespace:  BovineLabs.Core.Editor.Helpers


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CreateForProperty(SerializedProperty serializedProperty, string defa)
 │   → void                                                           │
 │ CreateForAsset(TextAsset asset, string defaultName, byte[] bytes)  │
 │   → TextAsset                                                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Editor/Helpers/TextAssetHelper.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Helpers/TextAssetHelper.cs)
