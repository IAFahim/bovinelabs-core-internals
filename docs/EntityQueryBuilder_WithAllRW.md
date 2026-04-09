# EntityQueryBuilder.WithAllRW

## Inner Workings Diagram

```
 EntityQueryBuilder.WithAllRW(type)
 ══════════════════════════════════════════════════════════════════
 Cleaner shorthand for adding a ReadWrite component requirement to a query

 PURPOSE:
 ═════════
 Standard EntityQueryBuilder requires verbose syntax for read-write types.
 WithAllRW wraps this into a single call with correct AccessMode.


 ┌─────────────────────────────────────────────────────────────────────┐
 │  WITHOUT WithAllRW (verbose):                                       │
 │  ┌───────────────────────────────────────────────────────────────┐ │
 │  │  var q = builder.WithAll(                                     │ │
 │  │      ComponentType.ReadWrite<MyComponent>())                  │ │
 │  │          .Build(ref state);                                   │ │
 │  └───────────────────────────────────────────────────────────────┘ │
 │                                                                     │
 │  WITH WithAllRW (clean):                                            │
 │  ┌───────────────────────────────────────────────────────────────┐ │
 │  │  var q = builder.WithAllRW(typeof(MyComponent))               │ │
 │  │          .Build(ref state);                                   │ │
 │  └───────────────────────────────────────────────────────────────┘ │
 └─────────────────────────────────────────────────────────────────────┘


 IMPLEMENTATION
 ┌──────────────────────────────────────────────────────────────────────┐
 │  EntityQueryBuilder WithAllRW(                                       │
 │      this EntityQueryBuilder builder, ComponentType type)            │
 │  {                                                                   │
 │      // Guard: ignore default/empty types                           │
 │      if (type == default) return builder;                            │
 │                                                                      │
 │      // Create a 1-element FixedList32Bytes<ComponentType>           │
 │      var list = new FixedList32Bytes<ComponentType>                  │
 │      {                                                               │
 │          new ComponentType                                            │
 │          {                                                           │
 │              TypeIndex      = type.TypeIndex,                        │
 │              AccessModeType = ComponentType.AccessMode.ReadWrite,    │
 │              //                 ^^^^^^^^^^^^^^^^^^^^^^^^             │
 │              //                 KEY: forces ReadWrite access          │
 │          }                                                           │
 │      };                                                              │
 │                                                                      │
 │      return builder.WithAll(ref list);                               │
 │  }                                                                   │
 └──────────────────────────────────────────────────────────────────────┘


 COMPONENT TYPE ACCESS MODE COMPARISON
 ┌──────────────────────────────────────────────────────────────────┐
 │                                                                  │
 │  WithAll(type)  →  AccessMode = ReadOnly                         │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  ComponentType                                             │  │
 │  │  ┌──────────────┬─────────────────────────────┐            │  │
 │  │  │ TypeIndex    │ AccessMode = ReadOnly        │            │  │
 │  │  └──────────────┴─────────────────────────────┘            │  │
 │  │  → System can READ but not WRITE                           │  │
 │  │  → Dependency: reads only                                  │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  WithAllRW(type) → AccessMode = ReadWrite                        │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  ComponentType                                             │  │
 │  │  ┌──────────────┬─────────────────────────────┐            │  │
 │  │  │ TypeIndex    │ AccessMode = ReadWrite       │            │  │
 │  │  └──────────────┴─────────────────────────────┘            │  │
 │  │  → System can READ and WRITE                              │  │
 │  │  → Dependency: exclusive write access                      │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 QUERY BUILDING FLOW
 ┌──────────────────────────────────────────────────────────────────┐
 │  EntityQueryBuilder                                               │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  All:    [MyComponent(RW)]   ← WithAllRW added this        │  │
 │  │  Any:    [ ]                                                │  │
 │  │  None:   [ ]                                                │  │
 │  │  Options: Default                                           │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                     │                                             │
 │                     ▼                                             │
 │  .Build(ref state)                                                │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Resolves to EntityQuery:                                   │  │
 │  │  - Matches archetypes containing MyComponent                │  │
 │  │  - System registered with RW access → proper job deps       │  │
 │  │  - GetComponentTypeHandle<T>(false) = RW handle             │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘

 SIMILAR EXTENSIONS IN SAME FILE:
 ════════════════════════════════
 • WithAll(type)     → ReadOnly access mode
 • WithAllRW(type)   → ReadWrite access mode
 • WithAny(type)     → ReadOnly
 • WithAnyRW(type)   → ReadWrite
 • WithNone(type)    → exclusion filter
 • WithAnyWriteGroup<T>() → auto-resolve write group components
 • WithNoneWriteGroup<T>() → auto-resolve write group exclusions
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/EntityQueryBuilderExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityQueryBuilderExtensions.cs)
- [BovineLabs.Core/Utility/SubSceneUtil.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/SubSceneUtil.cs)
- [SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.CodeGen.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.CodeGen.cs)
