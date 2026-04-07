# ShortHalfUnion — Inner Workings

## Overview

ShortHalfUnion is an explicit-layout union struct that overlaps a `short` (16-bit signed
integer) and a `half` (16-bit IEEE 754 float) at the same memory offset, enabling
zero-cost type reinterpretation between the two representations.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│           ShortHalfUnion (StructLayout.Explicit, 2 bytes)                   │
│                                                                             │
│  Memory:  ┌─────────────────┐                                              │
│  Offset 0 │ short  ShortValue│  ← 16-bit signed integer                   │
│  Offset 0 │ half   HalfValue │  ← 16-bit IEEE 754 half-precision float    │
│           └─────────────────┘                                              │
│           (both fields occupy the SAME 2 bytes)                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Memory Layout

```
  ShortHalfUnion instance (2 bytes total)
  ┌──┬──┐
  │b0│b1│   Byte 0 (low), Byte 1 (high) in little-endian
  └──┴──┘

  Read as short:  ShortValue = (int16)(b0 | b1 << 8)
  Read as half:   HalfValue  = IEEE 754 half-precision float with same bit pattern

  Example: short value = 16256 (0x3F80)
  ┌──────┬──────┐
  │ 0x80 │ 0x3F │   Little-endian bytes
  └──────┴──────┘
  ShortValue = 16256
  HalfValue  = 1.0  (IEEE 754 half: sign=0, exp=15, mantissa=0)

  IEEE 754 Half-Precision Format (16 bits):
  ┌───┬──────────┬─────────────┐
  │ S │ EEEEE    │ MMMMMMMMMM  │
  │ 1 │ 5 bits   │ 10 bits     │
  │ign│ exponent │ mantissa    │
  └───┴──────────┴─────────────┘
```

## Usage Pattern

```
  // short → half (reinterpret bits)
  short rawBits = 0x3C00;  // = 0.5 in half-precision
  var union = new ShortHalfUnion(rawBits);
  half h = union.HalfValue;  // h = 0.5 (zero cost, no conversion)

  // half → short (reinterpret bits)
  half myHalf = new half(3.14f);
  var union = new ShortHalfUnion(myHalf);
  short bits = union.ShortValue;  // raw IEEE 754 bits (zero cost)

  ┌──────────────────────────────────────────────────────────┐
  │  Constructor:                                            │
  │                                                          │
  │  ShortHalfUnion(short value)                             │
  │  {                                                       │
  │    this.HalfValue = default;  // zero the other field    │
  │    this.ShortValue = value;   // write actual value      │
  │  }                                                       │
  │                                                          │
  │  ShortHalfUnion(half value)                              │
  │  {                                                       │
  │    this.ShortValue = default;  // zero the other field   │
  │    this.HalfValue = value;     // write actual value     │
  │  }                                                       │
  └──────────────────────────────────────────────────────────┘
```

## Why Union Instead of Cast

```
  REGULAR CAST (conversion, NOT reinterpretation):
  ┌─────────────────────────────────────────────────────────┐
  │  short s = 16256;                                       │
  │  half h = (half)s;  // THIS IS A NUMERIC CONVERSION    │
  │  // h = 16256.0 (converted to nearest half-precision)  │
  └─────────────────────────────────────────────────────────┘

  UNION (bit reinterpretation, ZERO conversion):
  ┌─────────────────────────────────────────────────────────┐
  │  short s = 16256;  // bit pattern 0x3F80               │
  │  var u = new ShortHalfUnion(s);                        │
  │  half h = u.HalfValue;  // h = 1.0                    │
  │  // Same bits, interpreted as IEEE 754 half-float      │
  └─────────────────────────────────────────────────────────┘

  Performance: Union is a NO-OP at runtime (same memory, just different type).
               Cast does actual floating-point conversion work.
```

## Key Design Decisions

- **Explicit layout**: `StructLayout(LayoutKind.Explicit)` with both fields at
  `FieldOffset(0)` forces the CLR to overlap them — no copying, no conversion.
- **2 bytes total**: Both `short` and `half` are 16-bit types, so the union is
  exactly 2 bytes — no padding.
- **Burst-compatible**: Pure value type with no managed dependencies.
- **Constructor zeros first**: The non-target field is explicitly set to `default`
  to satisfy C# definite assignment rules for explicit-layout structs.

## Source File

- `BovineLabs.Core/Utility/ShortHalfUnion.cs`
