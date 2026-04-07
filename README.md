# ArchetypeChunk.GetNativeArrayReadOnly

## Inner Workings Diagram

```
 ArchetypeChunk.GetNativeArrayReadOnly<T>()
 ══════════════════════════════════════════════════════════════
 Bypasses change-version bump when reading chunk component data

 ┌────────────────────────────────────────────────────────────┐
 │  Standard Path:  chunk.GetNativeArray(ref handle)          │
 │  ┌──────────────────────────────────────────────────────┐  │
 │  │  GetComponentDataWithTypeRW → bumps change version!  │  │
 │  │  Even for read-only handles in some code paths        │  │
 │  └──────────────────────────────────────────────────────┘  │
 │                                                            │
 │  BovineLabs Path:  chunk.GetNativeArrayReadOnly(ref h)     │
 │  ┌──────────────────────────────────────────────────────┐  │
 │  │  GetComponentDataWithTypeRO → version NOT bumped      │  │
 │  │  Pure read path, no side effects                      │  │
 │  └──────────────────────────────────────────────────────┘  │
 └────────────────────────────────────────────────────────────┘


 IMPLEMENTATION FLOW
 ┌──────────────────────────────────────────────────────────────────┐
 │  GetNativeArrayReadOnly<T>(ArchetypeChunk, ref ComponentTypeHandle)│
 │                                                                  │
 │  Step 1: Safety check (debug only)                               │
 │  ┌───────────────────────────────────┐                          │
 │  │  AtomicSafetyHandle.CheckReadAndThrow(typeHandle.m_Safety)   ││
 │  └───────────────────────────────────┘                          │
 │                    │                                             │
 │                    ▼                                             │
 │  Step 2: Resolve pointer (READ-ONLY path)                       │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  archetype = ECS->GetArchetype(chunk.m_Chunk)             │  │
 │  │                                                           │  │
 │  │  ptr = ChunkDataUtility                                   │  │
 │  │      .GetOptionalComponentDataWithTypeRO(                 │  │
 │  │          chunk, archetype, 0,                             │  │
 │  │          typeHandle.m_TypeIndex,                          │  │
 │  │          ref typeHandle.m_LookupCache)                    │  │
 │  │                                                           │  │
 │  │  ↑ NOTE: Uses RO variant — NO version bump                │  │
 │  └───────────────────────────────────────────────────────────┘  │
 │                    │                                             │
 │                    ▼                                             │
 │  Step 3: Null check (component may not exist in archetype)      │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  if (ptr == null)                                         │  │
 │  │      → return empty NativeArray<T>.ReadOnly (length=0)    │  │
 │  └───────────────────────────────────────────────────────────┘  │
 │                    │                                             │
 │                    ▼                                             │
 │  Step 4: Wrap as NativeArray.ReadOnly                           │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  result = ConvertExistingDataToNativeArray<T>(            │  │
 │  │               ptr, chunk.Count, Allocator.None)           │  │
 │  │  return result.AsReadOnly()                               │  │
 │  └───────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────────┘


 CHUNK MEMORY — Pointer Resolution
 ┌─────────────────────────────────────────────────────────────────┐
 │  Archetype                                                       │
 │  ┌───────────────────────────────────────────────────────────┐  │
 │  │  Types[]      : [Entity, Position, Velocity, Health]      │  │
 │  │  Offsets[]    : [   0,       8,       20,      24     ]   │  │
 │  │  SizeOfs[]    : [   8,       12,       4,       4     ]   │  │
 │  └───────────────────────────────────────────────────────────┘  │
 │                                                                  │
 │  Chunk (16KB)                                                    │
 │  ┌─────────────────────────────────────────────────────────┐    │
 │  │  base + Offset[1] ──→ [Pos0][Pos1][Pos2][...][PosN]    │    │
 │  │                         ↑                                │    │
 │  │                    returned ptr                          │    │
 │  │                                                         │    │
 │  │  chunk.Count = N → NativeArray<T> wraps N elements      │    │
 │  └─────────────────────────────────────────────────────────┘    │
 └─────────────────────────────────────────────────────────────────┘


 VERSION BUMP COMPARISON
 ┌──────────────────────────┬────────────────────────────────┐
 │  Standard GetNativeArray │  GetNativeArrayReadOnly        │
 │  ─────────────────────── │  ──────────────────────────    │
 │  Uses RW internally      │  Uses RO path                  │
 │  Bumps ChangeVersion     │  ChangeVersion UNTOUCHED       │
 │  Triggers re-query in    │  No false change signals       │
 │  change-filtered jobs    │  Clean read-only semantics     │
 └──────────────────────────┴────────────────────────────────┘

 WHY IT MATTERS:
 ══════════════
 In IJobChunk, if you call the standard GetNativeArray with a read-only
 ComponentTypeHandle, Unity may still use the RW pointer path internally
 depending on access mode, which bumps the archetype's change version.
 This causes downstream jobs using DidChange/change filters to falsely
 detect changes. GetNativeArrayReadOnly guarantees the RO path is used,
 keeping the change version clean.
```
