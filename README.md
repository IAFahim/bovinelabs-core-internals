# ArchetypeChunk.DidChange

## Inner Workings Diagram

```
 ArchetypeChunk.DidChange()  -- Safe version check without false dependencies
 ═══════════════════════════════════════════════════════════════════════════

 CALLER (e.g. IJobChunk)
 ┌─────────────────────────────────────────┐
 │  uint cachedVersion = ...;              │  ← saved earlier from
 │                                         │    GlobalSystemVersion
 │  if (chunk.DidChange(                   │
 │      typeIndex,                         │
 │      ref typeLookupCache,               │
 │      cachedVersion))                    │
 │  {                                      │
 │      // Component changed               │
 │  }                                      │
 └──────────┬──────────────────────────────┘
            │
            ▼
 ┌──────────────────────────────────────────────────────────────────────┐
 │  ArchetypeChunkExtensions.DidChange()                                │
 │                                                                      │
 │  1. Resolve typeIndex → indexInArchetype                             │
 │     ┌─────────────────────────────┐                                  │
 │     │  ChunkDataUtility           │                                  │
 │     │  .GetIndexInTypeArray(      │                                  │
 │     │    archetype,               │                                  │
 │     │    typeIndex,               │                                  │
 │     │    ref typeLookupCache)     │  ← cache avoids repeated lookups│
 │     └──────────┬──────────────────┘                                  │
 │                │                                                     │
 │  2. Read change version from chunk                                  │
 │     ┌─────────────────────────────────────────────────┐             │
 │     │  archetype->Chunks.GetChangeVersion(             │             │
 │     │      indexInArchetype,                           │             │
 │     │      chunk.m_Chunk.ListIndex)                    │             │
 │     └──────────┬──────────────────────────────────────┘             │
 │                │                                                     │
 │  3. Compare versions                                                │
 │     ┌─────────────────────────────────────────────────┐             │
 │     │  ChangeVersionUtility.DidChange(                 │             │
 │     │      changeVersion,                              │             │
 │     │      userVersion)                                │             │
 │     │                                                  │             │
 │     │  returns: changeVersion > userVersion            │             │
 │     └──────────────────────────────────────────────────┘             │
 └──────────────────────────────────────────────────────────────────────┘


 ECS CHUNK MEMORY LAYOUT (Conceptual)
 ┌─────────────────────────────────────────────────────────────────────┐
 │  Archetype                                                          │
 │  ┌────────────────────────────────────────────────────────────────┐ │
 │  │  Types[]        = [Entity, Position, Velocity, ...]           │ │
 │  │  Chunks                                                   │ │
 │  │  ┌──────────────────────────────────────────────────────────┐ │ │
 │  │  │  ChangeVersions[] per component type per chunk list index│ │ │
 │  │  │                                                          │ │ │
 │  │  │  Chunk 0: [ver=42, ver=7, ver=13, ...]  ← per type      │ │ │
 │  │  │  Chunk 1: [ver=42, ver=8, ver=13, ...]                   │ │ │
 │  │  │  Chunk 2: [ver=42, ver=7, ver=14, ...]                   │ │ │
 │  │  └──────────────────────────────────────────────────────────┘ │ │
 │  └────────────────────────────────────────────────────────────────┘ │
 │                                                                     │
 │  Chunk (16KB block)                                                 │
 │  ┌───────────────────────────────────────────────────────────────┐  │
 │  │  Header: Count, ListIndex, MetaChunkEntity, ...               │  │
 │  │  ┌───────────────┬──────────────────┬────────────────────┐    │  │
 │  │  │ Entity[0..N]  │ Position[0..N]   │ Velocity[0..N]     │    │  │
 │  │  │ E0 E1 E2 ...  │ P0 P1 P2 ...     │ V0 V1 V2 ...       │    │  │
 │  │  └───────────────┴──────────────────┴────────────────────┘    │  │
 │  └───────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────┘


 VERSION COMPARISON FLOW
 ┌────────────────┐         ┌──────────────────────┐
 │ cachedVersion  │         │ changeVersion        │
 │ (user saved)   │         │ (from archetype      │
 │    e.g. 7      │         │  chunk version array) │
 │                │         │    e.g. 13           │
 └───────┬────────┘         └──────────┬───────────┘
         │                             │
         └──────────┬──────────────────┘
                    │
           ┌────────▼────────┐
           │  13 > 7 ?       │
           │  → true:        │
           │  DID CHANGE     │
           └─────────────────┘

 KEY INSIGHT:
 ═══════════
 Unlike the standard ArchetypeChunk.DidChange<T>(), this extension accepts a
 raw TypeIndex + lookupCache instead of a ComponentTypeHandle<T>. This avoids
 creating a safety handle dependency — it reads the version counter directly
 from the archetype's per-chunk version array without registering as a reader
 or writer, preventing false dependency chains in parallel jobs.

 The typeLookupCache (short) persists across calls for the same archetype,
 skipping the O(N) type-array scan on subsequent invocations.
