# SerializedHelper.IterateAllChildren

## Inner Workings Diagram

```
 SerializedHelper.IterateAllChildren
 ======================================================================
 Namespace:  BovineLabs.Core.Editor.Helpers


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ IterateAllChildren(SerializedObject root, bool includeScript, bool si
 │   → IEnumerable<Ser                                                │
 │ IterateAllChildrenAndFlatten(SerializedObject root)                │
 │   → IEnumerable<Ser                                                │
 │ IterateAllChildrenAndFlatten(SerializedProperty iterator)          │
 │   → IEnumerable<Ser                                                │
 │ GetChildren(SerializedProperty property)                           │
 │   → IEnumerable<Ser                                                │
 │ GetFieldType(this SerializedProperty property)                     │
 │   → Type                                                           │
 │ GetFieldInfo(this SerializedProperty property)                     │
 │   → FieldInfo                                                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
Helpers: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Editor/Helpers/SerializedHelper.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Helpers/SerializedHelper.cs)
