```
╔══════════════════════════════════════════════════════════════════════════════╗
║          mathex.add — SIMD Accelerated Integer Array Addition               ║
║              Source: BovineLabs.Core/Utility/mathex.cs                       ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  Adds a scalar integer value to every element in an integer array.
  Uses int4 SIMD intrinsics to process 4 integers per instruction.

SIGNATURES
══════════

  void add(NativeArray<int> output, NativeArray<int> input, int value)
  void add(int* dst, int* src, int length, int value)

  ● dst and src may NOT alias ([NoAlias] / [ReadOnly] attributes)
  ● output.Length must equal input.Length (debug-checked)

ALGORITHM — SIMD LOOP
══════════════════════

  ┌──────────────────────────────────────────────────────────────────┐
  │  Cast pointers to int4* (128-bit SIMD width = 4 × int32)        │
  │                                                                  │
  │  dst4 = (int4*)dst;                                             │
  │  src4 = (int4*)src;                                             │
  │  numSamples4 = length >> 2;    // length / 4                    │
  │                                                                  │
  │  for (i = 0..numSamples4):                                      │
  │      dst4[i] = src4[i] + value;   // 4 adds in 1 SIMD op       │
  └──────────────────────────────────────────────────────────────────┘

MEMORY → SIMD REGISTER → MEMORY PIPELINE
═════════════════════════════════════════

  src (int32 array in memory):
  ┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┐
  │  10 │  20 │  30 │  40 │  50 │  60 │  70 │  80 │
  └──┬──┴──┬──┴──┬──┴──┬──┴──┬──┴──┬──┴──┬──┴──┬──┘
     └─────┴─────┴──┬──┴─────┘     └─────┴─────┴──┬──┘
                    │                             │
                    ▼                             ▼
  int4 load #0:              int4 load #1:
  ┌─────┬─────┬─────┬─────┐ ┌─────┬─────┬─────┬─────┐
  │  10 │  20 │  30 │  40 │ │  50 │  60 │  70 │  80 │
  └──┬──┴──┬──┴──┬──┴──┬──┘ └──┬──┴──┬──┴──┬──┴──┬──┘
     │     │     │     │        │     │     │     │
     ▼     ▼     ▼     ▼        ▼     ▼     ▼     ▼
     +     +     +     +        +     +     +     +        value = 5
     │     │     │     │        │     │     │     │
     ▼     ▼     ▼     ▼        ▼     ▼     ▼     ▼
  ┌─────┬─────┬─────┬─────┐ ┌─────┬─────┬─────┬─────┐
  │  15 │  25 │  35 │  45 │ │  55 │  65 │  75 │  85 │
  └─────┴─────┴─────┴─────┘ └─────┴─────┴─────┴─────┘
     │                       │
     ▼                       ▼
  dst store #0:              dst store #1:
  ┌─────┬─────┬─────┬─────┬─────┬─────┬─────┬─────┐
  │  15 │  25 │  35 │  45 │  55 │  65 │  75 │  85 │
  └─────┴─────┴─────┴─────┴─────┴─────┴─────┴─────┘

TAIL HANDLING
═════════════

  For arrays where length % 4 != 0, remaining elements are processed scalar:

  ┌──────────────────────────────────────────────────────────────────┐
  │  for (i = numSamples4 << 2 .. length):                          │
  │      dst[i] = src[i] + value;      // scalar fallback           │
  └──────────────────────────────────────────────────────────────────┘

  Example: length = 6 (numSamples4 = 1, tail = 2 elements)

  SIMD:  ┌─────┬─────┬─────┬─────┐
         │ [0] │ [1] │ [2] │ [3] │  ← processed by int4
         └─────┴─────┴─────┴─────┘
  Tail:                     ┌─────┬─────┐
                            │ [4] │ [5] │  ← processed scalar
                            └─────┴─────┘

int4 SIMD REGISTER DETAIL
══════════════════════════

  ┌───────────────────────────────────────────────────┐
  │                int4 Register (128-bit)            │
  ├──────────┬──────────┬──────────┬──────────┤
  │  Lane 0  │  Lane 1  │  Lane 2  │  Lane 3  │
  │  int32   │  int32   │  int32   │  int32   │
  └──────────┴──────────┴──────────┴──────────┘

  src4[i] loads 4 contiguous ints into one register.
  Adding a scalar broadcasts it to all 4 lanes:

     src4[i] + value
     ┌───────────────────┐      ┌───┐
     │ 10 │ 20 │ 30 │ 40 │  +   │ 5 │   (broadcast)
     └───────────────────┘      └───┘
           │                     │
           ▼                     ▼
     ┌───────────────────────────────┐
     │ 15 │ 25 │ 35 │ 45 │           │  result stored to dst
     └───────────────────────────────┘

BURST COMPILATION DETAILS
══════════════════════════

  ● [BurstCompile] on the pointer overload
  ● [NoAlias] on dst — enables load/store reordering
  ● [ReadOnly] on src — compiler knows src won't change
  ● [AssumeRange(0, int.MaxValue)] on length — eliminates bounds checks
  ● AggressiveInlining on the NativeArray wrapper

  Burst will emit SIMD instructions:
    ARM64: LD1 → ADD (vector + scalar broadcast) → ST1
    x86-64: MOVAPS/MOVUPS → PADDD → MOVAPS/MOVUPS
```
