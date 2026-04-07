# ButtonEvent - Inner Workings

## Overview

`ButtonEvent` is a simple struct that provides thread-safe, single-consumer boolean event semantics. It supports `TryConsume()` and `TryProduce()` operations, ensuring that an event is consumed exactly once and produced only when not already set. This prevents duplicate events in multi-system scenarios.

```
File: BovineLabs.Core/Utility/ButtonEvent.cs
Struct: ButtonEvent
Fields: bool Value
Methods: TryConsume() → bool, TryProduce(bool) → bool
```

## Architecture

```
  ┌────────────────────────────────────────────────────────────┐
  │                    ButtonEvent                              │
  │                                                            │
  │  ┌──────────────────────────────────────────────────────┐ │
  │  │  bool Value                                          │ │
  │  │  • false = no pending event                          │ │
  │  │  • true  = event is pending, waiting to be consumed  │ │
  │  └──────────────────────────────────────────────────────┘ │
  │                                                            │
  │  ┌──────────────────────────────────────────────────────┐ │
  │  │  TryConsume() → bool                                 │ │
  │  │  Atomically reads and resets the event               │ │
  │  └──────────────────────────────────────────────────────┘ │
  │                                                            │
  │  ┌──────────────────────────────────────────────────────┐ │
  │  │  TryProduce(bool value = true) → bool                │ │
  │  │  Sets the event if not already set                   │ │
  │  └──────────────────────────────────────────────────────┘ │
  └────────────────────────────────────────────────────────────┘
```

## TryConsume - Event Consumption

```
  TryConsume():
  ┌───────────────────────────────────────────────────────────┐
  │                                                           │
  │  IF Value == true:                                        │
  │    Value = false    ← reset to "consumed" state           │
  │    return true      ← event was consumed                  │
  │                                                           │
  │  ELSE:                                                    │
  │    return false     ← no event to consume                 │
  │                                                           │
  │  ┌─────────────────────────────────────────────────────┐ │
  │  │  State Machine:                                      │ │
  │  │                                                      │ │
  │  │  Value=true ──TryConsume()──► Value=false            │ │
  │  │  (pending)    returns true    (consumed)              │ │
  │  │                                                      │ │
  │  │  Value=false ──TryConsume()──► Value=false            │ │
  │  │  (no event)   returns false   (unchanged)            │ │
  │  └─────────────────────────────────────────────────────┘ │
  └───────────────────────────────────────────────────────────┘
```

## TryProduce - Event Production

```
  TryProduce(value = true):
  ┌───────────────────────────────────────────────────────────┐
  │                                                           │
  │  IF value == true AND Value == false:                     │
  │    Value = true     ← set to "pending" state              │
  │    return true      ← event was produced                  │
  │                                                           │
  │  ELSE:                                                    │
  │    return false     ← event already pending, or           │
  │                       value param was false               │
  │                                                           │
  │  ┌─────────────────────────────────────────────────────┐ │
  │  │  State Machine:                                      │ │
  │  │                                                      │ │
  │  │  Value=false ──TryProduce(true)──► Value=true        │ │
  │  │  (idle)      returns true         (pending)           │ │
  │  │                                                      │ │
  │  │  Value=true ──TryProduce(true)───► Value=true         │ │
  │  │  (pending)   returns false        (unchanged)         │ │
  │  │              event NOT duplicated!                     │ │
  │  │                                                      │ │
  │  │  Value=false ──TryProduce(false)──► Value=false       │ │
  │  │  (idle)       returns false        (no-op)            │ │
  │  └─────────────────────────────────────────────────────┘ │
  └───────────────────────────────────────────────────────────┘
```

## Multi-System Event Flow

```
  Scenario: Input system produces "JumpPressed" event,
            Movement system consumes it.

  ┌─────────────────┐                    ┌─────────────────┐
  │  Input System   │                    │  Movement System │
  │                 │                    │                  │
  │  TryProduce()   │                    │  TryConsume()    │
  └────────┬────────┘                    └────────┬─────────┘
           │                                      │
           ▼                                      ▼
  ┌──────────────────────────────────────────────────────────┐
  │              ButtonEvent (component data)                 │
  │                                                          │
  │  Timeline:                                               │
  │  ════════                                                │
  │                                                          │
  │  Frame 1:  Value=false                                   │
  │    Input:     TryProduce(true) → true  (Value → true)   │
  │    Movement:  TryConsume()     → true  (Value → false)  │
  │    Event consumed! ✓                                     │
  │                                                          │
  │  Frame 2:  Value=false                                   │
  │    Input:     (no input this frame)                      │
  │    Movement:  TryConsume()     → false (no event)       │
  │    Nothing happens ✓                                     │
  │                                                          │
  │  Frame 3:  Value=false                                   │
  │    Input:     TryProduce(true) → true  (Value → true)   │
  │    Movement:  TryConsume()     → true  (Value → false)  │
  │    Event consumed! ✓                                     │
  └──────────────────────────────────────────────────────────┘
```

