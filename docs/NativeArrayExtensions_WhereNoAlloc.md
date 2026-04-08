# NativeArrayExtensions.WhereNoAlloc

## Inner Workings Diagram

```
 NativeArrayExtensions.WhereNoAlloc
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

## Verified Data

> [Run test snippet](../snippets/memory-allocators/NativeArrayExtensions_WhereNoAlloc.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Extensions/NativeArrayExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeArrayExtensions.cs)
