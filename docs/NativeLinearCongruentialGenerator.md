NativeLinearCongruentialGenerator — Inner Workings
====================================================

Provides extremely fast procedural random generation directly in Burst.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────────┐
  │     NativeLinearCongruentialGenerator                    │
  │                                                          │
  │  ┌──────────────────┐     ┌────────────────────────┐     │
  │  │  int* current     │────►│  Heap-allocated int    │     │
  │  │  (state pointer)  │     │  ┌──────────────────┐  │     │
  │  └──────────────────┘     │  │   seed / state   │  │     │
  │                           │  └──────────────────┘  │     │
  │  ┌──────────────────┐     └────────────────────────┘     │
  │  │  allocatorLabel  │                                     │
  │  └──────────────────┘                                     │
  │                                                           │
  │  [Safety: AtomicSafetyHandle m_Safety]                    │
  └──────────────────────────────────────────────────────────┘


LCG ALGORITHM — Turbo Pascal Variant
─────────────────────────────────────

  The Linear Congruential Generator formula:

    X[n+1] = (a * X[n] + c) mod m

  Constants (Turbo Pascal LCG):
  ┌───────────────────────────────────────────────┐
  │  Multiplier (a) = 134775813  (0x0808FD05)    │
  │  Increment  (c) = 1                           │
  │  Modulus    (m) = int.MaxValue (0x7FFFFFFF)   │
  └───────────────────────────────────────────────┘

  Implementation uses bitwise AND instead of modulo:
    result = (134775813 * x + 1) & 0x7FFFFFFF


STATE MACHINE
─────────────

  Each call to Next() transforms the state:

    ┌───────────┐     ┌────────────────────────────────────┐
    │ *current  │────►│ x1 = (134775813 * x + 1) & 0x7FFF │
    │   (x)     │     │           FFFF                     │
    └───────────┘     └──────────────┬─────────────────────┘
         │                           │
         │    ┌──────────────────────┘
         │    │
         │    ▼
         │  ┌───────────────┐
         │  │ *current = x1 │   (state updated in-place)
         │  └───────┬───────┘
         │          │
         │          ▼
         │  return x1
         │
         ▼
    Next call uses x1 as input


GENERATION EXAMPLE
──────────────────

  Seed = 42:

  Step  │  State (x)  │  Calculation                          │  Output
  ──────┼─────────────┼──────────────────────────────────────┼─────────────
    1   │     42      │  (134775813 * 42 + 1) & 0x7FFFFFFF  │  1312714347
    2   │ 1312714347  │  (134775813 * 1312714347 + 1) & ... │   392149189
    3   │  392149189  │  (134775813 * 392149189 + 1) & ...  │  1878928046
  ...   │    ...      │  ...                                  │    ...

  Period: 2^31 - 1 = 2,147,483,647 values before repeating


MEMORY LAYOUT
─────────────

  Stack/Job struct:
  ┌────────────────────────────────────────────────┐
  │  int* current       │ 8 bytes  │ State pointer │
  │  allocatorLabel     │ ~2 bytes │ Allocator     │
  │  [m_Safety]         │ ~16 bytes│ [Debug only]  │
  └────────────────────────────────────────────────┘

  Heap (4 bytes total!):
  ┌────────┐
  │  int   │  ← current points here
  │ state  │
  └────────┘


NEXT() OPERATION BREAKDOWN
──────────────────────────

  ┌──────────────────────────────────────────────────────┐
  │  int Next()                                          │
  │  {                                                   │
  │      [Safety] CheckWriteAndThrow(m_Safety)           │
  │                                                      │
  │      var x  = *current;       // Load state          │
  │      var x1 = (134775813 * x  // Multiply            │
  │               + 1)            // Increment            │
  │               & 0x7FFFFFFF;   // Modulo via AND       │
  │      *current = x1;           // Store new state      │
  │                                                      │
  │      return x1;               // Return new value     │
  │  }                                                   │
  │                                                      │
  │  ⚡ Total: 1 multiply, 1 add, 1 AND, 2 memory ops   │
  │  ⚡ No branches, no divisions, no lookups             │
  │  ⚡ Fully Burst-compatible                           │
  └──────────────────────────────────────────────────────┘


