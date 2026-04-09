# ReferenceData

## Inner Workings Diagram

```
 ReferenceData
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Reference<T>             Null                                      │
 │ bool                     IsCreated                                 │
 │ T                        Value                                     │
 │ bool                     operator                                  │
 │ ReferenceData            ReferenceData                             │
 │ struct                   ReferenceData                             │
 │ byte*                    Ptr                                       │
 │ long                     Align8Union                               │
 │ ReferenceHeader*         Header                                    │
 │ struct                   ReferenceHeader                           │
 │ void*                    ValidationPtr                             │
 │ int                      Length                                    │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Create(void* ptr, int length, MemoryAllocator allocator)           │
 │   → Reference<T>                                                   │
 │ Create(byte[] data, MemoryAllocator allocator)                     │
 │   → Reference<T>                                                   │
 │ Create(T value, MemoryAllocator allocator)                         │
 │   → Reference<T>                                                   │
 │ GetUnsafePtr()                                                     │
 │   → void*                                                          │
 │ Equals(Reference<T> other)                                         │
 │   → bool                                                           │
 │ Equals(object obj)                                                 │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
ReferenceData: TYPE NOT FOUND
ReferenceHeader: TYPE NOT FOUND
Verified: 0 checks, 2 failures
```

## Source

- [BovineLabs.Core/Collections/Reference.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Reference.cs)
