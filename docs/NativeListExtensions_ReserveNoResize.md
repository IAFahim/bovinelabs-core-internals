# NativeListExtensions.ReserveNoResize

## Inner Workings Diagram

```
NativeListExtensions.ReserveNoResize
======================================================================
Namespace:  BovineLabs.Core.Extensions

Structure:
┌────────────────────────────────────────────────────────────────────┐
│ class                    NativeListExtensions                      │
└────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeListExtensions_ReserveNoResize.cs) — 11 assertions passing
>
> Key findings:
> - Static class with ReserveNoResize as extension method
> - Has NativeList<T> overload (4 params: list, length, T*, ref int)
> - Has ParallelWriter overload (4 params)
> - Returns void; grows list without zeroing new elements

## Source

- [BovineLabs.Core/Extensions/NativeListExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeListExtensions.cs)
