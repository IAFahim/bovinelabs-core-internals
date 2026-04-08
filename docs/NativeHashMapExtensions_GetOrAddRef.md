# NativeHashMapExtensions.GetOrAddRef

## Inner Workings Diagram

```
 NativeHashMapExtensions.GetOrAddRef
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeHashMapExtensions                   │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeHashMapExtensions_GetOrAddRef.cs) — 15 assertions passing
>
> Key findings:
> - Method exists on both NativeHashMapExtensions and NativeParallelHashMapExtensions
> - GetOrAddRef for new key returns default value and adds to map
> - Mutation through returned ref is immediately visible via TryGetValue
> - GetOrAddRef for existing key returns current value (does not overwrite)
> - Overload with no defaultValue argument uses default(TValue)=0

## Source

- [BovineLabs.Core/Extensions/NativeHashMapExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeHashMapExtensions.cs)
