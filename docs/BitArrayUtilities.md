```
╔══════════════════════════════════════════════════════════════════════════════╗
║        BitArrayUtilities — Low-Level Bitwise Get/Set Operations             ║
║         Source: BovineLabs.Core/Collections/BitArray.cs                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  Static utility class providing the low-level bit manipulation
  primitives used by all BitArray structs (8/16/32/64/128/256).
  Every method is aggressively inlined for zero abstraction cost.

CLASS STRUCTURE
═══════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  static class BitArrayUtilities                                 │
  │                                                                 │
  │  ┌─ GET operations ────────────────────────────────────────────┐│
  │  │  Get8 (uint index, byte    data) → bool                     ││
  │  │  Get16(uint index, ushort  data) → bool                     ││
  │  │  Get32(uint index, uint    data) → bool                     ││
  │  │  Get64(uint index, ulong   data) → bool                     ││
  │  │  Get128(uint/uint index, ulong d1, ulong d2) → bool         ││
  │  │  Get256(uint index, ulong d1,d2,d3,d4) → bool               ││
  │  └─────────────────────────────────────────────────────────────┘│
  │                                                                 │
  │  ┌─ SET operations ────────────────────────────────────────────┐│
  │  │  Set8 (uint index, ref byte    data, bool value)            ││
  │  │  Set16(uint index, ref ushort  data, bool value)            ││
  │  │  Set32(uint index, ref uint    data, bool value)            ││
  │  │  Set64(uint index, ref ulong   data, bool value)            ││
  │  │  Set128(uint/int index, ref ulong d1,d2, bool value)        ││
  │  │  Set256(uint index, ref ulong d1,d2,d3,d4, bool value)     ││
  │  └─────────────────────────────────────────────────────────────┘│
  └─────────────────────────────────────────────────────────────────┘

GET — BIT TEST PATTERN
═══════════════════════

  All Get methods follow the same formula:

  ┌─────────────────────────────────────────────────────────────────┐
  │  (data & (1 << index)) != 0                                    │
  │                                                                 │
  │  Step 1: Create mask    →  1 shifted left by index position    │
  │  Step 2: AND with data  →  isolates the target bit             │
  │  Step 3: Compare != 0   →  converts to boolean                 │
  └─────────────────────────────────────────────────────────────────┘

  Visual (uint, index=5):
                    ┌─── index=5
                    │
  mask = 1u << 5 = 0b00000000_00000000_00000000_00100000
                     ▲
  data            = 0b10101010_10101010_10101010_10101010
                     │
  data & mask     = 0b00000000_00000000_00000000_00100000  → true

GET128 — DUAL FIELD DISPATCH
════════════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  Get128(int index, ulong data1, ulong data2):                  │
  │                                                                 │
  │  if index < 64:                                                 │
  │    return (data1 & (1UL << index)) != 0                        │
  │  else:                                                          │
  │    return (data2 & (1UL << (index - 64))) != 0                 │
  └─────────────────────────────────────────────────────────────────┘

  Index routing:

       0         63 64        127
       ├──────────┤├──────────┤
       │  data1   ││  data2   │
       │  range   ││  range   │
       └──────────┘└──────────┘
         if < 64       else

GET256 — QUAD FIELD DISPATCH
════════════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  Get256(uint index, ulong d1, d2, d3, d4):                     │
  │                                                                 │
  │  index < 128?                                                   │
  │    ├─ index < 64?  → (d1 & (1UL << idx))       != 0           │
  │    └─ else         → (d2 & (1UL << (idx-64)))  != 0           │
  │  else:                                                          │
  │    ├─ index < 192? → (d3 & (1UL << (idx-128))) != 0           │
  │    └─ else         → (d4 & (1UL << (idx-192))) != 0           │
  └─────────────────────────────────────────────────────────────────┘

  Decision tree:
                     ┌── index < 128? ──┐
                     │                  │
              index < 64?         index < 192?
              ┌──┴──┐             ┌──┴──┐
            data1  data2        data3  data4

SET — BIT WRITE PATTERN
════════════════════════

  All Set methods follow a conditional OR/AND pattern:

  ┌─────────────────────────────────────────────────────────────────┐
  │  Set32(uint index, ref uint data, bool value):                  │
  │                                                                 │
  │  if value == true:                                              │
  │    data = data | (1u << index)         ← SET bit (OR)          │
  │  else:                                                          │
  │    data = data & ~(1u << index)        ← CLEAR bit (AND NOT)   │
  └─────────────────────────────────────────────────────────────────┘

  Visual — SET bit 3 to true:
                     ┌─── index=3
  mask = 1u << 3 = 0b00000000_00000000_00000000_00001000

  data (before) = 0b00000000_00000000_00000000_00000000
  data | mask   = 0b00000000_00000000_00000000_00001000  ← bit 3 now 1

  Visual — CLEAR bit 3 (set to false):
  ~mask          = 0b11111111_11111111_11111111_11110111

  data (before) = 0b00000000_00000000_00000000_00001000
  data & ~mask  = 0b00000000_00000000_00000000_00000000  ← bit 3 now 0

SET128 — DUAL FIELD WITH BRANCH
═══════════════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  Set128(int index, ref ulong data1, ref ulong data2, bool v):  │
  │                                                                 │
  │  if index < 64:                                                 │
  │    data1 = v ? data1|(1UL<<idx) : data1&~(1UL<<idx)           │
  │  else:                                                          │
  │    data2 = v ? data2|(1UL<<(idx-64)) : data2&~(1UL<<(idx-64)) │
  └─────────────────────────────────────────────────────────────────┘

SET256 — QUAD FIELD WITH BRANCH CHAIN
══════════════════════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  Set256(uint index, ref ulong d1,d2,d3,d4, bool v):            │
  │                                                                 │
  │  if index < 64:                                                 │
  │    d1 = v ? d1|(1UL<<idx)         : d1&~(1UL<<idx)            │
  │  elif index < 128:                                              │
  │    d2 = v ? d2|(1UL<<(idx-64))    : d2&~(1UL<<(idx-64))       │
  │  elif index < 192:                                              │
  │    d3 = v ? d3|(1UL<<(idx-128))   : d3&~(1UL<<(idx-128))      │
  │  else:                                                          │
  │    d4 = v ? d4|(1UL<<(idx-192))   : d4&~(1UL<<(idx-192))      │
  └─────────────────────────────────────────────────────────────────┘

PERFORMANCE CHARACTERISTICS
══════════════════════════

  ┌─────────────────────┬──────────────────────────────────────────┐
  │ Method              │ Expected assembly                        │
  ├─────────────────────┼──────────────────────────────────────────┤
  │ Get8/16/32/64       │ BT (bit test) + SETcc                    │
  │ Set8/16/32/64       │ BTS/BTR (bit test and set/reset)         │
  │ Get128              │ CMP + BT (branch + bit test)             │
  │ Set128              │ CMP + BTS/BTR                            │
  │ Get256              │ 2× CMP + BT (cascade branch)             │
  │ Set256              │ 3× CMP + BTS/BTR (cascade branch)        │
  └─────────────────────┴──────────────────────────────────────────┘

  All methods are [MethodImpl(AggressiveInlining)] — the BitArray
  indexer operators compile down to raw bit instructions with
  zero function call overhead in Burst-compiled code.
```

## Verified Data

> [Run test snippet](../snippets/core-collections/BitArrayUtilities.cs) — 30 assertions passing
>
> Key findings:
> - Static class (abstract + sealed) with Get8/16/32/64/128/256 and Set8/16/32/64/128/256 methods
> - Get32(index, data) correctly returns bit state; Get64, Get8, Get16 verified
> - Get128 has (int, ulong, ulong) overload for dual-field dispatch
> - Set32 signature: (uint index, ref uint data, bool value)

## Source

- [BovineLabs.Core/Collections/BitArray.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/BitArray.cs)
