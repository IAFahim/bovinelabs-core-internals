# TimerTriggerResetJob - Inner Workings

## Overview

`TimerTriggerResetJob<T>` is a standalone `IJobChunk` that bulk-resets timer remaining values across entire chunks by detecting which entities have remaining values that are non-zero and marking them as changed. It uses a pre-allocated block of zeros and `MemCmp` to efficiently determine which chunks need reset.

```
File: BovineLabs.Core/Model/TimerTriggerResetJob.cs
Generic Parameter:
  T - The remaining-time component type (IComponentData, float-sized)
```

## Architecture

```
 ┌────────────────────────────────────────────────────────────────────┐
 │              TimerTriggerResetJob<T> : IJobChunk                   │
 │                                                                    │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  Fields:                                                     │  │
 │  │                                                              │  │
 │  │  RemainingHandle : ComponentTypeHandle<T>                    │  │
 │  │      The timer remaining component to check                  │  │
 │  │                                                              │  │
 │  │  Zeros : void* (ReadOnly, NativeDisableUnsafePtrRestriction) │  │
 │  │      Pointer to pre-allocated zero-filled memory block       │  │
 │  │      Size = sizeof(float) * max_chunk_count                  │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 │                                                                    │
 │  ┌──────────────────────────────────────────────────────────────┐  │
 │  │  Execute(in ArchetypeChunk chunk, ...)                       │  │
 │  │                                                              │  │
 │  │  Step 1: Read remainings as RO pointer                       │  │
 │  │           remainings = GetRequiredComponentDataPtrRO(T)      │  │
 │  │                                                              │  │
 │  │  Step 2: Compare with zero block                             │  │
 │  │           length = sizeof(float) * chunk.Count               │  │
 │  │           MemCmp(remainings, Zeros, length)                  │  │
 │  │                                                              │  │
 │  │  Step 3: If any non-zero values found → mark changed         │  │
 │  │           SetChangeFilter(ref RemainingHandle)               │  │
 │  └──────────────────────────────────────────────────────────────┘  │
 └────────────────────────────────────────────────────────────────────┘
```

## Execution Flow

```
  Chunk Data                    Zero Block
  ┌──────────────────┐         ┌──────────────────┐
  │ Remaining[0] = 0 │         │       0.0        │
  │ Remaining[1] = 3 │◄─ MemCmp ─►│       0.0        │
  │ Remaining[2] = 0 │         │       0.0        │
  │ Remaining[3] = 1 │         │       0.0        │
  │ Remaining[4] = 0 │         │       0.0        │
  │ ...              │         │ ...              │
  └──────────────────┘         └──────────────────┘
         │
         ▼
  MemCmp returns non-zero (different!)
  → SetChangeFilter(ref RemainingHandle)
         │
         ▼
  ┌──────────────────────────────────────────────────────┐
  │  The chunk is now marked as "changed" for the        │
  │  TRemaining component. Downstream systems that use   │
  │  change version filtering on TRemaining will now     │
  │  process this chunk on their next update.            │
  └──────────────────────────────────────────────────────┘
```

## Use Case: Resetting Timers

```
  Timer workflow:
  ═══════════════

  1. Entity gets TActive = true
  2. Timer system sets Remaining = Duration, decrements each frame
  3. Remaining hits 0 → TOn = false (timer expired)
  4. Entity stays with Remaining = 0

  Problem:
  ════════
  TRemaining = 0 in every entity. No change detected.
  Downstream systems that filter on TRemaining changes
  never see these entities.

  Solution:
  ═════════
  TimerTriggerResetJob detects entities with non-zero
  remaining values and explicitly sets the change filter
  on the chunk.

  ┌──────────────────────────────────────────────────────────┐
  │                                                          │
  │  TimerTriggerResetJob is scheduled alongside a timer     │
  │  reset operation that writes new non-zero remaining      │
  │  values. The job then marks those chunks as changed      │
  │  so downstream systems process them.                     │
  │                                                          │
  └──────────────────────────────────────────────────────────┘
```

## Memory Layout

```
  Pre-allocated Zeros block (allocated externally):
  ┌────────────────────────────────────────────────────────────┐
  │ 0x00000000 0x00000000 0x00000000 ... 0x00000000           │
  │  float[0]   float[1]   float[2]       float[MAX_CHUNK-1]  │
  │  (all bits = 0, i.e. IEEE 754 +0.0f)                      │
  └────────────────────────────────────────────────────────────┘
  ▲
  │
  Zeros pointer passed to job

  Chunk's Remaining data:
  ┌────────────────────────────────────────────────────────────┐
  │ 0.0f  3.5f  0.0f  1.2f  0.0f  0.0f  2.8f  ...            │
  │  [0]   [1]   [2]   [3]   [4]   [5]   [6]                  │
  └────────────────────────────────────────────────────────────┘
                           │
  MemCmp compares byte-by-byte across sizeof(float) * chunk.Count bytes
  Returns 0 if ALL remaining values are exactly 0.0f
  Returns non-zero if ANY value is non-zero
```

## Change Filter Propagation

```
  ┌───────────────┐     ┌───────────────────────┐     ┌──────────────────┐
  │ Timer System  │     │ TimerTriggerResetJob  │     │ Downstream       │
  │               │     │                       │     │ System           │
  │ Sets Remaining│────►│ Detects non-zero      │────►│ Change filter    │
  │ to Duration   │     │ remaining values      │     │ fires on         │
  │               │     │                       │     │ TRemaining       │
  │               │     │ SetChangeFilter()     │     │ → Processes      │
  │               │     │ marks chunk dirty     │     │   entities       │
  └───────────────┘     └───────────────────────┘     └──────────────────┘

  The key insight: writing Remaining values via pointer doesn't
  automatically trigger Unity's change detection. SetChangeFilter()
  explicitly marks the component as changed on the chunk level.
```

## Integration Pattern

```
  // Typical usage in a system:
  var resetJob = new TimerTriggerResetJob<TRemaining>
  {
      RemainingHandle = remainingHandle,
      Zeros = zerosPtr,  // pre-allocated zeroed memory
  };
  state.Dependency = resetJob.ScheduleParallel(query, state.Dependency);
```

## Key Design Decisions

1. **RO Read + SetChangeFilter**: The job reads remaining values as read-only (`GetRequiredComponentDataPtrRO`), then explicitly calls `SetChangeFilter` to mark the chunk dirty. This avoids the overhead of a write unless actually needed.

2. **Bulk Comparison**: `MemCmp` compares the entire chunk's float array against a zero block in one call -- no per-entity iteration needed.

3. **External Zero Block**: The `Zeros` pointer is provided externally (not allocated by the job), avoiding per-job allocation overhead. The pointer must point to enough zeroed memory for the largest possible chunk.

4. **BurstCompiled**: Marked with `[BurstCompile]` for maximum performance.

5. **Standalone Job**: Unlike other timer utilities, this is a pure `IJobChunk` struct with no enclosing helper type -- it's meant to be composed into existing timer systems.

## Verified Data

```
with: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Model/TimerTriggerResetJob.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Model/TimerTriggerResetJob.cs)