## Edge Cases

```
  1. Double Produce (deduplication):
  ┌──────────────────────────────────────────────────────────┐
  │  Value = false                                           │
  │  System A: TryProduce(true) → true  (Value → true)      │
  │  System B: TryProduce(true) → false (Value stays true)   │
  │  Only ONE event recorded, no duplication!                │
  └──────────────────────────────────────────────────────────┘

  2. Double Consume (single-consumption):
  ┌──────────────────────────────────────────────────────────┐
  │  Value = true                                            │
  │  System A: TryConsume() → true  (Value → false)         │
  │  System B: TryConsume() → false (already consumed)       │
  │  Only ONE system gets the event!                         │
  └──────────────────────────────────────────────────────────┘

  3. Produce with false:
  ┌──────────────────────────────────────────────────────────┐
  │  Value = false                                           │
  │  TryProduce(false) → false (no-op, value param is false) │
  │  Value stays false                                       │
  └──────────────────────────────────────────────────────────┘

  4. Produce then Produce (no consume):
  ┌──────────────────────────────────────────────────────────┐
  │  Value = false                                           │
  │  TryProduce(true) → true  (Value → true)                 │
  │  TryProduce(true) → false (already pending)              │
  │  Event coalesced - multiple produces = single event      │
  └──────────────────────────────────────────────────────────┘
```

## State Transition Diagram

```
                    TryProduce(true)
          ┌──────────────────────────────┐
          │                              │
          │    returns true              │    returns false
          │    (first producer)          │    (already pending)
          │                              │
          ▼                              │
  ┌───────────────┐                      │
  │   PENDING     │──────────────────────┘
  │   Value=true  │
  └───────┬───────┘
          │
          │ TryConsume()
          │ returns true
          │
          ▼
  ┌───────────────┐        TryConsume()
  │   IDLE        │◄──────────────────────┐
  │   Value=false │  returns false         │
  └───────┬───────┘  (no event)           │
          │                               │
          │ TryProduce(true)              │
          │ returns true                  │
          └───────────────────────────────┘

  Self-loop on IDLE:
    TryConsume() on IDLE → false (stays IDLE)

  Self-loop on PENDING:
    TryProduce(true) on PENDING → false (stays PENDING)
```

## Usage as ECS Component

```
  // Define as part of a component:
  public struct JumpInput : IComponentData
  {
      public ButtonEvent JumpPressed;
  }

  // Producer system (Input):
  public partial class InputSystem : SystemBase
  {
      protected override void OnUpdate()
      {
          if (/* jump button pressed */)
          {
              // TryProduce returns true if event was set
              // Returns false if already pending (deduped)
              jumpData.JumpPressed.TryProduce();
          }
      }
  }

  // Consumer system (Movement):
  public partial class MovementSystem : SystemBase
  {
      protected override void OnUpdate()
      {
          if (jumpData.JumpPressed.TryConsume())
          {
              // Execute jump exactly once
              ApplyJumpForce();
          }
      }
  }
```

## Key Design Decisions

1. **Struct, Not Class**: `ButtonEvent` is a struct, making it compatible with ECS component data (unmanaged, no GC allocation).

2. **Simple Bool**: The entire state is a single bool. No queues, no counters. Events that aren't consumed before the next produce are coalesced (deduplicated).

3. **Try-Prefix Pattern**: Both methods use the `Try` prefix convention, returning `bool` to indicate success. Callers can check the return value or ignore it.

4. **Single-Consumer Guarantee**: `TryConsume` atomically reads and resets, ensuring only one consumer processes the event even if multiple systems try.

5. **No Thread Safety Primitives**: The struct itself has no locks or atomics. Thread safety depends on Unity ECS scheduling (e.g., systems run on main thread, or jobs are properly ordered).

6. **Coalescing Semantics**: Multiple `TryProduce` calls between `TryConsume` calls result in a single event. This is intentional for button-like inputs where only the "pressed" state matters, not the count.
