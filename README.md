# NativeThreadStream

**Block-based, thread-safe streaming allocator that avoids pre-allocating large arrays.**

## Overview

NativeThreadStream solves the problem of collecting variable-sized data from many parallel
Unity DOTS jobs without knowing the total count upfront. Instead of pre-allocating one giant
array per thread (wasting memory) or using a single shared stream (requiring atomic locks),
it gives each worker thread its own linked-list of fixed-size 4KB blocks. Each thread writes
into its current block; when the block fills up, a new one is allocated and linked. Later,
a single-threaded reader can walk each thread's block chain sequentially.

The structure supports writing heterogeneous types (different sizes per write) into the same
stream, making it ideal for event/command queues where each event may have a different payload
size.

---

## Memory Layout

### Top-Level: UnsafeThreadStream (2 words)

```
UnsafeThreadStream  (16 bytes on 64-bit)
╔═════════════════════╤══════════════════════════════════════════════════╗
║ Field               │ Description                                    ║
╠═════════════════════╪══════════════════════════════════════════════════╣
║ blockData*          │ Pointer to UnsafeThreadStreamBlockData          ║
║ allocator           │ AllocatorManager.AllocatorHandle (copyable)    ║
╚═════════════════════╧══════════════════════════════════════════════════╝
```

### UnsafeThreadStreamBlockData (the shared control block)

```
                        UnsafeThreadStreamBlockData
╔══════════════════════════════════════════════════════════════════════════╗
║                                                                        ║
║  Offset 0x00:  Allocator          AllocatorManager.AllocatorHandle     ║
║                                                                        ║
║  Offset 0x08:  Blocks**           Pointer to array of                  ║
║                                    UnsafeThreadStreamBlock*             ║
║                                    [0 .. ForEachCount-1]               ║
║                                                                        ║
║  Offset 0x10:  Ranges*            Pointer to array of                  ║
║                                    UnsafeThreadStreamRange              ║
║                                    [0 .. ForEachCount-1]               ║
║                                                                        ║
║  ── Immediately after this struct (same allocation) ──────────────      ║
║  Offset 0x18:  Blocks[0]*         UnsafeThreadStreamBlock*             ║
║  Offset 0x20:  Blocks[1]*         UnsafeThreadStreamBlock*             ║
║  ...                                                                   ║
║  Offset 0x18 + 8*(ForEachCount-1): Blocks[ForEachCount-1]*             ║
║                                                                        ║
╚══════════════════════════════════════════════════════════════════════════╝
  sizeof = 0x18 + 8 * ForEachCount    (allocated as a single block)

  Ranges is a separate allocation:
  Offset 0x00: Ranges[0]  UnsafeThreadStreamRange  (40 bytes each)
  Offset 0x28: Ranges[1]  UnsafeThreadStreamRange
  ...
```

### UnsafeThreadStreamBlock (4KB linked-list node)

```
  UnsafeThreadStreamBlock          Total size = 4 * 1024 = 4096 bytes
╔════════════════════════════════════════════════════════════════════════╗
║                                                                      ║
║  Offset 0x00:  Next*         Pointer to next block in chain         ║
║                              (null if last block)                    ║
║                                                                      ║
║  Offset 0x08:  Data[0]      ┌──────────────────────────────────┐    ║
║  Offset 0x09:  Data[1]      │                                  │    ║
║  ...                         │   Usable data area               │    ║
║                              │   = 4096 - 8 = 4088 bytes        │    ║
║                              │                                  │    ║
║  Offset 0xFF8: Data[4087]   └──────────────────────────────────┘    ║
║                                                                      ║
╚════════════════════════════════════════════════════════════════════════╝
  Usable payload per block = AllocationSize - sizeof(void*) = 4096 - 8 = 4088 bytes
```

### UnsafeThreadStreamRange (per-thread write state, 40 bytes)

