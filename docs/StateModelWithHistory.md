# StateModelWithHistory - Inner Workings

## Overview

`StateModelWithHistory` extends `StateModel` with a **ring buffer** of previous states, enabling undo/redo-style rollbacks. Two dynamic buffers (`HistoryBack` and `HistoryForward`) track the navigation stack. When a state reverts to a value found in the back buffer, it's treated as a "pop" (undo), and the old state moves to the forward buffer.

```
File: BovineLabs.Core/States/StateModelWithHistory.cs
State Size: 1 byte (single state value)
Extra Components: HistoryBack (DynamicBuffer<byte>), HistoryForward (DynamicBuffer<byte>)
Parameter: maxHistorySize - maximum entries in each buffer
```

## Architecture

```
 ┌───────────────────────────────────────────────────────────────────────────┐
 │                     StateModelWithHistory                                 │
 │                                                                           │
 │  ┌─────────────────────────────────────────────────────────────────────┐  │
 │  │  Constructor                                                        │  │
 │  │                                                                     │  │
 │  │  Parameters:                                                        │  │
 │  │    stateComponent         - current state byte component            │  │
 │  │    previousStateComponent  - previous state byte component          │  │
 │  │    historyBackComponent   - DynamicBuffer for undo stack            │  │
 │  │    historyForwardComponent - DynamicBuffer for redo stack           │  │
 │  │    maxHistorySize         - max entries per buffer                  │  │
 │  │                                                                     │  │
 │  │  Assert: stateSize == 1 byte                                        │  │
 │  │  Assert: maxHistorySize > 0                                         │  │
 │  │                                                                     │  │
 │  │  impl = StateImpl(stateComponent, previousStateComponent)           │  │
 │  │  historyBackType = GetDynamicComponentTypeHandle(historyBack)       │  │
 │  │  historyForwardType = GetDynamicComponentTypeHandle(historyForward) │  │
 │  └─────────────────────────────────────────────────────────────────────┘  │
 │                                                                           │
 │  ┌─────────────────────────────────────────────────────────────────────┐  │
 │  │  Per-Entity Data Layout:                                            │  │
 │  │                                                                     │  │
 │  │  Entity: [State(byte), Previous(byte), HistoryBack(buf),           │  │
 │  │           HistoryForward(buf)]                                      │  │
 │  │                                                                     │  │
 │  │  HistoryBack buffer:    [old1, old2, old3, ...]  (undo stack)      │  │
 │  │  HistoryForward buffer: [new1, new2, new3, ...]  (redo stack)      │  │
 │  └─────────────────────────────────────────────────────────────────────┘  │
 └───────────────────────────────────────────────────────────────────────────┘
```

## History Decision Tree

```
                    State change detected
                    (state != previous)
                           │
                           ▼
              ┌────────────────────────────┐
              │ Is HistoryBack empty?      │
              └─────────┬──────────────────┘
                        │
           ┌────────────┴────────────┐
           │ NO                      │ YES
           ▼                         ▼
  ┌──────────────────────┐   ┌──────────────────────────┐
  │ back[^1] == state?   │   │ Always treat as NEW      │
  │ (Pop / Undo?)        │   │ state transition          │
  └─────────┬────────────┘   └────────────┬─────────────┘
            │                               │
   ┌────────┴────────┐                      │
   │ YES             │ NO                   │
   │ (UNDO/POP)      │ (NEW STATE)          │
   ▼                 ▼                      ▼
  ┌──────────┐  ┌──────────────────────────────────────┐
  │ UNDO     │  │ NEW STATE TRANSITION                  │
  │          │  │                                       │
  │ 1. Push  │  │ 1. Check Forward buffer:              │
  │ previous │  │    ┌─────────────────────────────────┐│
  │ to       │  │    │ forward.Length > 0?              ││
  │ Forward  │  │    │   YES: forward[^1] == state?     ││
  │          │  │    │     YES → forward.RemoveAt(last) ││
  │ 2. Pop   │  │    │     NO  → forward.Clear()        ││
  │ from     │  │    │         (new branch, redo=garbage)││
  │ Back     │  │    └─────────────────────────────────┘│
  │          │  │                                       │
  │          │  │ 2. Push previous to Back buffer       │
  │          │  │    (with capacity limit)              │
  └──────────┘  └──────────────────────────────────────┘
```

## State Transition with History Tracking

