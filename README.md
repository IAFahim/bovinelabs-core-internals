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

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderMultiHashMap.cs)
