NativeParallelMultiHashMapFallback — Inner Workings
=====================================================

Prevents map capacity exceptions by spilling over into a fallback queue.


HIGH-LEVEL ARCHITECTURE
───────────────────────

  ┌─────────────────────────────────────────────────────────────────────┐
  │       NativeParallelMultiHashMapFallback<TKey, TValue>             │
  │                                                                     │
  │  ┌─────────────────────────────────┐  ┌──────────────────────────┐  │
  │  │ NativeParallelMultiHashMap      │  │ NativeQueue<FallbackData>│  │
  │  │    HashMap                      │  │    Fallback              │  │
  │  │                                 │  │                          │  │
  │  │  ┌───────────────────────────┐  │  │  ┌──────┐ ┌──────┐     │  │
  │  │  │ keys[]    values[] next[] │  │  │  │ FB#0 │ │ FB#1 │ ... │  │
  │  │  │                           │  │  │  └──────┘ └──────┘     │  │
  │  │  │ Pre-allocated capacity    │  │  │  FIFO queue of          │  │
  │  │  └───────────────────────────┘  │  │  overflow entries       │  │
  │  └─────────────────────────────────┘  └──────────────────────────┘  │
  └─────────────────────────────────────────────────────────────────────┘


THE PROBLEM IT SOLVES
─────────────────────

  NativeParallelMultiHashMap has a fixed capacity at write time:

  ┌───────────────────────────────────────────────────────────────┐
  │ Parallel Writer tries Add():                                  │
  │                                                               │
  │  ┌─────────┐                                                  │
  │  │ Worker 0│──► TryAdd ──► SUCCESS ──► written to keys/values │
  │  ├─────────┤                                                  │
  │  │ Worker 1│──► TryAdd ──► SUCCESS ──► written to keys/values │
  │  ├─────────┤                                                  │
  │  │ Worker 2│──► TryAdd ──► FAIL! Capacity exceeded! ──► 💥    │
  │  └─────────┘                                                  │
  │                                                               │
  │  In parallel jobs, you can't resize. The job would fail.      │
  └───────────────────────────────────────────────────────────────┘

  The Fallback pattern:
  ┌───────────────────────────────────────────────────────────────┐
  │ Parallel Writer tries Add():                                  │
  │                                                               │
  │  ┌─────────┐                                                  │
  │  │ Worker 0│──► TryReserve(1) ──► SUCCESS ──► direct write   │
  │  ├─────────┤                                                  │
  │  │ Worker 1│──► TryReserve(1) ──► FAIL ──► Fallback queue    │
  │  ├─────────┤                                                  │
  │  │ Worker 2│──► TryReserve(1) ──► FAIL ──► Fallback queue    │
  │  └─────────┘                                                  │
  │                                                               │
  │  No crash! Overflow goes to lock-free NativeQueue.            │
  └───────────────────────────────────────────────────────────────┘


PARALLEL WRITER — ADD FLOW
──────────────────────────

  ParallelWriter.Add(key, item):
  ┌───────────────────────────────────────────────────────────────┐
  │                                                               │
  │  hashMap.TryReserve(1, out idx)                               │
  │       │                                                       │
  │       ├── SUCCESS (Likely path) ──────────────────────┐       │
  │       │                                               │       │
  │       │   data = hashMap.m_Writer.m_Buffer            │       │
  │       │   WriteArrayElement(data->keys,   idx, key)    │       │
  │       │   WriteArrayElement(data->values, idx, item)   │       │
  │       │   WriteArrayElement(data->next,   idx, hash)   │       │
  │       │                                               │       │
  │       │   ⚡ Direct memory write — no hash computation │       │
  │       │   ⚡ Bypasses normal Add() for raw speed       │       │
  │       │                                               │       │
  │       └── FAIL (Unlikely path) ───────────────┐        │       │
  │                                               │        │       │
  │           fallback.Enqueue(                   │        │       │
  │               FallbackData(key, item, hash)   │        │       │
  │           )                                   │        │       │
  │                                               │        │       │
  │           ⚡ Thread-safe queue enqueue         │        │       │
  │                                               ▼        ▼       │
  │                                    ┌─────────────────────┐    │
  │                                    │ Entry is preserved! │    │
  │                                    └─────────────────────┘    │
  └───────────────────────────────────────────────────────────────┘

  Note: Hint.Likely() guides branch prediction for the common case.


FALLBACK DATA STRUCTURE
───────────────────────

  ┌─────────────────────────────────────────────────┐
  │ readonly struct FallbackData                    │
  │ ┌─────────┬───────────┬───────────────────────┐ │
  │ │ TKey    │ TValue    │ int                   │ │
  │ │  Key    │  Value    │  Hash                 │ │
  │ └─────────┴───────────┴───────────────────────┘ │
  │                                                 │
  │ Pre-computed hash avoids recomputation later    │
  └─────────────────────────────────────────────────┘


