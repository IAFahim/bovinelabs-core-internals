BlobBuilderExtensions - Simplifies Allocating Complex Collections Inside Blob Builders
========================================================================================

Overview
--------

BlobBuilderExtensions is a static utility class that provides high-level
extension methods on Unity's BlobBuilder.  It hides the complexity of
allocating nested blob data structures (hash maps, multi-hash maps, perfect
hash maps, arrays) and provides convenient "Construct" methods that copy
data from managed and native collections into blob storage in a single call.

Additionally, it exposes internal BlobBuilder mechanics via unsafe reflection
(BlobBuilderInternal) to enable raw memory allocation and pointer patching
for advanced scenarios.

Architecture Diagram
--------------------

  ┌─────────────────────────────────────────────────────────────────────┐
  │                     BlobBuilderExtensions                           │
  │                     (public static class)                           │
  │                                                                     │
  │  ┌─ Low-Level Allocation ────────────────────────────────────────┐  │
  │  │                                                                │  │
  │  │  Allocate(ref BlobBuilder, int size)                           │  │
  │  │    → raw byte* allocation via BlobBuilderInternal              │  │
  │  │                                                                │  │
  │  │  Allocate<T>(ref BlobBuilder, ref BlobPtr<T>, int size)       │  │
  │  │    → typed pointer allocation with patch registration         │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  │                                                                     │
  │  ┌─ Array Construction ──────────────────────────────────────────┐  │
  │  │                                                                │  │
  │  │  Construct<T>(ref builder, ref BlobArray<T>, NativeArray<T>)  │  │
  │  │  Construct<T>(ref builder, ref BlobArray<T>, NativeList<T>)   │  │
  │  │    → allocate + bulk MemCpy from source                       │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  │                                                                     │
  │  ┌─ HashMap Construction ────────────────────────────────────────┐  │
  │  │                                                                │  │
  │  │  ConstructHashMap<TKey,TValue>(                                │  │
  │  │      ref builder, ref BlobHashMap,                             │  │
  │  │      ref NativeParallelHashMap)                                │  │
  │  │                                                                │  │
  │  │  ConstructHashMap<TKey,TValue>(                                │  │
  │  │      ref builder, ref BlobHashMap,                             │  │
  │  │      Dictionary<TKey,TValue>)                                  │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  │                                                                     │
  │  ┌─ HashMap Allocation (manual fill) ────────────────────────────┐  │
  │  │                                                                │  │
  │  │  AllocateHashMap<TKey,TValue>(                                 │  │
  │  │      ref builder, ref BlobHashMap, int capacity)               │  │
  │  │    → returns BlobBuilderHashMap for manual Add() calls         │  │
  │  │                                                                │  │
  │  │  AllocateHashMap<TKey,TValue>(                                 │  │
  │  │      ref builder, ref BlobHashMap,                             │  │
  │  │      int capacity, int bucketCapacityRatio)                    │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  │                                                                     │
  │  ┌─ MultiHashMap Construction ───────────────────────────────────┐  │
  │  │                                                                │  │
  │  │  ConstructMultiHashMap<TKey,TValue>(                           │  │
  │  │      ref builder, ref BlobMultiHashMap,                        │  │
  │  │      ref NativeParallelMultiHashMap)                           │  │
  │  │                                                                │  │
  │  │  AllocateMultiHashMap<TKey,TValue>(                            │  │
  │  │      ref builder, ref BlobMultiHashMap, int capacity)          │  │
  │  │    → returns BlobBuilderMultiHashMap                           │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  │                                                                     │
  │  ┌─ PerfectHashMap Construction ─────────────────────────────────┐  │
  │  │                                                                │  │
  │  │  ConstructPerfectHashMap<TKey,TValue>(                         │  │
  │  │      ref builder, ref BlobPerfectHashMap,                      │  │
  │  │      NativeHashMap, TValue nullValue)                          │  │
  │  │    → returns BlobBuilderPerfectHashMap                         │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  │                                                                     │
  │  ┌─ Internal Access ─────────────────────────────────────────────┐  │
  │  │                                                                │  │
  │  │  GetListPtr(BlobBuilder) → IntPtr                              │  │
  │  │    → exposes internal allocation list for advanced use         │  │
  │  └────────────────────────────────────────────────────────────────┘  │
  └─────────────────────────────────────────────────────────────────────┘


