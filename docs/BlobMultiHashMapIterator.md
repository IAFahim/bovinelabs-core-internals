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

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobMultiHashMapIterator.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobMultiHashMapIterator<TKey>
  Kind: struct, 8 bytes

  Fields:
    Int32 Key (private)
    Int32 NextIndex (private)

  Note: Used to iterate multi-value entries in BlobMultiHashMap.
  Pattern: TryGetFirstValue gives initial iterator, TryGetNextValue advances it.

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobMultiHashMapIterator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobMultiHashMapIterator.cs)
