# Example Code — TODO

Each topic needs a fully runnable, unit-tested code example demonstrating:
1. **When to use it** (use case)
2. **How to use it** (complete code)
3. **What to use instead** (alternatives comparison)

Every example MUST pass a unit test before the doc is written.

## Testability Categories

- **A: Standalone** — No ECS world needed. Pure data structures, allocators, utilities.
- **B: ECS World** — Needs a live EntityManager/SystemState. Lookups, caches, commands.
- **C: System Lifecycle** — Needs OnCreate/OnUpdate/OnDestroy cycle. Functions, settings.
- **D: Editor Only** — ScriptableObjects, custom inspectors, UIElements. Batchmode only.

## Progress

### Tier 1 — Standalone Data Structures (Testable immediately)

- [x] `Reference.md` — Create, access, null check, equality, mutation (15/15 pass)
- [x] `Functions.md` — API surface, FunctionData layout, lifecycle, decision matrix (17/17 pass)
- [x] `IFunction.md` — Covered by Functions example
- [x] `FunctionsBuilder.md` — Covered by Functions example
- [x] `FunctionsHash.md` — API surface + TryExecute, covered by Functions example
- [ ] `IColumn.md` — Implementing a custom column
- [ ] `MultiHashColumn.md` — Multi-map column with iteration
- [ ] `OrderedListColumn.md` — Sorted column with iteration
- [ ] `CustomChunkIterator.md` — Bitmask iteration over entity indices

### Tier 2 — ECS-World Required (Need EntityManager)

- [x] `IEntityCommands.md` — Compare all 3 implementations side by side (14/14 pass)
- [x] `EntityManagerCommands.md` — Covered by EntityCommands example
- [x] `CommandBufferCommands.md` — Covered by EntityCommands example
- [x] `CommandBufferParallelCommands.md` — Covered by EntityCommands example
- [ ] `EntityCache.md` — Cache archetype lookups for fast multi-component access
- [ ] `UnsafeComponentLookup.md` — Compare with standard ComponentLookup
- [ ] `UnsafeBufferLookup.md` — Compare with standard BufferLookup
- [ ] `UnsafeEntityDataAccess.md` — Raw pointer chunk access
- [ ] `ChangeFilterLookup.md` — Manual change version control
- [ ] `SharedComponentLookup.md` — Shared component data access
- [ ] `UnsafeEnableableLookup.md` — Non-generic enableable check

### Tier 3 — System Lifecycle (Need full system cycle)

- [ ] `SettingsSingleton.md` — Create, load, access settings
- [ ] `SettingsAttributes.md` — Grouping, subdirectories, world targeting

### Tier 4 — Editor Only (Batchmode verification only)

- [ ] `ComponentAssetBase.md` — Type hash resolution
- [ ] `ComponentAsset.md` — Standard component asset
- [ ] `EnableableComponentAsset.md` — Enableable validation
- [ ] `ComponentFieldAsset.md` — Field offset resolution
- [ ] `TypeAsset.md` — Type resolution
- [ ] `ElementEditor.md` — Custom inspector lifecycle
- [ ] `ObjectSelectionProxy.md` — Object wrapping for Inspector
- [ ] `SearchElement.md` — Search popup field
- [ ] `RuntimeContentCatalogUtility.md` — Catalog parsing

## Completed Examples

1. **ReferenceExample.cs** — 15/15 pass. Shows create from struct/bytes/byte[], null, equality, mutation, roundtrip via ReferenceData
2. **EntityCommandsExample.cs** — 14/14 pass. Shows interface contract, size comparison (112/168/160 bytes), field inspection, decision matrix
3. **FunctionsExample.cs** — 17/17 pass. Shows IFunction contract, FunctionData 32B, builder API, Functions vs FunctionsHash decision matrix
