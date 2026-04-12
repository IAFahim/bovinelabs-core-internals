# Ptr

## Inner Workings Diagram

```
 Ptr
 ======================================================================
 Namespace:  BovineLabs.Core.Utility

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ T*                       Value                                     │
 │ bool                     IsCreated                                 │
 │ T                        Ref                                       │
 │ bool                     operator                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Equals(Ptr<T> other)                                               │
 │   → bool                                                           │
 │ Equals(object obj)                                                 │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
Ptr<T>
  Kind: struct, 8 bytes, ns=BovineLabs.Core.Utility
  Properties: Boolean IsCreated, T& Ref
  Methods: Equals (x2), GetHashCode

Verified: 1 checks, 0 failures
```


> Tested example: [Example/ReferenceExample.cs](../Example/ReferenceExample.cs)


> Tested example: [Example/FunctionsExample.cs](../Example/FunctionsExample.cs)

## Source

- [BovineLabs.Core/Utility/Ptr.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Ptr.cs)
