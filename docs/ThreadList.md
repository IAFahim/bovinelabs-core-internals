ThreadList — Inner Workings
============================

Avoids lock contention by using thread-local storage for temporary lists.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌──────────────────────────────────────────────────────────────────────┐
  │                          ThreadList                                  │
  │                                                                      │
  │  ┌────────────────────────────────┐                                  │
  │  │  AllocatorHandle allocator     │                                  │
  │  └────────────────────────────────┘                                  │
  │                                                                      │
  │  ┌────────────────────────────────┐                                  │
  │  │  Lists* buffer                 │───┐                              │
  │  │  (array of cache-line-sized    │   │                              │
  │  │   structs, one per thread)     │   │                              │
  │  └────────────────────────────────┘   │                              │
  │                                       ▼                              │
  │  ┌───────────────────────────────────────────────────────────────┐   │
  │  │          buffer: Lists[JobsUtility.ThreadIndexCount]          │   │
  │  │                                                               │   │
  │  │  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────┐   │   │
  │  │  │ Lists[0]         │ │ Lists[1]         │ │ Lists[N]     │   │   │
  │  │  │ ┌──────────────┐ │ │ ┌──────────────┐ │ │ ┌──────────┐ │   │   │
  │  │  │ │UnsafeList<by-│ │ │ │UnsafeList<by-│ │ │ │UnsafeLis-│ │   │   │
  │  │  │ │te> List      │ │ │ │te> List      │ │ │ │t<byte>   │ │   │   │
  │  │  │ │(512 initial) │ │ │ │(512 initial) │ │ │ │(512 init)│ │   │   │
  │  │  │ └──────────────┘ │ │ └──────────────┘ │ │ └──────────┘ │   │   │
  │  │  │  64 bytes padded │ │  64 bytes padded │ │ 64 bytes pad │   │   │
  │  │  └──────────────────┘ └──────────────────┘ └──────────────┘   │   │
  │  └───────────────────────────────────────────────────────────────┘   │
  └──────────────────────────────────────────────────────────────────────┘


CACHE-LINE ISOLATION
────────────────────

  Each Lists entry is padded to JobsUtility.CacheLineSize (64 bytes):

  ┌──────── 64-byte cache line ────────┐
  │                                     │
  │  UnsafeList<byte> List              │
  │  ┌────────────────────────────────┐ │
  │  │ Ptr:  buffer pointer           │ │
  │  │ Length: current element count  │ │
  │  │ Capacity: 512 (initial)        │ │
  │  │ ... padding ...                │ │
  │  └────────────────────────────────┘ │
  │                                     │
  └─────────────────────────────────────┘

  Why cache-line sized?
  ┌──────────────────────────────────────────────────────────────┐
  │  CPU caches data in 64-byte "cache lines"                    │
  │                                                              │
  │  If Thread 0 and Thread 1 share a cache line:                │
  │  ┌────────────────────┬────────────────────┐                 │
  │  │ Thread 0 data      │ Thread 1 data      │ ← Same line!  │
  │  │ (write)            │ (write)            │                 │
  │  └────────────────────┴────────────────────┘                 │
  │       │                       │                              │
  │       └── FALSE SHARING ──────┘                              │
  │       CPU must invalidate cache across cores                 │
  │       → Massive slowdown on parallel work                    │
  │                                                              │
  │  With 64-byte padding:                                       │
  │  ┌────────────────────┐  ┌────────────────────┐              │
  │  │ Thread 0 data      │  │ Thread 1 data      │              │
  │  │ (own cache line)   │  │ (own cache line)   │              │
  │  └────────────────────┘  └────────────────────┘              │
  │       No false sharing! Each thread's data is isolated.      │
  └──────────────────────────────────────────────────────────────┘


THREAD ACCESS PATTERN
─────────────────────

  ┌──────────────────────────────────────────────────────────────────┐
  │  GetList():                                                      │
  │                                                                  │
  │    threadIndex = JobsUtility.ThreadIndex                         │
  │         │                                                        │
  │         ▼                                                        │
  │    ┌──────────────────────────────────────────────────────┐      │
  │    │ ref list = UnsafeUtility.ArrayElementAsRef<Lists>(   │      │
  │    │     buffer,                                           │      │
  │    │     threadIndex    ← Direct index, NO locking         │      │
  │    │ )                                                      │      │
  │    └──────────────────────────────────────────────────────┘      │
  │                                                                  │
  │    return ref list.List    ← Return reference to UnsafeList      │
  │                                                                  │
  │  ⚡ Zero contention: each thread accesses only its own slot       │
  │  ⚡ O(1) lookup: simple array index by thread ID                  │
  │  ⚡ No atomics, no locks, no Interlocked operations               │
  └──────────────────────────────────────────────────────────────────┘


