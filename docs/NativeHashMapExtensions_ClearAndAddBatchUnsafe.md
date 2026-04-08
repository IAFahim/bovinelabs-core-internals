# NativeHashMapExtensions.ClearAndAddBatchUnsafe

## Inner Workings Diagram

```
NativeHashMapExtensions.ClearAndAddBatchUnsafe
======================================================================
Namespace:  BovineLabs.Core.Extensions

Structure:
┌────────────────────────────────────────────────────────────────────┐
│ class                    NativeHashMapExtensions                   │
└────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeHashMapExtensions_ClearAndAddBatchUnsafe.cs) — 16 assertions passing
>
> Key findings:
> - Extension method on NativeParallelHashMapExtensions accepts (NativeArray keys, NativeArray values)
> - Clears map completely, then batch-inserts all key-value pairs
> - Old keys confirmed absent after batch replace
> - Sequential batch operations fully replace previous content each time

## Source

- [BovineLabs.Core/Extensions/NativeHashMapExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeHashMapExtensions.cs)
