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

## Verified Data

> [Run test snippet](../snippets/core-collections/UnsafePartialKeyedMap.cs) — 22 assertions passing
>
> Key findings:
> - Generic struct UnsafePartialKeyedMap<TValue>; pointer-based (int* keys, TValue* values)
> - 5-parameter constructor (keys, values, length, bucketCapacity, allocator)
> - TryGetFirstValue/TryGetNextValue for multi-value enumeration
> - Static Create/Destroy factory methods; IsCreated property, indexer
> - Update(int*, TValue*, int) for refreshing data; multiple Dispose overloads

## Verified Data

```
UnsafePartialKeyedMap<int>
  Kind: struct, 56 bytes
  Fields:
    Int32* values (private)
    Int32* keys (private)
    Int32 count (private)
    Int32 nextCapacity (private)
    Int32 bucketCapacity (private)
    Int32* <Next>k__BackingField (private)
    Int32* <Buckets>k__BackingField (private)
    AllocatorHandle <Allocator>k__BackingField (private)
  Methods:
    Void Dispose()
    JobHandle Dispose(JobHandle)
    Void Update(Int32*, Int32*, Int32)
    Boolean TryGetFirstValue(Int32, out Int32&, out UnsafeKeyedMapIterator&)
    Boolean TryGetNextValue(out Int32&, UnsafeKeyedMapIterator&)
  Static Methods:
    UnsafePartialKeyedMap`1* Create(Int32*, Int32*, Int32, Int32, AllocatorHandle)
    Void Destroy(UnsafePartialKeyedMap`1*)
Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/UnsafePartialKeyedMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafePartialKeyedMap.cs)
