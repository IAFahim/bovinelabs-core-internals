# EntityQuery.ReplaceSharedComponentFilter

## Inner Workings Diagram

```
 EntityQuery.ReplaceSharedComponentFilter<T>(index, sharedComponent)
 ═══════════════════════════════════════════════════════════════════════
 Swaps a shared component filter value WITHOUT rebuilding the query

 PURPOSE:
 ═════════
 Normally, changing a shared component filter requires ResetFilter +
 AddSharedComponentFilter, which triggers query rebuilding.
 ReplaceSharedComponentFilter does an in-place swap of the SCD index.


 ┌──────────────────────────────────────────────────────────────────────┐
 │  void ReplaceSharedComponentFilter<T>(                              │
 │      this EntityQuery query, int index, T sharedComponent)          │
 │    where T : unmanaged, ISharedComponentData                        │
 └───────────────────────────┬──────────────────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Validate index bounds                                      │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  AssertRange(index, impl->_Filter.Shared.Count);              │  │
 │  │  → throws if index < 0 or >= count of shared filters         │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 2: Release old shared component reference                     │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  impl->_Access->EntityComponentStore                         │  │
 │  │      ->RemoveSharedComponentReference_Unmanaged(              │  │
 │  │          impl->_Filter.Shared                                 │  │
 │  │              .SharedComponentIndex[index]);                   │  │
 │  │                                                                │  │
 │  │  Decrements ref count on the OLD SCD value                    │  │
 │  │  If ref count reaches 0, the SCD slot may be freed            │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 3: Insert new shared component value                          │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  impl->_Filter.Shared.IndexInEntityQuery[index] =             │  │
 │  │      query.GetIndexInEntityQuery(                             │  │
 │  │          TypeManager.GetTypeIndex<T>());                      │  │
 │  │                                                                │  │
 │  │  impl->_Filter.Shared.SharedComponentIndex[index] =           │  │
 │  │      impl->_Access->InsertSharedComponent_Unmanaged(          │  │
 │  │          sharedComponent);                                    │  │
 │  │                                                                │  │
 │  │  The new SCD value is registered (or ref count incremented    │  │
 │  │  if it already exists) and the filter now points to it.       │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 SHARED COMPONENT FILTER SWAP
 ┌──────────────────────────────────────────────────────────────────┐
 │  EntityQuery._Filter.Shared (before)                              │
 │  ┌───────────────────────────────────────────────────────────┐   │
 │  │  IndexInEntityQuery    = [ 2 ]       ← slot in query      │   │
 │  │  SharedComponentIndex  = [ 5 ]       ← "RedTeam" (old)    │   │
 │  └───────────────────────────────────────────────────────────┘   │
 │                                                                  │
 │                    ReplaceSharedComponentFilter(0, "BlueTeam")    │
 │                            │                                     │
 │                            ▼                                     │
 │  EntityQuery._Filter.Shared (after)                              │
 │  ┌───────────────────────────────────────────────────────────┐   │
 │  │  IndexInEntityQuery    = [ 2 ]       ← unchanged           │   │
 │  │  SharedComponentIndex  = [ 8 ]       ← "BlueTeam" (new)    │   │
 │  └───────────────────────────────────────────────────────────┘   │
 └──────────────────────────────────────────────────────────────────┘


 SHARED COMPONENT DATA STORE
 ┌──────────────────────────────────────────────────────────────────┐
 │  EntityComponentStore.SharedComponentData                        │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Index  │ Value          │ RefCount                        │  │
 │  │  ───────┼────────────────┼──────────                       │  │
 │  │    3    │ SharedTeam(Red) │ 2                               │  │
 │  │    5    │ SharedTeam(Red) │ 1 → 0  (released!)             │  │
 │  │    8    │ SharedTeam(Blue)│ 0 → 1  (newly inserted)        │  │
 │  │   12    │ SharedTeam(...) │ 4                               │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 QUERY AFFECTED MATCHING
 ┌──────────────────────────────────────────────────────────────────┐
 │  BEFORE: filter on SharedTeam(Red)                               │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Archetype [Position, SharedTeam(Red)]                     │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │ Chunk0: [E0, E1, E2]  ← matches query               │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  │  Archetype [Position, SharedTeam(Blue)]                    │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │ Chunk0: [E3, E4]       ← does NOT match              │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  AFTER: filter now on SharedTeam(Blue)                           │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Archetype [Position, SharedTeam(Red)]                     │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │ Chunk0: [E0, E1, E2]  ← does NOT match               │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  │  Archetype [Position, SharedTeam(Blue)]                    │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │ Chunk0: [E3, E4]       ← matches query               │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘

 KEY ADVANTAGE:
 ═════════════
 No query rebuild. The EntityQueryImpl's matching archetype list
 stays valid — only the filter value changes. This avoids expensive
 archetype scanning and chunk list reconstruction.
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/EntityQueryExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityQueryExtensions.cs)
- [BovineLabs.Core/Internal/EntityQueryInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/EntityQueryInternal.cs)
- [BovineLabs.Core/Extensions/EntityQueryBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityQueryBuilderExtensions.cs)
