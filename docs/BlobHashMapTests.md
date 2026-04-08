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

> [Run test snippet](../snippets/blob-system/BlobHashMapTests.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core.Tests/Collections/Blobs/BlobHashMapTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/Blobs/BlobHashMapTests.cs)
