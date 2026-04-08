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

## Verified Data

> [Run test snippet](../snippets/core-collections/UnsafePerfectHashMap.cs) — 28 assertions passing
>
> Key findings:
> - Generic struct UnsafePerfectHashMap<TKey,TValue>; 4-param ctor (NativeArray keys, NativeArray values, nullValue, Allocator)
> - TryGetValue returns false for empty hash slots (key hashing to unused slot)
> - Indexer get/set works; in-place value update confirmed
> - 3-key and 5-key maps verified; size scales as power-of-2

## Source

- [BovineLabs.Core/Collections/UnsafePerfectHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafePerfectHashMap.cs)
