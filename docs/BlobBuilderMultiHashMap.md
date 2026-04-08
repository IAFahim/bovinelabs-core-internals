# BlobBuilderMultiHashMap

## Inner Workings Diagram

```
 BlobBuilderMultiHashMap
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
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobBuilderMultiHashMap.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderMultiHashMap.cs)
