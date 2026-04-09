# DynamicUntypedBuffer

**Stores mixed unmanaged types manually inside a single DynamicBuffer<byte>, each element tracked by offset, size, type hash, and alignment.**

## Overview

DynamicUntypedBuffer is a type-erased list that stores heterogeneous unmanaged types
side-by-side in one contiguous `DynamicBuffer<byte>`. Unlike `DynamicBuffer<T>` which holds
elements of a single type, this buffer can store an `int` at index 0, a `float3` at index 1,
a `MyCustomStruct` at index 2, and so on.

Each element is tracked by per-element metadata arrays: **Offsets[]** (byte position in data
area), **Sizes[]** (size in bytes), **Types[]** (Burst type hash for runtime type checking),
and **Alignments[]** (alignment requirement). The data area is a packed, properly-aligned
sequence of variable-size blobs.

This is useful for ECS patterns where an entity needs to hold an arbitrary set of typed
parameters without pre-defining each as a separate component or buffer.

---

## Memory Layout

### Buffer-Level View

```
  DynamicBuffer<byte> contents:
  ╔══════════════════════════════════════════════════════════════════════════════╗
  ║                                                                            ║
  ║  [0x00]  DynamicUntypedBufferHelper  (40 bytes header)                    ║
  ║          ┌──────────────────────────────────────────────────────┐          ║
  ║          │ OffsetsOffset      (int)                            │          ║
  ║          │ SizesOffset        (int)                            │          ║
  ║          │ TypesOffset        (int)                            │          ║
  ║          │ AlignmentsOffset   (int)                            │          ║
  ║          │ DataOffset         (int)                            │          ║
  ║          │ Count              (int) — # of elements           │          ║
  ║          │ Capacity           (int) — max elements            │          ║
  ║          │ DataCapacity       (int) — data area size in bytes │          ║
  ║          │ DataAllocatedIndex (int) — data high-water mark    │          ║
  ║          │ Log2MinGrowth      (int)                            │          ║
  ║          └──────────────────────────────────────────────────────┘          ║
  ║                                                                            ║
  ║  [0x28]  Offsets[Capacity]        (4 * Capacity bytes)                    ║
  ║          ┌──────┬──────┬──────┬──────┐                                    ║
  ║          │  0   │  4   │ 16   │  ... │  byte offset into Data[]            ║
  ║          └──────┴──────┴──────┴──────┘                                    ║
  ║                                                                            ║
  ║  [aligned]  Sizes[Capacity]        (4 * Capacity bytes)                   ║
  ║          ┌──────┬──────┬──────┬──────┐                                    ║
  ║          │  4   │ 12   │  8   │  ... │  sizeof(T) for each element        ║
  ║          └──────┴──────┴──────┴──────┘                                    ║
  ║                                                                            ║
  ║  [aligned]  Types[Capacity]        (4 * Capacity bytes)                   ║
  ║          ┌──────┬──────┬──────┬──────┐                                    ║
  ║          │ H(int)│H(f3) │H(str)│  ... │  BurstRuntime.GetHashCode32<T>()   ║
  ║          └──────┴──────┴──────┴──────┘                                    ║
  ║                                                                            ║
  ║  [aligned]  Alignments[Capacity]   (1 * Capacity bytes)                   ║
  ║          ┌───┬───┬───┬───┐                                              ║
  ║          │ 4 │16 │ 8 │...│  alignment of each element                    ║
  ║          └───┴───┴───┴───┘                                              ║
  ║                                                                            ║
  ║  [aligned to 16]  Data[DataCapacity]  (variable-size data area)           ║
  ║          ┌──────────────────────────────────────────────────────┐          ║
  ║          │ [0]  int(42)     │ padding │ [1]  float3(1,2,3)     │  ...     ║
  ║          │ 4 bytes          │ 12 bytes│ 12 bytes              │          ║
  ║          └──────────────────────────────────────────────────────┘          ║
  ║                                                                            ║
  ╚══════════════════════════════════════════════════════════════════════════════╝
```

### Header Struct Detail

