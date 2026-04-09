# ConfigVarAttribute

## Inner Workings Diagram

```
 ConfigVarAttribute
 ======================================================================
 Defined as: ConfigVarAttribute
 Namespace:  BovineLabs.Core.ConfigVars

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ConfigVarAttribute                                                 │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   Name                                      │
 │ string                   Description                               │
 │ string                   DefaultValue                              │
 │ bool                     IsReadOnly                                │
 │ bool                     IsHidden                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Equals(ConfigVarAttribute other)                                   │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 │ RectToVector4(Vector4 v4)                                          │
 │   → string                                                         │
 │ StringToVector4(string s)                                          │
 │   → Vector4                                                        │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/ConfigVars/ConfigVarAttribute.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/ConfigVars/ConfigVarAttribute.cs)
