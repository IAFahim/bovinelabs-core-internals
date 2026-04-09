# IntFloatUnion — Inner Workings

## Overview

IntFloatUnion is an explicit-layout union struct that overlaps an `int` (32-bit signed
integer) and a `float` (32-bit IEEE 754 single-precision) at the same memory offset,
enabling zero-cost bit reinterpretation between the two.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│            IntFloatUnion (StructLayout.Explicit, 4 bytes)                   │
│                                                                             │
│  Memory:  ┌──────────────────┐                                             │
│  Offset 0 │ int   IntValue   │  ← 32-bit signed integer                  │
│  Offset 0 │ float FloatValue │  ← 32-bit IEEE 754 float                  │
│           └──────────────────┘                                             │
│           (both fields occupy the SAME 4 bytes)                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Memory Layout

```
  IntFloatUnion instance (4 bytes total)
  ┌──┬──┬──┬──┐
  │b0│b1│b2│b3│  Little-endian: b0=LSB, b3=MSB
  └──┴──┴──┴──┘

  Read as int:    IntValue   = int32 bit pattern
  Read as float:  FloatValue = IEEE 754 float with same bits

  Example: float 1.0f → bits 0x3F800000
  ┌──────┬──────┬──────┬──────┐
  │ 0x00 │ 0x00 │ 0x80 │ 0x3F │  (little-endian)
  └──────┴──────┴──────┴──────┘
  IntValue   = 1065353216 (0x3F800000)
  FloatValue = 1.0f

  IEEE 754 Single-Precision Format (32 bits):
  ┌───┬───────────────┬───────────────────────────────┐
  │ S │ EEEEEEEE      │ MMMMMMMMMMMMMMMMMMMMMMM       │
  │ 1 │ 8 bits        │ 23 bits                       │
  │sig│ exponent      │ mantissa                      │
  └───┴───────────────┴───────────────────────────────┘
```

## Usage Patterns

```
  // float → int (extract raw bits)
  float f = 3.14f;
  var u = new IntFloatUnion(f);
  int bits = u.IntValue;
  // bits = 0x4048F5C3 (no conversion, just reinterpretation)

  // int → float (construct float from bits)
  int rawBits = 0x42C80000;  // = 100.0f in IEEE 754
  var u = new IntFloatUnion(rawBits);
  float reconstructed = u.FloatValue;  // = 100.0f

  // Common use: manipulate float bits for fast math operations
  var u = new IntFloatUnion(myFloat);
  u.IntValue += 1;        // Increment mantissa → next representable float
  float next = u.FloatValue;

  ┌──────────────────────────────────────────────────────────┐
  │  Constructor:                                           │
  │                                                         │
  │  IntFloatUnion(int value)                               │
  │  {                                                      │
  │    this.FloatValue = 0;   // zero the other field       │
  │    this.IntValue = value; // write actual value         │
  │  }                                                      │
  │                                                         │
  │  IntFloatUnion(float value)                             │
  │  {                                                      │
  │    this.IntValue = 0;     // zero the other field       │
  │    this.FloatValue = value; // write actual value       │
  │  }                                                      │
  └──────────────────────────────────────────────────────────┘
```

## Common Applications

```
  ┌─────────────────────────────────────────────────────────────────┐
  │  1. Fast absolute value:                                        │
  │     var u = new IntFloatUnion(f);                               │
  │     u.IntValue &= 0x7FFFFFFF;  // Clear sign bit               │
  │     float abs = u.FloatValue;                                   │
  │                                                                 │
  │  2. Fast floor for positive floats:                             │
  │     var u = new IntFloatUnion(f);                               │
  │     int exponent = (u.IntValue >> 23) - 127;                    │
  │     // ... manipulate bits for fast floor                       │
  │                                                                 │
  │  3. Bit-level comparison:                                       │
  │     // Compare floats by integer comparison of their bits       │
  │     // Works because IEEE 754 mantissa order matches integer    │
  │                                                                 │
  │  4. Serialization:                                              │
  │     // Write float as int to binary stream without conversion   │
  │     // Reinterpret on read side — no precision loss             │
  └─────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Explicit layout**: `StructLayout(LayoutKind.Explicit)` with both fields at
  `FieldOffset(0)` forces memory overlap — zero-cost reinterpretation.
- **4 bytes**: Both `int` and `float` are 32-bit types; no padding needed.
- **Burst-compatible**: Pure unmanaged value type, works in Burst jobs.
- **Constructor zeros first**: The non-target field is set to `default` to satisfy
  C# definite assignment for explicit-layout structs.

## Verified Data

```
that: TYPE NOT FOUND
float: TYPE NOT FOUND
Verified: 0 checks, 2 failures
```

## Source File

- `BovineLabs.Core/Utility/IntFloatUnion.cs`

## Source

- [BovineLabs.Core/Utility/IntFloatUnion.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/IntFloatUnion.cs)
