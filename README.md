# UnsafePartialKeyedMap

## Inner Workings Diagram

```
 UnsafePartialKeyedMap
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ bool                     IsCreated                                 │
 │ int*                     Next                                      │
 │ int*                     Buckets                                   │
 │ int*                     Next                                      │
 │ int*                     Buckets                                   │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Destroy(UnsafePartialKeyedMap<TValue>* listData)                   │
 │   → void                                                           │
 │ Dispose()                                                          │
 │   → void                                                           │
 │ Dispose(JobHandle inputDeps)                                       │
 │   → JobHandle                                                      │
 │ Update(int* newKeys, TValue* newValues, int newLength)             │
 │   → void                                                           │
 │ TryGetFirstValue(int key, out TValue item, out UnsafeKeyedMapIterat)│
 │   → bool                                                           │
 │ TryGetNextValue(out TValue item, ref UnsafeKeyedMapIterator it)    │
 │   → bool                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Collections/UnsafePartialKeyedMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafePartialKeyedMap.cs)
