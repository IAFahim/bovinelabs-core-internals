# TimerEnableable - Inner Workings

## Overview

`TimerEnableable<TOn, TRemaining, TActive, TDuration>` is similar to `Timer<TOn,...>` but uses **enableable components** (IEnableableComponent) for `TOn` and `TActive` instead of data values. This means entities are automatically enabled/disabled at the chunk level when their timer completes, allowing other systems to query for enabled timers without checking a bool field.

```
File: BovineLabs.Core/Model/TimerEnableable.cs
Generic Parameters:
  TOn        - Enableable component: timer is active (IEnableableComponent)
  TRemaining - float-sized component: time remaining
  TActive    - Enableable component: trigger to start/restart (IEnableableComponent)
  TDuration  - float-sized component: per-entity duration
Key: TOn and TActive use ENABLED BITS, not bool data values
```

## Architecture

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │         TimerEnableable<TOn, TRemaining, TActive, TDuration>            │
 │                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  OnCreate(ref SystemState)                                       │  │
 │  │                                                                  │  │
 │  │  Query:                                                          │  │
 │  │    .WithAllRW<TRemaining, TOn>()                                │  │
 │  │    .WithAll<TActive, TDuration>()                                │  │
 │  │    .WithOptions(                                                 │  │
 │  │      IgnoreComponentEnabledState  ◄── KEY: ignores enable bits   │  │
 │  │      | FilterWriteGroup          ◄── allows coexisting timers    │  │
 │  │    )                                                             │  │
 │  │                                                                  │  │
 │  │  TOn      → RW handle (enable bits will be modified)            │  │
 │  │  TRemaining → RW handle (float values decremented)              │  │
 │  │  TActive    → RO handle (enable bits read as trigger)           │  │
 │  │  TDuration  → RO handle (float durations read)                  │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 │                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────┐  │
 │  │  OnUpdate(ref SystemState, UpdateTimeJob)                        │  │
 │  │                                                                  │  │
 │  │  job.DeltaTime = WorldUnmanaged.Time.DeltaTime                   │  │
 │  │  job.SystemVersion = LastSystemVersion                           │  │
 │  │  ScheduleParallel(query, dependency)                             │  │
 │  └───────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────┘
```

## UpdateTimeJob - Dual Phase Processing

```
 ┌─────────────────────────────────────────────────────────────────────────┐
 │                UpdateTimeJob : IJobChunk                                │
 │                                                                         │
 │  ═══════════════════════════════════════════════════════════════════     │
 │  PHASE 1: Handle new activations (activeChanged)                       │
 │  ═══════════════════════════════════════════════════════════════════     │
 │                                                                         │
 │    triggers    = GetRequiredEnabledBitsRO(TActive)  ◄── BITWISE        │
 │    durationOns = GetRequiredEnabledBitsRO(TOn)      ◄── BITWISE        │
 │    durations   = GetRequiredComponentDataPtrRO(TDuration) (float*)     │
 │    remainings  = GetRequiredComponentDataPtrRW(TRemaining) (float*)    │
 │                                                                         │
 │    for i in 0..chunk.Count:                                            │
 │      ┌──────────────────────────────────────────────────────────┐      │
 │      │  IF Bitwise.IsSet(triggers, i)     // TActive enabled   │      │
 │      │  AND NOT Bitwise.IsSet(durationOns, i) // TOn disabled  │      │
 │      │  THEN                                                    │      │
 │      │      remainings[i] = durations[i]                        │      │
 │      │      // (set timer to per-entity duration)               │      │
 │      └──────────────────────────────────────────────────────────┘      │
 │                                                                         │
 │  ═══════════════════════════════════════════════════════════════════     │
 │  PHASE 2: Decrement timers & update TOn enable bits                    │
 │  ═══════════════════════════════════════════════════════════════════     │
 │                                                                         │
 │    original = GetRequiredEnabledBitsRO(TOn)   ◄── current enable mask  │
 │    updated  = original  (copy by value)                                │
 │    remainings = ptr to float array                                     │
 │                                                                         │
 │    CalculateOn(remainings, &updated, count, deltaTime):                │
 │      for i in 0..min(64, count):           // First 64 bits (ULong0)  │
 │        remainings[i] = max(0, remainings[i] - dt)                      │
 │        UnsafeBitArray.Set(isOn, i, remainings[i] != 0)                 │
 │      for i in 0..min(64, count - 64):      // Next 64 bits (ULong1)   │
 │        remainings[i+64] = max(0, remainings[i+64] - dt)               │
 │        UnsafeBitArray.Set(isOn+1, i, remainings[i+64] != 0)           │
 │                                                                         │
 │    ┌─────────────────────────────────────────────────────────────┐     │
 │    │  Compare: updated.ULong0 != original.ULong0                │     │
 │    │          || updated.ULong1 != original.ULong1              │     │
 │    │                                                             │     │
 │    │  IF hasChanged:                                             │     │
 │    │    GetRequiredEnabledBitsRW(TOn, out count)                 │     │
 │    │    enabledBits = updated                                    │     │
 │    │    *count = chunk.Count - countbits(ULong0) - countbits()  │     │
 │    │    // count = number of DISABLED entities                   │     │
 │    └─────────────────────────────────────────────────────────────┘     │
 └─────────────────────────────────────────────────────────────────────────┘