```
  DynamicUntypedBufferHelper  — 40 bytes (10 ints)
  ┌──────────┬─────────────────────┬──────────────────────────────────────────┐
  │ Offset   │ Field               │ Purpose                                  │
  ├──────────┼─────────────────────┼──────────────────────────────────────────┤
  │ 0x00     │ OffsetsOffset       │ Byte offset from buffer start to Offsets[]│
  │ 0x04     │ SizesOffset         │ Byte offset from buffer start to Sizes[] │
  │ 0x08     │ TypesOffset         │ Byte offset from buffer start to Types[] │
  │ 0x0C     │ AlignmentsOffset    │ Byte offset from buffer start to Aln[]   │
  │ 0x10     │ DataOffset          │ Byte offset from buffer start to Data[]  │
  │ 0x14     │ Count               │ Number of stored elements                │
  │ 0x18     │ Capacity            │ Max elements (metadata array slots)      │
  │ 0x1C     │ DataCapacity        │ Total bytes in Data area                 │
  │ 0x20     │ DataAllocatedIndex  │ Next free byte offset in Data            │
  │ 0x24     │ Log2MinGrowth       │ Growth granularity as log2               │
  └──────────┴─────────────────────┴──────────────────────────────────────────┘
```

### Data Area Layout (The Key Innovation)

```
  Data[] area — packed, aligned, variable-size elements:
  ════════════════════════════════════════════════════════════════

  Index 0: int (4 bytes, align 4)
  Index 1: float3 (12 bytes, align 16) ← needs padding!
  Index 2: double (8 bytes, align 8)
  Index 3: short (2 bytes, align 2)

  Data area byte-by-byte:
  ┌────────┬────────────────┬──────────────────────┬──────────┬───┬──────────┬─────┐
  │ int    │  PAD (12 bytes)│      float3          │  double  │P  │  short   │ ... │
  │ 42     │  (alignment)   │  (1.0, 2.0, 3.0)    │  3.14    │AD │  7       │     │
  │ 4B     │  align→16      │  12B                 │  8B      │6B │  2B      │     │
  └────────┴────────────────┴──────────────────────┴──────────┴───┴──────────┴─────┘
  │←─ 0 ─→│                 │←──── 16 ────────────→│←─ 28 ──→│   │←─ 36 ──→│
  │        │                 │                      │          │   │          │
  Offsets[0]=0              Offsets[1]=16          Offsets[2]=28  Offsets[3]=36
  Sizes[0]=4                Sizes[1]=12            Sizes[2]=8     Sizes[3]=2
  Types[0]=H(int)           Types[1]=H(float3)     Types[2]=H(double) Types[3]=H(short)
  Aln[0]=4                  Aln[1]=16              Aln[2]=8       Aln[3]=2

  DataAllocatedIndex = 38  (= 36 + 2)
```

---

## Alignment Algorithm

```
  AlignDataIndex helper — accounts for Data pointer's own alignment:
  ─────────────────────────────────────────────────────────────────

  WHY: Data area alignment is relative to absolute memory address,
  not just byte offset. If Data* itself is misaligned, simple
  offset alignment is wrong.

  Algorithm:
  ┌──────────────────────────────────────────────────────────┐
  │ 1. dataMisalignment = (int)(Data* & (align - 1))        │
  │                                                          │
  │ 2. IF dataMisalignment == 0:                             │
  │    └─ return Align(dataIndex, align)  // simple case     │
  │                                                          │
  │ 3. ELSE:                                                 │
  │    └─ return Align(dataIndex + dataMisalignment, align)  │
  │               - dataMisalignment                         │
  └──────────────────────────────────────────────────────────┘

  Example:
    Data* = 0x1006 (misaligned by 6 for align=16)
    dataIndex = 10, align = 16
    
    dataMisalignment = 6
    result = Align(10 + 6, 16) - 6 = Align(16, 16) - 6 = 16 - 6 = 10
    → So absolute address = 0x1006 + 10 = 0x1010 (16-aligned ✓)
```

---

## Add Algorithm

```
  DynamicUntypedBuffer.Add<TValue>(value):
  ────────────────────────────────────────

  1. IF Count == Capacity:
     └─ Resize metadata arrays (grow Capacity)

  2. idx = Count++
  3. size = sizeof(TValue)
  4. align = alignof(TValue)
  5. dataAllocIndex = AlignDataIndex(DataAllocatedIndex, align)
     ┌──────────────────────────────────────────────────────┐
     │ Aligns current write position to the element's       │
     │ alignment requirement. May add padding bytes.        │
     └──────────────────────────────────────────────────────┘

  6. IF dataAllocIndex + size > DataCapacity:
     └─ ResizeData (grow DataCapacity, may loop to find enough)

  7. dst = Data + dataAllocIndex
  8. MemCpy(&value → dst, size)         ← copy value bytes into data area

  9. Record metadata:
     Offsets[idx]     = dataAllocIndex
     Sizes[idx]       = size
     Types[idx]       = BurstRuntime.GetHashCode32<TValue>()
     Alignments[idx]  = (byte)align

  10. DataAllocatedIndex = dataAllocIndex + size
  11. RETURN idx
```

