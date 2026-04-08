# SingletonCollectionUtil

## Inner Workings Diagram

```
 SingletonCollectionUtil
 ======================================================================
 Defined as: ISingletonCollectionUtil
 Namespace:  BovineLabs.Core.SingletonCollection

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Allocator                CurrentAllocator                          │
 │ UnsafeList<TC>*          ContainersUnsafe                          │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ClearRewind(JobHandle handle)                                      │
 │   → void                                                           │
 │ Dispose()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/SingletonCollection/SingletonCollectionUtil.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/SingletonCollection/SingletonCollectionUtil.cs)
