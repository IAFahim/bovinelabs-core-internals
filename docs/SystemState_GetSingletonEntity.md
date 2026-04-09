# SystemState.GetSingletonEntity

## Inner Workings Diagram

```
 SystemState.GetSingletonEntity<T>()
 ══════════════════════════════════════════════════════════════════
 Retrieves the singleton Entity for a component type from SystemState

 PURPOSE:
 ═════════
 Simplifies singleton access from within a system. Builds a temporary
 query, completes dependencies, and returns the singleton entity.


 ┌──────────────────────────────────────────────────────────────────┐
 │  Entity GetSingletonEntity<T>(ref this SystemState state)        │
 └───────────────────────────┬──────────────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Build temporary query                                      │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var query = new EntityQueryBuilder(Allocator.Temp)          │  │
 │  │      .WithAll<T>()                                           │  │
 │  │      .WithOptions(EntityQueryOptions.IncludeSystems)         │  │
 │  │      .Build(ref state);                                      │  │
 │  │                                                              │  │
 │  │  Note: IncludeSystems allows matching entities in system     │  │
 │  │  worlds, which is where many singletons live.                │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 2: Complete all pending dependencies                          │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  query.CompleteDependency();                                 │  │
 │  │                                                              │  │
 │  │  Blocks until all jobs writing to matched entities finish.   │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 3: Return singleton entity                                    │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  return query.GetSingletonEntity();                          │  │
 │  │                                                              │  │
 │  │  Unity's standard API: reads entity from the single          │  │
 │  │  matched chunk.                                              │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 TEMPORARY QUERY LIFECYCLE
 ┌──────────────────────────────────────────────────────────────────┐
 │  SystemState                                                      │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  ref state                                                 │  │
 │  │    │                                                       │  │
 │  │    ▼                                                       │  │
 │  │  EntityQueryBuilder(Allocator.Temp)                        │  │
 │  │    .WithAll<T>()                                           │  │
 │  │    .WithOptions(IncludeSystems)                            │  │
 │  │    .Build(ref state)                                       │  │
 │  │    │                                                       │  │
 │  │    ▼                                                       │  │
 │  │  EntityQuery (temporary, disposed when state disposed)     │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │  Matches archetypes containing T                     │ │  │
 │  │  │  Must have exactly 1 matching entity (singleton)     │ │  │
 │  │  │                                                       │ │  │
 │  │  │  Chunk: [Entity_singleton]                            │ │  │
 │  │  │           ↑                                          │ │  │
 │  │  │  GetSingletonEntity() returns this Entity             │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 ALSO AVAILABLE (same pattern):
 ┌──────────────────────────────────────────────────────────────────┐
 │  • GetSingletonEntity<T>()       → Entity                         │
 │  • TryGetSingletonEntity<T>()    → bool + Entity                  │
 │  • HasSingleton<T>()             → bool                           │
 │  • GetSingleton<T>()             → T (component value)            │
 │  • SetSingleton<T>(value)        → void (write)                   │
 │  • GetSingletonRW<T>()           → RefRW<T>                       │
 │  • TryGetSingleton<T>()          → bool + T                       │
 │  • GetSingletonBuffer<T>()       → DynamicBuffer<T>               │
 │  • TryGetSingletonBuffer<T>()    → bool + DynamicBuffer<T>        │
 │  • GetManagedSingleton<T>()      → T (class component)            │
 └──────────────────────────────────────────────────────────────────┘

 IncludeSystems flag is constant across all these methods:
 ┌──────────────────────────────────────────────────────────────────┐
 │  const EntityQueryOptions QueryOptions =                         │
 │      EntityQueryOptions.IncludeSystems;                          │
 │                                                                  │
 │  This ensures system-level singleton entities (like those        │
 │  created by [GenerateTestsForBurstCompatibility] or system       │
 │  managed singletons) are found.                                  │
 └──────────────────────────────────────────────────────────────────┘

## Verified Data

```
component: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/SystemStateExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/SystemStateExtensions.cs)
- [BovineLabs.Core/Internal/WorldInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/WorldInternal.cs)
- [SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.CodeGen.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.CodeGen.cs)
