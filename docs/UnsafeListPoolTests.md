# UnsafeListPoolTests

## Inner Workings Diagram

```
 UnsafeListPoolTests
 ======================================================================
 Defined as: UnsafeListPoolTests
 Namespace:  BovineLabs.Core.Tests.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ UnsafeListPoolTests                                                │
 ├────────────────────────────────────────────────────────────────────┤
 │ UnsafeListPool<int>      Pool                                      │
 │ NativeArray<int>         Seen                                      │
 │ NativeArray<int>         Failures                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ TryAdd_ParallelProducers_ReturnsEachValueExactlyOnce()             │
 │   → void                                                           │
 │ Dispose_WhenPoolContainsLists_DisposesReturnedLists()              │
 │   → void                                                           │
 │ GetOrCreate_WhenPoolIsEmpty_ReturnsValidList()                     │
 │   → void                                                           │
 │ ReturnOrDispose_WhenPoolIsFull_DisposesList()                      │
 │   → void                                                           │
 │ TryGet_ParallelConsumers_ReturnsEachListExactlyOnce()              │
 │   → void                                                           │
 │ TryGet_BurstIJobFor_ParallelConsumers_ReturnEachListExactlyOnce()  │
 │   → void                                                           │
 │ Execute(int index)                                                 │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/memory-allocators/UnsafeListPoolTests.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
> ```

```
BovineLabs.Core.Collections.UnsafeListPool<T>
  Kind: struct (ValueType=True)
  Size (T=int): 32 bytes

  Interfaces:
    System.IDisposable

  Constructors:
    .ctor(Int32 capacity, Allocator allocator)

  Properties:
    public Boolean IsCreated

  Methods:
    public Void Dispose()
    public Boolean TryAdd(UnsafeList`1 element)
    public Boolean TryGet(UnsafeList`1& element)
    public UnsafeList`1 GetOrCreate(Int32 minimumCapacity, AllocatorHandle listAllocator)
    public Void ReturnOrDispose(UnsafeList`1 list)

  Functional Tests (pool lifecycle):
    IsCreated: True
    GetOrCreate(16) on empty pool: IsCreated=True, Capacity=16
    Added 42 to list, Length=1
    ReturnOrDispose(list) succeeded
    GetOrCreate(4) after return: IsCreated=True, Length=1, [0]=42
    Dispose() succeeded

Verified: 13 checks, 0 failures
```

## Source

- [BovineLabs.Core.Tests/Collections/UnsafeListPoolTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/UnsafeListPoolTests.cs)
