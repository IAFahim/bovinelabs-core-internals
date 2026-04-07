# InitSystemBase — Inner Workings

## Overview

InitSystemBase is an abstract base class for ECS systems that should execute exactly
once during initialization and then automatically remove themselves from the update
loop, eliminating per-frame overhead.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    InitSystemBase : SystemBase                               │
│                                                                             │
│  [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]      │
│                                                                             │
│  OnUpdate():                                                                │
│    1. Get InitializationSystemGroup                                         │
│    2. RemoveSystemFromUpdateList(this)                                      │
│    3. NEVER RUNS AGAIN                                                      │
│                                                                             │
│  Subclass overrides: NOTHING (OnUpdate is sealed by design)                 │
│  Subclass implements: protected override void OnDestroy() etc.              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Lifecycle Flow

```
  App Start
     │
     ▼
  ┌──────────────────────────────────────────────────────────────────────┐
  │  InitializationSystemGroup                                           │
  │                                                                      │
  │  ┌─ Update Order (OrderFirst=true, so InitSystem runs first): ─────┐│
  │  │                                                                  ││
  │  │  1. InitSystemBase.OnUpdate()                                    ││
  │  │     │                                                            ││
  │  │     ├── World.GetExistingSystemManaged<InitializationSystemGroup>()│
  │  │     │                                                            ││
  │  │     └── initialization.RemoveSystemFromUpdateList(this)           ││
  │  │        │                                                         ││
  │  │        ▼                                                         ││
  │  │  ┌──────────────────────────────────────────────────────────┐    ││
  │  │  │  Before:  [InitSys, OtherSys, AnotherSys, ...]           │    ││
  │  │  │            ^^^^^^^^                                       │    ││
  │  │  │            this system                                    │    ││
  │  │  │                                                          │    ││
  │  │  │  After:   [OtherSys, AnotherSys, ...]                    │    ││
  │  │  │            InitSys is GONE from update list              │    ││
  │  │  └──────────────────────────────────────────────────────────┘    ││
  │  │                                                                  ││
  │  │  2. OtherSys.OnUpdate()  ← normal systems continue              ││
  │  │  3. AnotherSys.OnUpdate()                                       ││
  │  └──────────────────────────────────────────────────────────────────┘│
  │                                                                      │
  │  Next Frame:                                                         │
  │  InitSystemBase is NOT in the list → zero overhead!                  │
  └──────────────────────────────────────────────────────────────────────┘
```

## Update Timeline

```
  Frame 1 (InitializationSystemGroup):
  ═════════════════════════════════════
  ┌────────────────────────────────────────────────────────────┐
  │  InitSystemA.OnUpdate()  → removes self ──► X (gone)      │
  │  InitSystemB.OnUpdate()  → removes self ──► X (gone)      │
  │  RegularSystem.OnUpdate() → runs normally                  │
  └────────────────────────────────────────────────────────────┘

  Frame 2+:
  ══════
  ┌────────────────────────────────────────────────────────────┐
  │  RegularSystem.OnUpdate() → runs normally                  │
  │                                                            │
  │  InitSystemA: NOT IN LIST (zero cost)                      │
  │  InitSystemB: NOT IN LIST (zero cost)                      │
  └────────────────────────────────────────────────────────────┘
```

## Scheduling Context

```
  ┌──────────────────────────────────────────────────────────────────┐
  │  InitializationSystemGroup                                       │
  │  ┌─ OrderFirst systems ──────────────────────────────────────┐  │
  │  │  InitSystemBase subclasses run FIRST                       │  │
  │  │  (OrderFirst = true attribute)                             │  │
  │  └───────────────────────────────────────────────────────────┘  │
  │  ┌─ Regular systems ─────────────────────────────────────────┐  │
  │  │  Normal init systems run after                             │  │
  │  └───────────────────────────────────────────────────────────┘  │
  └──────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Self-removing**: The system removes itself during its first (and only) update,
  rather than using a `bool hasInitialized` check that costs a branch every frame.
- **OrderFirst = true**: Ensures initialization systems run before any other system
  in the `InitializationSystemGroup`, guaranteeing setup is complete.
- **Minimal abstract class**: No abstract methods to implement — subclasses just
  exist in the group and are removed after one run. Custom init logic goes in
  the constructor or `OnCreate`.
- **Not destroyed**: The system object still exists in the World; it's just removed
  from the update list. It can be re-added if needed.

## Source File

- `BovineLabs.Core/Utility/InitSystemBase.cs`
