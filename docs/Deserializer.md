# Deserializer — Inner Workings

## Overview

Deserializer is an unmanaged struct that reads typed data from a byte buffer by
advancing a `CurrentIndex` pointer. It provides zero-copy reads via unsafe pointer
reinterpretation — no copying, no boxing, no allocations.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Deserializer (unmanaged struct)                         │
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  NativeArray<byte> data     (read-only byte source)                   │  │
│  │  int CurrentIndex           (read cursor position)                    │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  Read<T>()        → read value, advance by sizeof(T)                       │
│  Peek<T>()        → read value, DON'T advance                              │
│  ReadBuffer<T>(n) → read n elements, advance by n*sizeof(T)               │
│  Offset(n)        → skip n bytes                                           │
│  Reset()          → set CurrentIndex back to 0                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Memory Layout

```
  NativeArray<byte> data
  ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
  │00│01│02│03│04│05│06│07│08│09│0A│0B│0C│0D│0E│0F│10│11│12│13│
  └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
  ├───────────┤ ← Read<int>() reads 4 bytes
  │  int val  │    CurrentIndex: 0 → 4
  ├───────────┤
              ├───────────┤ ← Read<float>() reads 4 bytes
              │  float v  │    CurrentIndex: 4 → 8
              ├───────────┤
                          ├──┤ ← Read<byte>() reads 1 byte
                          │b │    CurrentIndex: 8 → 9
                          ├──┤
  Current pointer = data.GetUnsafeReadOnlyPtr() + CurrentIndex
```

## Read<T>() Flow

```
  Read<T>() where T : unmanaged
         │
         ▼
  ┌───────────────────────────────────────────────────────────┐
  │  1. ptr = data.GetUnsafeReadOnlyPtr() + CurrentIndex      │
  │                                                           │
  │     data buffer:                                          │
  │     [...][ T bytes at CurrentIndex ][................]     │
  │           ^                                               │
  │           ptr                                             │
  │                                                           │
  │  2. result = UnsafeUtility.ReadArrayElement<T>(ptr, 0)    │
  │     // Reinterprets bytes at ptr as type T                │
  │     // ZERO COPY — just pointer cast                      │
  │                                                           │
  │  3. CurrentIndex += sizeof(T)                             │
  │     // Advance cursor past the read data                  │
  │                                                           │
  │  4. return result                                         │
  └───────────────────────────────────────────────────────────┘
```

## ReadBuffer<T>() Flow

```
  ReadBuffer<T>(length=3)
         │
         ▼
  ┌───────────────────────────────────────────────────────────┐
  │  1. ptr = (T*)(data.GetUnsafeReadOnlyPtr() + CurrentIndex)│
  │                                                           │
  │     data buffer:                                          │
  │     [...][ T₀ ][ T₁ ][ T₂ ][.........................]    │
  │           ^                                               │
  │           ptr (cast to T*)                                │
  │                                                           │
  │  2. CurrentIndex += length * sizeof(T)                    │
  │     // Advance cursor past all 3 elements                 │
  │                                                           │
  │  3. return ptr                                            │
  │     // Caller gets pointer directly into the buffer       │
  │     // WARNING: only valid while data is alive!           │
  └───────────────────────────────────────────────────────────┘
```

## Construction Paths

```
  ┌─────────────────────────────────────┐  ┌────────────────────────────────────┐
  │  new Deserializer(                  │  │  new Deserializer(                 │
  │    NativeArray<byte> data,          │  │    byte* ptr,                      │
  │    int offset = 0)                  │  │    int length,                     │
  │                                     │  │    int offset = 0)                 │
  │  Stores NativeArray directly.       │  │                                    │
  │  Safety handle inherited.           │  │  Wraps raw pointer as              │
  └─────────────────────────────────────┘  │  NativeArray via                   │
                                           │  ConvertExistingDataToNativeArray   │
                                           │  + manual safety handle creation    │
                                           └────────────────────────────────────┘
```

## Peek vs Read

```
  Peek<T>()                          Read<T>()
  ══════════                         ════════

  Read value, DON'T advance          Read value, DO advance
  ┌────────────┐                     ┌────────────┐
  │ return val │                     │ return val │
  │ index SAME │                     │ index +=sz │
  └────────────┘                     └────────────┘

  Use case: look ahead to decide     Use case: consume the next
  what to deserialize next           typed value from the stream
```

## Properties

```
  ┌──────────────────────────────────────────────────────────────┐
  │  IsCreated   → data.IsCreated (check before use!)            │
  │  Data        → NativeArray<byte> (raw access to full buffer) │
  │  CurrentIndex→ get/set read position                         │
  │  Current     → byte* at CurrentIndex (raw pointer)           │
  │  IsAtEnd     → CurrentIndex == data.Length                   │
  └──────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Zero-copy reads**: `UnsafeUtility.ReadArrayElement` reinterprets memory in-place
  rather than copying bytes into a new struct.
- **Manual cursor management**: `CurrentIndex` is `get/set` so callers can seek
  arbitrarily, but standard `Read<T>()` auto-advances.
- **Pointer-based construction**: Accepts raw `byte*` for maximum flexibility when
  deserializing from `UnsafeList`, `NativeList`, or native plugin buffers.
- **ReadBuffer returns pointer**: Gives direct access into the underlying buffer for
  zero-copy array access — caller must ensure lifetime.

## Verified Data

```
that: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source File

- `BovineLabs.Core/Utility/Deserializer.cs`

## Source

- [BovineLabs.Core/Utility/Deserializer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Deserializer.cs)
