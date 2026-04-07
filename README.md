# IState - Inner Workings

## Overview

`IState<T>` is a generic interface that acts as a contract for state components used with the BovineLabs Core state system. It ensures that any state component contains a `Value` property of type `T` where `T` implements `IBitArray<T>`, providing bitmask-based state representation.

```
File: BovineLabs.Core/States/IState.cs
Interface: IState<T> : IComponentData
Constraint: T : unmanaged, IBitArray<T>
Property: T Value { get; set; }
```

## Interface Definition

```
  ┌────────────────────────────────────────────────────────────────┐
  │                   IState<T> : IComponentData                   │
  │                                                                │
  │  where T : unmanaged, IBitArray<T>                            │
  │                                                                │
  │  ┌──────────────────────────────────────────────────────────┐ │
  │  │  T Value { get; set; }                                   │ │
  │  │       │                                                  │ │
  │  │       ▼                                                  │ │
  │  │  ┌───────────────────────────────────────────────────┐   │ │
  │  │  │  IBitArray<T> contract:                           │   │ │
  │  │  │  • bool this[byte index] { get; set; }            │   │ │
  │  │  │  • Bit-array access by byte key                   │   │ │
  │  │  │  • Various sizes: BitArray8, BitArray32,          │   │ │
  │  │  │    BitArray64, BitArray256, etc.                  │   │ │
  │  │  └───────────────────────────────────────────────────┘   │ │
  │  └──────────────────────────────────────────────────────────┘ │
  └────────────────────────────────────────────────────────────────┘
```

## Type Hierarchy

```
  ┌─────────────────────────────────┐
  │  IComponentData                 │  (Unity ECS base)
  │  ─────────────────────          │
  └──────────────┬──────────────────┘
                 │ implements
                 ▼
  ┌─────────────────────────────────┐
  │  IState<T>                      │  (BovineLabs Core)
  │  ─────────────────────          │
  │  where T : IBitArray<T>         │
  │                                 │
  │  T Value { get; set; }         │
  └──────────────┬──────────────────┘
                 │ implemented by
                 ▼
  ┌──────────────────────────────────────────────────────────────┐
  │  Concrete State Component (user-defined)                     │
  │                                                              │
  │  public struct GameState : IState<BitArray256>               │
  │  {                                                           │
  │      public BitArray256 Value { get; set; }                  │
  │  }                                                           │
  │                                                              │
  │  This component is a SINGLETON on an entity                  │
  │  Accessed via AppAPI methods                                 │
  └──────────────────────────────────────────────────────────────┘
```

## BitArray Sizes

```
  ┌──────────────────┬───────────┬─────────────────────────────────┐
  │  Type            │  Bits     │  State Keys (byte indices)      │
  ├──────────────────┼───────────┼─────────────────────────────────┤
  │  BitArray8       │  8 bits   │  0..7    (8 states)             │
  │  BitArray32      │  32 bits  │  0..31   (32 states)            │
  │  BitArray64      │  64 bits  │  0..63   (64 states)            │
  │  BitArray128     │  128 bits │  0..127  (128 states)           │
  │  BitArray256     │  256 bits │  0..255  (256 states)           │
  └──────────────────┴───────────┴─────────────────────────────────┘

  Example: BitArray256
  ┌────────────────────────────────────────────────────────────────┐
  │  Byte 0  │  Byte 1  │  Byte 2  │  ...  │  Byte 31             │
  │  10110010│  00001111│  11000000│  ...  │  00000001             │
  │  keys 0-7│ keys 8-15│keys 16-23│  ...  │ keys 248-255          │
  └────────────────────────────────────────────────────────────────┘
  Each bit = one state flag (enabled/disabled)
  Key = bit index (0..255)
```

## Usage with AppAPI

