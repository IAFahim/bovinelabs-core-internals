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

```
NativeThreadStream.Reader
  Kind: struct
  Size: 64 bytes
Fields:
  [0] Reader reader  (private)
  [40] Int32 remainingBlocks  (private)
  [48] AtomicSafetyHandle m_Safety  (private)
Properties:
  Int32 ForEachCount
  Int32 RemainingItemCount
Methods:
  Int32 BeginForEachIndex(Int32)
  Void EndForEachIndex()
  Byte* ReadUnsafePtr(Int32)
  T& Read()
  Int32 Count()
  Void ReadLarge(Byte*, Int32)
  Void ReadLarge(Byte*, Int32)
NativeThreadStream.Writer
  Kind: struct
  Size: 24 bytes
Methods:
  Void Write(T)
  T& Allocate()
  Byte* Allocate(Int32)
  Void WriteLarge(NativeArray`1)
  Void WriteLarge(NativeSlice`1)
  Void WriteLarge(Byte*, Int32)
Verified: 6 checks, 0 failures
```
NativeThreadStream.Reader
  Kind: struct
  Size: 64 bytes
Fields:
  [0] Reader reader  (private)
  [40] Int32 remainingBlocks  (private)
  [48] AtomicSafetyHandle m_Safety  (private)
Properties:
  Int32 ForEachCount
  Int32 RemainingItemCount
Methods:
  Int32 BeginForEachIndex(Int32)
  Void EndForEachIndex()
  Byte* ReadUnsafePtr(Int32)
  T& Read()
  Int32 Count()
  Void ReadLarge(Byte*, Int32)
  Void ReadLarge(Byte*, Int32)
NativeThreadStream.Writer
  Kind: struct
  Size: 24 bytes
Methods:
  Void Write(T)
  T& Allocate()
  Byte* Allocate(Int32)
  Void WriteLarge(NativeArray`1)
  Void WriteLarge(NativeSlice`1)
  Void WriteLarge(Byte*, Int32)
Verified: 6 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs)