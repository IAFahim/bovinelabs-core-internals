# EntityManagerExtensions.GetOrCreateSingletonEntity

## Inner Workings Diagram

```
 EntityManagerExtensions.GetOrCreateSingletonEntity
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

## Source

- [BovineLabs.Core/Extensions/EntityManagerExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityManagerExtensions.cs)
