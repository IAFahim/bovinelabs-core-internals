# Serializer — Inner Workings

## Overview

Serializer is an unmanaged struct that writes typed data into a growing byte buffer
backed by `UnsafeList<byte>`. It provides append-only writes with automatic resizing,
plus no-resize variants for performance-critical paths.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Serializer (unmanaged struct, IDisposable)             │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  UnsafeList<byte>* Data    (growable byte buffer, heap-allocated)     │  │
│  │  int Length                (current written byte count)               │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  Add<T>(val)              → append value, resize if needed                  │
│  AddNoResize<T>(val)      → append value, NO resize (faster)               │
│  AddBuffer<T>(array)      → append entire array                            │
│  Allocate<T>()            → reserve space, return index for later write     │
│  GetAllocation<T>(idx)    → get pointer to previously allocated spot        │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Memory Layout

```
  UnsafeList<byte> (heap allocated)
  ┌─────────────────────────────────────────────────────────────────┐
  │  Ptr ──► [ byte buffer on heap ]                                 │
  │  Length:  currently used bytes                                    │
  │  Capacity: total allocated bytes                                  │
  │                                                                   │
  │  Heap buffer:                                                     │
  │  ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬────────────────────────────┐   │
  │  │00│01│02│03│04│05│06│07│08│09│   unused capacity          │   │
  │  └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴────────────────────────────┘   │
  │  ├───────────┤                                                    │
  │  │  int val  │  ← Add<int>(42) wrote 4 bytes at offset 0         │
  │  ├───────────┤                                                    │
  │              ├──┤                                                  │
  │              │ff│  ← Add<byte>(0xFF) wrote 1 byte at offset 4     │
  │              ├──┤                                                  │
  │  Length = 5,  Capacity = 24                                        │
  └─────────────────────────────────────────────────────────────────┘
```

## Add<T>() — With Resize

```
  Add<T>(value)
         │
         ▼
  ┌───────────────────────────────────────────────────────────┐
  │  1. AddRange(&value, sizeof(T))                           │
  │                                                           │
  │     value on stack:                                       │
  │     ┌──────────┐                                          │
  │     │ T bytes  │ ──memcpy──► appended to Data->Ptr        │
  │     └──────────┘         at Data->Length                  │
  │                                                           │
  │  2. UnsafeList handles resize:                            │
  │     if (Length + sizeof(T) > Capacity):                   │
  │       allocate new larger buffer                          │
  │       copy old data                                       │
  │       free old buffer                                     │
  │                                                           │
  │  3. Data->Length += sizeof(T)                             │
  └───────────────────────────────────────────────────────────┘
```

## AddNoResize<T>() — No Resize (Faster)

```
  AddNoResize<T>(value)
         │
         ▼
  ┌───────────────────────────────────────────────────────────┐
  │  1. AddRangeNoResize(&value, sizeof(T))                   │
  │                                                           │
  │     Skips capacity check entirely.                        │
  │     Caller MUST have called EnsureExtraCapacity() first!  │
  │                                                           │
  │  2. memcpy value bytes → Data->Ptr + Data->Length         │
  │  3. Data->Length += sizeof(T)                             │
  │                                                           │
  │  ⚡ FASTER: no branch, no potential realloc               │
  └───────────────────────────────────────────────────────────┘
```

## Allocate + GetAllocation (Write-Back Pattern)

```
  // Reserve space for header, write data, then fill header:
  int headerIdx = serializer.Allocate<Header>();
  // headerIdx = current Length, Length advances by sizeof(Header)
  // Data at that position is UNINITIALIZED

  // ... write lots of data ...

  // Now fill in the header with final values:
  Header* hdr = serializer.GetAllocation<Header>(headerIdx);
  hdr->count = actualCount;
  hdr->offset = dataStartOffset;

  ┌─────────────────────────────────────────────────────────┐
  │  Buffer:                                                │
  │  ┌──────────┬─────────────────────────────────────────┐ │
  │  │  Header  │  data... data... data...                │ │
  │  │ (uninit) │                                           │ │
  │  └──────────┴─────────────────────────────────────────┘ │
  │       ▲                                                 │
  │       │ GetAllocation<Header>(headerIdx) returns ptr    │
  │       │ Caller writes through pointer to fill in        │
  └─────────────────────────────────────────────────────────┘
```

## Buffer Write Variants

```
  ┌──────────────────────────────────────────────────────────────────┐
  │  AddBuffer<T>(NativeArray<T>)                                     │
  │    → memcpy from array's pointer, length * sizeof(T) bytes       │
  │                                                                   │
  │  AddBuffer<T>(NativeSlice<T>)                                     │
  │    → MemCpyStride to handle stride != sizeof(T)                  │
  │    (for sliced buffers with different element spacing)            │
  │                                                                   │
  │  AddBuffer<T>(T* ptr, int length)                                 │
  │    → raw pointer + count, memcpy length * sizeof(T)              │
  │                                                                   │
  │  AddBuffer(byte* ptr, int size)                                   │
  │    → raw byte copy, size bytes                                    │
  └──────────────────────────────────────────────────────────────────┘
```

## EnsureExtraCapacity

```
  EnsureExtraCapacity(capacity)
         │
         ▼
  ┌───────────────────────────────────────────────────────────┐
  │  if (Length + capacity > Capacity):                        │
  │    Capacity = Length + capacity                            │
  │    // UnsafeList handles realloc internally               │
  │  // else: already enough space, no-op                     │
  │                                                           │
  │  Call before a series of AddNoResize calls:               │
  │    serializer.EnsureExtraCapacity(sizeof(int) * count);   │
  │    for (...) serializer.AddNoResize<int>(values[i]);      │
  └───────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Pointer-based**: Stores `UnsafeList<byte>*` (pointer) not value, so the struct
  is small and can be passed by value while sharing the same underlying buffer.
- **NoResize variants**: Critical for inner-loop performance where you pre-calculate
  capacity and want to avoid branch/realloc overhead per element.
- **Allocate/GetAllocation**: Enables forward-reference patterns common in binary
  serialization (write header placeholder, write body, fill header with offsets/sizes).
- **IDisposable**: Caller must `Dispose()` to free the UnsafeList.

## Source File

- `BovineLabs.Core/Utility/Serializer.cs`

## Source

- [BovineLabs.Core/Utility/Serializer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Serializer.cs)
- [BovineLabs.Core/Utility/Deserializer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Deserializer.cs)
