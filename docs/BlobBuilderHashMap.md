# BlobBuilderHashMap

## Inner Workings Diagram

```
 BlobBuilderHashMap
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ int                      Capacity                                  │
 │ int                      Count                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Add(TKey key, TValue item)                                         │
 │   → void                                                           │
 │ TryAdd(TKey key, TValue value)                                     │
 │   → bool                                                           │
 │ ContainsKey(TKey key)                                              │
 │   → bool                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobBuilderHashMap.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderHashMap.cs)
