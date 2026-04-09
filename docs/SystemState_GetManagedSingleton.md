# SystemState.GetManagedSingleton

## Inner Workings Diagram

```
 SystemState.GetManagedSingleton<T>()
 ══════════════════════════════════════════════════════════════════
 Retrieves managed (class) components as singletons from SystemState

 PURPOSE:
 ═════════
 Unity's standard GetSingleton<T>() only works with unmanaged
 IComponentData. This extension handles class-based IComponentData
 singletons by using WithAllRW and the standard query GetSingleton.


 ┌──────────────────────────────────────────────────────────────────┐
 │  T GetManagedSingleton<T>(ref this SystemState state,            │
 │                            bool completeDependency = true)        │
 │    where T : class, IComponentData                               │
 └───────────────────────────┬──────────────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Build query with RW access                                 │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var query = new EntityQueryBuilder(Allocator.Temp)          │  │
 │  │      .WithAllRW<T>()           ← ReadWrite for managed types │  │
 │  │      .WithOptions(QueryOptions)                               │  │
 │  │      .Build(ref state);                                       │  │
 │  │                                                               │  │
 │  │  QueryOptions = EntityQueryOptions.IncludeSystems             │  │
 │  │                                                               │  │
 │  │  Why WithAllRW instead of WithAll?                            │  │
 │  │  Managed components require write access to retrieve           │  │
 │  │  the managed object reference from the EntityComponentStore.  │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 2: Optionally complete dependencies                           │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  if (completeDependency)                                      │  │
 │  │      query.CompleteDependency();                              │  │
 │  │                                                               │  │
 │  │  Default is true; caller can opt out if deps are              │  │
 │  │  already synced.                                              │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 3: Retrieve managed singleton                                 │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  return query.GetSingleton<T>();                              │  │
 │  │                                                               │  │
 │  │  Unity resolves the managed object from the ECS store         │  │
 │  │  for the single matched entity.                               │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 MANAGED VS UNMANAGED COMPONENT STORAGE
 ┌──────────────────────────────────────────────────────────────────┐
 │  UNMANAGED (struct IComponentData):                              │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Stored directly in chunk memory                            │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │ Chunk: [...][Position: x,y,z][Velocity: vx,vy,vz]   │ │  │
 │  │  │                  ↑ inline data                        │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  │  Burst-compatible, no GC allocations                       │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  MANAGED (class IComponentData):                                 │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Chunk stores a TYPE INDEX only (pointer-sized)            │  │
 │  │  ┌──────────────────────────────────────────────────────┐ │  │
 │  │  │ Chunk: [...][ManagedRef: idx=7]                       │ │  │
 │  │  │                       │                               │ │  │
 │  │  │                       ▼                               │ │  │
 │  │  │ ManagedComponentStore:                                │ │  │
 │  │  │ ┌──────┬──────────────────────────────┐               │ │  │
 │  │  │ │ Idx  │ Object Reference             │               │ │  │
 │  │  │ │  7   │ MyManagedComponent instance  │ ← on GC heap │ │  │
 │  │  │ └──────┴──────────────────────────────┘               │ │  │
 │  │  └──────────────────────────────────────────────────────┘ │  │
 │  │  NOT Burst-compatible, uses GC heap                        │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 CALL FLOW DIAGRAM
 ┌──────────────────────────────────────────────────────────────────┐
 │  System (ISystem or SystemBase)                                   │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  void OnUpdate(ref SystemState state)                      │  │
 │  │  {                                                          │  │
 │  │      var config = state.GetManagedSingleton<GameConfig>();  │  │
 │  │      // config is a class, e.g.                            │  │
 │  │      // public class GameConfig : IComponentData            │  │
 │  │      // {                                                   │  │
 │  │      //     public string SceneName;                        │  │
 │  │      //     public float Difficulty;                        │  │
 │  │      // }                                                   │  │
 │  │  }                                                          │  │
 │  └────────────────────────────────────────────────────────────┘  │
 │         │                                                        │
 │         ▼                                                        │
 │  GetManagedSingleton<GameConfig>(ref state)                      │
 │         │                                                        │
 │         ▼                                                        │
 │  query.WithAllRW<GameConfig>()                                   │
 │         │  (IncludeSystems)                                       │
 │         ▼                                                        │
 │  CompleteDependency() ──→ query.GetSingleton<GameConfig>()       │
 │         │                                     │                  │
 │         │                                     ▼                  │
 │         │                          ManagedComponentStore lookup  │
 │         │                                     │                  │
 │         └─────────────────────────────────────┘                  │
 │                          │                                       │
 │                          ▼                                       │
 │              Returns GameConfig instance (class ref)             │
 └──────────────────────────────────────────────────────────────────┘


 TRYGetManagedSingleton VARIANT:
 ┌──────────────────────────────────────────────────────────────────┐
 │  bool TryGetManagedSingleton<T>(ref state, out T component,      │
 │                                  bool completeDependency = true)  │
 │                                                                   │
 │  Checks CalculateEntityCount() != 1 first.                       │
 │  Returns false + default(T) if no singleton exists.              │
 └──────────────────────────────────────────────────────────────────┘

 COMPILED-OUT IN BUILDS:
 ═════════════════════
 The entire GetManagedSingleton method is wrapped in:
     #if !UNITY_DISABLE_MANAGED_COMPONENTS
 This means it compiles away entirely in builds that disable managed
 components (e.g. some console/platform targets).

## Verified Data

```
IComponentData: TYPE NOT FOUND
GameConfig: TYPE NOT FOUND
ref: TYPE NOT FOUND
Verified: 0 checks, 3 failures
```

## Source

- [BovineLabs.Core/Extensions/SystemStateExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/SystemStateExtensions.cs)
- [BovineLabs.Core/Internal/WorldInternal.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/WorldInternal.cs)
- [SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.CodeGen.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/BovineLabs.FacetGenerator/FacetGenerator.CodeGen.cs)
