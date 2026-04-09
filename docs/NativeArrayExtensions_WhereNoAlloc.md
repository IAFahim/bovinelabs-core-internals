# NativeArrayExtensions.WhereNoAlloc

## Inner Workings Diagram

```
 NativeArrayExtensions.WhereNoAlloc
 ======================================================================
 Defined as: IPredicate
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Equals                                                             │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    NativeArrayExtensions                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Check(T other)                                                     │
 │   → bool                                                           │
 │ Min(this NativeArray<int> collection)                              │
 │   → int                                                            │
 │ Min(this NativeArray<float> collection)                            │
 │   → float                                                          │
 │ Max(this NativeArray<int> collection)                              │
 │   → int                                                            │
 │ Max(this NativeArray<float> collection)                            │
 │   → float                                                          │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/memory-allocators/NativeArrayExtensions_WhereNoAlloc.cs)
> ```bash
> cat snippets/memory-allocators/NativeArrayExtensions_WhereNoAlloc.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
> ```

```
BovineLabs.Core.Extensions.NativeArrayExtensions
  Kind: static class (Abstract=True, Sealed=True)

  Nested Interface: IPredicate<T>
    IsInterface: True
    Method: Boolean Check(Int32)

  Methods (public static):
    Boolean All<T,TPredicate>(NativeArray`1 collection, TPredicate predicate)
    Boolean Any<T,TPredicate>(NativeArray`1& collection, TPredicate& predicate)
    Void Clear<T>(NativeArray`1 array)
    NativeArray`1 Clone<T>(NativeArray`1 array, Allocator allocator)
    T& ElementAt<T>(NativeArray`1 array, Int32 index)
    Ptr`1 ElementAtAsPtr<T>(NativeArray`1 array, Int32 index)
    T& ElementAtRO<T>(NativeArray`1 array, Int32 index)
    T& ElementAtRO<T>(ReadOnly array, Int32 index)
    T& ElementAtROUnsafe<T>(NativeArray`1 array, Int32 index)
    T& ElementAtROUnsafe<T>(ReadOnly array, Int32 index)
    Void Fill<T>(NativeArray`1 array, T value)
    T FirstOrDefault<T,TPredicate>(NativeArray`1 collection, TPredicate predicate)
    Int32 IndexOf<T,TPredicate>(NativeArray`1 collection, TPredicate predicate)
    Int32 Max(NativeArray`1 collection)
    Single Max(NativeArray`1 collection)
    Int32 Min(NativeArray`1 collection)
    Single Min(NativeArray`1 collection)
    Void Reverse<T>(NativeArray`1 array)
    NativeArray`1 Select<TInput,TOutput,TSelector>(NativeArray`1 array, TSelector selector, Allocator allocator)
    NativeArray`1 Where<T,TPredicate>(NativeArray`1 array, TPredicate predicate, Allocator allocator)
    NativeArray`1 WhereNoAlloc<T,TPredicate>(NativeArray`1 array, TPredicate predicate)

  Functional Tests:
    WhereNoAlloc [10,20,30,40,50,60] with Equals(30):
      Result Length: 1
      Result[0]: 30
    Min/Max on [5,2,8,1]:
      Min: 1
      Max: 8

Verified: 26 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeArrayExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeArrayExtensions.cs)