```
  UnsafeThreadStreamRange
╔═════════════════════╤═══════════════════════════════════════════════════╗
║ Offset  Field        │ Description                                     ║
╠═════════════════════╪═══════════════════════════════════════════════════╣
║ 0x00    Block*       │ Head of this thread's block chain (first block) ║
║ 0x08    OffsetIn-    │ Byte offset to first data byte in first block   ║
║         FirstBlock   │ (= sizeof(Next*) = 8 normally)                  ║
║ 0x0C    ElementCount │ Total number of items written by this thread    ║
║ 0x10    LastOffset   │ Byte offset past the last byte written in the   ║
║         │             │ last block (used by reader for bounds)          ║
║ 0x14    NumberOf-    │ Number of blocks after the first (0 = 1 block)  ║
║         Blocks       │                                                 ║
║ 0x18    Current-     │ Block currently being written to                ║
║         Block*       │                                                 ║
║ 0x20    CurrentPtr   │ Write pointer: next free byte in current block  ║
║ 0x28    Current-     │ End boundary of current block                   ║
║         BlockEnd     │ = (byte*)CurrentBlock + 4096                    ║
╚═════════════════════╧═══════════════════════════════════════════════════╝
```

---

## Full Memory Map

```
  NativeThreadStream
       │
       ▼
  UnsafeThreadStream
       │
       ▼
  UnsafeThreadStreamBlockData  ◄──── shared by Writer + Reader
  ┌─────────────────────────────────────────────────────────────┐
  │ Allocator                                                   │
  │ Blocks** ──────────────┐                                    │
  │ Ranges* ───────┐       │                                    │
  └────────────────┼───────┼────────────────────────────────────┘
                   │       │
        ┌──────────┘       │
        │                  │
        ▼                  ▼
  Ranges[]            Blocks[]               (both length = ForEachCount,
        │                  │                  ForEachCount = JobsUtility.ThreadIndexCount)
        │                  │
  ┌─────┴─────┐      ┌────┴─────┐
  │           │      │          │
  ▼           ▼      ▼          ▼
 Range[0]  Range[1]  Blocks[0] Blocks[1] ...
  │  │                    │
  │  │                    ▼
  │  │              ┌─────────────────┐     ┌─────────────────┐
  │  │              │ Block (4KB)     │────▶│ Block (4KB)     │──▶ null
  │  │              │ ┌─────┐        │     │ ┌─────┐        │
  │  │              │ │Next*│───────┬─│     │ │Next*│──▶ null │
  │  │              │ ├─────┤      │ │     │ ├─────┤        │
  │  └──────────────│▶Data │◄─────┘ │     │ │Data │        │
  │                 │ │ ... │ Offset│      │ │ ... │        │
  │  CurrentPtr ──▶ │ └─────┘       │      │ └─────┘        │
  │  CurrentBlock──▶│               │      │                │
  │                 └─────────────────┘     └─────────────────┘
  │
  │  Range[0] detail:
  │  ┌──────────────────────────────────────┐
  │  │ Block* ────────────── first block    │
  │  │ OffsetInFirstBlock ── 8 (past Next*) │
  │  │ ElementCount ──────── e.g. 42        │
  │  │ LastOffset ────────── last write end │
  │  │ NumberOfBlocks ────── e.g. 1         │
  │  │ CurrentBlock* ─────── active block   │
  │  │ CurrentPtr ────────── next free byte │
  │  │ CurrentBlockEnd ───── end boundary   │
  │  └──────────────────────────────────────┘
```

---

## Thread Safety: Partition-By-Index

```
  Unity Job System provides JobsUtility.ThreadIndex
  ──────────────────────────────────────────────────

  Thread 0 (worker)          Thread 1 (worker)          Thread N (worker)
  ┌──────────────────┐      ┌──────────────────┐      ┌──────────────────┐
  │ Writer.Allocate() │      │ Writer.Allocate() │      │ Writer.Allocate() │
  │                  │      │                  │      │                  │
  │ threadIndex=0    │      │ threadIndex=1    │      │ threadIndex=N    │
  │      │           │      │      │           │      │      │           │
  │      ▼           │      │      ▼           │      │      ▼           │
  │  Ranges[0]       │      │  Ranges[1]       │      │  Ranges[N]       │
  │  (no lock!)      │      │  (no lock!)      │      │  (no lock!)      │
  │                  │      │                  │      │                  │
  │  Blocks[0]       │      │  Blocks[1]       │      │  Blocks[N]       │
  │  chain           │      │  chain           │      │  chain           │
  └──────────────────┘      └──────────────────┘      └──────────────────┘
         │                          │                          │
         └──────── NO SHARING ──────┴──────────────────────────┘

  Key insight: Each thread accesses ONLY its own Ranges[threadIndex]
  and its own block chain. Zero contention. No atomics, no locks.
```

