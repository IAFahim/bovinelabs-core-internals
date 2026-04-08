```
╔══════════════════════════════════════════════════════════════════════════════╗
║      BitArray8 / BitArray16 / BitArray32 / BitArray64 — Fixed Bitmasks      ║
║         Source: BovineLabs.Core/Collections/BitArray.cs                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  Four fixed-size bitmask structs for small component flags.
  Each wraps a single primitive field and implements IBitArray<T>.

  All are unmanaged, blittable, serializable, and Burst-friendly.

TYPE HIERARCHY
══════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │                    IBitArray<T>                                 │
  │  (where T : unmanaged, IBitArray<T>)                           │
  │  ├── Capacity : uint                                           │
  │  ├── AllFalse : bool                                           │
  │  ├── AllTrue  : bool                                           │
  │  ├── this[int] : bool                                          │
  │  ├── BitAnd(T) : T                                             │
  │  ├── BitOr(T)  : T                                             │
  │  ├── BitNot()  : T                                             │
  │  └── CountBits() : int                                         │
  ├─────────────────────────────────────────────────────────────────┤
  │                                                                 │
  │  ┌──────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐       │
  │  │BitArray8 │ │BitArray16 │ │BitArray32 │ │BitArray64 │       │
  │  │  byte    │ │  ushort   │ │   uint    │ │   ulong   │       │
  │  │  8 bits  │ │  16 bits  │ │  32 bits  │ │  64 bits  │       │
  │  │  1 byte  │ │  2 bytes  │ │  4 bytes  │ │  8 bytes  │       │
  │  └──────────┘ └───────────┘ └───────────┘ └───────────┘       │
  └─────────────────────────────────────────────────────────────────┘

MEMORY COMPARISON
═════════════════

  ┌──────────────┬────────────┬────────────────┬──────────────────┐
  │ Type         │ Backing    │ bool equiv.    │ Savings          │
  ├──────────────┼────────────┼────────────────┼──────────────────┤
  │ BitArray8    │ byte (1B)  │ bool[8] = 8B  │ 87.5% (8×)      │
  │ BitArray16   │ ushort(2B) │ bool[16]= 16B │ 87.5% (8×)      │
  │ BitArray32   │ uint  (4B) │ bool[32]= 32B │ 87.5% (8×)      │
  │ BitArray64   │ ulong (8B) │ bool[64]= 64B │ 87.5% (8×)      │
  └──────────────┴────────────┴────────────────┴──────────────────┘

BIT LAYOUT — BitArray8
═══════════════════════

  Single byte field 'data':

    Bit:  7  6  5  4  3  2  1  0
          ┌──┬──┬──┬──┬──┬──┬──┬──┐
  data =  │ 0│ 1│ 1│ 0│ 0│ 1│ 0│ 1│  = 0x65 = 101
          └──┴──┴──┴──┴──┴──┴──┴──┘
           ▲           ▲        ▲
           │           │        └── bit 0: true
           │           └── bit 2: true
           └── bit 6: true

BIT LAYOUT — BitArray16
════════════════════════

  ushort field 'data' (2 bytes):

    Byte 1 (high)      Byte 0 (low)
    ┌───────────────┬───────────────┐
    │ bits [15..8]  │ bits [7..0]   │
    └───────────────┴───────────────┘
        ushort (2 bytes, 16 bits)

BIT LAYOUT — BitArray32
════════════════════════

  uint field 'data' (4 bytes):

    ┌───────────┬───────────┬───────────┬───────────┐
    │ bits[31:24]│bits[23:16]│bits[15:8] │ bits[7:0] │
    └───────────┴───────────┴───────────┴───────────┘
                     uint (4 bytes, 32 bits)

BIT LAYOUT — BitArray64
════════════════════════

  ulong field 'data' (8 bytes):

    ┌───────────┬───────────┬───────────┬───────────┬───────────┬───────────┬───────────┬───────────┐
    │ bits[63:56]│bits[55:48]│bits[47:40]│bits[39:32]│bits[31:24]│bits[23:16]│bits[15:8] │ bits[7:0] │
    └───────────┴───────────┴───────────┴───────────┴───────────┴───────────┴───────────┴───────────┘
                                      ulong (8 bytes, 64 bits)

INDEXER OPERATIONS — Get / Set
══════════════════════════════

  GET (read bit at index):
  ┌─────────────────────────────────────────────────────────────────┐
  │  Get32(index, data):                                           │
  │    return (data & (1u << index)) != 0                          │
  │                                                                 │
  │  Example: index=3, data=0b...00010100                          │
  │    mask = 1u << 3 = 0b...00001000                              │
  │    data & mask = 0b...00000000  → false                        │
  │                                                                 │
  │  Example: index=2, data=0b...00010100                          │
  │    mask = 1u << 2 = 0b...00000100                              │
  │    data & mask = 0b...00000100  → true                         │
  └─────────────────────────────────────────────────────────────────┘

  SET TRUE (write 1 to bit):
  ┌─────────────────────────────────────────────────────────────────┐
  │  Set32(index, ref data, true):                                  │
  │    data = data | (1u << index)       ← OR to set bit           │
  └─────────────────────────────────────────────────────────────────┘

  SET FALSE (write 0 to bit):
  ┌─────────────────────────────────────────────────────────────────┐
  │  Set32(index, ref data, false):                                 │
  │    data = data & ~(1u << index)      ← AND with complement     │
  └─────────────────────────────────────────────────────────────────┘

OPERATOR OVERLOADS
══════════════════

  All four types support the same operator set:

  ┌──────────┬───────────────────────────────────────────────────┐
  │ Operator │ Implementation                                    │
  ├──────────┼───────────────────────────────────────────────────┤
  │  a | b   │ new BitArrayN(a.data | b.data)                   │
  │  a & b   │ new BitArrayN(a.data & b.data)                   │
  │  ~a      │ new BitArrayN(~a.data)                           │
  │  a == b  │ a.data == b.data                                 │
  │  a != b  │ a.data != b.data                                 │
  └──────────┴───────────────────────────────────────────────────┘

  CountBits uses Unity.Mathematics math.countbits():
  ● BitArray8:  countbits((uint)data)
  ● BitArray16: countbits((uint)data)
  ● BitArray32: countbits(data)
  ● BitArray64: countbits(data)
  All map to hardware POPCNT/NEON CNT instruction when available.

CONSTRUCTOR FROM INDICES
════════════════════════

  BitArray64(Span<uint> bitIndexTrue):
  ┌─────────────────────────────────────────────────────────────────┐
  │  data = 0;                                                      │
  │  foreach (bitIndex in bitIndexTrue):                            │
  │    if bitIndex < Capacity:                                      │
  │      data |= (1UL << bitIndex);                                │
  │                                                                 │
  │  Example: indices = {0, 3, 7}                                   │
  │  data = (1<<0)|(1<<3)|(1<<7) = 0b10001001 = 137               │
  └─────────────────────────────────────────────────────────────────┘

STATIC FIELDS
═════════════

  Each type has two static readonly instances:

  ┌──────────────┬─────────────────────────────┬─────────────────────┐
  │ Type         │ All                         │ None                │
  ├──────────────┼─────────────────────────────┼─────────────────────┤
  │ BitArray8    │ new(byte.MaxValue) = 0xFF   │ default  = 0x00    │
  │ BitArray16   │ new(ushort.MaxValue)=0xFFFF │ default  = 0x0000  │
  │ BitArray32   │ new(uint.MaxValue)=~0u      │ default  = 0       │
  │ BitArray64   │ new(ulong.MaxValue)=~0UL    │ default  = 0       │
  └──────────────┴─────────────────────────────┴─────────────────────┘

USE CASES
══════════

  ● BitArray8  — Up to 8 enum flags as a component field
  ● BitArray16 — Layer selection (up to 16 layers)
  ● BitArray32 — Unity layer mask compatibility (32 layers)
  ● BitArray64 — Extended flags, category masks, up to 64 features
```

## Verified Data

> [Run test snippet](../snippets/core-collections/BitArray8_16_32_64.cs) — 65 assertions passing
>
> Key findings:
> - BitArray8: 1 byte, Capacity=8; BitArray16: 2 bytes, Capacity=16
> - BitArray32: 4 bytes, Capacity=32; BitArray64: 8 bytes, Capacity=64
> - All sizes support indexer, CountBits, AllFalse/AllTrue, BitOr/BitAnd/BitNot
> - Static fields All (all bits set) and None (no bits set) verified for all sizes
> - BitArray64 operator overloads |, &, ~ and equality verified

## Source

- [BovineLabs.Core/Collections/BitArray.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/BitArray.cs)
- [BovineLabs.Core.Tests/Models/TimerEnableableTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Models/TimerEnableableTests.cs)
- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMapHelper2.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicVariableMapHelper2.cs)
