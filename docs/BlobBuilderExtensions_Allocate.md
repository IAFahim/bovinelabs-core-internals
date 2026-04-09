# BlobBuilderExtensions.Allocate

## Inner Workings Diagram

```
 BlobBuilderExtensions.Allocate
 ======================================================================
 Defined as: BlobAllocation
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobAllocation                                                     │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    BlobBuilderExtensions                     │
 │ NativeList<BlobAllocat   Allocations                               │
 │ NativeList<OffsetPtrPa   Patches                                   │
 │ int                      CurrentChunkIndex                         │
 │ int                      ChunkSize                                 │
 │ int                      Size                                      │
 │ byte*                    P                                         │
 │ int                      AllocIndex                                │
 │ int                      Offset                                    │
 │ int*                     OffsetPtr                                 │
 │ BlobDataRef              Target                                    │
 │ int                      Length                                    │
 │ byte*                    P                                         │
 │ int                      Index                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Allocate(this ref BlobBuilder blobBuilder, int size)               │
 │   → void*                                                          │
 │ GetListPtr(this BlobBuilder builder)                               │
 │   → IntPtr                                                         │
 │ Allocate(int size, int alignment)                                  │
 │   → BlobDataRef                                                    │
 │ AllocationToPointer(BlobDataRef blobDataRef)                       │
 │   → void*                                                          │
 │ AllocateBlobAssetReference(ref BlobBuilder target, ref BlobPtr<BlobAs
 │   → void                                                           │
 │ CompareTo(SortedIndex other)                                       │
 │   → int                                                            │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobBuilderExtensions_Allocate.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobBuilderExtensions.Allocate (internals)

  BlobBuilderInternal (struct)
    BlobAllocation (struct)
      Int32 Size (public)
      Byte* P (public)
    BlobDataRef (struct)
      Int32 AllocIndex (public)
      Int32 Offset (public)
    OffsetPtrPatch (struct)
      Int32* OffsetPtr (public)
      BlobDataRef Target (public)
      Int32 Length (public)
    SortedIndex (struct)
      Byte* P (public)
      Int32 Index (public)

  Allocate overloads: 2
    Void* Allocate(BlobBuilder& blobBuilder, Int32 size)
    T* Allocate(BlobBuilder& blobBuilder, BlobPtr`1& ptr, Int32 size)
  GetListPtr: IntPtr GetListPtr(BlobBuilder builder)

Verified: 7 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs)
