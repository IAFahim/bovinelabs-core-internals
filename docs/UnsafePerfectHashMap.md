# UnsafePerfectHashMap

## Inner Workings Diagram

```
 UnsafePerfectHashMap
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ TKey*                    Keys                                      │
 │ TValue*                  Values                                    │
 │ int                      Size                                      │
 │ TValue                   NullValue                                 │
 │ bool                     IsCreated                                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Free(UnsafePerfectHashMap<TKey, TValue>* data)                     │
 │   → void                                                           │
 │ Dispose()                                                          │
 │   → void                                                           │
 │ TryGetValue(TKey key, out TValue item)                             │
 │   → bool                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Collections/UnsafePerfectHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafePerfectHashMap.cs)
