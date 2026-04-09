```
╔══════════════════════════════════════════════════════════════════════════════╗
║         BitArray128 — 128-bit Unmanaged Bitmask Mapped to v128              ║
║         Source: BovineLabs.Core/Collections/BitArray.cs                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  A 128-bit bitmask stored as two contiguous ulong fields.
  Can be constructed from v128 SIMD registers for direct
  interoperability with Burst intrinsics.

MEMORY LAYOUT
═════════════

  struct BitArray128 — 16 bytes total, 128 bits:

  ┌─────────────────────────────────┬─────────────────────────────────┐
  │         data1 (ulong)           │         data2 (ulong)           │
  │         bits [0..63]            │         bits [64..127]          │
  └─────────────────────────────────┴─────────────────────────────────┘
   ◄──────────── 16 bytes total ────────────▶

V128 SIMD MAPPING
═════════════════

  The constructor BitArray128(v128 initValue) maps directly:

  v128 register:
  ┌─────────────────────────────────┬─────────────────────────────────┐
  │          ULong0 (64-bit)        │          ULong1 (64-bit)        │
  └───────────────┬─────────────────┴───────────────┬─────────────────┘
                  │                                 │
                  ▼                                 ▼
  BitArray128:
  ┌─────────────────────────────────┬─────────────────────────────────┐
  │          data1 = ULong0         │          data2 = ULong1         │
  │          bits [0..63]           │          bits [64..127]         │
  └─────────────────────────────────┴─────────────────────────────────┘

  This means BitArray128 can be loaded/stored directly with
  128-bit SIMD load/store instructions:

  ┌─────────────────────────────────────────────────────────────────┐
  │  // Load from memory into v128                                  │
  │  v128 simd = load128(&bitArray);                               │
  │                                                                 │
  │  // Store v128 back to BitArray128                              │
  │  BitArray128 result = new BitArray128(simd);                   │
  └─────────────────────────────────────────────────────────────────┘

BIT INDEX MAPPING
═════════════════

  ┌──────────────┬─────────────┬────────────────────┐
  │  Index Range │  Field      │  Local Shift       │
  ├──────────────┼─────────────┼────────────────────┤
  │   0 ..  63   │  data1      │  index             │
  │  64 .. 127   │  data2      │  index - 64        │
  └──────────────┴─────────────┴────────────────────┘

  Get128(index, data1, data2):
  ┌─────────────────────────────────────────────────────────────────┐
  │  index < 64 ?                                                   │
  │    (data1 & (1UL << index)) != 0                                │
  │  :                                                              │
  │    (data2 & (1UL << (index - 64))) != 0                         │
  └─────────────────────────────────────────────────────────────────┘

BITWISE OPERATIONS — FIELD-PAIR PARALLEL
════════════════════════════════════════

  OR (a | b):
  ┌──────────┐ ┌──────────┐     ┌──────────────┐
  │  a.data1 │ │  b.data1 │     │ a.d1 | b.d1  │
  ├──────────┤ ├──────────┤ ──▶ ├──────────────┤
  │  a.data2 │ │  b.data2 │     │ a.d2 | b.d2  │
  └──────────┘ └──────────┘     └──────────────┘

  AND (a & b):
  ┌──────────┐ ┌──────────┐     ┌──────────────┐
  │  a.data1 │ │  b.data1 │     │ a.d1 & b.d1  │
  ├──────────┤ ├──────────┤ ──▶ ├──────────────┤
  │  a.data2 │ │  b.data2 │     │ a.d2 & b.d2  │
  └──────────┘ └──────────┘     └──────────────┘

  NOT (~a):
  ┌──────────┐     ┌──────────────┐
  │  a.data1 │ ──▶ │  ~a.data1   │
  ├──────────┤     ├──────────────┤
  │  a.data2 │     │  ~a.data2   │
  └──────────┘     └──────────────┘

  CountBits:
  ┌─────────────────────────────────────────────────────────────────┐
  │  countbits(data1) + countbits(data2)                            │
  │  → maps to POPCNT + POPCNT (x86) or CNT+CNT (ARM NEON)       │
  └─────────────────────────────────────────────────────────────────┘

EQUALITY
════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  (a == b)  →  a.data1 == b.data1 && a.data2 == b.data2        │
  │  (a != b)  →  a.data1 != b.data1 || a.data2 != b.data2        │
  └─────────────────────────────────────────────────────────────────┘

CONSTRUCTOR FROM INDICES
════════════════════════

  BitArray128(Span<uint> bitIndexTrue):
  ┌─────────────────────────────────────────────────────────────────┐
  │  data1 = data2 = 0;                                            │
  │  foreach (bitIndex in bitIndexTrue):                            │
  │    if bitIndex < 64:                                            │
  │      data1 |= (1UL << bitIndex)                                │
  │    elif bitIndex < 128:                                         │
  │      data2 |= (1UL << (bitIndex - 64))                         │
  │                                                                 │
  │  Example: indices = {0, 63, 64, 127}                           │
  │  data1 = (1<<0) | (1<<63) = 0x8000000000000001                │
  │  data2 = (1<<0) | (1<<63) = 0x8000000000000001                │
  └─────────────────────────────────────────────────────────────────┘

SIMD INTEROP PATTERN
═════════════════════

  BitArray128 can be directly used with Burst intrinsics:

  ┌─────────────────────────────────────────────────────────────────┐
  │  BitArray128 mask = ...;                                        │
  │                                                                 │
  │  // Reinterpret as v128 for SIMD operations                    │
  │  v128 v = new v128(mask.Data1, mask.Data2);                   │
  │                                                                 │
  │  // Use in SIMD comparison/mask operations                     │
  │  v128 result = and(v, someOtherV128);                          │
  │                                                                 │
  │  // Convert back                                                │
  │  BitArray128 masked = new BitArray128(result);                 │
  └─────────────────────────────────────────────────────────────────┘

USE CASES
══════════

  ● Medium-sized flag sets (up to 128 component categories)
  ● SIMD-friendly filtering masks
  ● 128-bit component type matching in archetype queries
  ● Interop with v128 for vectorized bit manipulation
```

