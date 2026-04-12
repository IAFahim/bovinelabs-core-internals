# Example Code — TODO

Each topic has a fully runnable, unit-tested code example demonstrating:
1. **When to use it** (use case)
2. **How to use it** (complete code)
3. **What to use instead** (alternatives comparison)

Every example MUST pass a unit test before the doc is written.

## Progress

### Tier 1 — Standalone Data Structures

- [x] `Reference.md` — ReferenceExample.cs (15/15 pass)
- [x] `Functions.md` + `IFunction.md` + `FunctionsBuilder.md` + `FunctionsHash.md` — FunctionsExample.cs (17/17 pass)
- [x] `IColumn.md` + `MultiHashColumn.md` + `OrderedListColumn.md` — ColumnsExample.cs (21/21 pass)
- [x] `CustomChunkIterator.md` + `QueryEntityEnumerator.md` — CustomChunkIteratorExample.cs (10/10 pass)

### Tier 2 — ECS-World Lookups

- [x] `UnsafeComponentLookup.md` + `UnsafeBufferLookup.md` + `UnsafeEntityDataAccess.md`
- [x] `UnsafeEnableableLookup.md` + `EntityCache.md` + `ChangeFilterLookup.md` + `SharedComponentLookup.md`
  — UnsafeLookupsExample.cs (45/45 pass)

### Tier 3 — Entity Commands

- [x] `IEntityCommands.md` + `EntityManagerCommands.md` + `CommandBufferCommands.md` + `CommandBufferParallelCommands.md`
  — EntityCommandsExample.cs (14/14 pass)

### Tier 3 — Settings

- [x] `SettingsSingleton.md` + `SettingsAttributes.md` — SettingsExample.cs (12/12 pass)

### Tier 4 — Component Assets

- [x] `ComponentAssetBase.md` + `ComponentAsset.md` + `EnableableComponentAsset.md`
- [x] `ComponentFieldAsset.md` + `TypeAsset.md`
  — ComponentAssetsExample.cs (11/11 pass)

### Tier 4 — Editor UI

- [x] `ElementEditor.md` + `ObjectSelectionProxy.md` + `SearchElement.md`
  — EditorUIExample.cs (15/15 pass)

### Tier 4 — Utilities

- [x] `RuntimeContentCatalogUtility.md` — RuntimeContentCatalogExample.cs (4/4 pass)

## ALL 31 TOPICS COMPLETE

Total test results: 164 checks, 0 failures across 9 example files.

## Completed Examples

1. **ReferenceExample.cs** (15/15) — Create, null, equality, mutation, ReferenceData roundtrip
2. **EntityCommandsExample.cs** (14/14) — Interface contract, size comparison, field inspection
3. **FunctionsExample.cs** (17/17) — IFunction, FunctionData, builder API, lifecycle
4. **ColumnsExample.cs** (21/21) — IColumn, MultiHashColumn, OrderedListColumn, OrderedListIterator
5. **CustomChunkIteratorExample.cs** (10/10) — 3-strategy iteration, QueryEntityEnumerator
6. **UnsafeLookupsExample.cs** (45/45) — All 7 lookup types, EntityCache, decision matrix
7. **SettingsExample.cs** (12/12) — SettingsSingleton, 3 attributes, lifecycle
8. **ComponentAssetsExample.cs** (11/11) — 5 component assets, hierarchy
9. **EditorUIExample.cs** (15/15) — ElementEditor, ElementProperty, ObjectSelectionProxy, SearchElement
10. **RuntimeContentCatalogExample.cs** (4/4) — Catalog parsing, SubScene discovery
