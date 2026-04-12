# CustomChunkIterator — v128 Bitmask Chunk Iteration

## Overview

CustomChunkIterator<T> provides efficient entity iteration within an ArchetypeChunk using v128
bitmask intrinsics. It has an adaptive strategy: simple linear loop when no enableable mask is
present, range-based iteration for sparse masks, and bit-by-bit iteration for dense masks.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ICustomChunkIterator (interface)                                   │
│    void Execute(int entityIndexInChunk)                             │
│                                                                     │
│  CustomChunkIterator<T>  where T : unmanaged, ICustomChunkIterator  │
│                                                                     │
│  Constructor: CustomChunkIterator(T execute)                        │
│                                                                     │
│  Execute(in ArchetypeChunk chunk, bool useEnabledMask,              │
│          in v128 chunkEnabledMask)                                  │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  Path 1: !useEnabledMask                                    │    │
│  │    Simple for loop 0..chunkEntityCount                      │    │
│  │    executor.Execute(i) for each                             │    │
│  ├─────────────────────────────────────────────────────────────┤    │
│  │  Path 2: useEnabledMask && edgeCount <= 4                   │    │
│  │    Use EnabledBitUtility.TryGetNextRange for ranges         │    │
│  │    Inner loop iterates within each enabled range            │    │
│  ├─────────────────────────────────────────────────────────────┤    │
│  │  Path 3: useEnabledMask && edgeCount > 4                    │    │
│  │    Bit-by-bit iteration over ULong0 (0..63)                 │    │
│  │    Then ULong1 (64..chunkEntityCount)                       │    │
│  │    if (mask64 & 1) != 0: Execute                            │    │
│  │    mask64 >>= 1                                             │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

## Adaptive Strategy

```
  Edge count = transitions between 0→1 and 1→0 in the bitmask
  ┌──────────────────────────────────────────────────────────────┐
  │  0111100001111111000011111111000000000011110000...          │
  │    ↑↑  ↑↑↑↑      ↑↑                                        │
  │    edges = transitions (count of rising/falling edges)       │
  │                                                              │
  │  edgeCount <= 4:                                             │
  │    → Few transitions = wide ranges enabled                   │
  │    → Use TryGetNextRange for efficient batch processing      │
  │                                                              │
  │  edgeCount > 4:                                              │
  │    → Many transitions = sparse/fragmented                    │
  │    → Bit-by-bit is actually faster (less range overhead)    │
  └──────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Adaptive threshold at 4 edges**: Empirically chosen cutoff — range iteration has setup
  overhead that only pays off when ranges are wide (few edges).
- **v128 = 128 bits**: Supports chunks up to 128 entities (Unity ECS standard).
- **Two ulong halves**: Processes ULong0 (indices 0-63) then ULong1 (64-127) separately to
  avoid branch complexity.
- **ICustomChunkIterator interface**: Users implement this interface on their own unmanaged struct
  with the per-entity Execute logic.

## Verified Data

```
ICustomChunkIterator
  Kind: interface
  Methods: Void Execute(int entityIndexInChunk)

CustomChunkIterator<T>
  Kind: struct, generic (1 param)

Verified: 1 checks, 0 failures
```


> Tested example: [Example/CustomChunkIteratorExample.cs](../Example/CustomChunkIteratorExample.cs)

## Source

- [BovineLabs.Core/Iterators/CustomChunkIterator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/CustomChunkIterator.cs)
