# DebugUtil.SplitInt — Inner Workings

## Overview

SplitInt decomposes a floating-point number into separate integer and decimal parts
using pure math (`math.trunc` and `math.frac`), enabling debug logging inside
Burst-compiled jobs where `float.ToString()` is unavailable.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DebugUtil (static class)                                  │
│                                                                             │
│   SplitInt(float value, int digits, out int integer, out int decimals)      │
│   SplitInt(double value, int digits, out int integer, out int decimals)     │
│                                                                             │
│   Burst-compatible! Uses Unity.Mathematics only.                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Algorithm

```
  Input:  value = 10.1234, digits = 2

  Step 1: integer = (int)math.trunc(10.1234)
          ┌─────────────────────────────────────┐
          │  trunc(10.1234) = 10.0              │
          │  (int)10.0 = 10                     │
          │  integer = 10                       │
          └─────────────────────────────────────┘

  Step 2: multi = math.pow(10, digits)
          ┌─────────────────────────────────────┐
          │  pow(10, 2) = 100                   │
          └─────────────────────────────────────┘

  Step 3: decimals = (int)(math.frac(value) * multi)
          ┌─────────────────────────────────────┐
          │  frac(10.1234) = 0.1234             │
          │  0.1234 * 100 = 12.34              │
          │  (int)12.34 = 12                    │
          │  decimals = 12                      │
          └─────────────────────────────────────┘

  Result: integer=10, decimals=12  →  "10.12"
```

## Mathematical Operations Detail

```
  math.trunc(v)                    math.frac(v)
  ═════════════                    ════════════

  Returns integer part,            Returns fractional part:
  truncating toward zero.          v - floor(v)

    v        trunc(v)                v        frac(v)
  ───────   ──────────             ───────   ─────────
   3.7    →   3                     3.7    →  0.7
  -3.7    →  -3                    -3.7    →  0.3
   0.5    →   0                     0.5    →  0.5

  ┌─────────────────────────────────────────────────────────┐
  │  multiplier table:                                      │
  │  digits=0 → multi=1     → decimals captures 0 places   │
  │  digits=1 → multi=10    → decimals captures 1 place    │
  │  digits=2 → multi=100   → decimals captures 2 places   │
  │  digits=3 → multi=1000  → decimals captures 3 places   │
  └─────────────────────────────────────────────────────────┘
```

## Usage Pattern in Burst Jobs

```
  [BurstCompile]
  void Execute()
  {
      float myValue = ComputeSomething();

      DebugUtil.SplitInt(myValue, 2,
          out int integerPart,
          out int decimalPart);

      // Can now log with integer-only debug:
      // $"Value: {integerPart}.{decimalPart}"
      // e.g. "Value: 10.12"

      // Inside Burst, you can't do myValue.ToString()
      // but you CAN write int values to debug logs
      // or NativeText / FixedString
  }
```

## More Examples

```
  ┌──────────────────────────────────────────────────────────────┐
  │  Input              digits   integer   decimals              │
  │  ─────────────────  ──────   ───────   ────────              │
  │  42.0               2        42        0                     │
  │  -7.5678            3        -7        567                   │
  │  0.99999            4        0         9999                  │
  │  100.1              1        100       1                     │
  │  3.14159            0        3         0                     │
  └──────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Pure math only**: `math.trunc` and `math.frac` are Burst-compatible intrinsics —
  no string formatting, no boxing, no managed calls.
- **Negative handling**: `trunc` truncates toward zero, so `-3.7` gives `integer=-3`
  and `frac(-3.7) = 0.3`, yielding `-3.3` when recombined. Users must handle sign
  of decimal part if needed.
- **Precision limit**: `digits` controls how many decimal places to capture; beyond
  ~7 digits for float / ~15 for double, precision is lost due to IEEE 754.
- **Assert guard**: `Check.Assume(digits >= 0)` catches misuse in debug builds.

## Source File

- `BovineLabs.Core/Utility/DebugUtil.cs`

## Source

- [BovineLabs.Core/Utility/DebugUtil.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/DebugUtil.cs)
