# UntypedDynamicBuffer

## Inner Workings Diagram

```
 UntypedDynamicBuffer
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ struct                   UntypedDynamicBuffer                      │
 │ int                      AlignOf                                   │
 │ AtomicSafetyHandle       m_Safety0                                 │
 │ AtomicSafetyHandle       m_Safety1                                 │
 │ int                      m_SafetyReadOnlyCount                     │
 │ int                      m_SafetyReadWriteCount                    │
 │ byte                     m_IsReadOnly                              │
 │ byte                     m_useMemoryInitPattern                    │
 │ byte                     m_memoryInitPattern                       │
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
UntypedDynamicBuffer: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Collections/UntypedDynamicBuffer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UntypedDynamicBuffer.cs)