API Call Flow Diagrams
-----------------------

1. ConstructHashMap (from NativeParallelHashMap)
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

  ┌─────────────────────────────────────┐
  │ NativeParallelHashMap<TKey, TValue> │
  │  Count = N                          │
  └──────────────┬──────────────────────┘
                 │
                 ▼
  ┌──────────────────────────────────────────────────────┐
  │ ConstructHashMap(ref builder, ref blobHashMap, src)  │
  │                                                      │
  │  count = source.Count()                              │
  │         │                                            │
  │         ▼                                            │
  │  AllocateHashMap(ref builder, ref blobHashMap,       │
  │                  capacity: count)                    │
  │         │                                            │
  │         ▼                                            │
  │  ┌──────────────────────────────────────────────┐   │
  │  │  Returns: BlobBuilderHashMap<TKey,TValue>     │   │
  │  │                                               │   │
  │  │  Internally creates BlobBuilderHashMapData:   │   │
  │  │    ratio = (count <= 16384) ? 3 : 2           │   │
  │  │    bucketCap = ceilpow2(count × ratio)        │   │
  │  │    Allocate Values[count]                     │   │
  │  │    Allocate Keys[count]                       │   │
  │  │    Allocate Next[count]                       │   │
  │  │    Allocate Buckets[bucketCap]                │   │
  │  │    Allocate Count[1]                          │   │
  │  │    Clear buckets→-1, next→-1                  │   │
  │  └──────────────────────────────────────────────┘   │
  │         │                                            │
  │         ▼                                            │
  │  foreach (k, v) in source:                          │
  │    hashMapBuilder.Add(k, v)                          │
  │    └─ TryAdd: hash bucket, insert at count++         │
  └──────────────────────────────────────────────────────┘
                 │
                 ▼  (after builder.CreateBlobAssetReference)
         BlobAssetReference<BlobHashMap<TKey,TValue>>


2. Construct (NativeArray → BlobArray)
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

  ┌─────────────────────────┐
  │  NativeArray<T> src     │
  │  [a, b, c, d, e]        │
  └────────────┬────────────┘
               │
               ▼
  ┌────────────────────────────────────────────────────┐
  │  Construct<T>(ref builder, ref BlobArray<T> dest,  │
  │               in NativeArray<T> src)               │
  │                                                    │
  │  blobArr = builder.Allocate(ref dest, src.Length)  │
  │                                                    │
  │  dst = &blobArr[0]                                 │
  │  srcPtr = src.GetUnsafeReadOnlyPtr()               │
  │  bytes = src.Length × sizeof(T)                    │
  │                                                    │
  │  UnsafeUtility.MemCpy(dst, srcPtr, bytes)          │
  │    ┌──────────────────────────────────────┐        │
  │    │ Bulk copy, single memcpy call        │        │
  │    │ No per-element copy loop             │        │
  │    └──────────────────────────────────────┘        │
  └────────────────────────────────────────────────────┘


