# LimitedRateNoCatchUpManager — Inner Workings

## Overview

LimitedRateNoCatchUpManager implements `IRateManager` to limit a component system
group to a fixed timestep rate, but crucially does NOT accumulate debt or attempt
catch-up. This prevents the "death spiral" where slow frames cause more fixed updates,
causing more slowness, ad infinitum.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│          LimitedRateNoCatchUpManager : IRateManager                         │
│                                                                             │
│  float Timestep            (target update interval, e.g. 1/30 = 0.0333s)   │
│  bool didPushTime           (true when time was pushed this frame)          │
│  double lastPushedTime      (wall-clock time of last accepted update)       │
│                                                                             │
│  ShouldGroupUpdate(group) → bool                                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Death Spiral Problem (and how this solves it)

```
  STANDARD FixedStepSimulation (WITH catch-up):
  ═══════════════════════════════════════════════

  Frame 1: dt=0.1s, target=0.033s
    → Runs 3 fixed updates (0.099s caught up)
    → 0.001s debt carried forward

  Frame 2: dt=0.2s (slow frame!)
    → Previous debt 0.001s + 0.2s = 0.201s
    → Runs 6 fixed updates → frame takes even longer
    → MORE debt accumulates

  Frame 3: dt=0.5s (spiraling!)
    → Runs 15+ fixed updates → simulation EXPLODES 💀


  LimitedRateNoCatchUpManager (NO catch-up):
  ═════════════════════════════════════════════

  Frame 1: elapsed=0.1s > timestep=0.033s → UPDATE, lastPushed=0.1
  Frame 2: elapsed=0.25s > timestep=0.033s → UPDATE, lastPushed=0.25
           (only 1 update regardless of how much time passed)
  Frame 3: elapsed=0.28s < timestep=0.033s → SKIP (not enough time)
  Frame 4: elapsed=0.35s > timestep=0.033s → UPDATE, lastPushed=0.35
           (debt from frame 2 is FORGIVEN — no spiral)
```

## ShouldGroupUpdate Algorithm

```
  ShouldGroupUpdate(group)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────────────────┐
  │                                                                           │
  │  CHECK 1: Already pushed this frame?                                      │
  │  ┌─────────────────────────────────────────────────────────────────────┐  │
  │  │  if (didPushTime):                                                  │  │
  │  │    group.World.PopTime()    ← restore previous time                 │  │
  │  │    didPushTime = false                                              │  │
  │  │    return false             ← don't update again                    │  │
  │  └─────────────────────────────────────────────────────────────────────┘  │
  │                                                                           │
  │  CHECK 2: Has enough time elapsed?                                        │
  │  ┌─────────────────────────────────────────────────────────────────────┐  │
  │  │  deltaTime = ElapsedTime - lastPushedTime                           │  │
  │  │                                                                     │  │
  │  │  if (ElapsedTime - lastPushedTime < Timestep):                      │  │
  │  │    return false             ← not enough time yet, skip             │  │
  │  └─────────────────────────────────────────────────────────────────────┘  │
  │                                                                           │
  │  UPDATE: Accept this frame                                                │
  │  ┌─────────────────────────────────────────────────────────────────────┐  │
  │  │  lastPushedTime = ElapsedTime         ← remember when we updated    │  │
  │  │                                                                     │  │
  │  │  World.PushTime(                     ← override time for children   │  │
  │  │    TimeData(ElapsedTime, deltaTime)   ←   elapsed=now, dt=real gap  │  │
  │  │  )                                                                  │  │
  │  │                                                                     │  │
  │  │  didPushTime = true                   ← mark for cleanup next call  │  │
  │  │  return true                          ← yes, run the group!         │  │
  └─────────────────────────────────────────────────────────────────────┘  │
  └───────────────────────────────────────────────────────────────────────────┘
```

## Timeline Visualization

```
  Wall clock:  0.0   0.033   0.050   0.100   0.120   0.200   0.233   0.300
               │      │       │       │       │       │       │       │
  Target:      ▼      ▼               ▼               ▼       ▼       ▼
  (every 0.033s)

  Actual:      U      -       -       U       -       -       U       U
  updates:     ▲                              ▲                       ▲
  lastPushed:  0.0                          0.100                  0.300

  U = Update ran   - = Skipped (not enough time or already pushed)

  Note: Between 0.100 and 0.300, 0.2s elapsed (6 timesteps of debt)
  but only ONE update runs at 0.300 — the 0.167s of debt is FORGIVEN.
```

## Push/Pop Time Stack

```
  ┌──────────────────────────────────────────────────────────────────┐
  │  World Time Stack:                                               │
  │                                                                  │
  │  Before PushTime:                                                │
  │    [TimeData(elapsed=0.300, dt=0.016)]  ← real frame time       │
  │                                                                  │
  │  PushTime(TimeData(0.300, 0.200)):                               │
  │    [TimeData(elapsed=0.300, dt=0.016)]  ← saved                 │
  │    [TimeData(elapsed=0.300, dt=0.200)]  ← active (large delta!) │
  │                                                                  │
  │  Child systems see deltaTime=0.200                               │
  │  (They get the REAL time gap, not the target timestep)           │
  │                                                                  │
  │  PopTime (next ShouldGroupUpdate call):                          │
  │    [TimeData(elapsed=0.300, dt=0.016)]  ← restored              │
  └──────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **No debt accumulation**: `lastPushedTime` is set to current `ElapsedTime`, not
  `lastPushedTime + Timestep`. This means any "missed" timesteps are simply skipped.
- **Real delta time**: Systems receive the actual time gap since last update, not
  the fixed timestep. This means simulation speed varies slightly but never spirals.
- **Push/Pop pattern**: Uses Unity's built-in `World.PushTime/PopTime` to safely
  override time for child systems and restore it afterward.
- **Single update per frame**: `didPushTime` flag ensures at most one update per
  frame even if the group's ShouldGroupUpdate is called multiple times.

## Source File

- `BovineLabs.Core/Utility/LimitedRateNoCatchUpManager.cs`
