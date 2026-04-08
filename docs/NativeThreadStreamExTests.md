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

## Source

- [BovineLabs.Core.Tests/Collections/ThreadStream/NativeThreadStreamExTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/ThreadStream/NativeThreadStreamExTests.cs)
