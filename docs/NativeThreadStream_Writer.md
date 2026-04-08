# NativeThreadStream.Writer

## Inner Workings Diagram

```
 NativeThreadStream.Writer
 ======================================================================
 Defined as: NativeThreadStream
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ NativeThreadStream                                                 │
 ├────────────────────────────────────────────────────────────────────┤
 │ int                      ForEachCount                              │
 │ bool                     IsCreated                                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ IsEmpty()                                                          │
 │   → bool                                                           │
 │ AsReader()                                                         │
 │   → Reader                                                         │
 │ AsWriter()                                                         │
 │   → Writer                                                         │
 │ Count()                                                            │
 │   → int                                                            │
 │ Dispose()                                                          │
 │   → void                                                           │
 │ Dispose(JobHandle dependency)                                      │
 │   → JobHandle                                                      │
 │ Equals(NativeThreadStream other)                                   │
 │   → bool                                                           │
 │ GetHashCode()                                                      │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs)
