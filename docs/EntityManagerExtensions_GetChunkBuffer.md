# EntityManagerExtensions.GetChunkBuffer

## Inner Workings Diagram

```
 EntityManagerExtensions.GetChunkBuffer
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    EntityManagerExtensions                   │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ NumberOfArchetype(this EntityManager entityManager)                │
 │   → int                                                            │
 │ GetComponentDataRaw(this EntityManager entityManager, Entity entity, 
 │   → void*                                                          │
 │ GetSharedComponentRaw(this EntityManager entityManager, Entity entity
 │   → void*                                                          │
 │ GetSharedComponentManagedBoxed(this EntityManager entityManager, Enti
 │   → object                                                         │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
EntityManagerExtensions: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/EntityManagerExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityManagerExtensions.cs)
