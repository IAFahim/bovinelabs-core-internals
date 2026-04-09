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

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeThreadStream_Writer.cs) — 11 assertions passing
>
> Key findings:
> - Writer is a nested struct inside NativeThreadStream with 2 fields (writer, m_Safety)
> - Methods: Allocate(int size) → pointer, Write<T> (generic), WriteLarge
> - Write<T> confirmed as generic method

## Verified Data

```
NativeThreadStream.Writer
  Kind: struct
  Size: 24 bytes
Fields:
  [0] Writer writer  (private)
  [8] AtomicSafetyHandle m_Safety  (private)
Methods:
  Void Write(T)
  T& Allocate()
  Byte* Allocate(Int32)
  Void WriteLarge(NativeArray`1)
  Void WriteLarge(NativeSlice`1)
  Void WriteLarge(Byte*, Int32)
Verified: 4 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs)
