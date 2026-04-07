# CodecService — Inner Workings

## Overview

CodecService wraps native LZ4 compression/decompression via P/Invoke to `liblz4`,
operating directly on raw byte pointers and NativeArrays with zero managed overhead.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CodecService (static)                              │
│                                                                             │
│   ┌────────────────┐    ┌─────────────────┐    ┌──────────────────────────┐ │
│   │ Compress()     │    │ Decompress()    │    │ GetBoundedSize()        │ │
│   │                │    │                 │    │                          │ │
│   │ src → LZ4 → dst│    │ src → LZ4 → dst │    │ srcSize → upper bound  │ │
│   └───────┬────────┘    └───────┬─────────┘    └────────────┬────────────┘ │
│           │                     │                           │              │
└───────────┼─────────────────────┼───────────────────────────┼──────────────┘
            │                     │                           │
            ▼                     ▼                           ▼
  ┌─────────────────────────────────────────────────────────────────────┐
  │                      liblz4 (native shared lib)                      │
  │                                                                      │
  │   LZ4_compressBound()    LZ4_compress_default()    LZ4_decompress_  │
  │                                                   safe()            │
  └─────────────────────────────────────────────────────────────────────┘
```

## Compress Flow — With Allocation

```
  CodecService.Compress(LZ4, src, srcSize, out dst, allocator)
         │
         ▼
  ┌─────────────────────────────────────────────────────────┐
  │  1. boundedSize = GetBoundedSize(LZ4, srcSize)          │
  │     └─► LZ4_compressBound(srcSize)                      │
  │                                                         │
  │  2. dst = Memory.Unmanaged.Allocate(boundedSize, 16, alloc) │
  │     ┌──────────────────────────────────────────────┐    │
  │     │  Allocated buffer (boundedSize bytes)         │    │
  │     │  ████████████████████████████████████████████ │    │
  │     └──────────────────────────────────────────────┘    │
  │                                                         │
  │  3. compressedSize = Compress(LZ4, src, srcSize,        │
  │                                dst, boundedSize)         │
  │     └─► LZ4_compress_default(src, dst, srcSize, bound)  │
  │                                                         │
  │  4. if (compressedSize < 0):                            │
  │     └─► Free(dst), dst = null  // compression failed    │
  │                                                         │
  │  5. return compressedSize                               │
  └─────────────────────────────────────────────────────────┘
```

## Compress Flow — Pre-Allocated Buffer

```
  CodecService.Compress(LZ4, src, srcSize, dst, boundedSize)
         │
         ▼
  ┌──────────────────────────────────────────────────┐
  │                                                  │
  │  src buffer                  dst buffer           │
  │  ┌────────────────┐         ┌──────────────────┐ │
  │  │ ████████████████│ ──LZ4──►│ ████████████████ │ │
  │  │ uncompressed    │         │ compressed       │ │
  │  │ srcSize bytes   │         │ boundedSize cap  │ │
  │  └────────────────┘         └──────────────────┘ │
  │                                                  │
  │  Returns: actual compressed size (int)           │
  └──────────────────────────────────────────────────┘
```

## Decompress Flow

```
  CodecService.Decompress(LZ4, compressedData, compressedSize,
                          decompressedData, decompressedSize)
         │
         ▼
  ┌──────────────────────────────────────────────────────────────┐
  │                                                              │
  │  compressed buffer               decompressed buffer         │
  │  ┌──────────────┐                ┌──────────────────────┐   │
  │  │ ████ LZ4 ████│  ──inflate──►  │ ████████████████████  │   │
  │  │ compressedSize│                │ decompressedSize     │   │
  │  └──────────────┘                └──────────────────────┘   │
  │                                                              │
  │  LZ4_decompress_safe(src, dst, compressedSize, dstCapacity) │
  │                                                              │
  │  Returns: result > 0 ? true : false                         │
  │                                                              │
  │  NOTE: User must know decompressed size ahead of time!      │
  │  NOTE: Buffer too small → returns false (safe, no corrupt)  │
  └──────────────────────────────────────────────────────────────┘
```

## P/Invoke Signatures

```
  ┌──────────────────────────────────────────────────────────────────┐
  │  DllImport("liblz4")                                            │
  │                                                                  │
  │  [LZ4_compressBound]                                             │
  │    int CompressBoundLZ4(int srcSize)                             │
  │    └─► Worst-case output size for given input                    │
  │                                                                  │
  │  [LZ4_compress_default]                                          │
  │    int CompressLZ4(byte* src, byte* dst, int srcSize,            │
  │                     int dstCapacity)                              │
  │    └─► Compresses src into dst, returns compressed size          │
  │                                                                  │
  │  [LZ4_decompress_safe]                                           │
  │    int DecompressLZ4(byte* src, byte* dst, int compressedSize,   │
  │                       int dstCapacity)                            │
  │    └─► Decompresses safely; fails gracefully if dst too small    │
  └──────────────────────────────────────────────────────────────────┘
```

## Codec Enum

```
  enum Codec : byte
  ┌──────────────┐
  │  LZ4 = 0     │   Currently the only codec;
  │              │   switch/case architecture allows
  │              │   future codecs (Zstd, etc.)
  └──────────────┘
```

## Key Design Decisions

- **Raw pointer API**: Operates on `byte*` for maximum flexibility — can wrap
  NativeArrays, UnsafeLists, or any unmanaged memory.
- **16-byte alignment**: Allocated buffers use 16-byte alignment for SIMD-friendly access.
- **Safe decompression**: Uses `LZ4_decompress_safe` which validates bounds rather than
  `LZ4_decompress_fast` which can corrupt memory on mismatch.
- **Error handling**: Negative return from compression triggers immediate free+null;
  decompression returns a boolean for easy flow control.

## Source File

- `BovineLabs.Core/Utility/CodecService.cs`

## Source

- [BovineLabs.Core/Utility/CodecService.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/CodecService.cs)