### Visual: Adding Mixed Types

```
  INITIAL: Count=0, DataAllocatedIndex=0, DataCapacity=64

  ── Add<int>(42): size=4, align=4 ──────────────────────────────
  dataAllocIndex = AlignDataIndex(0, 4) = 0
  Data: [42(int)                               ...free...       ]
        ↑ offset=0
  Offsets=[0], Sizes=[4], Types=[H(int)], Alignments=[4]
  DataAllocatedIndex = 4

  ── Add<float3>(1,2,3): size=12, align=16 ─────────────────────
  dataAllocIndex = AlignDataIndex(4, 16) = 16  ← 12 bytes padding!
  Data: [42 | PAD PAD PAD PAD | float3(1,2,3)   ...free...     ]
        0    4               16                   28
  Offsets=[0,16], Sizes=[4,12], Types=[H(int),H(f3)], Aln=[4,16]
  DataAllocatedIndex = 28

  ── Add<short>(7): size=2, align=2 ────────────────────────────
  dataAllocIndex = AlignDataIndex(28, 2) = 28   ← no padding needed
  Data: [42 | PAD | float3 | short  ...free...                    ]
        0    4   16        28  30
  Offsets=[0,16,28], Sizes=[4,12,2], Types=[H(i),H(f3),H(s)], Aln=[4,16,2]
  DataAllocatedIndex = 30
```

---

## Read Algorithm

```
  DynamicUntypedBuffer.ElementAtRO<TValue>(index):
  ──────────────────────────────────────────────────

  1. CheckIndexInRange(index)         // 0 <= index < Count
  2. CheckType<TValue>(index):
     ├─ expected = BurstRuntime.GetHashCode32<TValue>()
     ├─ actual = Types[index]
     └─ IF expected != actual → THROW (type mismatch!)
  3. offset = Offsets[index]
  4. RETURN ref *(TValue*)(Data + offset)

  Type safety is enforced at runtime via the stored type hash.
  Reading with the wrong type throws InvalidOperationException.
```

---

## RemoveAt Algorithm (Compact + Realign)

```
  DynamicUntypedBuffer.RemoveAt(index):
  ─────────────────────────────────────

  PHASE 1: Shift metadata arrays down
  ┌──────────────────────────────────────────────────────────────┐
  │ MemMove(Offsets[index],    Offsets[index+1],    remaining)   │
  │ MemMove(Sizes[index],      Sizes[index+1],      remaining)   │
  │ MemMove(Types[index],      Types[index+1],      remaining)   │
  │ MemMove(Alignments[index], Alignments[index+1], remaining)   │
  │ Count--                                                      │
  └──────────────────────────────────────────────────────────────┘

  PHASE 2: Compact and realign the data area
  ┌──────────────────────────────────────────────────────────────┐
  │ dataIndex = 0                                                │
  │ FOR i = 0 TO Count-1:                                        │
  │   dataIndex = AlignDataIndex(dataIndex, Alignments[i])       │
  │   IF Offsets[i] != dataIndex:                                │
  │     MemMove(Data+dataIndex, Data+Offsets[i], Sizes[i])       │
  │   Offsets[i] = dataIndex                                     │
  │   dataIndex += Sizes[i]                                      │
  │                                                              │
  │ DataAllocatedIndex = dataIndex                               │
  └──────────────────────────────────────────────────────────────┘
```

### Visual: RemoveAt In Action

```
  BEFORE: 3 elements — int(42), float3(1,2,3), short(7)

  Data:   [42 | PAD PAD PAD | float3 | short ]
          0    4              16      28  30
  
  Offsets=[0, 16, 28],  Sizes=[4, 12, 2]
  Types=[H(i), H(f3), H(s)],  Alignments=[4, 16, 2]
  Count=3, DataAllocatedIndex=30

  ── RemoveAt(1) — remove the float3 ────────────────────────────

  PHASE 1: Shift metadata (remove slot 1):
  Offsets = [0, 28]        ← was [0, 16, 28], shifted down
  Sizes   = [4, 2]         ← was [4, 12, 2]
  Types   = [H(i), H(s)]   ← was [H(i), H(f3), H(s)]
  Aln     = [4, 2]          ← was [4, 16, 2]
  Count = 2

  PHASE 2: Compact data area:
  ┌─────────────────────────────────────────────────────────────┐
  │ i=0: dataIndex = AlignDataIndex(0, 4) = 0                  │
  │      Offsets[0]=0, already at 0, no move. dataIndex=4      │
  │                                                             │
  │ i=1: dataIndex = AlignDataIndex(4, 2) = 4                  │
  │      Old Offsets[1]=28 → NEW=4.                             │
  │      MemMove(Data+4, Data+28, 2) ← copy short from 28 to 4 │
  │      Offsets[1] = 4. dataIndex = 6                         │
  │                                                             │
  │ DataAllocatedIndex = 6                                     │
  └─────────────────────────────────────────────────────────────┘

  AFTER:
  Data:   [42 | short | ...freed...                             ]
          0    4    6
  
  Offsets=[0, 4], Sizes=[4, 2]
  Types=[H(i), H(s)], Alignments=[4, 2]
  Count=2, DataAllocatedIndex=6
  Data area reclaimed from 30 bytes → 6 bytes!
```