---

## Write Algorithm (Writer.Allocate)

```
  Writer.Allocate(int size):
  ────────────────────────
  
  1. threadIndex = JobsUtility.ThreadIndex
  2. range = &Ranges[threadIndex]           // This thread's private state
  3. ptr    = range->CurrentPtr
  4. end    = ptr + size
  5. range->CurrentPtr = end                // Optimistically advance

  6. IF end > range->CurrentBlockEnd:       // Block overflow!
     │
     ├─ oldBlock = range->CurrentBlock
     ├─ newBlock = AllocateBlock(oldBlock, threadIndex)
     │              ┌────────────────────────────────────────┐
     │              │ Allocate new 4KB block from allocator  │
     │              │ Link: oldBlock->Next = newBlock        │
     │              │        newBlock->Next = old old Next   │
     │              │ Update: Blocks[threadIndex] if 1st     │
     │              └────────────────────────────────────────┘
     │
     ├─ range->CurrentBlock    = newBlock
     ├─ range->CurrentPtr      = newBlock->Data + size
     ├─ range->CurrentBlockEnd = newBlock + 4096
     │
     ├─ IF range->Block == null:             // First block ever?
     │  ├─ range->Block = newBlock
     │  └─ range->OffsetInFirstBlock = offset to Data
     │  ELSE:
     │  └─ range->NumberOfBlocks++
     │
     └─ ptr = newBlock->Data                 // Return start of new block

  7. range->ElementCount++
  8. range->LastOffset = CurrentPtr - CurrentBlock
  9. RETURN ptr
```

### Write Diagram: Filling a Block Then Overflowing

```
  State 1: Block partially filled
  ┌───────────────────────────────────────────────────────────┐
  │ Block (4096 bytes)                                        │
  │ ┌──────┬──────────────────────┬───────────────────────┐  │
  │ │Next* │ D D D D D D D D D D │      FREE SPACE       │  │
  │ │ 8B   │     written data     │                       │  │
  │ └──────┴──────────────────────┴───────────────────────┘  │
  │         ▲                            ▲                   │
  │    OffsetInFirstBlock           CurrentPtr               │
  │                                  CurrentBlockEnd ────────│─ byte* + 4096
  └───────────────────────────────────────────────────────────┘

  State 2: Write doesn't fit, allocate new block and chain
  ┌──────────────────────────────────────┐     ┌──────────────────────────────────────┐
  │ Block A (full)                       │     │ Block B (new)                        │
  │ ┌──────┬──────────────────────┬────┐ │     │ ┌──────┬────────────┬──────────────┐ │
  │ │Next* │ D D D D D D D D D D │ DD │ │────▶│ │Next* │ new data   │  FREE SPACE  │ │
  │ │ ──── │──────────────────── │────│ │     │ │ NULL │            │              │ │
  │ └──────┴──────────────────────┴────┘ │     │ └──────┴────────────┴──────────────┘ │
  └──────────────────────────────────────┘     └──────────────────────────────────────┘
         ▲ CurrentBlock ──────────────────────────────▶ CurrentBlock
         ▲ Block (head)                                  ▲ CurrentPtr
         LastOffset=4096                                  CurrentBlockEnd = B+4096
```

---

## Read Algorithm (Reader)

```
  Reader.BeginForEachIndex(int foreachIndex):
  ──────────────────────────────────────────
  
  1. range = &Ranges[foreachIndex]
  2. m_RemainingItemCount = range->ElementCount
  3. m_LastBlockSize      = range->LastOffset
  4. m_CurrentBlock       = range->Block             // First block
  5. m_CurrentPtr         = (byte*)Block + OffsetInFirstBlock
  6. m_CurrentBlockEnd    = (byte*)Block + 4096
  7. RETURN m_RemainingItemCount


  Reader.ReadUnsafePtr(int size):
  ───────────────────────────────

  1. m_RemainingItemCount--
  2. ptr = m_CurrentPtr
  3. m_CurrentPtr += size
  
  4. IF m_CurrentPtr > m_CurrentBlockEnd:    // Crossed block boundary
     │
     ├─ m_CurrentBlock    = m_CurrentBlock->Next   // Follow linked list
     ├─ m_CurrentPtr      = m_CurrentBlock->Data + size
     ├─ m_CurrentBlockEnd = m_CurrentBlock + 4096
     └─ ptr = m_CurrentBlock->Data
  
  5. RETURN ptr
```

### Read Diagram: Walking Two Blocks

```
  BeginForEachIndex(0):
  
  Block A                                    Block B
  ┌──────┬──────────────────┬──────────┐    ┌──────┬───────────────┬─────┐
  │Next* │ item0 │ item1 │  │ item2 ╔══╪══▶│Next* │ item3 │ item4 │     │
  │  ──▶ │       │       │  │       ║  │    │ NULL │       │       │     │
  └──────┴───┬───┴───┬───┴──┴───────╨──┘    └──────┴───┬───┴───┬──┴─────┘
             │       │        ▲                         │       │
         Read<int> Read<int> │                    Read<int> Read<int>
             │       │    CurrentPtr                  │       │
             │       │  (crossed boundary             │       │
             │       │   → follow Next*)              │       │
             ▼       ▼                                ▼       ▼
         LastOffset ──────────────────────────── LastOffset of last block
```

---

## Block Allocation Strategy (Allocate method in BlockData)

```
  Two cases for block linking:

  CASE 1: oldBlock == null  (very first allocation for this thread)
  ───────────────────────────────────────────────────────────────
  
  Before:  Blocks[i] ──▶ existing_head
                                │
                                ▼
  
  After:   Blocks[i] ──▶ new_block ──▶ existing_head
                                │
                     new_block->Next = Blocks[i]  (old head)
                     Blocks[i] = new_block

  CASE 2: oldBlock != null  (appending after current block)
  ──────────────────────────────────────────────────────────
  
  Before:  oldBlock ──▶ oldNext
                                │
                                ▼
  
  After:   oldBlock ──▶ new_block ──▶ oldNext
                                │
                     new_block->Next = oldBlock->Next
                     oldBlock->Next = new_block
```

---

## Large Write Handling (WriteLarge)

Writes larger than one block's usable area (4088 bytes) are chunked:

```
  WriteLarge(data, size):
  ───────────────────────
  
  Input:  size bytes of data
  
  MaxLargeSize = 4088  (= 4096 - 8)
  
  allocationCount    = size / 4088
  allocationRemainder = size % 4088
  
  STEP 1: Write remainder FIRST (optimization)
  ┌───────────────────────────┐
  │ Allocate(remainder bytes) │  ← fills partial block first
  │ MemCpy from end of data   │     often fits in existing block
  └───────────────────────────┘
  
  STEP 2: Write full chunks in loop
  ┌───────────────────────────┐
  │ for i = 0..count-1:       │
  │   Allocate(4088 bytes)    │  ← each gets its own block
  │   MemCpy chunk[i]         │
  └───────────────────────────┘
  
  Why remainder first?
  ────────────────────
  The remainder is typically small and often fits in the current
  block's remaining space, avoiding an unnecessary new allocation.
  The full chunks always need fresh blocks anyway.
```

---

## Data Flow: Complete Lifecycle

