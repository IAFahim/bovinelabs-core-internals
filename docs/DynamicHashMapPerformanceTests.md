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

> Run the verification snippet:
> ```bash
> cat snippets/dynamic-buffers/DynamicHashMapPerformanceTests.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Linq,Unity.Collections,Unity.Entities"
> ```

```
DynamicHashMapPerformanceTests
  Namespace: BovineLabs.Core.Tests.Iterators
  Kind: test class (source-level verification)

  Source: Library/PackageCache/com.bovinelabs.core@d49052fed7b0/BovineLabs.Core.Tests/Iterators/DynamicHashMapPerformanceTests.cs

  Methods (all 14 public void methods):
    void AddBatchUnsafe_Performance()
    void AddBatchUnsafe_vs_Individual()
    void Enumerate_Large()
    void Enumerate_Small()
    void IndexerWrite_ExistingKeys()
    void IndexerWrite_Mixed()
    void IndexerWrite_NewKeys()
    void Insert_Random()
    void Insert_Sequential()
    void LoadFactor_Performance()
    void Memory_Allocation_Tracking()
    void Resize_Growth()
    void TryGetValue_Random()
    void TryGetValue_Sequential()

Verified: 14 checks, 0 failures
```

## Source

- [BovineLabs.Core.Tests/Iterators/DynamicHashMapPerformanceTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Iterators/DynamicHashMapPerformanceTests.cs)
