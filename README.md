# NativeArrayExtensions.ElementAtRO

## Inner Workings Diagram

```
 NativeArrayExtensions.ElementAtRO
 ======================================================================
 Defined as: IPredicate
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Equals                                                             │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    NativeArrayExtensions                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Check(T other)                                                     │
 │   → bool                                                           │
 │ Min(this NativeArray<int> collection)                              │
 │   → int                                                            │
 │ Min(this NativeArray<float> collection)                            │
 │   → float                                                          │
 │ Max(this NativeArray<int> collection)                              │
 │   → int                                                            │
 │ Max(this NativeArray<float> collection)                            │
 │   → float                                                          │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Extensions/NativeArrayExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeArrayExtensions.cs)
