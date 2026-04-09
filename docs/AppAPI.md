# AppAPI - Inner Workings

## Overview

`AppAPI` is a static helper class providing main-thread access to `IState<T>` ECS singleton components. It offers methods to read, set, enable, and disable individual state bits within a bitmask using either string names (resolved via `KSettingsBase`) or raw byte keys.

```
File: BovineLabs.Core/States/AppAPI.cs
Class: AppAPI (static)
Target: IState<T> singletons where T : IBitArray<T>
Methods: StateCurrent, StateIsEnabled, StateSet, StateEnable, StateDisable
```

## API Surface

```
  ┌─────────────────────────────────────────────────────────────────────────┐
  │                        AppAPI (static)                                  │
  │                                                                         │
  │  ┌───────────────────────────────────────────────────────────────────┐  │
  │  │  READ Methods                                                     │  │
  │  │                                                                   │  │
  │  │  StateCurrent<T, TA>(EntityManager)       → TA (bitmask value)   │  │
  │  │  StateCurrent<T, TA>(ref SystemState)     → TA                   │  │
  │  │  StateIsEnabled<T, TA, TS>(EntityManager, name) → bool           │  │
  │  └───────────────────────────────────────────────────────────────────┘  │
  │                                                                         │
  │  ┌───────────────────────────────────────────────────────────────────┐  │
  │  │  WRITE Methods (exclusive - clears all, sets one)                 │  │
  │  │                                                                   │  │
  │  │  StateSet<T, TA, TS>(EntityManager, name)                        │  │
  │  │  StateSet<T, TA, TS>(ref SystemState, name)                      │  │
  │  │  StateSet<T, TA>(EntityManager, byte key)                        │  │
  │  │  StateSet<T, TA>(ref SystemState, byte key)                      │  │
  │  └───────────────────────────────────────────────────────────────────┘  │
  │                                                                         │
  │  ┌───────────────────────────────────────────────────────────────────┐  │
  │  │  ADDITIVE Methods (sets/clears one bit, keeps others)             │  │
  │  │                                                                   │  │
  │  │  StateEnable<T, TA, TS>(EntityManager, name)                     │  │
  │  │  StateEnable<T, TA, TS>(ref SystemState, name)                   │  │
  │  │  StateEnable<T, TA>(EntityManager, byte key)                     │  │
  │  │  StateEnable<T, TA>(ref SystemState, byte key)                   │  │
  │  │                                                                   │  │
  │  │  StateDisable<T, TA, TS>(EntityManager, name)                    │  │
  │  │  StateDisable<T, TA, TS>(ref SystemState, name)                  │  │
  │  │  StateDisable<T, TA>(EntityManager, byte key)                    │  │
  │  │  StateDisable<T, TA>(ref SystemState, byte key)                  │  │
  │  └───────────────────────────────────────────────────────────────────┘  │
  └─────────────────────────────────────────────────────────────────────────┘
```

## StateCurrent - Read Operation

```
  AppAPI.StateCurrent<T, TA>(entityManager):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  entityManager.GetSingleton<T>().Value                        │
  │       │                                                       │
  │       ▼                                                       │
  │  ┌─────────────────────────────────────────────────────────┐ │
  │  │  IState singleton entity                                │ │
  │  │  ┌───────────────────────────────────────────────────┐ │ │
  │  │  │  GameState : IState<BitArray256>                   │ │ │
  │  │  │  Value = BitArray256 {                             │ │ │
  │  │  │    bits: 0b10110100... (256 bits total)            │ │ │
  │  │  │  }                                                 │ │ │
  │  │  └───────────────────────────────────────────────────┘ │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │       │                                                       │
  │       ▼                                                       │
  │  Returns: BitArray256 (copy of the entire bitmask)            │
  └───────────────────────────────────────────────────────────────┘
```

## StateIsEnabled - Check Single State

```
  AppAPI.StateIsEnabled<T, TA, TS>(entityManager, "Running"):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  Step 1: key = KSettingsBase<TS, byte>.NameToKey("Running")  │
  │          key = 3                                              │
  │                                                               │
  │  Step 2: TryGetSingleton<T> → GameState                      │
  │          IF not found → return false                          │
  │                                                               │
  │  Step 3: return v.Value[key]                                  │
  │          = v.Value[3]                                         │
  │          = (bit 3 of bitmask) → true/false                    │
  │                                                               │
  │  BitArray256[3]:                                              │
  │  ┌─────────────────────────────────────────────────────┐     │
  │  │  Index: 7  6  5  4  3  2  1  0                      │     │
  │  │  Bits:  0  1  0  1  1  0  1  0                      │     │
  │  │                     ↑                                │     │
  │  │                     key=3 → returns TRUE             │     │
  │  └─────────────────────────────────────────────────────┘     │
  └───────────────────────────────────────────────────────────────┘
```

## StateSet - Exclusive Set (Clears All Others)

