     1|# DynamicUntypedBuffer
     2|
     3|**Stores mixed unmanaged types manually inside a single DynamicBuffer<byte>, each element tracked by offset, size, type hash, and alignment.**
     4|
     5|## Overview
     6|
     7|DynamicUntypedBuffer is a type-erased list that stores heterogeneous unmanaged types
     8|side-by-side in one contiguous `DynamicBuffer<byte>`. Unlike `DynamicBuffer<T>` which holds
     9|elements of a single type, this buffer can store an `int` at index 0, a `float3` at index 1,
    10|a `MyCustomStruct` at index 2, and so on.
    11|
    12|Each element is tracked by per-element metadata arrays: **Offsets[]** (byte position in data
    13|area), **Sizes[]** (size in bytes), **Types[]** (Burst type hash for runtime type checking),
    14|and **Alignments[]** (alignment requirement). The data area is a packed, properly-aligned
    15|sequence of variable-size blobs.
    16|
    17|This is useful for ECS patterns where an entity needs to hold an arbitrary set of typed
    18|parameters without pre-defining each as a separate component or buffer.
    19|
    20|---
    21|
    22|## Memory Layout
    23|
    24|### Buffer-Level View
    25|
    26|```
    27|  DynamicBuffer<byte> contents:
    28|  ╔══════════════════════════════════════════════════════════════════════════════╗
    29|  ║                                                                            ║
    30|  ║  [0x00]  DynamicUntypedBufferHelper  (40 bytes header)                    ║
    31|  ║          ┌──────────────────────────────────────────────────────┐          ║
    32|  ║          │ OffsetsOffset      (int)                            │          ║
    33|  ║          │ SizesOffset        (int)                            │          ║
    34|  ║          │ TypesOffset        (int)                            │          ║
    35|  ║          │ AlignmentsOffset   (int)                            │          ║
    36|  ║          │ DataOffset         (int)                            │          ║
    37|  ║          │ Count              (int) — # of elements           │          ║
    38|  ║          │ Capacity           (int) — max elements            │          ║
    39|  ║          │ DataCapacity       (int) — data area size in bytes │          ║
    40|  ║          │ DataAllocatedIndex (int) — data high-water mark    │          ║
    41|  ║          │ Log2MinGrowth      (int)                            │          ║
    42|  ║          └──────────────────────────────────────────────────────┘          ║
    43|  ║                                                                            ║
    44|  ║  [0x28]  Offsets[Capacity]        (4 * Capacity bytes)                    ║
    45|  ║          ┌──────┬──────┬──────┬──────┐                                    ║
    46|  ║          │  0   │  4   │ 16   │  ... │  byte offset into Data[]            ║
    47|  ║          └──────┴──────┴──────┴──────┘                                    ║
    48|  ║                                                                            ║
    49|  ║  [aligned]  Sizes[Capacity]        (4 * Capacity bytes)                   ║
    50|  ║          ┌──────┬──────┬──────┬──────┐                                    ║
    51|  ║          │  4   │ 12   │  8   │  ... │  sizeof(T) for each element        ║
    52|  ║          └──────┴──────┴──────┴──────┘                                    ║
    53|  ║                                                                            ║
    54|  ║  [aligned]  Types[Capacity]        (4 * Capacity bytes)                   ║
    55|  ║          ┌──────┬──────┬──────┬──────┐                                    ║
    56|  ║          │ H(int)│H(f3) │H(str)│  ... │  BurstRuntime.GetHashCode32<T>()   ║
    57|  ║          └──────┴──────┴──────┴──────┘                                    ║
    58|  ║                                                                            ║
    59|  ║  [aligned]  Alignments[Capacity]   (1 * Capacity bytes)                   ║
    60|  ║          ┌───┬───┬───┬───┐                                              ║
    61|  ║          │ 4 │16 │ 8 │...│  alignment of each element                    ║
    62|  ║          └───┴───┴───┴───┘                                              ║
    63|  ║                                                                            ║
    64|  ║  [aligned to 16]  Data[DataCapacity]  (variable-size data area)           ║
    65|  ║          ┌──────────────────────────────────────────────────────┐          ║
    66|  ║          │ [0]  int(42)     │ padding │ [1]  float3(1,2,3)     │  ...     ║
    67|  ║          │ 4 bytes          │ 12 bytes│ 12 bytes              │          ║
    68|  ║          └──────────────────────────────────────────────────────┘          ║
    69|  ║                                                                            ║
    70|  ╚══════════════════════════════════════════════════════════════════════════════╝
    71|```
    72|
    73|### Header Struct Detail
    74|
    75|```
    76|  DynamicUntypedBufferHelper  — 40 bytes (10 ints)
    77|  ┌──────────┬─────────────────────┬──────────────────────────────────────────┐
    78|  │ Offset   │ Field               │ Purpose                                  │
    79|  ├──────────┼─────────────────────┼──────────────────────────────────────────┤
    80|  │ 0x00     │ OffsetsOffset       │ Byte offset from buffer start to Offsets[]│
    81|  │ 0x04     │ SizesOffset         │ Byte offset from buffer start to Sizes[] │
    82|  │ 0x08     │ TypesOffset         │ Byte offset from buffer start to Types[] │
    83|  │ 0x0C     │ AlignmentsOffset    │ Byte offset from buffer start to Aln[]   │
    84|  │ 0x10     │ DataOffset          │ Byte offset from buffer start to Data[]  │
    85|  │ 0x14     │ Count               │ Number of stored elements                │
    86|  │ 0x18     │ Capacity            │ Max elements (metadata array slots)      │
    87|  │ 0x1C     │ DataCapacity        │ Total bytes in Data area                 │
    88|  │ 0x20     │ DataAllocatedIndex  │ Next free byte offset in Data            │
    89|  │ 0x24     │ Log2MinGrowth       │ Growth granularity as log2               │
    90|  └──────────┴─────────────────────┴──────────────────────────────────────────┘
    91|```
    92|
    93|### Data Area Layout (The Key Innovation)
    94|
    95|```
    96|  Data[] area — packed, aligned, variable-size elements:
    97|  ════════════════════════════════════════════════════════════════
    98|
    99|  Index 0: int (4 bytes, align 4)
   100|  Index 1: float3 (12 bytes, align 16) ← needs padding!
   101|  Index 2: double (8 bytes, align 8)
   102|  Index 3: short (2 bytes, align 2)
   103|
   104|  Data area byte-by-byte:
   105|  ┌────────┬────────────────┬──────────────────────┬──────────┬───┬──────────┬─────┐
   106|  │ int    │  PAD (12 bytes)│      float3          │  double  │P  │  short   │ ... │
   107|  │ 42     │  (alignment)   │  (1.0, 2.0, 3.0)    │  3.14    │AD │  7       │     │
   108|  │ 4B     │  align→16      │  12B                 │  8B      │6B │  2B      │     │
   109|  └────────┴────────────────┴──────────────────────┴──────────┴───┴──────────┴─────┘
   110|  │←─ 0 ─→│                 │←──── 16 ────────────→│←─ 28 ──→│   │←─ 36 ──→│
   111|  │        │                 │                      │          │   │          │
   112|  Offsets[0]=0              Offsets[1]=16          Offsets[2]=28  Offsets[3]=36
   113|  Sizes[0]=4                Sizes[1]=12            Sizes[2]=8     Sizes[3]=2
   114|  Types[0]=H(int)           Types[1]=H(float3)     Types[2]=H(double) Types[3]=H(short)
   115|  Aln[0]=4                  Aln[1]=16              Aln[2]=8       Aln[3]=2
   116|
   117|  DataAllocatedIndex = 38  (= 36 + 2)
   118|```
   119|
   120|---
   121|
   122|## Alignment Algorithm
   123|
   124|```
   125|  AlignDataIndex helper — accounts for Data pointer's own alignment:
   126|  ─────────────────────────────────────────────────────────────────
   127|
   128|  WHY: Data area alignment is relative to absolute memory address,
   129|  not just byte offset. If Data* itself is misaligned, simple
   130|  offset alignment is wrong.
   131|
   132|  Algorithm:
   133|  ┌──────────────────────────────────────────────────────────┐
   134|  │ 1. dataMisalignment = (int)(Data* & (align - 1))        │
   135|  │                                                          │
   136|  │ 2. IF dataMisalignment == 0:                             │
   137|  │    └─ return Align(dataIndex, align)  // simple case     │
   138|  │                                                          │
   139|  │ 3. ELSE:                                                 │
   140|  │    └─ return Align(dataIndex + dataMisalignment, align)  │
   141|  │               - dataMisalignment                         │
   142|  └──────────────────────────────────────────────────────────┘
   143|
   144|  Example:
   145|    Data* = 0x1006 (misaligned by 6 for align=16)
   146|    dataIndex = 10, align = 16
   147|    
   148|    dataMisalignment = 6
   149|    result = Align(10 + 6, 16) - 6 = Align(16, 16) - 6 = 16 - 6 = 10
   150|    → So absolute address = 0x1006 + 10 = 0x1010 (16-aligned ✓)
   151|```
   152|
   153|---
   154|
   155|## Add Algorithm
   156|
   157|```
   158|  DynamicUntypedBuffer.Add<TValue>(value):
   159|  ────────────────────────────────────────
   160|
   161|  1. IF Count == Capacity:
   162|     └─ Resize metadata arrays (grow Capacity)
   163|
   164|  2. idx = Count++
   165|  3. size = sizeof(TValue)
   166|  4. align = alignof(TValue)
   167|  5. dataAllocIndex = AlignDataIndex(DataAllocatedIndex, align)
   168|     ┌──────────────────────────────────────────────────────┐
   169|     │ Aligns current write position to the element's       │
   170|     │ alignment requirement. May add padding bytes.        │
   171|     └──────────────────────────────────────────────────────┘
   172|
   173|  6. IF dataAllocIndex + size > DataCapacity:
   174|     └─ ResizeData (grow DataCapacity, may loop to find enough)
   175|
   176|  7. dst = Data + dataAllocIndex
   177|  8. MemCpy(&value → dst, size)         ← copy value bytes into data area
   178|
   179|  9. Record metadata:
   180|     Offsets[idx]     = dataAllocIndex
   181|     Sizes[idx]       = size
   182|     Types[idx]       = BurstRuntime.GetHashCode32<TValue>()
   183|     Alignments[idx]  = (byte)align
   184|
   185|  10. DataAllocatedIndex = dataAllocIndex + size
   186|  11. RETURN idx
   187|```
   188|
   189|### Visual: Adding Mixed Types
   190|
   191|```
   192|  INITIAL: Count=0, DataAllocatedIndex=0, DataCapacity=64
   193|
   194|  ── Add<int>(42): size=4, align=4 ──────────────────────────────
   195|  dataAllocIndex = AlignDataIndex(0, 4) = 0
   196|  Data: [42(int)                               ...free...       ]
   197|        ↑ offset=0
   198|  Offsets=[0], Sizes=[4], Types=[H(int)], Alignments=[4]
   199|  DataAllocatedIndex = 4
   200|
   201|  ── Add<float3>(1,2,3): size=12, align=16 ─────────────────────
   202|  dataAllocIndex = AlignDataIndex(4, 16) = 16  ← 12 bytes padding!
   203|  Data: [42 | PAD PAD PAD PAD | float3(1,2,3)   ...free...     ]
   204|        0    4               16                   28
   205|  Offsets=[0,16], Sizes=[4,12], Types=[H(int),H(f3)], Aln=[4,16]
   206|  DataAllocatedIndex = 28
   207|
   208|  ── Add<short>(7): size=2, align=2 ────────────────────────────
   209|  dataAllocIndex = AlignDataIndex(28, 2) = 28   ← no padding needed
   210|  Data: [42 | PAD | float3 | short  ...free...                    ]
   211|        0    4   16        28  30
   212|  Offsets=[0,16,28], Sizes=[4,12,2], Types=[H(i),H(f3),H(s)], Aln=[4,16,2]
   213|  DataAllocatedIndex = 30
   214|```
   215|
   216|---
   217|
   218|## Read Algorithm
   219|
   220|```
   221|  DynamicUntypedBuffer.ElementAtRO<TValue>(index):
   222|  ──────────────────────────────────────────────────
   223|
   224|  1. CheckIndexInRange(index)         // 0 <= index < Count
   225|  2. CheckType<TValue>(index):
   226|     ├─ expected = BurstRuntime.GetHashCode32<TValue>()
   227|     ├─ actual = Types[index]
   228|     └─ IF expected != actual → THROW (type mismatch!)
   229|  3. offset = Offsets[index]
   230|  4. RETURN ref *(TValue*)(Data + offset)
   231|
   232|  Type safety is enforced at runtime via the stored type hash.
   233|  Reading with the wrong type throws InvalidOperationException.
   234|```
   235|
   236|---
   237|
   238|## RemoveAt Algorithm (Compact + Realign)
   239|
   240|```
   241|  DynamicUntypedBuffer.RemoveAt(index):
   242|  ─────────────────────────────────────
   243|
   244|  PHASE 1: Shift metadata arrays down
   245|  ┌──────────────────────────────────────────────────────────────┐
   246|  │ MemMove(Offsets[index],    Offsets[index+1],    remaining)   │
   247|  │ MemMove(Sizes[index],      Sizes[index+1],      remaining)   │
   248|  │ MemMove(Types[index],      Types[index+1],      remaining)   │
   249|  │ MemMove(Alignments[index], Alignments[index+1], remaining)   │
   250|  │ Count--                                                      │
   251|  └──────────────────────────────────────────────────────────────┘
   252|
   253|  PHASE 2: Compact and realign the data area
   254|  ┌──────────────────────────────────────────────────────────────┐
   255|  │ dataIndex = 0                                                │
   256|  │ FOR i = 0 TO Count-1:                                        │
   257|  │   dataIndex = AlignDataIndex(dataIndex, Alignments[i])       │
   258|  │   IF Offsets[i] != dataIndex:                                │
   259|  │     MemMove(Data+dataIndex, Data+Offsets[i], Sizes[i])       │
   260|  │   Offsets[i] = dataIndex                                     │
   261|  │   dataIndex += Sizes[i]                                      │
   262|  │                                                              │
   263|  │ DataAllocatedIndex = dataIndex                               │
   264|  └──────────────────────────────────────────────────────────────┘
   265|```
   266|
   267|### Visual: RemoveAt In Action
   268|
   269|```
   270|  BEFORE: 3 elements — int(42), float3(1,2,3), short(7)
   271|
   272|  Data:   [42 | PAD PAD PAD | float3 | short ]
   273|          0    4              16      28  30
   274|  
   275|  Offsets=[0, 16, 28],  Sizes=[4, 12, 2]
   276|  Types=[H(i), H(f3), H(s)],  Alignments=[4, 16, 2]
   277|  Count=3, DataAllocatedIndex=30
   278|
   279|  ── RemoveAt(1) — remove the float3 ────────────────────────────
   280|
   281|  PHASE 1: Shift metadata (remove slot 1):
   282|  Offsets = [0, 28]        ← was [0, 16, 28], shifted down
   283|  Sizes   = [4, 2]         ← was [4, 12, 2]
   284|  Types   = [H(i), H(s)]   ← was [H(i), H(f3), H(s)]
   285|  Aln     = [4, 2]          ← was [4, 16, 2]
   286|  Count = 2
   287|
   288|  PHASE 2: Compact data area:
   289|  ┌─────────────────────────────────────────────────────────────┐
   290|  │ i=0: dataIndex = AlignDataIndex(0, 4) = 0                  │
   291|  │      Offsets[0]=0, already at 0, no move. dataIndex=4      │
   292|  │                                                             │
   293|  │ i=1: dataIndex = AlignDataIndex(4, 2) = 4                  │
   294|  │      Old Offsets[1]=28 → NEW=4.                             │
   295|  │      MemMove(Data+4, Data+28, 2) ← copy short from 28 to 4 │
   296|  │      Offsets[1] = 4. dataIndex = 6                         │
   297|  │                                                             │
   298|  │ DataAllocatedIndex = 6                                     │
   299|  └─────────────────────────────────────────────────────────────┘
   300|
   301|  AFTER:
   302|  Data:   [42 | short | ...freed...                             ]
   303|          0    4    6
   304|  
   305|  Offsets=[0, 4], Sizes=[4, 2]
   306|  Types=[H(i), H(s)], Alignments=[4, 2]
   307|  Count=2, DataAllocatedIndex=6
   308|  Data area reclaimed from 30 bytes → 6 bytes!
   309|```
   310|
   311|---
   312|
   313|## Resize Strategy
   314|
   315|```
   316|  Two independent growth areas:
   317|  ══════════════════════════════
   318|  
   319|  1. ELEMENT CAPACITY (metadata arrays):
   320|     - Grows when Count == Capacity
   321|     - Powers of 2, controlled by Log2MinGrowth
   322|     - Resizes: Offsets[], Sizes[], Types[], Alignments[]
   323|     
   324|  2. DATA CAPACITY (raw data area):
   325|     - Grows when aligned write position exceeds DataCapacity
   326|     - Powers of 2, controlled by Log2MinGrowth
   327|     - May need multiple doublings to fit a large element
   328|     
   329|  Both can grow independently — adding many small elements
   330|  only grows element capacity, while adding one large element
   331|  only grows data capacity.
   332|  
   333|  Resize flow:
   334|  ┌─────────────────────────────────────────────────────────────┐
   335|  │ 1. Copy old metadata + data to temp allocations             │
   336|  │ 2. buffer.ResizeUninitialized(headerSize + newSize)        │
   337|  │ 3. Rebuild header with new offsets                          │
   338|  │ 4. Copy old data back into new positions                    │
   339|  │ 5. Update helper pointer (buffer may have moved!)           │
   340|  └─────────────────────────────────────────────────────────────┘
   341|```
   342|
   343|---
   344|
   345|## Data Flow: Complete Lifecycle
   346|
   347|```
   348|  ╔═══════════════════════════════════════════════════════════════════════════╗
   349|  ║                  DYNAMIC UNTYPED BUFFER LIFECYCLE                        ║
   350|  ╠═══════════════════════════════════════════════════════════════════════════╣
   351|  ║                                                                         ║
   352|  ║  1. DECLARE                                                              ║
   353|  ║     struct MyBuf : IDynamicUntypedBuffer { byte Value { get; } }        ║
   354|  ║                                                                         ║
   355|  ║  2. INITIALIZE                                                           ║
   356|  ║     buffer.InitializeUntypedBuffer<MyBuf>(capacity: 8)                  ║
   357|  ║         │                                                               ║
   358|  ║         ├─ CalcCapacityCeilPow2 for element capacity and data capacity  ║
   359|  ║         ├─ CalculateDataSize (metadata arrays + data area)             ║
   360|  ║         └─ buffer.ResizeUninitialized + write header + offsets          ║
   361|  ║                                                                         ║
   362|  ║  3. ADD MIXED TYPES                                                      ║
   363|  ║     var buf = buffer.AsUntypedBuffer<MyBuf>();                          ║
   364|  ║     buf.Add<int>(42);          → idx 0, 4 bytes, aligned to 4          ║
   365|  ║     buf.Add<float3>(1,2,3);    → idx 1, 12 bytes, aligned to 16        ║
   366|  ║     buf.Add<MyStruct>(...);    → idx 2, N bytes, aligned to A          ║
   367|  ║                                                                         ║
   368|  ║  4. READ WITH TYPE CHECKING                                              ║
   369|  ║     int i = buf.ElementAtRO<int>(0);       → OK (type hash matches)    ║
   370|  ║     float3 f = buf.ElementAtRO<float3>(1); → OK                         ║
   371|  ║     float x = buf.ElementAtRO<float>(0);   → THROWS (type mismatch!)   ║
   372|  ║                                                                         ║
   373|  ║  5. REMOVE (compacts data)                                               ║
   374|  ║     buf.RemoveAt(1);        → removes float3, compacts data area       ║
   375|  ║     buf.ElementAtRO<int>(0); → still works (re-indexed)                ║
   376|  ║                                                                         ║
   377|  ╚═══════════════════════════════════════════════════════════════════════════╝
   378|```
   379|
   380|---
   381|
   382|## Key Design Decisions
   383|
   384|1. **Per-element type hash**: Each stored element records `BurstRuntime.GetHashCode32<T>()`
   385|   at write time. Reads verify the hash matches, preventing type confusion bugs in
   386|   burst-compiled code where generics are concrete.
   387|
   388|2. **Alignment-aware data area**: The data area isn't a simple byte stream. Each write
   389|   position is aligned to the element's natural alignment, accounting for the Data pointer's
   390|   actual address. This ensures SIMD types (float3, float4) are properly aligned.
   391|
   392|3. **RemoveAt compacts data**: Unlike hash maps (which use free lists), removing an element
   393|   from the untyped buffer compacts the data area to eliminate gaps. This is because the
   394|   buffer is a sequential list, not a sparse map.
   395|
   396|4. **Two independent capacity axes**: Element capacity (number of slots) and data capacity
   397|   (bytes of storage) grow independently. This avoids wasting memory when you have many
   398|   small elements or few large elements.
   399|
   400|5. **Offset-based pointers**: All internal arrays are referenced via byte offsets from the
   401|   header start, not raw pointers. This survives buffer resizes (which may relocate the
   402|   entire buffer in memory).
   403|
   404|6. **Alignment stored as byte**: Since alignments are always small powers of 2 (≤ 128 or so),
   405|   a single byte per element suffices, saving 3 bytes per slot compared to an int.
   406|
   407|---
   408|
   409|## Performance Characteristics
   410|
   411|| Operation          | Average  | Worst    | Notes                                  |
   412||--------------------|----------|----------|----------------------------------------|
   413|| Add<T>             | O(1)     | O(N)     | Worst = resize + copy                  |
   414|| ElementAt<T>(idx)  | O(1)     | O(1)     | Direct offset lookup + type check      |
   415|| Set<T>(idx, val)   | O(1)     | O(1)     | Direct offset write + type check       |
   416|| RemoveAt(idx)      | O(N)     | O(N)     | Shift metadata + compact data area     |
   417|| Clear              | O(1)     | O(1)     | Just resets Count and DataAllocIndex   |
   418|| Resize             | O(N)     | O(N)     | Copy all metadata + data to new buffer |
   419|
   420|**Per-element overhead**: 4 (offset) + 4 (size) + 4 (type) + 1 (alignment) = 13 bytes
   421|**Alignment waste**: Up to `max_alignment - 1` bytes of padding per element
   422|**Data area waste**: Up to `DataCapacity - DataAllocatedIndex` bytes after last element
   423|
   424|**Memory per element** = 13 bytes metadata + sizeof(T) + padding for alignment
   425|
   426|The RemoveAt compaction is the most expensive operation (O(N)), as it must re-layout
   427|all remaining data with proper alignment. For workloads with frequent removals, consider
   428|using a "swap and pop" pattern or marking entries as invalid instead.
   429|
   430|## Verified Data

