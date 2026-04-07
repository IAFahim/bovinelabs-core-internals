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

## Source

- [BovineLabs.Core/Utility/Ptr.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Ptr.cs)
