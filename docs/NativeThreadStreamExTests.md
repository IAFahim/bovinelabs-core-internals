# NativeThreadStreamExTests

## Inner Workings Diagram

```
 NativeThreadStreamExTests
 ======================================================================
 Defined as: NativeThreadStreamExTests
 Namespace:  BovineLabs.Core.Tests.Collections.ThreadStream


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WriteRead(int size)                                                │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/core-collections/NativeThreadStreamExTests.cs) — 16 assertions passing
>
> Key findings:
> - NativeThreadStream has Dispose, AsReader, AsWriter, Count, ToNativeArray
> - IsCreated and IsEmpty properties confirmed
> - Write-read lifecycle: Write(42), Write(100) → Count()==2, reader reads both values in order
> - ToNativeArray<int> returns array matching written data

## Verified Data

```
NativeThreadStream
  Kind: struct
Properties:
  Boolean IsCreated
Methods:
  Boolean IsEmpty()
  Reader AsReader()
  Writer AsWriter()
  Writer`1 AsWriter()
  Int32 Count()
  NativeArray`1 ToNativeArray(Allocator)
  Void Dispose()
  JobHandle Dispose(JobHandle)
  Boolean Equals(NativeThreadStream)
  Int32 GetHashCode()
Runtime Behavior:
  IsCreated=True
  Write(42), Write(100): Count()=2
  BeginForEachIndex(0)=2
  Read: 42, 100
  ToNativeArray: Length=2, [42,100]
Verified: 4 checks, 0 failures
```

## Source

- [BovineLabs.Core.Tests/Collections/ThreadStream/NativeThreadStreamExTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/ThreadStream/NativeThreadStreamExTests.cs)
