# NativeParallelMultiHashMapExtensions.GetUniqueKeyArray

## Inner Workings Diagram

```
 NativeParallelMultiHashMapExtensions.GetUniqueKeyArray
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeParallelMultiHashMapExtensions      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeParallelMultiHashMapExtensions_GetUniqueKeyArray.cs) — 17 assertions passing
>
> Key findings:
> - Static class with GetUniqueKeyArray(map, NativeList) extension method
> - Method exists and has correct signature but **throws NotImplementedException at runtime**
> - Other extension methods confirmed: Reserve, ClearAndAddBatch, AddBatchUnsafe, RecalculateBuckets

## Source

- [BovineLabs.Core/Extensions/NativeParallelMultiHashMapExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeParallelMultiHashMapExtensions.cs)
