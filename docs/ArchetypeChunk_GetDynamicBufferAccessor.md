# ArchetypeChunk.GetDynamicBufferAccessor

## Inner Workings Diagram

```
 ArchetypeChunk.GetDynamicBufferAccessor()
 ═══════════════════════════════════════════════════════════════════════
 Retrieves an UNTYPED buffer accessor from a chunk via DynamicComponentTypeHandle

 PURPOSE:
 ═══════
 Standard Unity ECS only provides typed BufferAccessor<T> via
 BufferTypeHandle<T>. This extension allows accessing buffers when the
 type is not known at compile time, using DynamicComponentTypeHandle.


 ┌──────────────────────────────────────────────────────────────────────┐
 │  GetDynamicBufferAccessor(                                          │
 │      this ArchetypeChunk chunk,                                     │
 │      ref DynamicComponentTypeHandle chunkBufferTypeHandle)          │
 │                                                                     │
 │  Returns: DynamicBufferAccessor (untyped)                           │
 └────────────┬─────────────────────────────────────────────────────────┘
              │
              ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Resolve type index in archetype                            │
 │  ┌────────────────────────────────────────────────────────────┐    │
 │  │  archetype = ECS->GetArchetype(chunk.m_Chunk)              │    │
 │  │                                                             │    │
 │  │  ChunkDataUtility.GetIndexInTypeArray(                      │    │
 │  │      archetype,                                             │    │
 │  │      handle.m_TypeIndex,                                    │    │
 │  │      ref typeIndexInArchetype)                              │    │
 │  │                                                             │    │
 │  │  handle.m_TypeLookupCache = typeIndexInArchetype            │    │
 │  └────────────────────────────────────────────────────────────┘    │
 │              │                                                      │
 │              ▼                                                      │
 │  Step 2: Validate type is actually a buffer                         │
 │  ┌────────────────────────────────────────────────────────────┐    │
 │  │  if (typeIndexInArchetype == -1)                            │    │
 │  │      → return default (empty accessor)                      │    │
 │  │                                                             │    │
 │  │  if (!archetype->Types[idx].IsBuffer)                       │    │
 │  │      → throw: "must be IBufferElementData"                  │    │
 │  └────────────────────────────────────────────────────────────┘    │
 │              │                                                      │
 │              ▼                                                      │
 │  Step 3: Get buffer data pointer (RO or RW)                        │
 │  ┌────────────────────────────────────────────────────────────┐    │
 │  │  ptr = handle.IsReadOnly                                   │    │
 │  │      ? ChunkDataUtility.GetComponentDataRO(...)             │    │
 │  │      : ChunkDataUtility.GetComponentDataRW(                 │    │
 │  │            ..., handle.GlobalSystemVersion)                 │    │
 │  └────────────────────────────────────────────────────────────┘    │
 │              │                                                      │
 │              ▼                                                      │
 │  Step 4: Read metadata from archetype                               │
 │  ┌────────────────────────────────────────────────────────────┐    │
 │  │  internalCapacity = archetype->BufferCapacities[idx]        │    │
 │  │  typeInfo = TypeManager.GetTypeInfo(handle.TypeIndex)       │    │
 │  │  elementSize  = typeInfo.ElementSize                        │    │
 │  │  elementAlign = typeInfo.AlignmentInBytes                   │    │
 │  │  stride = archetype->SizeOfs[idx]                           │    │
 │  │  length = chunk.Count                                       │    │
 │  └────────────────────────────────────────────────────────────┘    │
 │              │                                                      │
 │              ▼                                                      │
 │  Step 5: Construct DynamicBufferAccessor                            │
 │  ┌────────────────────────────────────────────────────────────┐    │
 │  │  return new DynamicBufferAccessor(                          │    │
 │  │      ptr, length, stride,                                  │    │
 │  │      elementSize, elementAlign,                             │    │
 │  │      internalCapacity)                                      │    │
 │  └────────────────────────────────────────────────────────────┘    │
 └─────────────────────────────────────────────────────────────────────┘


 CHUNK MEMORY — Buffer Headers Layout
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Chunk (16KB)                                                        │
 │  ┌───────────────────────────────────────────────────────────────┐  │
 │  │  Entity[]    Position[]    BufferHeader[] (e.g. MyBuffer)     │  │
 │  │  ┌──────┐   ┌──────────┐  ┌──────────────────────────────┐   │  │
 │  │  │ E0   │   │ P0       │  │ BH0: [ptr,len,capacity]      │   │  │
 │  │  │ E1   │   │ P1       │  │ BH1: [ptr,len,capacity]      │   │  │
 │  │  │ E2   │   │ P2       │  │ BH2: [ptr,len,capacity]      │   │  │
 │  │  └──────┘   └──────────┘  └──────────────────────────────┘   │  │
 │  │                                ↑                               │  │
 │  │  ptr points to first BH ───────┘                               │  │
 │  │  stride = sizeof(BufferHeader) in archetype                    │  │
 │  │  length = chunk.Count (number of entities)                     │  │
 │  └───────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 DynamicBufferAccessor STRUCT
 ┌──────────────────────────────────────────────────────────────┐
 │  byte* pointer      ─→ base of BufferHeader array in chunk   │
 │  int   Length       ─→ number of buffers (= entity count)    │
 │  int   stride       ─→ distance between consecutive headers  │
 │  int   ElementSize  ─→ sizeof one buffer element (T)         │
 │  int   ElementAlign ─→ alignment of buffer element           │
 │  int   internalCapacity ─→ inline buffer capacity             │
 │                                                              │
 │  Methods:                                                    │
 │  ┌───────────────────────────────────────────────────────┐   │
 │  │  GetBuffer<T>(index) → DynamicBuffer<T>               │   │
 │  │    header = (BufferHeader*)(pointer + index * stride)  │   │
 │  │    return new DynamicBuffer<T>(header, ...)            │   │
 │  │                                                        │   │
 │  │  GetUntypedBuffer(index) → UntypedDynamicBuffer        │   │
 │  │    same pointer math, returns untyped wrapper           │   │
 │  └───────────────────────────────────────────────────────┘   │
 └──────────────────────────────────────────────────────────────┘

 KEY DIFFERENCE FROM TYPED ACCESS:
 ══════════════════════════════════
 • Uses DynamicComponentTypeHandle (runtime-resolved type)
 • Returns DynamicBufferAccessor instead of BufferAccessor<T>
 • Carries element size/alignment for untyped element access
 • Supports both RO and RW via handle.IsReadOnly flag
 • Enables generic/system-driven buffer processing without generics
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/dynamic-buffers/ArchetypeChunk_GetDynamicBufferAccessor.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Extensions,BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
> ```

