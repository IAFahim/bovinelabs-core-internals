# BlobMultiHashMapIterator

## Inner Workings Diagram

```
 BlobMultiHashMapIterator
 ======================================================================
 Defined as: BlobMultiHashMapIterator
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BlobMultiHashMapIterator                                           │
 ├────────────────────────────────────────────────────────────────────┤
 │ TKey                     Key                                       │
 │ int                      NextIndex                                 │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/blob-system/BlobMultiHashMapIterator.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobMultiHashMapIterator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobMultiHashMapIterator.cs)
