# TimerFixed - Inner Workings

## Overview

`TimerFixed<TOn, TRemaining, TActive>` decrements fixed-duration float timers across entity chunks with high efficiency. Unlike the variable-duration `Timer<TOn, TRemaining, TActive, TDuration>`, this version takes a fixed duration at construction time, eliminating the need for a per-entity `TDuration` component.

```
File: BovineLabs.Core/Model/TimerFixed.cs
Generic Parameters:
  TOn        - bool-sized component: is timer currently active? (IComponentData)
  TRemaining - float-sized component: time left on timer (IComponentData)
  TActive    - bool-sized component: trigger to start/restart the timer (IComponentData)
Constructor: TimerFixed(float duration)  -- fixed duration for ALL entities
```

## Architecture

```
 ┌─────────────────────────────────────────────────────────────────────┐
 │                  TimerFixed<TOn, TRemaining, TActive>               │
 │                                                                     │
 │  ┌─────────────────────────────────────────────────────────────┐    │
 │  │  Constructor: TimerFixed(float duration)                    │    │
 │  │                                                             │    │
 │  │  • Stores fixed duration for all entities                  │    │
 │  │  • Assert.AreNotEqual(0, duration) -- must be non-zero     │    │
 │  └─────────────────────────────────────────────────────────────┘    │
 │                                                                     │
 │  ┌─────────────────────────────────────────────────────────────┐    │
 │  │  OnCreate(ref SystemState)                                  │    │
 │  │                                                             │    │
 │  │  Assertions:                                                │    │
 │  │    SizeOf(TOn)        == SizeOf(bool)                       │    │
 │  │    SizeOf(TRemaining) == SizeOf(float)                      │    │
 │  │    SizeOf(TActive)    == SizeOf(bool)                       │    │
 │  │                                                             │    │
 │  │  Query: .WithAllRW<TRemaining, TOn>()                      │    │
 │  │         .WithAll<TActive>()                                 │    │
 │  │         .FilterWriteGroup                                   │    │
 │  └─────────────────────────────────────────────────────────────┘    │
 │                                                                     │
 │  ┌─────────────────────────────────────────────────────────────┐    │
 │  │  OnUpdate(ref SystemState, UpdateTimeJob)                   │    │
 │  │                                                             │    │
 │  │  1. Update all type handles                                 │    │
 │  │  2. Set job.Duration  = this.duration (fixed!)              │    │
 │  │  3. Set job.DeltaTime = WorldUnmanaged.Time.DeltaTime       │    │
 │  │  4. Set job.SystemVersion = LastSystemVersion               │    │
 │  │  5. ScheduleParallel(query, dependency)                     │    │
 │  └─────────────────────────────────────────────────────────────┘    │
 └─────────────────────────────────────────────────────────────────────┘
```

## UpdateTimeJob - Chunk Processing Pipeline

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │                    UpdateTimeJob : IJobChunk                            │
 │                                                                         │
 │  Phase 1: Check if TActive changed                                     │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  activeChanged = chunk.DidChange(ref ActiveHandle, SystemVersion) │  │
 │  │                                                                   │  │
 │  │  IF activeChanged:                                                │  │
 │  │    for each entity in chunk:                                      │  │
 │  │      ┌──────────────────────────────────────────────────────┐     │  │
 │  │      │  IF triggers[i] == true  AND  durationOns[i] == false│     │  │
 │  │      │          (TActive is on)      (TOn is off)           │     │  │
 │  │      │  THEN                                                  │     │  │
 │  │      │      remainings[i] = this.Duration  ◄── FIXED value  │     │  │
 │  │      │      (reset timer to full duration)                   │     │  │
 │  │      └──────────────────────────────────────────────────────┘     │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 │                                 │                                       │
 │                                 ▼                                       │
 │  Phase 2: Decrement & compute On state                                 │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  IF activeChanged || DidChange(ref RemainingHandle):             │  │
 │  │                                                                   │  │
 │  │    CalculateOnVectorized(remainings, isOn, length, deltaTime):    │  │
 │  │      for i in 0..length:                                          │  │
 │  │        remainings[i] = max(0, remainings[i] - deltaTime)          │  │
 │  │        isOn[i] = (remainings[i] != 0)                             │  │
 │  │                                                                   │  │
 │  │    ┌─────────────────────────────────────────────────────────┐    │  │
 │  │    │  MemCmp Optimization:                                   │    │  │
 │  │    │  Compare new On[] with existing On[]                    │    │  │
 │  │    │                                                         │    │  │
 │  │    │  IF hasChanged:                                         │    │  │
 │  │    │    Copy isOn buffer → chunk's OnHandle (write back)    │    │  │
 │  │    │  ELSE:                                                  │    │  │
 │  │    │    Skip write (avoids triggering change filters!)      │    │  │
 │  │    └─────────────────────────────────────────────────────────┘    │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────┘