3. ConstructPerfectHashMap
~~~~~~~~~~~~~~~~~~~~~~~~~~~

  ┌────────────────────────────┐
  │  NativeHashMap<TKey,TValue>│
  │  + nullValue (sentinel)    │
  └────────────┬───────────────┘
               │
               ▼
  ┌──────────────────────────────────────────────────┐
  │  ConstructPerfectHashMap(ref builder,             │
  │      ref BlobPerfectHashMap, source, nullValue)   │
  │                                                   │
  │  → new BlobBuilderPerfectHashMap(                 │
  │       ref builder, ref data, source, nullValue)   │
  │                                                   │
  │  Internally:                                      │
  │    1. Assert unique hash codes                    │
  │    2. Find collision-free power-of-2 size         │
  │    3. Allocate Values[Capacity]                   │
  │    4. MemCpyReplicate nullValue into all slots    │
  │    5. For each entry: Values[hash & (Cap-1)] = v  │
  └──────────────────────────────────────────────────┘


Bucket Capacity Ratio Logic
----------------------------

  ┌─────────────────────────────────────────────────────────┐
  │  Adaptive Ratio Selection                               │
  │                                                         │
  │  const UseBucketCapacityRatioOfThreeUpTo = 16384        │
  │                                                         │
  │  if capacity <= 16384:                                  │
  │    ratio = 3   ┌──────────────────────────────────┐     │
  │               │ More buckets = fewer collisions   │     │
  │               │ Memory cost is small at low counts │     │
  │               └──────────────────────────────────┘     │
  │  else:                                                  │
  │    ratio = 2   ┌──────────────────────────────────┐     │
  │               │ Less overhead for large maps      │     │
  │               │ Still good hash distribution      │     │
  │               └──────────────────────────────────┘     │
  │                                                         │
  │  bucketCapacity = ceilpow2(capacity × ratio)            │
  │  BucketCapacityMask = bucketCapacity - 1                │
  │                                                         │
  │  Example: capacity=100, ratio=3                         │
  │    bucketCapacity = ceilpow2(300) = 512                 │
  │    512 buckets for 100 entries → ~5× over-provisioned   │
  │    But: near-zero collision probability                 │
  └─────────────────────────────────────────────────────────┘


BlobBuilderInternal (Unsafe Internals)
---------------------------------------

  The class contains a private struct `BlobBuilderInternal` that mirrors
  Unity's internal BlobBuilder layout, enabling low-level operations:

  ┌─────────────────────────────────────────────────────────────┐
  │  BlobBuilderInternal  (private, accessed via Unsafe.As)     │
  │                                                             │
  │  Fields:                                                    │
  │    AllocatorHandle Allocator                                │
  │    NativeList<BlobAllocation> Allocations                   │
  │    NativeList<OffsetPtrPatch> Patches                       │
  │    int CurrentChunkIndex                                    │
  │    int ChunkSize                                            │
  │                                                             │
  │  ┌─ BlobAllocation ──────────────────────────────────────┐  │
  │  │  int Size;    // current used size in this chunk       │  │
  │  │  byte* P;     // pointer to chunk memory               │  │
  │  └────────────────────────────────────────────────────────┘  │
  │                                                             │
  │  ┌─ OffsetPtrPatch ──────────────────────────────────────┐  │
  │  │  int* OffsetPtr;   // where to write the offset        │  │
  │  │  BlobDataRef Target; // where the offset points to     │  │
  │  │  int Length;        // 0 for BlobPtr, N for BlobArray   │  │
  │  └────────────────────────────────────────────────────────┘  │
  │                                                             │
  │  ┌─ BlobDataRef ─────────────────────────────────────────┐  │
  │  │  int AllocIndex;  // which allocation chunk             │  │
  │  │  int Offset;      // offset within that chunk          │  │
  │  └────────────────────────────────────────────────────────┘  │
  │                                                             │
  │  Allocate(size, alignment):                                 │
  │    if size > ChunkSize → separate allocation                │
  │    else → append to current chunk (with alignment padding)  │
  │                                                             │
  │  AllocateBlobAssetReference:                                │
  │    1. Align all chunks to 16 bytes                          │
  │    2. Compute running offsets for each chunk                │
  │    3. Sort chunks + patches by pointer address              │
  │    4. Allocate final contiguous buffer                      │
  │    5. MemCpy all chunks into final buffer                   │
  │    6. Apply patches: write relative offsets                 │
  │       *(int*)(data + offsetPtrLoc) = targetLoc - offsetPtrLoc │
  │    7. Write BlobAssetHeader (hash, length, allocator)       │
  └─────────────────────────────────────────────────────────────┘


