```
╔══════════════════════════════════════════════════════════════════════════════╗
║        BitArray256 — 256-bit Compact Bitmask for SIMD & Memory              ║
║         Source: BovineLabs.Core/Collections/BitArray.cs                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  A 256-bit unmanaged bitmask stored as 4 contiguous ulong (64-bit) fields.
  Replaces 256 individual booleans (256 bytes) with just 32 bytes —
  an 8× memory reduction. Burst-friendly, blittable, serializable.

MEMORY LAYOUT
═════════════

  struct BitArray256 — 32 bytes total, 256 bits:

  Offset  Field    Bits         Content
  ──────  ─────    ────         ───────
    0     data1    [  0.. 63]   ◀── first 64 bits
    8     data2    [ 64..127]   ◀── second 64 bits
   16     data3    [128..191]   ◀── third 64 bits
   24     data4    [192..255]   ◀── fourth 64 bits

  ┌─────────────────────────────────────────────────────────────────┐
  │ data1 (ulong)  │ data2 (ulong)  │ data3 (ulong)  │ data4 (ulong)│
  │ bits [0..63]   │ bits [64..127] │ bits[128..191] │ bits[192..255]│
  └─────────────────────────────────────────────────────────────────┘
   ◄────────────────── 32 bytes total ──────────────────▶

  vs. bool[256] = 256 bytes (1 byte per bool in C#)

  Memory savings: 256 → 32 bytes = 87.5% reduction!

BIT INDEXING MAPPING
════════════════════

  Bit index → field + local position:

  ┌──────────────┬─────────────┬────────────────────┐
  │  Index Range │  Field      │  Local Shift       │
  ├──────────────┼─────────────┼────────────────────┤
  │   0 ..  63   │  data1      │  index - 0         │
  │  64 .. 127   │  data2      │  index - 64        │
  │ 128 .. 191   │  data3      │  index - 128       │
  │ 192 .. 255   │  data4      │  index - 192       │
  └──────────────┴─────────────┴────────────────────┘

  Get256(index, data1, data2, data3, data4):
  ┌─────────────────────────────────────────────────────────────────┐
  │  index < 64?   → (data1 >> index) & 1                          │
  │  index < 128?  → (data2 >> (index - 64)) & 1                  │
  │  index < 192?  → (data3 >> (index - 128)) & 1                 │
  │  else          → (data4 >> (index - 192)) & 1                 │
  └─────────────────────────────────────────────────────────────────┘

BITWISE OPERATIONS
══════════════════

  All operations work field-by-field — effectively 4 parallel 64-bit ops:

  OR (a | b):
  ┌──────────┐ ┌──────────┐     ┌──────────┐
  │  a.data1 │ │  b.data1 │     │a.d1|b.d1 │
  │  a.data2 │ │  b.data2 │ ──▶ │a.d2|b.d2 │
  │  a.data3 │ │  b.data3 │     │a.d3|b.d3 │
  │  a.data4 │ │  b.data4 │     │a.d4|b.d4 │
  └──────────┘ └──────────┘     └──────────┘

  AND (a & b):
  Same layout, but with & instead of |

  NOT (~a):
  ┌──────────┐     ┌──────────┐
  │  a.data1 │ ──▶ │  ~a.d1   │
  │  a.data2 │     │  ~a.d2   │
  │  a.data3 │     │  ~a.d3   │
  │  a.data4 │     │  ~a.d4   │
  └──────────┘     └──────────┘

  CountBits:
  ┌─────────────────────────────────────────────────────────────────┐
  │  return countbits(data1) + countbits(data2) +                  │
  │         countbits(data3) + countbits(data4)                    │
  │  (uses hardware POPCNT when available via Unity.Mathematics)   │
  └─────────────────────────────────────────────────────────────────┘

  IsPowerOf2:
  ┌─────────────────────────────────────────────────────────────────┐
  │  countbits(data1) + countbits(data2) +                         │
  │  countbits(data3) + countbits(data4) == 1                      │
  │  → exactly one bit set                                         │
  └─────────────────────────────────────────────────────────────────┘

SET OPERATION — Set256
══════════════════════

  Setting bit at index to true:
  ┌─────────────────────────────────────────────────────────────────┐
  │  if index < 64:   data1 |= (1UL << index)                     │
  │  elif index <128:  data2 |= (1UL << (index - 64))             │
  │  elif index <192:  data3 |= (1UL << (index - 128))            │
  │  else:            data4 |= (1UL << (index - 192))             │
  └─────────────────────────────────────────────────────────────────┘

  Setting bit at index to false:
  ┌─────────────────────────────────────────────────────────────────┐
  │  Same branching, but with &= ~(1UL << localIndex)             │
  └─────────────────────────────────────────────────────────────────┘

IBitArray<T> INTERFACE
══════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  IBitArray<BitArray256>                                        │
  │  ├── uint Capacity => 256                                      │
  │  ├── bool AllFalse  => data1==0 && data2==0 && d3==0 && d4==0 │
  │  ├── bool AllTrue   => all == ulong.MaxValue                   │
  │  ├── bool this[int] { get; set; }                              │
  │  ├── BitArray256 BitAnd(other) => this & other                 │
  │  ├── BitArray256 BitOr(other)  => this | other                 │
  │  ├── BitArray256 BitNot()      => ~this                        │
  │  └── int CountBits()           => sum of countbits per field   │
  └─────────────────────────────────────────────────────────────────┘

USE CASES IN ECS
═════════════════

  ● Component category flags (up to 256 unique categories)
  ● Layer/type bitmask for filtering
  ● 256-way feature toggle in a single component field
  ● Serialization-friendly via [SerializeField] on each ulong
  ● UI editing via BitArray256Converter (PropertyDrawer)
```

## Verified Data

> [Run test snippet](../snippets/core-collections/BitArray256.cs) — 39 assertions passing
>
> Key findings:
> - 32 bytes total (4 × ulong fields: data1, data2, data3, data4)
> - Capacity property returns 256
> - Indexer get/set verified at boundaries: bit 0, 64 (data2), 128 (data3), 192 (data4), 255
> - CountBits() accurate across all 4 ulong fields
> - BitOr, BitAnd, BitNot produce correct results (BitNot on single bit → 255 bits set)

## Source

- [BovineLabs.Core.Editor/UI/BitArray256Converter.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/UI/BitArray256Converter.cs)
- [BovineLabs.Core/Collections/BitArray.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/BitArray.cs)
- [BovineLabs.Core/States/AppAPI.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/States/AppAPI.cs)