---

## Resize Strategy

```
  Two independent growth areas:
  ══════════════════════════════
  
  1. ELEMENT CAPACITY (metadata arrays):
     - Grows when Count == Capacity
     - Powers of 2, controlled by Log2MinGrowth
     - Resizes: Offsets[], Sizes[], Types[], Alignments[]
     
  2. DATA CAPACITY (raw data area):
     - Grows when aligned write position exceeds DataCapacity
     - Powers of 2, controlled by Log2MinGrowth
     - May need multiple doublings to fit a large element
     
  Both can grow independently — adding many small elements
  only grows element capacity, while adding one large element
  only grows data capacity.
  
  Resize flow:
  ┌─────────────────────────────────────────────────────────────┐
  │ 1. Copy old metadata + data to temp allocations             │
  │ 2. buffer.ResizeUninitialized(headerSize + newSize)        │
  │ 3. Rebuild header with new offsets                          │
  │ 4. Copy old data back into new positions                    │
  │ 5. Update helper pointer (buffer may have moved!)           │
  └─────────────────────────────────────────────────────────────┘
```

---

## Data Flow: Complete Lifecycle

```
  ╔═══════════════════════════════════════════════════════════════════════════╗
  ║                  DYNAMIC UNTYPED BUFFER LIFECYCLE                        ║
  ╠═══════════════════════════════════════════════════════════════════════════╣
  ║                                                                         ║
  ║  1. DECLARE                                                              ║
  ║     struct MyBuf : IDynamicUntypedBuffer { byte Value { get; } }        ║
  ║                                                                         ║
  ║  2. INITIALIZE                                                           ║
  ║     buffer.InitializeUntypedBuffer<MyBuf>(capacity: 8)                  ║
  ║         │                                                               ║
  ║         ├─ CalcCapacityCeilPow2 for element capacity and data capacity  ║
  ║         ├─ CalculateDataSize (metadata arrays + data area)             ║
  ║         └─ buffer.ResizeUninitialized + write header + offsets          ║
  ║                                                                         ║
  ║  3. ADD MIXED TYPES                                                      ║
  ║     var buf = buffer.AsUntypedBuffer<MyBuf>();                          ║
  ║     buf.Add<int>(42);          → idx 0, 4 bytes, aligned to 4          ║
  ║     buf.Add<float3>(1,2,3);    → idx 1, 12 bytes, aligned to 16        ║
  ║     buf.Add<MyStruct>(...);    → idx 2, N bytes, aligned to A          ║
  ║                                                                         ║
  ║  4. READ WITH TYPE CHECKING                                              ║
  ║     int i = buf.ElementAtRO<int>(0);       → OK (type hash matches)    ║
  ║     float3 f = buf.ElementAtRO<float3>(1); → OK                         ║
  ║     float x = buf.ElementAtRO<float>(0);   → THROWS (type mismatch!)   ║
  ║                                                                         ║
  ║  5. REMOVE (compacts data)                                               ║
  ║     buf.RemoveAt(1);        → removes float3, compacts data area       ║
  ║     buf.ElementAtRO<int>(0); → still works (re-indexed)                ║
  ║                                                                         ║
  ╚═══════════════════════════════════════════════════════════════════════════╝
```

---

## Key Design Decisions

1. **Per-element type hash**: Each stored element records `BurstRuntime.GetHashCode32<T>()`
   at write time. Reads verify the hash matches, preventing type confusion bugs in
   burst-compiled code where generics are concrete.

2. **Alignment-aware data area**: The data area isn't a simple byte stream. Each write
   position is aligned to the element's natural alignment, accounting for the Data pointer's
   actual address. This ensures SIMD types (float3, float4) are properly aligned.

3. **RemoveAt compacts data**: Unlike hash maps (which use free lists), removing an element
   from the untyped buffer compacts the data area to eliminate gaps. This is because the
   buffer is a sequential list, not a sparse map.

4. **Two independent capacity axes**: Element capacity (number of slots) and data capacity
   (bytes of storage) grow independently. This avoids wasting memory when you have many
   small elements or few large elements.

5. **Offset-based pointers**: All internal arrays are referenced via byte offsets from the
   header start, not raw pointers. This survives buffer resizes (which may relocate the
   entire buffer in memory).

6. **Alignment stored as byte**: Since alignments are always small powers of 2 (≤ 128 or so),
   a single byte per element suffices, saving 3 bytes per slot compared to an int.

---

## Performance Characteristics

| Operation          | Average  | Worst    | Notes                                  |
|--------------------|----------|----------|----------------------------------------|
| Add<T>             | O(1)     | O(N)     | Worst = resize + copy                  |
| ElementAt<T>(idx)  | O(1)     | O(1)     | Direct offset lookup + type check      |
| Set<T>(idx, val)   | O(1)     | O(1)     | Direct offset write + type check       |
| RemoveAt(idx)      | O(N)     | O(N)     | Shift metadata + compact data area     |
| Clear              | O(1)     | O(1)     | Just resets Count and DataAllocIndex   |
| Resize             | O(N)     | O(N)     | Copy all metadata + data to new buffer |

**Per-element overhead**: 4 (offset) + 4 (size) + 4 (type) + 1 (alignment) = 13 bytes
**Alignment waste**: Up to `max_alignment - 1` bytes of padding per element
**Data area waste**: Up to `DataCapacity - DataAllocatedIndex` bytes after last element

**Memory per element** = 13 bytes metadata + sizeof(T) + padding for alignment

The RemoveAt compaction is the most expensive operation (O(N)), as it must re-layout
all remaining data with proper alignment. For workloads with frequent removals, consider
using a "swap and pop" pattern or marking entries as invalid instead.

## Verified Data

> Run the verification snippet:
> ```bash
> cat snippets/dynamic-buffers/DynamicUntypedBuffer.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
> ```

```
PASS: DynamicUntypedBuffer: type exists
PASS: DynamicUntypedBuffer: is ValueType
PASS: DynamicUntypedBuffer: has Add<T>(T)
PASS: DynamicUntypedBuffer: has ElementAtRO<T>(int)
PASS: DynamicUntypedBuffer: has RemoveAt(int)
PASS: DynamicUntypedBuffer: has Clear()
PASS: DynamicUntypedBufferHelper: type exists
PASS: DynamicUntypedBufferHelper: is ValueType
PASS: DynamicUntypedBufferHelper: has StructLayout(LayoutKind.Sequential)
PASS: DynamicUntypedBufferHelper: has 10 fields
PASS: DynamicUntypedBufferHelper: has OffsetsOffset
PASS: DynamicUntypedBufferHelper: has SizesOffset
PASS: DynamicUntypedBufferHelper: has TypesOffset
PASS: DynamicUntypedBufferHelper: has AlignmentsOffset
PASS: DynamicUntypedBufferHelper: has DataOffset
PASS: DynamicUntypedBufferHelper: has Count
PASS: DynamicUntypedBufferHelper: has Capacity
PASS: DynamicUntypedBufferHelper: has DataCapacity
PASS: DynamicUntypedBufferHelper: has DataAllocatedIndex
PASS: DynamicUntypedBufferHelper: has Log2MinGrowth
PASS: DynamicUntypedBufferHelper: all 10 fields are int
PASS: DynamicUntypedBufferHelper: Marshal.SizeOf == 40 bytes
PASS: IDynamicUntypedBuffer: interface exists
PASS: IDynamicUntypedBuffer: is interface
PASS: IDynamicUntypedBuffer: implements IBufferElementData
PASS: IDynamicUntypedBuffer: has Value property returning byte
PASS: DynamicExtensions: has InitializeUntypedBuffer
PASS: DynamicExtensions: has AsUntypedBuffer

=== 28 PASSED, 0 FAILED ===
```

## Source

- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicUntypedBuffer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicUntypedBuffer.cs)
- [BovineLabs.Core/Iterators/DynamicHashMap/DynamicUntypedBufferHelper.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/DynamicUntypedBufferHelper.cs)
- [BovineLabs.Core.Tests/Iterators/DynamicUntypedBufferTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Iterators/DynamicUntypedBufferTests.cs)