```

## Enable Bit Operations Detail

```
  TOn Enable Bits (128-bit v128 = ULong0 + ULong1):

  Before decrement:
  ┌─────────────────────────────────┬─────────────────────────────────┐
  │            ULong0               │            ULong1               │
  │  1 1 1 1 0 0 1 1 ...           │  1 0 0 0 1 1 1 0 ...           │
  │  [6 entities enabled]           │  [4 entities enabled]           │
  └─────────────────────────────────┴─────────────────────────────────┘
                    │
                    ▼  CalculateOn(): decrement & compute new bits
  After decrement:
  ┌─────────────────────────────────┬─────────────────────────────────┐
  │            ULong0               │            ULong1               │
  │  1 1 0 1 0 0 1 0 ...           │  0 0 0 0 1 0 1 0 ...           │
  │  [4 enabled]  ^ ^               │  [2 enabled]                     │
  │               │ │                │                                 │
  │        timers expired!           │                                 │
  └─────────────────────────────────┴─────────────────────────────────┘

  DisabledCount = chunk.Count - countbits(ULong0) - countbits(ULong1)
                = 128 - 4 - 2 = 122 entities disabled
```

## State Machine (Enableable Variant)

```
                    Enable TActive component
                    on entity
       ┌──────────────────────────────────────┐
       │                                      │
       ▼                                      │
  ┌─────────┐                          ┌──────────────┐
  │  IDLE   │  TActive enabled         │  COUNTING    │
  │  TOn=   │  AND TOn disabled        │  TOn=        │
  │  disab. │─────────────────────►     │  enabled     │
  │  R=0    │  Remaining = Duration    │  R = Duration │
  └─────────┘                          └──────┬───────┘
       ▲                                      │
       │                              Each frame (ignored enabled state):
       │                              R = max(0, R - dt)
       │                              Update TOn enable bits
       │                                      │
       │                                      ▼
       │                              ┌──────────────────┐
       │                              │ R reaches 0       │
       │                              │ TOn bits → 0      │
       │                              │ (entity disabled)  │
       │                              └──────┬───────────┘
       │                                     │
       └─────────────────────────────────────┘
                    TOn disabled, Remaining = 0
```

## Key Difference from Timer/TimerFixed

```
  Timer/TimerFixed:
  ┌──────────────────────────────────────────────────────────┐
  │  TOn is a BOOL component value                          │
  │  Other systems must check: if(TOn.value == true) {...}  │
  │  Entity archetype unchanged when timer fires            │
  └──────────────────────────────────────────────────────────┘

  TimerEnableable:
  ┌──────────────────────────────────────────────────────────┐
  │  TOn is an ENABLEABLE component                         │
  │  Other systems query: WithPresent<TOn>() → only runs on │
  │  entities where TOn is enabled (no per-entity check)    │
  │  Query-based filtering: chunk-level iteration is faster  │
  │  DisabledCount tracks how many entities have timer off   │
  └──────────────────────────────────────────────────────────┘
```

## Bitwise Operations Flow

```
  Bitwise.IsSet(ulong* ptr, int index):
  ┌──────────────────────────────────────────┐
  │  word = index / 64                        │
  │  bit  = index % 64                        │
  │  return (ptr[word] >> bit) & 1 != 0      │
  └──────────────────────────────────────────┘

  UnsafeBitArray.Set(ulong* ptr, int index, bool value):
  ┌──────────────────────────────────────────┐
  │  word = index / 64                        │
  │  bit  = index % 64                        │
  │  IF value:                                │
  │    ptr[word] |= (1UL << bit)              │
  │  ELSE:                                    │
  │    ptr[word] &= ~(1UL << bit)             │
  └──────────────────────────────────────────┘

  CalculateOn processes in two batches:
  ┌──────────────────────────────────────────────────────┐
  │  Batch 1: indices 0..63   → ULong0 (first ulong)    │
  │  Batch 2: indices 64..127 → ULong1 (second ulong)   │
  │  Max chunk size = 128 entities                       │
  └──────────────────────────────────────────────────────┘
```

## Key Design Decisions

1. **IgnoreComponentEnabledState**: The query iterates ALL entities regardless of enable state, because the job itself manages enable bits.

2. **Enable Bits instead of Data**: Using enable bits for TOn/TActive means other systems can use `WithPresent<TOn>()` to only process entities with active timers - zero per-entity overhead.

3. **DisabledCount Maintenance**: After updating enable bits, the job manually updates the chunk's disabled count: `*count = chunk.Count - countbits(ULong0) - countbits(ULong1)`.

4. **v128 Copy-by-Value**: The enable bits (`EnabledMask`/`v128`) are copied by value (`var updated = original`), modified, then compared. This avoids read-write aliasing.

5. **Per-Entity Duration**: Unlike TimerFixed, each entity can have a different duration via the `TDuration` component.

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Model/TimerEnableable.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Model/TimerEnableable.cs)
- [BovineLabs.Core.Tests/Models/TimerEnableableTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Models/TimerEnableableTests.cs)