MEMORY LAYOUT (BYTE-LEVEL)
──────────────────────────

  Heap allocation:
  ┌─────────────────────────────────────────────────────────────┐
  │  buffer = allocate(sizeof(Lists) * ThreadIndexCount)        │
  │                                                             │
  │  Offset  │ Content                                         │
  │  ────────┼────────────────────────────────────────────────  │
  │  0x000   │ Lists[0].List = UnsafeList<byte>(ptr, len, cap) │
  │  0x040   │ Lists[1].List = UnsafeList<byte>(ptr, len, cap) │
  │  0x080   │ Lists[2].List = UnsafeList<byte>(ptr, len, cap) │
  │  ...     │ ...                                             │
  │  N*0x040 │ Lists[N].List = UnsafeList<byte>(ptr, len, cap) │
  └─────────────────────────────────────────────────────────────┘

  Each UnsafeList<byte> initially has Capacity = 512 bytes.


CONSTRUCTION
────────────

  ┌────────────────────────────────────────────────────────────────┐
  │ ThreadList(allocator):                                         │
  │                                                                │
  │  1. Allocate buffer                                            │
  │     buffer = alloc(sizeof(Lists) * ThreadIndexCount)           │
  │                                                                │
  │  2. Initialize each thread's list                              │
  │     for i = 0 to ThreadIndexCount:                             │
  │       buffer[i].List = new UnsafeList<byte>(512, allocator)   │
  │                                                                │
  │  ThreadIndexCount = max worker threads + 1 (main thread)       │
  │  Typically: 1 + JobsUtility.JobWorkerCount                     │
  └────────────────────────────────────────────────────────────────┘


USAGE PATTERN
─────────────

  Inside a Burst job:
  ┌──────────────────────────────────────────────────────────────┐
  │ // Each worker thread gets its own list — no contention      │
  │ ref var myList = threadList.GetList();                       │
  │                                                              │
  │ myList.Clear();  // Reset for this frame                     │
  │                                                              │
  │ // Write freely — this list is exclusive to this thread      │
  │ for (int i = 0; i < items; i++)                             │
  │ {                                                            │
  │     // Pack data into byte list                              │
  │     myList.AddRangeNoResize(...);                            │
  │ }                                                            │
  └──────────────────────────────────────────────────────────────┘

  Main thread reads all lists after jobs complete:
  ┌──────────────────────────────────────────────────────────────┐
  │ for (int t = 0; t < threadCount; t++)                        │
  │ {                                                            │
  │     ref var list = threadList.GetList(t);                    │
  │     // Process each thread's accumulated data                │
  │     ProcessBytes(list.Ptr, list.Length);                     │
  │ }                                                            │
  └──────────────────────────────────────────────────────────────┘


WHY BYTE LIST?
──────────────

  The list is UnsafeList<byte> (not generic) because:
  ┌──────────────────────────────────────────────────────────────┐
  │ • Byte is the universal unit — any unmanaged type can be     │
  │   written as raw bytes                                       │
  │ • Avoids generic type proliferation                          │
  │ • Users can reinterpret the byte buffer as any type:         │
  │                                                              │
  │   ref var list = threadList.GetList();                       │
  │   var intPtr = (int*)list.Ptr;                               │
  │   // Now reading ints from the byte buffer                   │
  └──────────────────────────────────────────────────────────────┘


DISPOSAL
────────

  ┌───────────────────────────────────────────────────────────┐
  │ Dispose():                                                │
  │                                                           │
  │  1. Free(buffer)                                          │
  │     └─ This frees the Lists array                         │
  │        NOTE: Does NOT dispose individual UnsafeList<byte>  │
  │        instances! Their backing buffers may leak.          │
  │        The list data (buffer pointer in each UnsafeList)   │
  │        is freed as part of the Lists allocation.           │
  │                                                           │
  │  2. buffer = null                                         │
  └───────────────────────────────────────────────────────────┘

  Wait — actually, the Dispose only frees the buffer array.
  The UnsafeList<byte> instances within each Lists are NOT
  individually disposed. The 512-byte buffers they point to
  are allocated separately and need their own disposal.
  However, since they share the same allocator, in practice
  with Allocator.Domain/TempJob this is managed by the allocator.


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌────────────────────┬──────────────────────────────────────────┐
  │ Operation          │ Cost                                     │
  ├────────────────────┼──────────────────────────────────────────┤
  │ GetList()          │ O(1) — array index by ThreadIndex        │
  │ No lock contention │ ZERO — each thread has isolated list     │
  │ No false sharing   │ ZERO — 64-byte cache-line isolation      │
  │ Initial capacity   │ 512 bytes per thread                     │
  │ Memory overhead    │ ThreadIndexCount * 64 bytes for structs  │
  │                    │ + ThreadIndexCount * 512 bytes for data  │
  └────────────────────┴──────────────────────────────────────────┘

## Source

- [BovineLabs.Core/Collections/ThreadList.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/ThreadList.cs)
- [BovineLabs.Core/Utility/PooledNativeList.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/PooledNativeList.cs)