## Verified Data

```
DynamicUntypedBuffer
  Kind: struct, 72 bytes
  Methods:
    public Int32 Add<TValue>(TValue value)
    private Void CheckSize(DynamicBuffer`1 buffer)
    public Void Clear()
    public TValue& ElementAt<TValue>(Int32 index)
    public TValue& ElementAtRO<TValue>(Int32 index)
    public Boolean Equals(Object obj)
    public Int32 GetHashCode()
    public Type GetType()
    private Void RefCheck()
    public Void RemoveAt(Int32 index)
    public Void Set<TValue>(Int32 index, TValue value)
    public String ToString()
  DynamicUntypedBufferHelper
    Kind: struct, 40 bytes
    Layout: Sequential
    Fields:
      Int32 OffsetsOffset (private)
      Int32 SizesOffset (private)
      Int32 TypesOffset (private)
      Int32 AlignmentsOffset (private)
      Int32 DataOffset (private)
      Int32 Count (private)
      Int32 Capacity (private)
      Int32 DataCapacity (private)
      Int32 DataAllocatedIndex (private)
      Int32 Log2MinGrowth (private)
  IDynamicUntypedBuffer
    IsInterface: True
    Implements IBufferElementData: True
    Value property: Byte
  DynamicExtensions
    InitializeUntypedBuffer: exists
    AsUntypedBuffer: exists
Verified: 16 checks, 0 failures
```


## Source

- [BovineLabs.Core/Iterators/DynamicUntypedBuffer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicUntypedBuffer.cs)
- [BovineLabs.Core/Iterators/DynamicUntypedBufferHelper.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicUntypedBufferHelper.cs)
- [BovineLabs.Core/Iterators/IDynamicUntypedBuffer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/IDynamicUntypedBuffer.cs)
