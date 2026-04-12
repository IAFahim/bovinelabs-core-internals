# Reference<T> and ReferenceData — Unmanaged Blob Alternative

## Overview

Reference<T> is an unmanaged alternative to BlobAssetReference<T> that works with
MemoryAllocator instead of Unity's blob system. It stores a pointer to data prefixed with a
ReferenceHeader (ValidationPtr + Length) for use-after-free detection.

```
┌─────────────────────────────────────────────────────────────────────┐
│  Reference<T> (readonly struct)  where T : unmanaged               │
│                                                                     │
│  Field:                                                             │
│    readonly ReferenceData data                                      │
│                                                                     │
│  Static:                                                            │
│    Reference<T> Null => default                                     │
│                                                                     │
│  Properties:                                                        │
│    bool IsCreated => data.Ptr != null                               │
│    ref T Value => UnsafeUtility.AsRef<T>(data.Ptr)                  │
│    ReferenceData ReferenceData => data                              │
│                                                                     │
│  Factory Methods:                                                   │
│    Create(void* ptr, int length, MemoryAllocator)                   │
│    Create(headerPtr, headerLen, dataPtr, dataLen, allocator)        │
│    Create(byte[] data, MemoryAllocator)                             │
│    Create(T value, MemoryAllocator)                                 │
│                                                                     │
│  Utility:                                                           │
│    void* GetUnsafePtr() => data.Ptr                                 │
│    operator ==, !=, Equals, GetHashCode                             │
├─────────────────────────────────────────────────────────────────────┤
│  ReferenceData (struct)                                             │
│    [StructLayout(LayoutKind.Explicit, Size = 8)]                    │
│    [FieldOffset(0)] byte* Ptr                                       │
│    [FieldOffset(0)] long Align8Union  ← forces 8-byte alignment    │
│                                                                     │
│  Validation:                                                        │
│    ValidateNotNull()  → throws if Ptr == null                       │
│    ValidateAllowNull() → skips null check                           │
│    Both check: Header->ValidationPtr == Ptr                         │
│      → catches use-after-free (ValidationPtr is set at creation)    │
├─────────────────────────────────────────────────────────────────────┤
│  ReferenceHeader (internal struct)                                  │
│    void* ValidationPtr  ← set to match Ptr at creation              │
│    int Length           ← total data length in bytes                │
└─────────────────────────────────────────────────────────────────────┘
```

## Memory Layout

```
Allocation (via MemoryAllocator):
┌─────────────────────────────────────────────────────────┐
│  ReferenceHeader          │  Data (T or raw bytes)      │
│  ┌──────────────────────┐ │                             │
│  │  void* ValidationPtr │ │                             │
│  │  int Length           │ │                             │
│  └──────────────────────┘ │                             │
│  sizeof(ReferenceHeader)   │  length bytes              │
└─────────────────────────────────────────────────────────┘
  ^                          ^
  buffer                     buffer + sizeof(ReferenceHeader)
                             = data.Ptr = data.Header->ValidationPtr
```

## Key Design Decisions

- **MemoryAllocator compatible**: Uses the library's own MemoryAllocator instead of Unity's
  NativeAllocation or BlobAssetReference system.
- **Use-after-free detection**: ReferenceHeader.ValidationPtr is set to match the data pointer
  at creation time. ValidateBurst/ValidateNonBurst check this still matches.
- **Explicit layout union**: ReferenceData uses FieldOffset(0) for both Ptr and Align8Union,
  creating a C-style union that guarantees 8-byte alignment.
- **No Dispose**: Unlike BlobAssetReference, Reference doesn't own its memory — the
  MemoryAllocator that created it handles cleanup via FreeAll.

## Verified Data

```
Reference<T>
  Kind: struct, generic (1 param)
  Properties: Boolean IsCreated, T& Value, ReferenceData ReferenceData
  Methods: Void* GetUnsafePtr, Equals (x2), GetHashCode
  Static Methods: Create (x5 overloads)

ReferenceData
  Kind: struct, 8 bytes
  Fields: Byte* Ptr, Int64 Align8Union (union for 8-byte alignment)
  [StructLayout(LayoutKind.Explicit, Size=8)]

Verified: 3 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Reference.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Reference.cs)
