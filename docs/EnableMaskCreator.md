# EnableMaskCreator - Inner Workings

## Overview

`EnableMaskCreator` is a static utility class that provides a factory method for constructing `EnabledMask` structs from raw pointers. It enables custom iteration over enableable components by manually building enable masks, bypassing Unity's normal query-based enable filtering.

```
File: BovineLabs.Core/Utility/EnableMaskCreator.cs
Class: EnableMaskCreator (static, unsafe)
Method: Create(SafeBitRef, int*) → EnabledMask
```

## Architecture

```
  ┌────────────────────────────────────────────────────────────────┐
  │              EnableMaskCreator (static, unsafe)                │
  │                                                                │
  │  ┌──────────────────────────────────────────────────────────┐ │
  │  │  Create(                                                 │ │
  │  │      SafeBitRef enableBitRef,                            │ │
  │  │      int* ptrChunkDisabledCount                          │ │
  │  │  ) → EnabledMask                                        │ │
  │  │                                                          │ │
  │  │  Returns: new EnabledMask(enableBitRef, ptrChunkDisabledCount) │
  │  └──────────────────────────────────────────────────────────┘ │
  └────────────────────────────────────────────────────────────────┘
```

## EnabledMask Construction

```
  EnabledMask Components:
  ┌────────────────────────────────────────────────────────────────┐
  │                                                                │
  │  ┌──────────────────────────────────────────────────────────┐ │
  │  │  SafeBitRef enableBitRef                                  │ │
  │  │  ──────────────────────────                               │ │
  │  │  A reference to a specific bit within a chunk's           │ │
  │  │  enable bit array. Points to the enable bits for one      │ │
  │  │  specific component type in the chunk.                    │ │
  │  │                                                           │ │
  │  │  Internal structure:                                      │ │
  │  │  ┌────────────────────────────────────────────────────┐  │ │
  │  │  │  ptr: void*  → points to the ulong array            │  │ │
  │  │  │  offset: int → bit offset within the array          │  │ │
  │  │  └────────────────────────────────────────────────────┘  │ │
  │  └──────────────────────────────────────────────────────────┘ │
  │                                                                │
  │  ┌──────────────────────────────────────────────────────────┐ │
  │  │  int* ptrChunkDisabledCount                               │ │
  │  │  ──────────────────────────────────                       │ │
  │  │  Pointer to the chunk's disabled entity counter.          │ │
  │  │  This counter tracks how many entities in the chunk       │ │
  │  │  have this component disabled.                            │ │
  │  │                                                           │ │
  │  │  *ptrChunkDisabledCount = number of disabled entities     │ │
  │  │  chunk.Count - *ptrChunkDisabledCount = enabled count     │ │
  │  └──────────────────────────────────────────────────────────┘ │
  │                                                                │
  │  ┌──────────────────────────────────────────────────────────┐ │
  │  │  EnabledMask                                              │ │
  │  │  ──────────                                               │ │
  │  │  Combines SafeBitRef + ptrChunkDisabledCount into one     │ │
  │  │  struct that can be used with chunk iteration APIs        │ │
  │  │  to iterate only over enabled entities.                   │ │
  │  └──────────────────────────────────────────────────────────┘ │
  └────────────────────────────────────────────────────────────────┘
```

## Chunk Enable Bit Layout

```
  ArchetypeChunk internal layout:
  ┌─────────────────────────────────────────────────────────────────┐
  │                                                                 │
  │  Entity Data:                                                   │
  │  ┌──────────┬──────────┬──────────┬──────────┬─────┐           │
  │  │ Entity 0 │ Entity 1 │ Entity 2 │ Entity 3 │ ... │           │
  │  │ Pos, Vel │ Pos, Vel │ Pos, Vel │ Pos, Vel │     │           │
  │  └──────────┴──────────┴──────────┴──────────┴─────┘           │
  │                                                                 │
  │  Enable Bits (per component type):                              │
  │  ┌──────────────────────────────────────────────────────────┐  │
  │  │  ComponentA enable bits:                                  │  │
  │  │  Byte 0: [1 1 0 1 1 0 1 1 ... ] (up to 64 entities)    │  │
  │  │  Byte 1: [0 1 1 0 1 1 0 0 ... ] (entities 64-127)      │  │
  │  │  ^                                                        │  │
  │  │  │                                                        │  │
  │  │  SafeBitRef points here                                   │  │
  │  │  (offset tells which component's enable bits)             │  │
  │  └──────────────────────────────────────────────────────────┘  │
  │                                                                 │
  │  Disabled Count (per component):                                │
  │  ┌──────────────────────────────────────────────────────────┐  │
  │  │  ComponentA disabled count: int = 3                       │  │
  │  │  (3 entities have ComponentA disabled)                    │  │
  │  │  ptrChunkDisabledCount points here                        │  │
  │  └──────────────────────────────────────────────────────────┘  │
  └─────────────────────────────────────────────────────────────────┘
```

