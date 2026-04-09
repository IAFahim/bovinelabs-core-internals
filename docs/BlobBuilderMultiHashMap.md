# BlobBuilderMultiHashMap

## Inner Workings Diagram

```
 BlobBuilderMultiHashMap
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ int                      Capacity                                  │
 │ int                      Count                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Add(TKey key, TValue item)                                         │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobBuilderMultiHashMap.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobBuilderMultiHashMap<TKey,TValue>
  Kind: struct, ref struct = True

  Properties:
    Int32 Capacity
    Int32 Count

  Methods:
    Void Add(Int32 key, Int32 item)
    ref Int32& Add(Int32 key)

  Note: Add(TKey) returns ref Int32&
  (allows multiple values per key — the multi-map pattern)

Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderMultiHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderMultiHashMap.cs)