```
  ┌───────────────────────────────────────────────────────────────┐
  │  IState as Singleton accessed via AppAPI:                     │
  │                                                               │
  │  // Get entire bitmask                                        │
  │  var state = AppAPI.StateCurrent<MyState, BitArray256>(em);   │
  │                                                               │
  │  // Check specific state by name                              │
  │  bool running = AppAPI.StateIsEnabled<MyState, BitArray256,   │
  │      MySettings>(em, "Running");                              │
  │                                                               │
  │  // Set state exclusively (all others off)                    │
  │  AppAPI.StateSet<MyState, BitArray256, MySettings>(           │
  │      em, "Paused");                                           │
  │                                                               │
  │  // Enable a specific state (additive)                        │
  │  AppAPI.StateEnable<MyState, BitArray256, MySettings>(        │
  │      em, "Running");                                          │
  │                                                               │
  │  // Disable a specific state                                  │
  │  AppAPI.StateDisable<MyState, BitArray256, MySettings>(       │
  │      em, "Jumping");                                          │
  └───────────────────────────────────────────────────────────────┘
```

## Bitmask Operations on IState.Value

```
  StateSet (exclusive - clears all, sets one):
  ┌────────────────────────────────────────────────────────────────┐
  │  Before: Value = 0b10110100                                    │
  │  Set "Running" (key=3):                                        │
  │  After:  Value = 0b00001000  ← only bit 3 set                 │
  └────────────────────────────────────────────────────────────────┘

  StateEnable (additive - sets one, keeps others):
  ┌────────────────────────────────────────────────────────────────┐
  │  Before: Value = 0b10110100                                    │
  │  Enable "Jumping" (key=5):                                     │
  │  After:  Value = 0b10110100 | 0b00100000 = 0b10110100 → 10110100│
  │          Wait, bit 5 already set? No change.                   │
  │                                                                │
  │  Actually: Value[5] = true                                     │
  │  After:  Value = 0b10110100 | (1 << 5) = 0b10110100          │
  │  (Bit 5 was already 1, no visible change)                     │
  └────────────────────────────────────────────────────────────────┘

  StateDisable (removes one):
  ┌────────────────────────────────────────────────────────────────┐
  │  Before: Value = 0b10110100                                    │
  │  Disable "Armed" (key=2):                                      │
  │  After:  Value = 0b10110100 & ~(1 << 2) = 0b10110000         │
  │          Bit 2 cleared                                         │
  └────────────────────────────────────────────────────────────────┘
```

## Integration with State Registration

```
  ┌──────────────────────┐      ┌──────────────────────┐
  │  StateAPI.Register() │      │  StateInstance        │
  │                      │      │  (stored on System)   │
  │  Links:              │─────►│                       │
  │  "Running" → key 3   │      │  State = typeof(MySt) │
  │  "Jumping" → key 5   │      │  StateKey = 3         │
  │                      │      │  StateInstanceComponent│
  │                      │      │  = typeof(TagRunning) │
  └──────────────────────┘      └──────────────────────┘

  The IState<T> singleton's Value bitmask is read by StateFlagModel
  or StateModel to determine which tag components to add/remove on
  entities, while AppAPI provides main-thread read/write access.
```

## Key Design Decisions

1. **Generic Constraint on IBitArray**: Forces all state components to use bitmask-based values, enabling efficient bitwise diffing by StateFlagModel.

2. **IComponentData Constraint**: IState components are ECS components, stored as singletons for global state or per-entity for local state.

3. **Separation of Concerns**: `IState<T>` defines the data contract. `StateAPI` handles registration. `AppAPI` provides access. `StateFlagModel`/`StateModel` handles transitions.

4. **BitArray Reuse**: Leverages the existing `IBitArray<T>` interface from `BovineLabs.Core.Collections`, providing standard bit-level indexing by byte key.

5. **Singleton Pattern**: Typically used as a singleton component representing global application state (game state, UI state, input state).

## Source

- [BovineLabs.Core/States/IState.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/IState.cs)
- [BovineLabs.Core/States/AppAPI.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/AppAPI.cs)
