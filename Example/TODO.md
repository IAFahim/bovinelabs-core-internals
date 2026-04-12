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

- [ ] `Functions.md` — Build and execute function pointers (needs a Burst job context)
- [ ] `FunctionsHash.md` — Hash-based dispatch
- [ ] `IFunction.md` — Implementing a custom IFunction
- [ ] `FunctionsBuilder.md` — Reflection-based registration
- [ ] `Reference.md` — Create, access, validate references with MemoryAllocator
- [ ] `IColumn.md` — Implementing a custom column
- [ ] `MultiHashColumn.md` — Multi-map column with iteration
- [ ] `OrderedListColumn.md` — Sorted column with iteration
- [ ] `CustomChunkIterator.md` — Bitmask iteration over entity indices
- [ ] `UnsafeEnableableLookup.md` — Non-generic enableable check (needs ECS world)

### Tier 2 — ECS-World Required (Need EntityManager)

- [ ] `IEntityCommands.md` — Compare all 3 implementations side by side
- [ ] `EntityManagerCommands.md` — Immediate structural changes
- [ ] `CommandBufferCommands.md` — Deferred structural changes
- [ ] `CommandBufferParallelCommands.md` — Parallel deferred changes
- [ ] `EntityCache.md` — Cache archetype lookups for fast multi-component access
- [ ] `UnsafeComponentLookup.md` — Compare with standard ComponentLookup
- [ ] `UnsafeBufferLookup.md` — Compare with standard BufferLookup
- [ ] `UnsafeEntityDataAccess.md` — Raw pointer chunk access
- [ ] `ChangeFilterLookup.md` — Manual change version control
- [ ] `SharedComponentLookup.md` — Shared component data access

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

(none yet)