```
ArchetypeChunkExtensions
  Kind: static class

Methods:
  GetDynamicBufferAccessor(this ArchetypeChunk, ref DynamicComponentTypeHandle) -> DynamicBufferAccessor
    Param0: ArchetypeChunk
    Param1: DynamicComponentTypeHandle& (by-ref=True)

DynamicBufferAccessor
  Kind: struct
  Size: 64 bytes
  Fields:
    [0] Byte* pointer
    [8] Int32 internalCapacity
    [12] Int32 stride
    [16] AtomicSafetyHandle safety0
    [32] AtomicSafetyHandle arrayInvalidationSafety
    [48] Boolean isReadOnly
    [52] Int32 <Length>k__BackingField
    [56] Int32 <ElementSize>k__BackingField
    [60] Int32 <ElementAlign>k__BackingField
  Properties:
    Int32 Length (canRead=True, canWrite=False)
    Int32 ElementSize (canRead=True, canWrite=False)
    Int32 ElementAlign (canRead=True, canWrite=False)
  Methods:
    DynamicBuffer`1 GetBuffer(Int32)
    UntypedDynamicBuffer GetUntypedBuffer(Int32)
    Void CheckWriteAccess()
    Void AssertIndexInRange(Int32)

UnsafeUntypedDynamicBufferAccessor
  Kind: struct
  Size: 24 bytes
  Fields:
    [0] Byte* pointer
    [8] Int32 internalCapacity
    [12] Int32 stride
    [16] Int32 <Length>k__BackingField
    [20] Int32 <ElementSize>k__BackingField
  Properties:
    Int32 Length
    Int32 ElementSize
  Methods:
    UnsafeUntypedDynamicBuffer GetUntypedBuffer(Int32)
    Void AssertIndexInRange(Int32)

Verified: 9 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/ArchetypeChunkExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/ArchetypeChunkExtensions.cs)
- [BovineLabs.Core/Internal/ArchetypeChunkInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/ArchetypeChunkInternal.cs)
- [BovineLabs.Core.Editor/Inspectors/ArchetypeChunkInspector.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Inspectors/ArchetypeChunkInspector.cs)
