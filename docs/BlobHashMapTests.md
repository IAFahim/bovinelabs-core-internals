# BlobHashMapTests

## Inner Workings Diagram

```
 BlobHashMapTests
 ======================================================================
 Defined as: BlobHashMapTests
 Namespace:  BovineLabs.Core.Tests.Collections.Blobs

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobHashMapTests                                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ BlobArray<int>>          HashMap                                   │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ NestedBlobs()                                                      │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobHashMapTests.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobHashMapTests - Dependency Verification

  BlobHashMap<int,int>
    Kind: struct, 44 bytes
    Fields: BlobHashMapData`2 Data
    Methods: Boolean TryGetValue, Boolean ContainsKey, BlobHashMapEnumerator`2 GetEnumerator
  BlobAssetReference<BlobHashMap<int,int>>: 8 bytes

Verified: 3 checks, 0 failures
```

## Source

- [BovineLabs.Core.Tests/Collections/Blobs/BlobHashMapTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/Blobs/BlobHashMapTests.cs)
