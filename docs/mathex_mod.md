```
╔══════════════════════════════════════════════════════════════════════════════╗
║                         mathex.mod — True Mathematical Modulus              ║
║              Source: BovineLabs.Core/Utility/mathex.cs                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

PROBLEM
═══════
  The C# % operator returns the REMAINDER, not the mathematical modulus.
  For negative dividends, remainder and modulus DIVERGE:

     remainder(-7, 3) = -1     ← C# % operator
     modulus(-7, 3)    = 2     ← mathematical definition

ALGORITHM
═════════

  ┌──────────────────────────────────────────────────────────────┐
  │  int mod(int x, int m)                                       │
  │  {                                                           │
  │      return ((x % m) + m) % m;                               │
  │  }                                                           │
  └──────────────────────────────────────────────────────────────┘

  Three-step pipeline:

      ┌───────────┐      ┌───────────┐      ┌───────────┐
      │  Step 1   │      │  Step 2   │      │  Step 3   │
      │  x % m    │─────▶│  + m      │─────▶│  % m      │──▶ result
      │ (C# rem)  │      │ (shift +) │      │ (wrap to  │
      └───────────┘      └───────────┘      │  [0, m))  │
                                            └───────────┘

STEP-BY-STEP TRACE
══════════════════

  Example: mod(-7, 3)

    Step 1:  -7 % 3 = -1          ← C# remainder (can be negative)
              │
              ▼
    Step 2:  -1 + 3 =  2          ← add divisor m to make non-negative
              │
              ▼
    Step 3:   2 % 3 =  2          ← final mod ensures result ∈ [0, m)
              │
              ▼
           result = 2 ✓

  Example: mod(7, 3)

    Step 1:   7 % 3 =  1
    Step 2:   1 + 3 =  4
    Step 3:   4 % 3 =  1          ← same as remainder for positive inputs
           result = 1 ✓

COMPARISON TABLE
════════════════

  ┌───────┬───────┬────────────┬────────────┬──────────────────────┐
  │   x   │   m   │  x % m     │  mod(x,m)  │  Same?               │
  ├───────┼───────┼────────────┼────────────┼──────────────────────┤
  │   7   │   3   │   1        │   1        │  ✓ yes               │
  │  -7   │   3   │  -1        │   2        │  ✗ no                │
  │   7   │  -3   │   1        │   1        │  ✓ yes               │
  │  -7   │  -3   │  -1        │  -1        │  ✓ yes (edge case)   │
  │  10   │   5   │   0        │   0        │  ✓ yes               │
  │  -1   │  256  │ -1         │  255       │  ✗ no                │
  └───────┴───────┴────────────┴────────────┴──────────────────────┘

WHY IT MATTERS IN GAMES
═══════════════════════

  ● Circular wrapping (angle normalization, grid wrapping)
  ● Ring-buffer indexing with negative offsets
  ● Periodic function evaluation

  Without true modulus, negative angles produce out-of-range indices:

    angleIndex = -90 % 360 = -90    ← BOOM: negative array index!
    angleIndex = mod(-90, 360) = 270 ← CORRECT: wraps to valid range

IMPLEMENTATION NOTES
═════════════════════

  ● Aggressively inlined ([MethodImpl(AggressiveInlining)])
  ● Burst-compiled via [BurstCompile] on containing class
  ● Two divisions + one addition — minimal overhead vs %
  ● Always produces result in range [0, m) for positive m
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Utility/mathex.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/mathex.cs)
- [BovineLabs.Core.Tests/Utility/mathexTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/mathexTests.cs)
- [BovineLabs.Core.Tests/Utility/MathExPerformanceTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/MathExPerformanceTests.cs)
