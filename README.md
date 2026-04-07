# TypeManagerEx

## Inner Workings Diagram

```
 TypeManagerEx
 ======================================================================
 Namespace:  BovineLabs.Core.Utility

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    TypeManagerEx                             │
 │ SharedStatic<IntPtr>     Ref                                       │
 │ SharedStatic<IntPtr>     Ref                                       │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Initialize()                                                       │
 │   → void                                                           │
 │ GetTypeName(TypeIndex typeIndex)                                   │
 │   → FixedString128B                                                │
 │ GetSystemName(SystemTypeIndex systemIndex)                         │
 │   → FixedString128B                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Utility/TypeManagerEx.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/TypeManagerEx.cs)