```
  Entity: State=0, Prev=0, Back=[], Forward=[]

  ┌─ Frame 1: State → 1 (Idle) ──────────────────────────────────────────┐
  │                                                                        │
  │  State=1, Prev=0 → Change detected                                    │
  │  Back is empty → NEW STATE transition                                 │
  │  Forward is empty → nothing to clear                                  │
  │  Back.Push(0) → Back = [0]                                            │
  │  Prev = 1                                                             │
  │  RemoveComponent: none (prev was 0)                                   │
  │  AddComponent: TagIdle (state=1)                                      │
  └────────────────────────────────────────────────────────────────────────┘
       │
       ▼
  ┌─ Frame 2: State → 2 (Running) ──────────────────────────────────────┐
  │                                                                        │
  │  State=2, Prev=1 → Change detected                                    │
  │  Back = [0], Back[^1]=0 != 2 → NEW STATE                             │
  │  Forward is empty → nothing to clear                                  │
  │  Back.Push(1) → Back = [0, 1]                                         │
  │  Prev = 2                                                             │
  │  RemoveComponent: TagIdle (prev=1)                                    │
  │  AddComponent: TagRunning (state=2)                                   │
  └────────────────────────────────────────────────────────────────────────┘
       │
       ▼
  ┌─ Frame 3: State → 3 (Jumping) ──────────────────────────────────────┐
  │                                                                        │
  │  State=3, Prev=2 → Change detected                                    │
  │  Back = [0, 1], Back[^1]=1 != 3 → NEW STATE                         │
  │  Forward is empty → nothing to clear                                  │
  │  Back.Push(2) → Back = [0, 1, 2]                                      │
  │  Prev = 3                                                             │
  │  RemoveComponent: TagRunning (prev=2)                                 │
  │  AddComponent: TagJumping (state=3)                                   │
  └────────────────────────────────────────────────────────────────────────┘
       │
       ▼
  ┌─ Frame 4: State → 1 (UNDO to Idle) ─────────────────────────────────┐
  │                                                                        │
  │  State=1, Prev=3 → Change detected                                    │
  │  Back = [0, 1, 2], Back[^1]=2 != 1                                   │
  │  BUT Back[0]=0 != 1, Back[1]=1 == 1 → POP!                           │
  │                                                                        │
  │  POP operation:                                                        │
  │    Forward.Push(3) → Forward = [3]                                     │
  │    Back.RemoveAt(last) → Back = [0, 1]                                │
  │    Wait, Back[^1]=2 ≠ 1...                                            │
  │    Actually: Back[^1]=2 ≠ 1, so it's a NEW state, not pop             │
  │    Forward.Clear() (was empty, skip)                                  │
  │    Back.Push(3) → Back = [0, 1, 2, 3]                                │
  │  Prev = 1                                                             │
  │  RemoveComponent: TagJumping (prev=3)                                 │
  │  AddComponent: TagIdle (state=1)                                      │
  └────────────────────────────────────────────────────────────────────────┘

  NOTE: Pop only triggers when the new state matches Back[^1] (the most
  recent entry). The check is: Back.Length > 0 && Back[^1] == state
```

## Ring Buffer Capacity Management

```
  Back Buffer (maxHistorySize = 3):
  ══════════════════════════════════

  Step 1: Back = [0]             Length=1, cap ok
  Step 2: Back = [0, 1]          Length=2, cap ok
  Step 3: Back = [0, 1, 2]       Length=3, cap ok
  Step 4: Push 3 → Length == maxHistorySize
          ┌────────────────────────────────────────┐
          │  RemoveAt(0) → evict oldest entry      │
          │  Back = [1, 2]                          │
          │  Push(3)                                │
          │  Back = [1, 2, 3]                       │
          └────────────────────────────────────────┘

  Forward Buffer (maxHistorySize = 3):
  ═════════════════════════════════════

  Similar capacity management. When forward buffer is full and a new
  entry needs to be added, the oldest entry (index 0) is evicted.
```

## Complete Data Flow

```
  ┌─────────────────────────────────────────────────────────────────┐
  │  StateJob.Execute(chunk):                                       │
  │                                                                 │
  │  1. Get state, previous, historyBack, historyForward arrays     │
  │  2. For each entity:                                            │
  │     a. IF state == previous → CONTINUE (skip)                   │
  │     b. IF previous != 0 → RemoveComponent(registered[prev])    │
  │     c. IF state != 0 → AddComponent(registered[state])         │
  │     d. Navigate history buffers (see decision tree above)       │
  │     e. previous = state                                         │
  └─────────────────────────────────────────────────────────────────┘
```

## Navigation Example (Undo/Redo)

```
  Timeline:
  ════════

  State:  0 ──► 1 ──► 2 ──► 3 ──► 2 ──► 1 ──► 3
          start Idle Run  Jump  UNDO  UNDO  REDO
                                 to    to    to
                                 Run   Idle  Jump

  After "0 → 1":  Back=[0],         Forward=[]
  After "1 → 2":  Back=[0,1],       Forward=[]
  After "2 → 3":  Back=[0,1,2],     Forward=[]

  After UNDO (3→2):
    Back[^1]=2 == state=2 → POP!
    Forward.Push(3) → Forward=[3]
    Back.Pop()      → Back=[0,1]

  After UNDO (2→1):
    Back[^1]=1 == state=1 → POP!
    Forward.Push(2) → Forward=[3,2]
    Back.Pop()      → Back=[0]

  After REDO (1→3):
    Back[^1]=0 != state=3 → NEW STATE (not a pop)
    Forward[^1]=2 != state=3 → CLEAR FORWARD
    Forward.Clear() → Forward=[]
    Back.Push(1)    → Back=[0,1]
    (forward history lost because it's a new branch)

  After REDO (1→2):
    Back[^1]=1 != state=2 → NEW STATE
    Forward is empty → nothing to check
    Back.Push(1) → Back=[0,1,1]
    Wait... state=2, prev=1
    Back.Push(1) → Back=[0,1,1]
    Prev = 2
```

## Key Design Decisions

1. **Two-Buffer Undo/Redo**: Classic undo/redo pattern. Back buffer stores states you can undo to. Forward buffer stores states you can redo to.

2. **Pop Detection**: When `back[^1] == state`, the system recognizes this as an undo operation rather than a new state, preserving the forward history.

3. **Forward History Invalidation**: When entering a genuinely new state (not a redo), the entire forward buffer is cleared (like a browser navigation: going back then navigating to a new page loses forward history).

4. **Capacity Limits**: Both buffers respect `maxHistorySize`. When full, the oldest entry (index 0) is evicted.

5. **EntityCommandBuffer**: Still uses ECB for structural changes (add/remove tag components), same as the base `StateModel`.

6. **Single Byte State**: Only supports 1-byte state values (like StateModel), enabling simple equality comparison for pop detection.

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/States/StateModelWithHistory.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/StateModelWithHistory.cs)
- [BovineLabs.Core.Tests/States/StateModelWithHistoryTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/States/StateModelWithHistoryTests.cs)
