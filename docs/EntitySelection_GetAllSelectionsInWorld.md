# EntitySelection.GetAllSelectionsInWorld

## Inner Workings Diagram

```
 EntitySelection.GetAllSelectionsInWorld
 ======================================================================
 Namespace:  BovineLabs.Core.Editor.Internal

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ bool                     IsSelected                                │
 │ World                    World                                     │
 │ Entity                   Entity                                    │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SelectEntity(World world, Entity entity)                           │
 │   → void                                                           │
 │ GetPrimaryEntityForAuthoringObject(World world, Object target)     │
 │   → Entity                                                         │
 │ GetAllSelectionsInWorld(World world)                               │
 │   → IEnumerable<Ent                                                │
 │ GetAllSelectionsInWorld(World world, NativeList<Entity> entities, Nat
 │   → void                                                           │
 │ GetAllSelectionsInWorld(World world, NativeList<Entity> entities, Nat
 │   → void                                                           │
 │ UnSelect()                                                         │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
Internal: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core.Editor/Internal/EntitySelection.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Internal/EntitySelection.cs)
