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

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderHashMap.cs)
