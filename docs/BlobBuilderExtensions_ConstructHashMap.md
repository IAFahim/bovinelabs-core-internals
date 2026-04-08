# BlobBuilderExtensions.ConstructHashMap

## Inner Workings Diagram

```
 BlobBuilderExtensions.ConstructHashMap
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

> [Run test snippet](../snippets/blob-system/BlobBuilderExtensions_ConstructHashMap.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs)
