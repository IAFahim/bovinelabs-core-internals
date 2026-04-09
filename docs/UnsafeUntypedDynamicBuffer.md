# UnsafeUntypedDynamicBuffer

## Inner Workings Diagram

```
 UnsafeUntypedDynamicBuffer
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ struct                   UnsafeUntypedDynamicBuffer                │
 │ int                      Length                                    │
 │ int                      Capacity                                  │
 │ bool                     IsEmpty                                   │
 │ bool                     IsCreated                                 │
 │ int                      ElementSize                               │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ResizeUninitialized(int length)                                    │
 │   → void                                                           │
 │ Resize(int length, NativeArrayOptions options)                     │
 │   → void                                                           │
 │ EnsureCapacity(int length)                                         │
 │   → void                                                           │
 │ Clear()                                                            │
 │   → void                                                           │
 │ Add(void* elem)                                                    │
 │   → int                                                            │
 │ AddRange(void* elem, int count)                                    │
 │   → void                                                           │
 │ RemoveRange(int index, int count)                                  │
 │   → void                                                           │
 │ RemoveAt(int index)                                                │
 │   → void                                                           │
 │ GetUnsafePtr()                                                     │
 │   → void*                                                          │
 │ GetUnsafeReadOnlyPtr()                                             │
 │   → void*                                                          │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
UnsafeUntypedDynamicBuffer: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Collections/UnsafeUntypedDynamicBuffer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafeUntypedDynamicBuffer.cs)
