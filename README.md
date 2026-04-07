```
╔══════════════════════════════════════════════════════════════════════════════╗
║       MiniString — Ultra-Compact 15-Byte Inline String for ECS              ║
║         Source: BovineLabs.Core/Collections/MiniString.cs                    ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  A 16-byte fixed-size UTF-8 string struct for use as ECS component fields.
  Stores up to 15 UTF-8 bytes with 1 byte for length — no heap allocation.

  Implements INativeList<byte> and IUTF8Bytes for Unity collection interop.

MEMORY LAYOUT
═════════════

  [StructLayout(LayoutKind.Sequential, Size = 16)]
  struct MiniString

  ┌────────┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
  │ byte00 │01│02│03│04│05│06│07│08│09│10│11│12│13│14│15│
  │ LENGTH │  U  U  U  U  U  U  U  U  U  U  U  U  U  U  U │
  │ (1 B)  │◄────────── UTF-8 DATA (up to 15 bytes) ──────▶│
  └────────┴──────────────────────────────────────────────┘
   ◄──────────────── Total: 16 bytes ────────────────▶

  byte00 (byte0000 of FixedBytes16) = UTF8LengthInBytes = Length
  byte01..byte15 = UTF-8 encoded character data

  Example: MiniString = "Hello"
  ┌────────┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┐
  │   5    │ H │ e │ l │ l │ o │ ? │ ? │ ? │ ? │ ? │ ? │ ? │ ? │ ? │ ? │
  │ length │      UTF-8 data (5 bytes used, 10 unused)                      │
  └────────┴───────────────────────────────────────────────────────────────┘

VS ALTERNATIVES
═══════════════

  ┌──────────────────┬───────────┬───────────────────┬─────────────────┐
  │ Type             │ Size      │ Max UTF-8 Length  │ Heap?           │
  ├──────────────────┼───────────┼───────────────────┼─────────────────┤
  │ string (C#)      │ 20+ ptr   │ unlimited         │ YES (GC)        │
  │ FixedString32    │ 32 bytes  │ 30 bytes          │ no              │
  │ FixedString64    │ 64 bytes  │ 62 bytes          │ no              │
  │ FixedString128   │ 128 bytes │ 126 bytes         │ no              │
  │ FixedString512   │ 512 bytes │ 510 bytes         │ no              │
  │ MiniString       │ 16 bytes  │ 15 bytes          │ no              │
  └──────────────────┴───────────┴───────────────────┴─────────────────┘

  MiniString is the SMALLEST option — half the size of FixedString32.
  Ideal for short identifiers, names, tags in dense ECS data.

POINTER ACCESS
══════════════

  GetUnsafePtr() returns a pointer to byte01 (skips the length byte):

  ┌────────┬────────────────────────────────────────────────────────┐
  │ byte00 │ byte01 byte02 byte03 ... byte15                       │
  │ length ▲                                                        │
  │        │                                                        │
  │        └─ GetUnsafePtr() points here (byte0001 of FixedBytes16)│
  │           = UnsafeUtility.AddressOf(ref bytes.byte0001)        │
  └────────┴────────────────────────────────────────────────────────┘

  This means the indexer this[index] accesses GetUnsafePtr()[index],
  which is byte01+index — the UTF-8 data area.

CONSTRUCTION PATHS
══════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  1. From string:                                                │
  │     new MiniString("Hello")                                    │
  │     → UTF8ArrayUnsafeUtility.Copy into byte01..byte15          │
  │     → UTF8LengthInBytes set to byte count                      │
  │     → CopyError checked (throws if >15 bytes)                  │
  │                                                                 │
  │  2. From FixedString32Bytes:                                    │
  │     new MiniString(fixedStr)                                   │
  │     → MemCpy from FixedString's data pointer                   │
  │     → Length = source.Length                                   │
  │                                                                 │
  │  3. Implicit conversions:                                       │
  │     MiniString ms = "text";        ← string → MiniString       │
  │     MiniString ms = fixedStr32;    ← FixedString32 → MiniString│
  │     FixedString32 fs = ms;         ← MiniString → FixedString32│
  └─────────────────────────────────────────────────────────────────┘

UTF-8 ENCODING DETAIL
═════════════════════

  UTF-8 code points use 1-4 bytes each:

  ┌─────────────────────────┬───────────┬───────────────────────┐
  │ Unicode Range           │ Bytes/Char │ Example               │
  ├─────────────────────────┼───────────┼───────────────────────┤
  │ U+0000..U+007F (ASCII)  │    1      │ 'A' = 0x41            │
  │ U+0080..U+07FF          │    2      │ 'é' = 0xC3 0xA9       │
  │ U+0800..U+FFFF          │    3      │ '好' = 0xE5 0xA5 0xBD │
  │ U+10000..U+10FFFF       │    4      │ '😀' = 0xF0 0x9F...   │
  └─────────────────────────┴───────────┴───────────────────────┘

  Max characters by encoding:
  ● ASCII only:  15 characters
  ● Latin extended: 7 characters
  ● CJK: 5 characters
  ● Emoji: 3 characters

EQUALITY COMPARISON
════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  bool operator ==(in MiniString a, in MiniString b):            │
  │                                                                 │
  │    int alen = a.UTF8LengthInBytes;                              │
  │    int blen = b.UTF8LengthInBytes;                              │
  │    return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(              │
  │        a.GetUnsafePtr(), alen,                                  │
  │        b.GetUnsafePtr(), blen);                                │
  │                                                                 │
  │  ● Compares length first, then byte-by-byte                    │
  │  ● Uses Burst-optimized UTF8 comparison                        │
  │  ● Does NOT call any methods on a/b (direct field access)      │
  │    to avoid potential issues with in parameters                 │
  └─────────────────────────────────────────────────────────────────┘

RESIZE OPERATIONS
═════════════════

  TryResize(int newLength, NativeArrayOptions clearOptions):

  ┌─────────────────────────────────────────────────────────────────┐
  │  if newLength < 0 or > 15: return false                        │
  │  if newLength == current:  return true                         │
  │                                                                 │
  │  if growing: MemClear(oldLen..newLen)  // zero new bytes       │
  │  if shrinking: MemClear(newLen..oldLen) // zero old bytes      │
  │                                                                 │
  │  UTF8LengthInBytes = newLength                                  │
  └─────────────────────────────────────────────────────────────────┘

  Clear():
  ┌─────────────────────────────────────────────────────────────────┐
  │  Length = 0;    ← just zero the length byte                    │
  └─────────────────────────────────────────────────────────────────┘

  Add(byte value):
  ┌─────────────────────────────────────────────────────────────────┐
  │  this[Length++] = value;   ← append byte, increment length    │
  └─────────────────────────────────────────────────────────────────┘

INTERFACES IMPLEMENTED
══════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  INativeList<byte>                                              │
  │  ├── int Length { get; set; }                                   │
  │  ├── int Capacity => 15                                        │
  │  ├── bool IsEmpty => Length == 0                               │
  │  ├── void Clear()                                              │
  │  ├── void Add(in byte value)                                   │
  │  ├── bool TryResize(int, NativeArrayOptions)                   │
  │  └── byte this[int] { get; set; }                              │
  │                                                                 │
  │  IUTF8Bytes                                                     │
  │  └── byte* GetUnsafePtr()                                      │
  │                                                                 │
  │  IEquatable<MiniString>                                        │
  │  └── bool Equals(MiniString)                                   │
  └─────────────────────────────────────────────────────────────────┘

USE CASES
══════════

  ● Entity names/tags (short identifiers)
  ● Component labels in minimal-memory archetypes
  ● Lookup keys that fit in 15 UTF-8 bytes
  ● Serialization-safe string fields (no GC pressure)
  ● Interop with FixedString32Bytes for Unity APIs
```
