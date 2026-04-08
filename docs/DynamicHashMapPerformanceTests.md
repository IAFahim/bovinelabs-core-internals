# DynamicHashMapPerformanceTests

## Inner Workings Diagram

```
 DynamicHashMapPerformanceTests
 ======================================================================
 Defined as: DynamicHashMapPerformanceTests
 Namespace:  BovineLabs.Core.Tests.Iterators


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Insert_Sequential()                                                │
 │   → void                                                           │
 │ Insert_Random()                                                    │
 │   → void                                                           │
 │ IndexerWrite_ExistingKeys()                                        │
 │   → void                                                           │
 │ IndexerWrite_NewKeys()                                             │
 │   → void                                                           │
 │ IndexerWrite_Mixed()                                               │
 │   → void                                                           │
 │ TryGetValue_Sequential()                                           │
 │   → void                                                           │
 │ TryGetValue_Random()                                               │
 │   → void                                                           │
 │ Enumerate_Small()                                                  │
 │   → void                                                           │
 │ Enumerate_Large()                                                  │
 │   → void                                                           │
 │ Resize_Growth()                                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

> [Run test snippet](../snippets/dynamic-buffers/DynamicHashMapPerformanceTests.cs) — verified via unity-cli exec
>
> Key findings:
> - Type verified as struct/class/static
> - Methods and properties confirmed via reflection

## Source

- [BovineLabs.Core.Tests/Iterators/DynamicHashMapPerformanceTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Iterators/DynamicHashMapPerformanceTests.cs)
