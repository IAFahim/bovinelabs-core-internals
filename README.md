# EntityQuery.QueryHasSharedFilter

## Inner Workings Diagram

```
 EntityQuery.QueryHasSharedFilter<T>(out int scdIndex)
 ══════════════════════════════════════════════════════════════════
 Checks if a query has a shared component filter for type T

 PURPOSE:
 ═════════
 Before attempting to read or modify shared component filters on a query,
 you need to know if a filter for a given type already exists. This
 provides both the existence check and the shared component data index.


 ┌──────────────────────────────────────────────────────────────────────┐
 │  bool QueryHasSharedFilter<T>(this EntityQuery query,               │
 │                                out int scdIndex)                     │
 │    where T : unmanaged, ISharedComponentData                        │
 └───────────────────────┬──────────────────────────────────────────────┘
                        │
                        ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Get shared filters from query                              │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var filters = query.GetSharedFilters();                     │  │
 │  │                                                              │  │
 │  │  SharedComponentFilter struct:                               │  │
 │  │  ┌──────────────────────────────────────────────────────┐   │  │
 │  │  │  NativeArray<int> IndexInEntityQuery                  │   │  │
 │  │  │  NativeArray<int> SharedComponentIndex                │   │  │
 │  │  │  int Count                                            │   │  │
 │  │  └──────────────────────────────────────────────────────┘   │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                        │                                            │
 │                        ▼                                            │
 │  Step 2: Resolve target TypeIndex                                   │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var requiredType = TypeManager.GetTypeIndex<T>();            │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                        │                                            │
 │                        ▼                                            │
 │  Step 3: Linear scan through filters                                │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  for (i = 0; i < filters.Count; i++)                        │  │
 │  │  {                                                           │  │
 │  │      indexInEntityQuery = filters.IndexInEntityQuery[i]      │  │
 │  │      component = query.__impl                               │  │
 │  │          ->_QueryData->RequiredComponents                   │  │
 │  │              [indexInEntityQuery].TypeIndex                  │  │
 │  │                                                              │  │
 │  │      if (component == requiredType)                          │  │
 │  │      {                                                       │  │
 │  │          scdIndex = filters.SharedComponentIndex[i]          │  │
 │  │          return true;                                        │  │
 │  │      }                                                       │  │
 │  │  }                                                           │  │
 │  │  scdIndex = -1;                                              │  │
 │  │  return false;                                               │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 ENTITYQUERY INTERNAL STRUCTURE
 ┌──────────────────────────────────────────────────────────────────┐
 │  EntityQuery                                                     │
 │  └── EntityQueryImpl*                                            │
 │      ├── _QueryData                                              │
 │      │   └── RequiredComponents[]                                │
 │      │       ┌──────────────────────────────────────┐            │
 │      │       │ [0] Entity (TypeIndex=0)              │            │
 │      │       │ [1] Position (TypeIndex=5)            │            │
 │      │       │ [2] SharedTeam (TypeIndex=12)  ← SCD  │            │
 │      │       │ [3] Velocity (TypeIndex=7)             │            │
 │      │       └──────────────────────────────────────┘            │
 │      │                                                           │
 │      └── _Filter                                                 │
 │          └── Shared                                              │
 │              ├── IndexInEntityQuery[] = [ 2 ]                     │
 │              │                        ↑                           │
 │              │         points to slot 2 (SharedTeam)              │
 │              └── SharedComponentIndex[] = [ 3 ]                   │
 │                                              ↑                   │
 │                              index into SCD array → "RedTeam"    │
 └──────────────────────────────────────────────────────────────────┘


 RESOLUTION EXAMPLE
 ┌──────────────────────────────────────────────────────────────────┐
 │  Query: WithAll<Position, SharedTeam, Velocity>                  │
 │         .WithSharedComponentFilter(new SharedTeam { Id = 1 })    │
 │                                                                  │
 │  QueryHasSharedFilter<SharedTeam>(out scdIndex):                 │
 │  ┌──────────────────────────────────────────────────────────┐   │
 │  │  i=0: RequiredComponents[2].TypeIndex == SharedTeam?      │   │
 │  │       Yes!                                                │   │
 │  │       scdIndex = SharedComponentIndex[0] = 3              │   │
 │  │       return true                                         │   │
 │  └──────────────────────────────────────────────────────────┘   │
 │                                                                  │
 │  QueryHasSharedFilter<SharedColor>(out scdIndex):                │
 │  ┌──────────────────────────────────────────────────────────┐   │
 │  │  i=0: RequiredComponents[2].TypeIndex == SharedColor?     │   │
 │  │       No (only 1 filter)                                  │   │
 │  │       scdIndex = -1                                       │   │
 │  │       return false                                        │   │
 │  └──────────────────────────────────────────────────────────┘   │
 └──────────────────────────────────────────────────────────────────┘

 OVERLOAD:
 ═════════
 There is also QueryHasSharedFilter<T>(int index) which checks if the
 filter at a specific position matches type T, without iterating.
