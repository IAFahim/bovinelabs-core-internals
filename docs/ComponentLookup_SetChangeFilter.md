# ComponentLookup.SetChangeFilter

## Inner Workings Diagram

```
 ComponentLookup<T>.SetChangeFilter(entity)
 ═════════════════════════════════════════════════════════════════════
 Marks a component as "changed" in the version tracker WITHOUT modifying its data

 PURPOSE:
 ═════════
 Sometimes you need downstream systems (using DidChange / change filters)
 to see a component as modified, even though you didn't write through the
 normal RW path. SetChangeFilter directly updates the change version.

 ┌──────────────────────────────────────────────────────────────────────┐
 │  void SetChangeFilter<T>(ref this ComponentLookup<T> lookup,        │
 │                           Entity entity)                             │
 └───────────────────────────┬──────────────────────────────────────────┘
                            │
                            ▼
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Step 1: Validate                                                   │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  CheckWriteAndThrow(m_Safety)                                 │  │
 │  │  AssertEntityHasComponent(entity, typeIndex, ref cache)       │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 2: Resolve chunk & archetype                                  │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  var chunk     = ecs->GetChunk(entity);                       │  │
 │  │  var archetype = ecs->GetArchetype(chunk);                    │  │
 │  │                                                                │  │
 │  │  if (archetype != m_Cache.Archetype)                          │  │
 │  │      m_Cache.Update(archetype, m_TypeIndex);                  │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 3: BUMP THE CHANGE VERSION (core operation)                   │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  archetype->Chunks.SetChangeVersion(                          │  │
 │  │      typeIndexInArchetype,                                    │  │
 │  │      chunk.ListIndex,                                        │  │
 │  │      lookup.GlobalSystemVersion);  ← current system version   │  │
 │  │                                                                │  │
 │  │  This is what normal GetComponentDataRW would do, but         │  │
 │  │  WITHOUT actually reading or writing any component data.      │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                            │                                        │
 │                            ▼                                        │
 │  Step 4: Journal (editor/dev only)                                  │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  if (m_RecordToJournal)                                       │  │
 │  │      GetComponentDataWithTypeRW(...)  // for journal logging  │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 VERSION TRACKING MECHANISM
 ┌──────────────────────────────────────────────────────────────────┐
 │  Archetype                                                       │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  Chunks (ArchetypeChunks)                                  │  │
 │  │  ┌──────────────────────────────────────────────────────┐  │  │
 │  │  │  ChangeVersion[typeIndexInArchetype][chunkListIndex]  │  │  │
 │  │  │                                                      │  │  │
 │  │  │  BEFORE SetChangeFilter:                             │  │  │
 │  │  │  ┌────────┬────────┬────────┬────────┐               │  │  │
 │  │  │  │ Type 0 │ Type 1 │ Type 2 │ Type 3 │               │  │  │
 │  │  │  │ v=100  │ v=100  │ v=47   │ v=100  │  ← chunk 0    │  │  │
 │  │  │  │ v=100  │ v=100  │ v=50   │ v=100  │  ← chunk 1    │  │  │
 │  │  │  └────────┴────────┴────────┴────────┘               │  │  │
 │  │  │                                                      │  │  │
 │  │  │  AFTER SetChangeFilter on entity in chunk 1, Type 2: │  │  │
 │  │  │  ┌────────┬────────┬────────┬────────┐               │  │  │
 │  │  │  │ Type 0 │ Type 1 │ Type 2 │ Type 3 │               │  │  │
 │  │  │  │ v=100  │ v=100  │ v=47   │ v=100  │  ← chunk 0    │  │  │
 │  │  │  │ v=100  │ v=100  │ v=200  │ v=100  │  ← chunk 1    │  │  │
 │  │  │  └────────┴────────┴────────┴────────┘               │  │  │
 │  │  │                         ↑ bumped to GlobalSystemVersion│  │  │
 │  │  └──────────────────────────────────────────────────────┘  │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 HOW DOWNSTREAM SYSTEMS DETECT THE CHANGE
 ┌──────────────────────────────────────────────────────────────────┐
 │  System B (uses DidChange filter):                                │
 │  ┌────────────────────────────────────────────────────────────┐  │
 │  │  savedVersion = 50;  // captured at start of frame         │  │
 │  │                                                            │  │
 │  │  chunk.DidChange(typeIdx, ref cache, savedVersion)         │  │
 │  │  → changeVersion(200) > savedVersion(50) = TRUE            │  │
 │  │  → "Yes, this chunk's component changed!"                  │  │
 │  └────────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘

 USE CASES:
 ═════════
 • Marking a component dirty after external modification (e.g. network sync)
 • Triggering re-processing without actually changing values
 • Manual change propagation in custom job pipelines
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/ComponentLookupExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/ComponentLookupExtensions.cs)
- [BovineLabs.Core/Iterators/UnsafeComponentLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/UnsafeComponentLookup.cs)
- [BovineLabs.Core/Iterators/SharedComponentLookup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/SharedComponentLookup.cs)
