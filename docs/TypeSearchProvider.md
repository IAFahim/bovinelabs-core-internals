# TypeSearchProvider

## Inner Workings Diagram

```
 TypeSearchProvider
 ======================================================================
 Namespace:  BovineLabs.Core.Editor.Component

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Type                     Type                                      │
 │ string                   Name                                      │
 │ string                   SimplifiedQualifiedName                   │
 │ string                   FullName                                  │
 │ bool                     IsUnmanaged                               │
 │ bool                     IsUnityObject                             │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Editor/Component/TypeSearchProvider.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Component/TypeSearchProvider.cs)