APPLY JOB — MERGING FALLBACK INTO HASHMAP
──────────────────────────────────────────

  After parallel writing completes, ApplyJob merges overflow entries:

  ┌───────────────────────────────────────────────────────────────┐
  │  ApplyJob.Execute():                                          │
  │                                                               │
  │  1. HashMap.RecalculateBucketsCached()                        │
  │     │                                                         │
  │     └─► Rebuild bucket chains from all entries (including     │
  │         those written directly to the array)                  │
  │                                                               │
  │  2. While Fallback.TryDequeue(out item):                      │
  │     │                                                         │
  │     └─► HashMap.Add(item.Key, item.Value, item.Hash)          │
  │         (Now single-threaded, so resize is safe)              │
  │                                                               │
  │         ┌────────────┐    ┌────────────┐    ┌────────────┐   │
  │         │ Dequeue FB │───►│ Add to map │───►│ Dequeue FB │   │
  │         │ entry #1   │    │ (may grow) │    │ entry #2   │   │
  │         └────────────┘    └────────────┘    └────────────┘   │
  │                                               │               │
  │                                               ▼               │
  │                                         ┌──────────┐         │
  │                                         │ Queue    │         │
  │                                         │ empty    │         │
  │                                         └──────────┘         │
  │                                                               │
  │  Result: HashMap now contains ALL entries                     │
  │  Returns: ReadOnly view of merged HashMap                    │
  └───────────────────────────────────────────────────────────────┘


BATCH ADD OPERATIONS
────────────────────

  AddBatch(keys, values, length):
  ┌───────────────────────────────────────────────────────────────┐
  │  if TryReserve(length, out idx)                               │
  │      │                                                        │
  │      ├── SUCCESS:                                             │
  │      │   MemCpy keys   to data->keys   + idx                  │
  │      │   MemCpy values to data->values + idx                  │
  │      │   Compute hash for each: nextPtr[i] = keys[i].Hash()   │
  │      │                                                        │
  │      └── FAIL:                                                │
  │          for each (key, value):                               │
  │              fallback.Enqueue(FallbackData(key, value, hash)) │
  │          (entries merged later via ApplyJob)                   │
  └───────────────────────────────────────────────────────────────┘

  AddBatch with pre-computed hashes:
  ┌───────────────────────────────────────────────────────────────┐
  │  if TryReserve(length, out idx)                               │
  │      ├── SUCCESS: MemCpy all 3 arrays (keys, values, hashes)  │
  │      └── FAIL: Enqueue each entry individually                │
  └───────────────────────────────────────────────────────────────┘


LIFECYCLE FLOW
──────────────

  ┌─────────────────────────────────────────────────────────────┐
  │ 1. Create                                                   │
  │    var map = new NativeParallelMultiHashMapFallback<K,V>(   │
  │        capacity: 1000, allocator: Allocator.TempJob);       │
  │                                                             │
  │ 2. Write (parallel)                                         │
  │    var writer = map.AsWriter();                             │
  │    writer.Add(key, value);  // Never throws!                │
  │                                                             │
  │ 3. Apply (single-thread merge)                              │
  │    map.Apply(dep, out var reader);                          │
  │    // reader = NativeParallelMultiHashMap.ReadOnly           │
  │                                                             │
  │ 4. Read                                                     │
  │    if (reader.TryGetValue(key, out var val)) ...            │
  │                                                             │
  │ 5. Clear for reuse                                          │
  │    map.Clear(jobHandle);                                    │
  │    // HashMap cleared, Fallback drained via ApplyJob        │
  │    // OR simply:                                            │
  │    map.Clear(); // Just clears HashMap, fallback still has  │
  │                  // entries (use Apply first!)              │
  │                                                             │
  │ 6. Dispose                                                  │
  │    map.Dispose();                                           │
  └─────────────────────────────────────────────────────────────┘


PERFORMANCE CHARACTERISTICS
───────────────────────────

  ┌─────────────────────┬───────────────────────────────────────────┐
  │ Operation           │ Cost                                      │
  ├─────────────────────┼───────────────────────────────────────────┤
  │ Add (direct path)   │ O(1) — TryReserve + raw memory write      │
  │ Add (fallback path) │ O(1) amortized — NativeQueue enqueue      │
  │ ApplyJob            │ O(fallback_count) — dequeue + rehash      │
  │ Batch Add           │ O(n) — bulk memcpy when possible          │
  │ Memory overhead     │ NativeQueue for overflow entries          │
  └─────────────────────┴───────────────────────────────────────────┘

  Key insight: The hot path (direct write) bypasses normal
  NativeParallelMultiHashMap.Add() and writes directly to reserved
  array slots, computing the hash inline for maximum throughput.

## Source

- [BovineLabs.Core/Collections/NativeParallelMultiHashMapFallback.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/NativeParallelMultiHashMapFallback.cs)
- [BovineLabs.Core.Tests/Collections/NativeParallelMultiHashMapFallbackTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Collections/NativeParallelMultiHashMapFallbackTests.cs)