## Verified Data

```
BitArray128
  Kind: struct
  Size: 16 bytes
Fields:
  [0] UInt64 data1  (private)
  [8] UInt64 data2  (private)
Properties:
  UInt64 Data1 { get; }
  UInt64 Data2 { get; }
  UInt32 Capacity { get; }
  Boolean AllFalse { get; }
  Boolean AllTrue { get; }
  String HumanizedData { get; }
  Boolean Item { get;set }
  Boolean Item { get;set }
Methods:
  BitArray128 BitAnd(BitArray128)
  BitArray128 BitOr(BitArray128)
  BitArray128 BitNot()
  Int32 CountBits()
  Boolean Equals(Object)
  Int32 GetHashCode()
  Boolean Equals(BitArray128)
Runtime Behavior:
  default: CountBits=0, AllFalse=True, AllTrue=False
  Capacity=128
  set[0]=true: bits[0]=True, CountBits=1
  set[63,64,127]=true: CountBits=4
  BitOr({0},{1}): bits[0]=True, bits[1]=True, CountBits=2
  BitAnd({0,1},{0}): bits[0]=True, bits[1]=False, CountBits=1
  BitNot({0}): bits[0]=False, CountBits=127
Verified: 6 checks, 0 failures
```
BitArray128
  Kind: struct
  Size: 16 bytes
Fields:
  [0] UInt64 data1  (private)
  [8] UInt64 data2  (private)
Properties:
  UInt64 Data1 { get; }
  UInt64 Data2 { get; }
  UInt32 Capacity { get; }
  Boolean AllFalse { get; }
  Boolean AllTrue { get; }
  String HumanizedData { get; }
  Boolean Item { get;set }
  Boolean Item { get;set }
Methods:
  BitArray128 BitAnd(BitArray128)
  BitArray128 BitOr(BitArray128)
  BitArray128 BitNot()
  Int32 CountBits()
  Boolean Equals(Object)
  Int32 GetHashCode()
  Boolean Equals(BitArray128)
Runtime Behavior:
  default: CountBits=0, AllFalse=True, AllTrue=False
  Capacity=128
  set[0]=true: bits[0]=True, CountBits=1
  set[63,64,127]=true: CountBits=4
  BitOr({0},{1}): bits[0]=True, bits[1]=True, CountBits=2
  BitAnd({0,1},{0}): bits[0]=True, bits[1]=False, CountBits=1
  BitNot({0}): bits[0]=False, CountBits=127
Verified: 6 checks, 0 failures
```

## Source

- [BovineLabs.Core/Collections/BitArray.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/BitArray.cs)
- [BovineLabs.Core/Utility/WriteGroupMatcher.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/WriteGroupMatcher.cs)
- [BovineLabs.Core/Extensions/ArchetypeChunkExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/ArchetypeChunkExtensions.cs)
