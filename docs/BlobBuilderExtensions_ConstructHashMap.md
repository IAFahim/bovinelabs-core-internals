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

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobBuilderExtensions_ConstructHashMap.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobBuilderExtensions — HashMap Construction Methods

  ConstructHashMap: 2 overload(s)
    Void ConstructHashMap(BlobBuilder& builder, BlobHashMap`2& blobHashMap, NativeParallelHashMap`2& source)
    Void ConstructHashMap(BlobBuilder& builder, BlobHashMap`2& blobHashMap, Dictionary`2 source)
  AllocateHashMap: 2 overload(s)
    BlobBuilderHashMap`2 AllocateHashMap(BlobBuilder& builder, BlobHashMap`2& blobHashMap, Int32 capacity)
    BlobBuilderHashMap`2 AllocateHashMap(BlobBuilder& builder, BlobHashMap`2& blobHashMap, Int32 capacity, Int32 bucketCapacityRatio)
  ConstructMultiHashMap: 1 overload(s)
    Void ConstructMultiHashMap(BlobBuilder& builder, BlobMultiHashMap`2& blobMultiHashMap, NativeParallelMultiHashMap`2& source)
  AllocateMultiHashMap: 2 overload(s)
    BlobBuilderMultiHashMap`2 AllocateMultiHashMap(BlobBuilder& builder, BlobMultiHashMap`2& blobMultiHashMap, Int32 capacity)
    BlobBuilderMultiHashMap`2 AllocateMultiHashMap(BlobBuilder& builder, BlobMultiHashMap`2& blobMultiHashMap, Int32 capacity, Int32 bucketCapacityRatio)
  ConstructPerfectHashMap: 1 overload(s)
    BlobBuilderPerfectHashMap`2 ConstructPerfectHashMap(BlobBuilder& builder, BlobPerfectHashMap`2& blobHashMap, NativeHashMap`2 source, TValue nullValue)
  AllocatePerfectHashMap: 0 overload(s)

  AllocateHashMap returns: BlobBuilderHashMap`2
  AllocateMultiHashMap returns: BlobBuilderMultiHashMap`2

Verified: 7 checks, 1 failures
```

> **Note**: 1 failure = AllocatePerfectHashMap has 0 overloads (method does not
> exist as a separate API; perfect hash maps use ConstructPerfectHashMap only).

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs)
