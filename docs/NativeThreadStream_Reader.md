# NativeThreadStream.Reader

## Inner Workings Diagram

```
 NativeThreadStream.Reader
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

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeThreadStream_Reader.cs) — 19 assertions passing
>
> Key findings:
> - Reader is a nested struct inside NativeThreadStream with 3 fields (reader, remainingBlocks, m_Safety)
> - Methods: BeginForEachIndex, EndForEachIndex, Read<T>, ReadUnsafePtr, ReadLarge, Count
> - Properties: RemainingItemCount, ForEachCount
> - Writer nested type also confirmed with Write<T>, Allocate, WriteLarge

## Source

- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs)