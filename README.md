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

## Source

- [BovineLabs.Core.Tests/Collections/UnsafeListPoolTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/UnsafeListPoolTests.cs)
