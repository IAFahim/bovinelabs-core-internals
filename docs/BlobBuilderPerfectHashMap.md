# BlobBuilderPerfectHashMap

## Inner Workings Diagram

```
 BlobBuilderPerfectHashMap
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobBuilderPerfectHashMap.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobBuilderPerfectHashMap<TKey,TValue>
  Kind: struct, ref struct = True

  Constructors:
    BlobBuilderPerfectHashMap(BlobBuilder& builder, BlobPerfectHashMap`2& data, NativeHashMap`2 hashmap, Int32 nullValue)

  Properties:
    Int32& Item

  Fields (private):
    Int32 capacity
    BlobBuilderArray`1 values

  Indexer: this[Int32] -> Int32&

Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderPerfectHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderPerfectHashMap.cs)