```
  ╔═══════════════════════════════════════════════════════════════════════╗
  ║                    COMPLETE LIFECYCLE                                ║
  ╠═══════════════════════════════════════════════════════════════════════╣
  ║                                                                     ║
  ║  1. CREATION (main thread)                                          ║
  ║     new NativeThreadStream(Allocator.Persistent)                     ║
  ║         │                                                           ║
  ║         ├─ AllocateBlockData (1 allocation)                         ║
  ║         │   └─ BlockData + Blocks[ForEachCount] array               ║
  ║         │                                                           ║
  ║         └─ AllocateForEach (1 allocation)                           ║
  ║             └─ Ranges[ForEachCount] array, zeroed                   ║
  ║                                                                     ║
  ║  2. PARALLEL WRITE (job system, N threads)                          ║
  ║     Writer writer = stream.AsWriter();                              ║
  ║     writer.Write(myStruct);                                         ║
  ║     writer.Allocate(42);  // 42 bytes of raw space                  ║
  ║         │                                                           ║
  ║         ├─ Thread 0: writes to Ranges[0], allocs blocks chain A     ║
  ║         ├─ Thread 1: writes to Ranges[1], allocs blocks chain B     ║
  ║         └─ Thread N: writes to Ranges[N], allocs blocks chain X     ║
  ║                                                                     ║
  ║  3. READING (single thread or parallel, read-only per index)        ║
  ║     Reader reader = stream.AsReader();                              ║
  ║     for (int i = 0; i < reader.ForEachCount; i++)                   ║
  ║     {                                                               ║
  ║         int count = reader.BeginForEachIndex(i);                    ║
  ║         for (int j = 0; j < count; j++)                             ║
  ║             MyType val = reader.Read<MyType>();                     ║
  ║         reader.EndForEachIndex();                                   ║
  ║     }                                                               ║
  ║         │                                                           ║
  ║         ├─ Reads thread 0's chain from Block[0] linked list        ║
  ║         ├─ Reads thread 1's chain from Block[1] linked list        ║
  ║         └─ Reads thread N's chain from Block[N] linked list        ║
  ║                                                                     ║
  ║  4. DISPOSAL                                                        ║
  ║     stream.Dispose()                                                ║
  ║         │                                                           ║
  ║         ├─ Free all blocks in all chains                            ║
  ║         ├─ Free Ranges allocation                                   ║
  ║         └─ Free BlockData + Blocks allocation                       ║
  ║                                                                     ║
  ╚═══════════════════════════════════════════════════════════════════════╝
```

---

## Key Design Decisions

1. **Fixed 4KB block size**: Sweet spot between allocation overhead (smaller blocks mean more
   allocations) and wasted space (larger blocks waste memory at the tail). 4096 aligns to
   OS page sizes and most allocators handle it efficiently.

2. **Per-thread partitioning by index**: Instead of a single shared stream with atomic
   compare-and-swap on write, each thread gets its own Ranges[i] and block chain.
   Zero contention during writes — the most common and hot path.

3. **Optimistic pointer advance**: The writer speculatively advances CurrentPtr before
   checking if it overflowed. If it did, a new block is allocated. This keeps the common
   case (fits in current block) to just a pointer bump and increment.

4. **Remainder-first large writes**: When writing data larger than one block, the remainder
   (which is small) is written first. This often fits in the current block's remaining space,
   avoiding an extra allocation.

5. **Heterogeneous types**: The stream is type-agnostic — it just stores bytes. The Writer
   and Reader provide typed convenience methods, but the underlying storage is raw memory.
   This allows mixing different struct types in the same stream.

6. **Reader tracks RemainingItemCount**: Each write increments ElementCount; the reader
   decrements it during reads. This provides safety checking (did you read everything?)
   without storing per-item size headers.

---

## Performance Characteristics

| Operation         | Complexity | Notes                                    |
|-------------------|------------|------------------------------------------|
| Write (fits)      | O(1)       | Pointer bump + increment                 |
| Write (new block) | O(1)*      | Allocation + linking, no copying         |
| Read (in block)   | O(1)       | Pointer bump                             |
| Read (cross block)| O(1)       | Pointer follow + bump                    |
| Count             | O(N)       | Sums ElementCount across all threads     |
| IsEmpty           | O(N)*      | Early-exits on first non-empty range     |
| ToNativeArray     | O(T)       | T = total items, sequential copy         |
| Dispose           | O(N*B)     | N = threads, B = blocks per thread       |

**Memory overhead per block**: 8 bytes (Next pointer) out of 4096 = 0.2%
**Worst-case waste per thread**: Up to ~4088 bytes in the last partially-filled block
**Allocation count**: 2 initial + 1 per block overflow (proportional to data volume)

*Write with new block involves a heap allocation, making it amortized O(1) rather than
 true O(1). In practice, Unity's Allocator handles fixed-size allocations very efficiently.

## Source

- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.cs)
- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.Writer.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.Writer.cs)
- [BovineLabs.Core/Collections/EventStream/NativeThreadStream.Reader.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/NativeThreadStream.Reader.cs)
