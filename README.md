```
╔══════════════════════════════════════════════════════════════════════════════╗
║       FixedArray<T,TS> — Fixed-Size Array Without Unsafe Fixed Buffers      ║
║         Source: BovineLabs.Core/Collections/FixedArray.cs                    ║
╚══════════════════════════════════════════════════════════════════════════════╝

OVERVIEW
════════
  A generic fixed-size array struct that uses an unmanaged storage type
  as backing memory. Avoids C# unsafe fixed buffer limitations while
  providing Burst-compatible, zero-allocation array semantics.

PROBLEM IT SOLVES
══════════════════

  C# fixed buffers have major restrictions:
  ● Can only hold primitive types (bool, byte, int, float, etc.)
  ● Cannot be generic
  ● Require unsafe context
  ● Cannot hold structs, enums, or other fixed arrays

  FixedArray<T,TS> solves this by:
  ● Accepting any unmanaged T (element type)
  ● Using TS (storage type) as a raw memory block
  ● Computing Length from sizeof(TS) / sizeof(T)

STRUCT DEFINITION
═════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  unsafe struct FixedArray<T, TS>                                │
  │      where T : unmanaged                                       │
  │      where TS : unmanaged                                      │
  │  {                                                              │
  │      private TS data;                                          │
  │                                                                 │
  │      int Length => sizeof(TS) / sizeof(T);                     │
  │      T* Buffer  => (T*)&data;  // fixed pointer                │
  │      T this[int] { get; set; }  // indexed access              │
  │      ref T ElementAt(int);      // ref return                  │
  │  }                                                              │
  └─────────────────────────────────────────────────────────────────┘

MEMORY MODEL
════════════

  The TS field provides raw storage; elements are overlaid on it:

  Example: FixedArray<float, float4>
    sizeof(TS) = 16 bytes
    sizeof(T)  = 4 bytes
    Length     = 16 / 4 = 4 elements

  Memory layout:
  ┌───────┬───────┬───────┬───────┐
  │ [0]   │ [1]   │ [2]   │ [3]   │  ← float elements
  │ float │ float │ float │ float │
  └───────┴───────┴───────┴───────┘
   ◄────── data: float4 (16B) ────▶

  Example: FixedArray<int, ulong>
    sizeof(TS) = 8 bytes
    sizeof(T)  = 4 bytes
    Length     = 8 / 4 = 2 elements

  ┌───────────┬───────────┐
  │   [0]     │   [1]     │
  │   int     │   int     │
  └───────────┴───────────┘
   ◄── data: ulong (8B) ──▶

  Example: FixedArray<byte, float4x4>
    sizeof(TS) = 64 bytes
    sizeof(T)  = 1 byte
    Length     = 64 / 1 = 64 elements

  ┌──┬──┬──┬──┬──┬──┬──┬──┬ ... ┬──┐
  │[0]│[1]│[2]│[3]│[4]│[5]│[6]│[7]│     │[63]│
  └──┴──┴──┴──┴──┴──┴──┴──┴──┴ ... ┴──┘
   ◄──── data: float4x4 (64B) ──────▶

INDEXER — READ
══════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  get:                                                           │
  │    CollectionHelper.CheckIndexInRange(index, Length);           │
  │    return UnsafeUtility.ReadArrayElement<T>(Buffer, index);    │
  │                           │                                     │
  │              ┌────────────┴──────────┐                          │
  │              │  Computes address:    │                          │
  │              │  Buffer + index * sizeof(T)                     │
  │              │  = &data + index * sizeof(T)                    │
  │              └───────────────────────┘                          │
  └─────────────────────────────────────────────────────────────────┘

  Visual for FixedArray<int, ulong>, reading [1]:

  data (ulong):
  ┌───────────┬───────────┐
  │   [0]     │   [1]     │
  │ address+0 │ address+4 │  ← ReadArrayElement reads at offset 4
  └───────────┴───────────┘

INDEXER — WRITE
═══════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  set:                                                           │
  │    CollectionHelper.CheckIndexInRange(index, Length);           │
  │    UnsafeUtility.WriteArrayElement(Buffer, index, value);      │
  │               │                                                 │
  │    Writes value at Buffer + index * sizeof(T)                  │
  └─────────────────────────────────────────────────────────────────┘

ELEMENTAT — REF RETURN
═══════════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  ref T ElementAt(int index):                                    │
  │    CheckIndexInRange(index, Length);                            │
  │    return ref UnsafeUtility.ArrayElementAsRef<T>(Buffer, idx); │
  │                                                                 │
  │  Returns a managed ref to the element — allows in-place        │
  │  modification without copying:                                  │
  │                                                                 │
  │    fixedArray.ElementAt(2).x = 5f;  // modify struct field     │
  └─────────────────────────────────────────────────────────────────┘

POINTER ACCESS PATTERN
══════════════════════

  The Buffer property uses fixed pinning:

  ┌─────────────────────────────────────────────────────────────────┐
  │  T* Buffer:                                                    │
  │    fixed (void* ptr = &data)    // pin 'data' in place         │
  │        return (T*)ptr;          // cast raw memory to T*       │
  └─────────────────────────────────────────────────────────────────┘

  This is safe because:
  ● FixedArray is a struct (stack or value-type contained)
  ● The fixed block pins for the duration of the accessor
  ● Burst compiles this to direct address computation

LENGTH COMPUTATION
══════════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  readonly int Length => sizeof(TS) / sizeof(T)                 │
  │                                                                 │
  │  ● Computed at compile time by Burst                           │
  │  ● No runtime division — becomes a constant                    │
  │  ● sizeof() works on unmanaged types in unsafe context         │
  └─────────────────────────────────────────────────────────────────┘

USAGE PATTERNS
══════════════

  ┌─────────────────────────────────────────────────────────────────┐
  │  // 4 floats using float4 as storage                           │
  │  FixedArray<float, float4> weights;                            │
  │  weights[0] = 1.0f;                                            │
  │                                                                 │
  │  // 8 bytes using ulong as storage                             │
  │  FixedArray<byte, ulong> flags;                                │
  │  flags[3] = 0xFF;                                              │
  │                                                                 │
  │  // 4 int3 vectors using a 48-byte storage struct              │
  │  struct Int3x4 { fixed int Data[12]; }                         │
  │  FixedArray<int3, Int3x4> normals;                             │
  │  normals[2] = new int3(1, 0, 0);                              │
  └─────────────────────────────────────────────────────────────────┘

  Note: Any unmanaged struct with sufficient size can serve as TS.
  Common choices: float4, float4x4, ulong, or custom fixed-buffer structs.
```
