# NativeListExtensions.ClearAddRange

## Inner Workings Diagram

```
 NativeListExtensions.ClearAddRange
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeListExtensions                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeListExtensions_ClearAddRange.cs) — 16 assertions passing
>
> Key findings:
> - ClearAddRange(IEnumerable) clears list then adds all elements
> - ClearAddRange(NativeArray) overload verified
> - ClearAddRange(NativeHashSet) overload verified (order not guaranteed)
> - Each call fully replaces previous content; length matches input size

## Source

- [BovineLabs.Core/Extensions/NativeListExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeListExtensions.cs)