## Why This Exists

```
  Normal Unity ECS flow:
  ┌───────────────────────────────────────────────────────────────┐
  │  EntityQuery.WithPresent<T>() → only iterates enabled entities│
  │  The query system automatically provides EnabledMask to       │
  │  IJobChunk.Execute() via useEnabledMask + chunkEnabledMask   │
  │                                                               │
  │  This works great for standard queries.                       │
  └───────────────────────────────────────────────────────────────┘

  Problem cases:
  ┌───────────────────────────────────────────────────────────────┐
  │  1. Custom iteration outside of IJobChunk                     │
  │  2. Need to iterate using a manually-constructed enable mask  │
  │  3. Combining enable bits from multiple sources               │
  │  4. Testing/debugging enable states                           │
  │                                                               │
  │  EnableMaskCreator allows manual construction of EnabledMask  │
  │  when you can't or don't want to use the standard query path  │
  └───────────────────────────────────────────────────────────────┘
```

## Usage Pattern

```
  // Acquiring enable bit reference for a component in a chunk:
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  // Get the chunk's enable bits for component T               │
  │  ref readonly var bits = ref chunk.GetRequiredEnabledBitsRO(  │
  │      ref componentTypeHandle);                                │
  │                                                               │
  │  // Get the disabled count pointer                            │
  │  ref var enabledBits = ref chunk.GetRequiredEnabledBitsRW(    │
  │      ref componentTypeHandle, out var disabledCount);         │
  │                                                               │
  │  // Create SafeBitRef from the bits                           │
  │  SafeBitRef bitRef = ...; // acquired from chunk internals    │
  │                                                               │
  │  // Manually construct EnabledMask                            │
  │  var mask = EnableMaskCreator.Create(bitRef, disabledCount);  │
  │                                                               │
  │  // Use the mask for custom iteration                         │
  │  // (iterate only enabled entities)                           │
  └───────────────────────────────────────────────────────────────┘
```

## Integration with IJobChunk

```
  Standard IJobChunk.Execute() receives enabled mask automatically:
  ┌───────────────────────────────────────────────────────────────┐
  │  void Execute(                                                │
  │      in ArchetypeChunk chunk,                                 │
  │      int unfilteredChunkIndex,                                │
  │      bool useEnabledMask,        ◄── true if any disabled    │
  │      in v128 chunkEnabledMask    ◄── auto-provided mask      │
  │  )                                                            │
  └───────────────────────────────────────────────────────────────┘

  EnableMaskCreator allows constructing this mask manually:
  ┌───────────────────────────────────────────────────────────────┐
  │  var customMask = EnableMaskCreator.Create(                   │
  │      myBitRef,                                                │
  │      myDisabledCountPtr                                       │
  │  );                                                           │
  │                                                               │
  │  // Now use customMask to iterate only over specific          │
  │  // enabled entities in a custom loop                         │
  └───────────────────────────────────────────────────────────────┘
```

## SafeBitRef Detail

```
  ┌───────────────────────────────────────────────────────────────┐
  │  SafeBitRef (Unity.Collections.LowLevel.Unsafe)               │
  │                                                               │
  │  Represents a reference to a single bit in memory:            │
  │                                                               │
  │  ┌─────────────────────────────────────────────────────────┐ │
  │  │  void* Ptr   → pointer to the bit array (ulong*)        │ │
  │  │  int Offset  → bit position within the array            │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                                                               │
  │  To read bit N:                                               │
  │    word = (Offset + N) / 64                                   │
  │    bit  = (Offset + N) % 64                                   │
  │    value = ((ulong*)Ptr)[word] >> bit & 1                     │
  └───────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

1. **Unsafe Static Class**: Marked `unsafe` because it works with raw pointers (`int*`). This is low-level infrastructure not meant for typical game code.

2. **Factory Pattern**: Single `Create` method that wraps the `EnabledMask` constructor. This exists because `EnabledMask`'s constructor may not be publicly accessible, or to provide a controlled access point.

3. **Pointer Semantics**: The `int* ptrChunkDisabledCount` parameter is a raw pointer to the chunk's internal disabled count, enabling direct modification of the chunk's enable state tracking.

4. **Bypass Query System**: Allows constructing enable masks without going through `EntityQuery`, useful for:
   - Custom iteration strategies
   - Combining multiple enable sources
   - Direct chunk manipulation in jobs

5. **Minimal Implementation**: The method is a single `new EnabledMask(...)` call. The value is in exposing this capability, not in complex logic.

6. **Integration Point**: Works alongside `GetRequiredEnabledBitsRW` and `GetRequiredEnabledBitsRO` extension methods to provide full manual control over enableable component iteration.

## Verified Data

```
that: TYPE NOT FOUND
EnabledMask: TYPE NOT FOUND
Verified: 0 checks, 2 failures
```

## Source

- [BovineLabs.Core/Utility/EnableMaskCreator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/EnableMaskCreator.cs)