```

## Timer State Machine

```
                    TActive = true
                    (trigger)
       ┌──────────────────────────────────────┐
       │                                      │
       ▼                                      │
  ┌─────────┐                          ┌─────────┐
  │  IDLE   │  TActive set to true     │  TICK   │
  │  TOn=0  │  AND TOn was false       │  TOn=1  │
  │  R=0    │──────────────────────►    │  R=DUR  │
  └─────────┘                           └────┬────┘
       ▲                                      │
       │                              Each frame:
       │                              R = max(0, R - dt)
       │                                      │
       │                                      ▼
       │                              ┌─────────────┐
       │                              │ R reaches 0  │
       │                              │ TOn → false  │
       │                              └──────┬──────┘
       │                                     │
       └─────────────────────────────────────┘
                    TOn = false, R = 0
                    Back to IDLE

  Where:
    R   = TRemaining value
    DUR = fixed duration (constructor parameter)
    dt  = DeltaTime per frame
```

## Per-Entity Timeline

```
  Entity E:  TActive=0, TOn=0, Remaining=0.0

  Frame 5:  TActive set to 1
            ┌─── TOn was false, so:
            │   Remaining = Duration (e.g. 3.0)
            │   TOn = true
            ▼
            Remaining: 3.0 → 2.9 → 2.8 → ... → 0.1 → 0.0
            TOn:       1    1    1         1    1    0
            ───────────────────────────────────────────► time
                                            ▲
                                            │ Remaining hits 0
                                            │ TOn → false
                                            │ Entity returns to idle

  Frame 20: TActive set to 1 again
            Remaining reset to 3.0, TOn → true, cycle repeats
```

## Vectorization Strategy

```
  CalculateOnVectorized() with LOOP.ExpectVectorized():
  ┌───────────────────────────────────────────────────┐
  │  for i in 0..chunk.Count:                         │
  │    remainings[i] = max(0, remainings[i] - dt)     │
  │    isOn[i]       = remainings[i] != 0             │
  │                                                   │
  │  Burst compiler auto-vectorizes this tight loop   │
  │  → SIMD subtractions and comparisons              │
  │  → No branching inside the loop body              │
  └───────────────────────────────────────────────────┘

  MemCmp guard:
  ┌───────────────────────────────────────────────────┐
  │  // Read existing On values (RO)                  │
  │  original = chunk.GetComponentDataPtrRO(OnHandle) │
  │                                                   │
  │  // Compare with computed buffer                  │
  │  hasChanged = MemCmp(original, updated, size)     │
  │                                                   │
  │  // Only write back if actually changed           │
  │  // → Avoids false change filter triggers         │
  │  IF hasChanged:                                   │
  │    chunk.GetNativeArray(OnHandle).CopyFrom(buf)   │
  └───────────────────────────────────────────────────┘
```

## Comparison: Timer vs TimerFixed

```
  Timer<TOn, TRemaining, TActive, TDuration>
  ├── Requires TDuration component on every entity
  ├── Per-entity durations (flexible)
  └── 4 component handles

  TimerFixed<TOn, TRemaining, TActive>
  ├── No TDuration component needed
  ├── Single fixed duration shared by all entities
  ├── 3 component handles
  └── Less memory per entity (no float duration stored)
```

## Key Design Decisions

1. **Fixed Duration**: By baking the duration into the struct at construction, we avoid a per-entity `TDuration` component, saving memory and a component handle lookup.

2. **Write Group Filtering**: `EntityQueryOptions.FilterWriteGroup` allows multiple timer systems to coexist without conflict.

3. **MemCmp Guard**: Only writes `TOn` values back to the chunk when they actually changed, preventing downstream change-filter cascade.

4. **Temp Buffer**: Uses a `NativeList<bool>` (reused across chunks) to compute new `On` values before comparing, avoiding read-write aliasing issues.

5. **Change Detection**: Both `TActive` changes and `TRemaining` changes trigger recalculation, ensuring `TOn` stays consistent.

## Source

- [BovineLabs.Core/Model/TimerFixed.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Model/TimerFixed.cs)
