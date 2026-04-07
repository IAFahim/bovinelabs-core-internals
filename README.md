# EntityLock — Inner Workings

## Overview

EntityLock provides per-entity spin locks that allow parallel jobs to safely write
to the same component type on different entities without a global lock. It uses a
two-level locking scheme: a global spin lock protects a lock lookup table, and
per-entity spin locks provide fine-grained mutual exclusion.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       EntityLock (unmanaged struct)                         │
│                                                                             │
│  ┌───────────────────────┐  ┌───────────────┐                              │
│  │  LockData* pairs       │  │ SpinLock*     │   Global lock                │
│  │  (array, one per      │  │ locksLock     │   protects table             │
│  │   worker thread)      │  │               │                              │
│  └───────────────────────┘  └───────────────┘                              │
│                                                                             │
│  Acquire(entity) → Lock     Release(lock) → void                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

## LockData Structure

```
  LockData (internal, one per potential concurrent thread)
  ┌──────────────────────────────────────────────────┐
  │  int Entity        │  Entity index this slot owns │
  │  SpinLock EntityLock│  The actual per-entity lock  │
  │  int Ref            │  Reference count             │
  └──────────────────────────────────────────────────┘

  Allocated as array: length = JobsUtility.ThreadIndexCount
  ┌────────┬────────┬────────┬────────┬─────────┐
  │ Lock[0]│ Lock[1]│ Lock[2]│ Lock[3]│ ... [N] │
  └────────┴────────┴────────┴────────┴─────────┘
```

## Acquire Flow — Two-Level Locking

```
  EntityLock.Acquire(entity)
         │
         ▼
  ┌─────────────────────────────────────────────────────────────────────┐
  │  PHASE 1: Find or assign a slot (under global lock)                │
  │                                                                     │
  │  locksLock->Acquire()          ← GLOBAL SPIN LOCK                  │
  │                                                                     │
  │    ┌───────────────────────────────────────────────────────────┐    │
  │    │  Fast path: scan for existing slot for this entity        │    │
  │    │                                                           │    │
  │    │  for i in 0..length:                                     │    │
  │    │    if pairs[i].Entity == entity.Index:                    │    │
  │    │      continue  (slot already assigned to this entity)     │    │
  │    │    index = i; break                                       │    │
  │    │                                                           │    │
  │    │  Result:                                                  │    │
  │    │    index != -1 → found a slot that's NOT for this entity  │    │
  │    │    index == -1 → all slots match this entity! (rare)      │    │
  │    └───────────────────────────────────────────────────────────┘    │
  │                                                                     │
  │    ┌───────────────────────────────────────────────────────────┐    │
  │    │  If index == -1, find empty slot (Ref == 0):              │    │
  │    │                                                           │    │
  │    │  for indexEmpty in 0..length:                             │    │
  │    │    if pairs[indexEmpty].Ref == 0:                         │    │
  │    │      pairs[indexEmpty].Entity = entity.Index              │    │
  │    │      index = indexEmpty                                   │    │
  │    │      break                                                │    │
  │    └───────────────────────────────────────────────────────────┘    │
  │                                                                     │
  │  pairs[index].Ref++            ← increment ref count               │
  │  locksLock->Release()          ← RELEASE GLOBAL LOCK               │
  │                                                                     │
  │  PHASE 2: Acquire per-entity lock (OUTSIDE global lock)            │
  │                                                                     │
  │  pairs[index].EntityLock.Acquire()  ← PER-ENTITY SPIN LOCK         │
  │                                                                     │
  │  return new Lock(this, index)                                       │
  └─────────────────────────────────────────────────────────────────────┘
```

## Release Flow

```
  Lock.Dispose()  (called via EntityLock.Release or using statement)
         │
         ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  1. pairs[index].EntityLock.Release()                        │
  │     ← Release per-entity spin lock FIRST                    │
  │     (other threads waiting on this entity can now proceed)  │
  │                                                             │
  │  2. locksLock->Acquire()                                    │
  │     pairs[index].Ref--                                      │
  │     locksLock->Release()                                    │
  │     ← Decrement ref count under global lock                 │
  │     When Ref hits 0, slot is eligible for reassignment      │
  └─────────────────────────────────────────────────────────────┘
```

## Concurrency Scenario

```
  Thread A (entity 5)        Thread B (entity 5)        Thread C (entity 9)
  ═══════════════════        ═══════════════════        ═══════════════════

  Acquire(entity5)           Acquire(entity5)           Acquire(entity9)
  ┌─ global lock ─┐          ┌─ global lock ─┐          ┌─ global lock ─┐
  │ slot 0 → e5   │          │ WAITS...       │          │ slot 0 → e5   │
  │ Ref: 0→1      │          │                │          │ slot 1 → FREE │
  └─── release ───┘          │                │          │ assign e9     │
  entityLock.Acquire()       │                │          │ Ref: 0→1      │
  === HAS LOCK ===           │                │          └─── release ───┘
                             │                │          entityLock.Acquire()
  ... writing ...            │                │          === HAS LOCK ===
                             │                │
                             └─ global lock ─┐          ... writing ...
                             │ slot 0 → e5   │
                             │ Ref: 1→2      │
                             └─── release ───┘
                             entityLock.Acquire()
                             ⏳ SPINNING...
                             (waits for Thread A)

  Lock.Dispose()
  entityLock.Release()
  === RELEASED ===           ← acquires entityLock
                             === HAS LOCK ===
                             ... writing ...

                             Lock.Dispose()
                             entityLock.Release()
                             Ref: 2→1→0 → slot freed
```

## Key Design Decisions

- **Two-level locking**: Global lock is held only during table lookup (fast), then
  per-entity lock provides fine-grained parallelism.
- **Hint.Likely optimization**: The fast path checks if a slot already belongs to the
  target entity first, using branch hints for the common case.
- **Reference counting**: Multiple threads can share a slot for the same entity;
  the slot is only freed when Ref drops to 0.
- **Lock ordering**: Must release global lock BEFORE acquiring entity lock to avoid
  deadlock (documented in source comment).
- **Fixed slot count**: `length = JobsUtility.ThreadIndexCount` — one slot per
  potential worker thread limits memory usage.

## Source File

- `BovineLabs.Core/Utility/EntityLock.cs`
- `BovineLabs.Core/Utility/SpinLock.cs` (used internally)