Patch Resolution Detail
------------------------

  During construction, offsets are stored as patches to be resolved when
  the final blob is assembled:

  Builder chunks (scattered memory):     Final blob (contiguous):
  ┌─────────┐ ┌──────────┐              ┌──────────────────────────┐
  │ Chunk 0  │ │ Chunk 1   │             │ BlobAssetHeader          │
  │ [root]   │ │ [arrays]  │             │ ┌──────────────────────┐ │
  │          │ │           │             │ │ Chunk 0 data         │ │
  │ BlobPtr  │ │           │   ────────▶ │ │ BlobArray offsets    │ │
  │ offset=? │ │           │             │ │ ... patched to       │ │
  │          │ │           │             │ │ relative offsets     │ │
  │          │ │ BlobArray │             │ │ ──────────────────── │ │
  │          │ │ data here │             │ │ Chunk 1 data         │ │
  └─────────┘ └──────────┘              │ └──────────────────────┘ │
                                         └──────────────────────────┘

  Patch formula:
    *(int*)(data + patchOffsetLoc) = targetOffsetLoc - patchOffsetLoc

  For BlobPtr<T>:  patch.Length = 0, writes just the offset
  For BlobArray<T>: patch.Length = N, writes offset AND length


Key Design Decisions
--------------------

1. **Static extension methods on BlobBuilder.**  Rather than subclassing or
   wrapping BlobBuilder, all functionality is exposed as extension methods.
   This allows chaining with Unity's built-in BlobBuilder API.

2. **Overloaded Allocate vs Construct pattern.**  "Allocate" methods return
   a builder for manual population.  "Construct" methods copy from an
   existing source in one call.  This gives users flexibility: simple cases
   use Construct, complex cases use Allocate + manual Add.

3. **Dictionary support for editor-time construction.**  The
   ConstructHashMap overload accepting `Dictionary<TKey,TValue>` enables
   baking from managed code without first converting to a NativeHashMap.

4. **Unsafe.As to access BlobBuilder internals.**  The BlobBuilderInternal
   struct is a layout-compatible mirror of Unity's internal type, accessed
   via `Unsafe.As<BlobBuilder, BlobBuilderInternal>`.  This is a deliberate
   internal-API dependency that could break between Unity versions.

5. **Adaptive bucket ratio threshold at 16384.**  The threshold is described
   as "somewhat arbitrary but tests have shown" better performance with ratio
   3 for small maps.  Above 16384 entries, ratio 2 reduces memory waste.

6. **Bulk memcpy for array construction.**  The `Construct<T>` methods use
   `UnsafeUtility.MemCpy` instead of per-element copying, which is
   significantly faster for large arrays.


Performance Characteristics
---------------------------

| Method                     | Complexity  | Notes                          |
|----------------------------|-------------|--------------------------------|
| Construct (NativeArray)    | O(n)        | Single memcpy                  |
| Construct (NativeList)     | O(n)        | Wraps AsArray + memcpy         |
| ConstructHashMap           | O(n)        | Per-element Add into builder   |
| AllocateHashMap            | O(1)        | Just allocates arrays          |
| ConstructPerfectHashMap    | O(n × k)    | k = doublings until collision  |
|                            |             | free; typically k ≤ 4          |
| Allocate (raw bytes)       | O(1)        | Chunk append or new chunk      |
| GetListPtr                 | O(1)        | Direct field access via Unsafe |

Memory overhead: None beyond what the underlying blob structures require.
All methods allocate exactly the capacity requested (plus bucket over-provision
for hash maps).

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobBuilderExtensions.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderExtensions.cs)
