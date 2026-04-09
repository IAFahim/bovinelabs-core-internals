# BlobBuilderHashMap

## Inner Workings Diagram

```
 BlobBuilderHashMap
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
 │ TryAdd(TKey key, TValue value)                                     │
 │   → bool                                                           │
 │ ContainsKey(TKey key)                                              │
 │   → bool                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/blob-system/BlobBuilderHashMap.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
> ```

```
BlobBuilderHashMap<TKey,TValue>
  Kind: struct, ref struct = True

  Properties:
    Int32 Capacity
    Int32 Count

  Methods:
    Void Add(Int32 key, Int32 item)
    Int32& AddUnique(Int32 key)
    Boolean TryAdd(Int32 key, Int32 value)
    Boolean ContainsKey(Int32 key)


Verified: 6 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/Blobs/HashMap/BlobBuilderHashMap.cs)