WHY THIS LCG?
─────────────

  ┌──────────────────┬──────────────────────┬──────────────────────┐
  │                  │  Unity.Random        │  NativeLCG           │
  ├──────────────────┼──────────────────────┼──────────────────────┤
  │  Algorithm       │  Xorshift            │  LCG                 │
  │  State size      │  4 uints (16 bytes)  │  1 int (4 bytes)     │
  │  Operations      │  4 shifts + XORs     │  1 MUL + ADD + AND   │
  │  Quality         │  High                │  Moderate            │
  │  Speed           │  Fast                │  Extremely fast      │
  │  Burst compat    │  Yes                 │  Yes                 │
  │  Deterministic   │  Yes                 │  Yes                 │
  └──────────────────┴──────────────────────┴──────────────────────┘

  Use NativeLCG when:
    • You need procedural variety, not cryptographic quality
    • Memory footprint matters (4 bytes vs 16 bytes)
    • Raw throughput is critical
    • Determinism across runs is needed (same seed = same sequence)


CONSTRUCTION
────────────

  ┌─────────────────────────────────────────────────────────────┐
  │ new NativeLinearCongruentialGenerator(seed: 12345,         │
  │                                       Allocator.Persistent) │
  │                                                             │
  │  1. Allocate 4 bytes on heap via Memory.Unmanaged           │
  │  2. *current = seed                                        │
  │  3. [Debug] Create AtomicSafetyHandle                      │
  └─────────────────────────────────────────────────────────────┘


DISPOSAL
────────

  ┌─────────────────────────────────────────────────────────────┐
  │ Dispose()                                                   │
  │  1. [Debug] DisposeSafetyHandle                             │
  │  2. Memory.Unmanaged.Free(current, allocatorLabel)          │
  │  3. current = null                                          │
  └─────────────────────────────────────────────────────────────┘


TYPICAL USAGE
─────────────

  ┌──────────────────────────────────────────────────────────┐
  │ var rng = new NativeLinearCongruentialGenerator(         │
  │     seed: 42, Allocator.TempJob);                        │
  │                                                          │
  │ // In a Burst job:                                       │
  │ int val = rng.Next();       // Fast procedural random    │
  │ int val2 = rng.Next();      // Next value in sequence    │
  │                                                          │
  │ rng.Dispose();                                           │
  └──────────────────────────────────────────────────────────┘

## Verified Data

```
NativeLinearCongruentialGenerator
  Kind: struct
  Size: 32 bytes
Fields:
  [0] Int32* current  (private)
  [8] AtomicSafetyHandle m_Safety  (private)
  [24] AllocatorHandle allocatorLabel  (private)
Properties:
Methods:
  Void Dispose()
  Int32 Next()
Runtime Behavior:
  Seed=42: Next()=1365616851 (formula: 1365616851)
  Next()=1621170208 (formula: 1621170208)
  Next()=882243745
  Formula: (134775813 * x + 1) & 0x7FFFFFFF
  Deterministic: seed=42 again: 1365616851, 1621170208
Verified: 5 checks, 0 failures
```
NativeLinearCongruentialGenerator
  Kind: struct
  Size: 32 bytes
Fields:
  [0] Int32* current  (private)
  [8] AtomicSafetyHandle m_Safety  (private)
  [24] AllocatorHandle allocatorLabel  (private)
Properties:
Methods:
  Void Dispose()
  Int32 Next()
Runtime Behavior:
  Seed=42: Next()=1365616851 (formula: 1365616851)
  Next()=1621170208 (formula: 1621170208)
  Next()=882243745
  Formula: (134775813 * x + 1) & 0x7FFFFFFF
  Deterministic: seed=42 again: 1365616851, 1621170208
Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/NativeLinearCongruentialGenerator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/NativeLinearCongruentialGenerator.cs)
- [BovineLabs.Core.Tests/Collections/NativeLinearCongruentialGeneratorTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/NativeLinearCongruentialGeneratorTests.cs)