```
  AppAPI.StateSet<T, TA, TS>(entityManager, "Paused"):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  Step 1: key = NameToKey("Paused") → key = 7                 │
  │                                                               │
  │  Step 2: Create new state with ONLY key bit set               │
  │          new T { Value = new TA { [7] = true } }              │
  │                                                               │
  │  Before:  Value = 0b10110100 (Running, Armed, Idle active)    │
  │  After:   Value = 0b10000000 (ONLY Paused active)             │
  │                    ↑                                          │
  │                    bit 7 = Paused                             │
  │                                                               │
  │  Step 3: entityManager.SetSingleton(newState)                 │
  └───────────────────────────────────────────────────────────────┘
```

## StateEnable/Disable - Additive Bit Operations

```
  StateEnable<T, TA>(entityManager, byte state):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  Internal: StateEnable<T, TA>(entityManager, state, true)     │
  │                                                               │
  │  Step 1: gameState = entityManager.GetSingletonRW<T>()        │
  │                                                               │
  │  Step 2: ref var r = ref UnsafeUtility.As<T, BitArray256>(    │
  │                           ref gameState.ValueRW);             │
  │          r[state] = true;                                     │
  │                                                               │
  │  Before:  Value = 0b10110000                                  │
  │  Enable key=2 (Armed):                                        │
  │  After:   Value = 0b10110100                                  │
  │                      ↑                                        │
  │                      bit 2 set, others unchanged               │
  └───────────────────────────────────────────────────────────────┘

  StateDisable<T, TA>(entityManager, byte state):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  Internal: StateEnable<T, TA>(entityManager, state, false)    │
  │                                                               │
  │  Before:  Value = 0b10110100                                  │
  │  Disable key=4 (Running):                                     │
  │  After:   Value = 0b10100100                                  │
  │                    ↑                                          │
  │                    bit 4 cleared, others unchanged             │
  └───────────────────────────────────────────────────────────────┘
```

## Internal Unsafe Bit Manipulation

```
  StateEnable<T, TA>(EntityManager, byte state, bool enabled):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  // Get RW reference to singleton                             │
  │  var gameState = entityManager.GetSingletonRW<T>();           │
  │                                                               │
  │  // Cast to BitArray256 via UnsafeUtility.As                 │
  │  // (works because T's layout starts with Value field)        │
  │  ref var r = ref UnsafeUtility.As<T, BitArray256>(            │
  │                  ref gameState.ValueRW);                      │
  │                                                               │
  │  // Set or clear the bit                                      │
  │  r[state] = enabled;                                          │
  │                                                               │
  │  ┌─────────────────────────────────────────────────────────┐ │
  │  │  BitArray256 indexer:                                    │ │
  │  │  set {                                                    │ │
  │  │    int word = index / 64;                                │ │
  │  │    int bit  = index % 64;                                │ │
  │  │    if (value) bits[word] |=  (1UL << bit);               │ │
  │  │    else       bits[word] &= ~(1UL << bit);               │ │
  │  │  }                                                        │ │
  │  └─────────────────────────────────────────────────────────┘ │
  └───────────────────────────────────────────────────────────────┘
```

## Debug Logging

```
  #if UNITY_EDITOR || BL_DEBUG
  ┌───────────────────────────────────────────────────────────────┐
  │  Each write operation logs via BLLogger singleton:            │
  │                                                               │
  │  StateSet:     "GameState set to Running"                     │
  │  StateEnable:  "GameState enabled Running"                    │
  │  StateDisable: "GameState disabled Jumping"                   │
  │                                                               │
  │  Uses TypeManagerEx.GetTypeName() for state component name    │
  └───────────────────────────────────────────────────────────────┘
  #endif
```

## EntityManager vs SystemState Overloads

```
  Every method has two overloads:
  ┌─────────────────────────────────────────────────────────────┐
  │  Method(EntityManager)    → calls method directly on EM     │
  │  Method(ref SystemState)  → delegates to EM overload        │
  │                                                             │
  │  The SystemState overload just extracts EntityManager:       │
  │    StateSet<T, TA, TS>(systemState.EntityManager, name)     │
  └─────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

1. **Static API**: No instantiation needed. Call directly: `AppAPI.StateEnable<GameState, BitArray256, GameSettings>(em, "Running")`.

2. **Singleton Access**: All operations assume the `IState<T>` component exists as a singleton. `GetSingleton`/`SetSingleton`/`GetSingletonRW` provide direct access.

3. **String-to-Key Resolution**: `KSettingsBase<TS, byte>.NameToKey()` converts string names to byte indices, enabling data-driven configuration.

4. **UnsafeUtility.As Cast**: The internal `StateEnable` method reinterprets the component as `BitArray256` for direct bit manipulation without boxing or virtual dispatch.

5. **AggressiveInlining**: All public methods are marked `[MethodImpl(AggressiveInlining)]` for zero-overhead abstraction.

6. **Conditional Debug Logging**: Log messages only in `UNITY_EDITOR` or `BL_DEBUG` builds, ensuring zero overhead in release.

## Verified Data

```
providing: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/States/AppAPI.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/AppAPI.cs)
