# SpinLock - Inner Workings

## Overview

`BovineLabs.Core.Utility.SpinLock` is a lightweight, value-type spinlock for ECS thread
synchronization. It uses `Interlocked.CompareExchange` for atomic acquisition and
`Volatile.Read/Write` for memory-barrier-correct spin-waiting and release. Being a struct,
it can be embedded directly in NativeContainers, job structs, and other blittable types
without managed heap allocation.

Adapted from `com.unity.collections/Unity.Collections/AllocatorManager.cs`.

---

## Source

`BovineLabs.Core/Utility/SpinLock.cs`

---

## Structure

```
+------------------------------------------------------------------+
|                 SpinLock  (public struct)                         |
+------------------------------------------------------------------+
|  private int lock;      // 0 = free, 1 = held                    |
+------------------------------------------------------------------+
|  void Acquire()                spin until locked                  |
|  bool TryAcquire()             one-shot attempt, no spin          |
|  bool TryAcquire(bool spin)    conditional: spin or one-shot      |
|  void Release()                set lock = 0                       |
+------------------------------------------------------------------+
```

---

## Acquire Flow (Full Spin)

```
 Acquire()
 ============================================

 for (;;)                          // spin forever until acquired
 {
   +----------------------------------------------------------+
   | Step 1: Optimistic Atomic Compare-And-Swap               |
   |                                                          |
   |   old = Interlocked.CompareExchange(ref lock, 1, 0)      |
   |                                                          |
   |   if (old == 0)                                          |
   |     return;  // WE GOT THE LOCK                          |
   |             // (atomically set lock 0 -> 1)               |
   +----------------------------------------------------------+
         |
         | lock was already 1 (held by another thread)
         v
   +----------------------------------------------------------+
   | Step 2: Spin-Wait (Reduce Cache Coherency Traffic)       |
   |                                                          |
   |   while (Volatile.Read(ref lock) == 1)                   |
   |   {                                                      |
   |     // busy-wait, but only doing                         |
   |     // volatile reads (no write traffic)                  |
   |   }                                                      |
   |                                                          |
   |   When lock becomes 0, exit inner loop                   |
   |   and retry Step 1                                       |
   +----------------------------------------------------------+
 }
```

### Visual Timeline: Contention Scenario

```
         Time ---------------------------------------------------->

Thread A |--- Acquire() CAS(0->1) SUCCESS --->|--- work ---| Release()
         |   lock = 1                         |             | lock = 0
                                                            |
Thread B |--- Acquire() CAS(fail, was 1) ---->|             |
         |   while(Volatile.Read==1) spin     |             |
         |   ... spin ...                     |             | CAS(0->1)
         |   ... spin ...                     |             | SUCCESS
         |                                    |             |-- work --|
         |                                    |             | Release()
         |                                    |             | lock = 0
                                                                     |
Thread C |--- Acquire() CAS(fail) -------------------------------->|
         |   while(Read==1) spin                                CAS OK
```

---

## TryAcquire Flow (No Spin)

```
 TryAcquire()
 ============================================

 Step 1: Cheap Volatile Read (avoid cache miss if locked)
   if (Volatile.Read(ref lock) != 0)
     return false;  // lock is held, bail immediately

 Step 2: Atomic CAS
   if (Interlocked.CompareExchange(ref lock, 1, 0) == 0)
     return true;   // acquired
   else
     return false;  // someone grabbed it between Step 1 and Step 2

 Timeline:
                        Time ------>
 Thread A:  Read(0) -> CAS(0->1) -> TRUE  (lock acquired)
 Thread B:  Read(1) -> FALSE             (early exit, no CAS)
 Thread C:  Read(0) -> CAS(fail) -> FALSE (race with Thread A)
```

---

## TryAcquire(bool spin) Flow

```
 TryAcquire(bool spin)
 ============================================

 if (spin == true)
   |
   +---> Acquire()    // blocks until acquired
   |     return true;  // always succeeds (eventually)
   |
 else
   |
   +---> TryAcquire()  // one-shot, may fail
         return result;
```

---

## Release Flow

```
 Release()
 ============================================

 Volatile.Write(ref lock, 0)

   |  Ensures all prior writes by this thread are visible
   |  BEFORE the lock release is visible to other threads.
   |
   |  Memory ordering guarantee:
   |    All writes inside critical section
   |    are committed before lock = 0 becomes visible.
   |
   v
 Other threads spinning on Volatile.Read(ref lock)
 will now see 0 and attempt CAS.
```

---

## Memory Model & Atomicity

```
                    CPU Core 0                CPU Core 1
                    +----------+              +----------+
                    | L1 Cache |              | L1 Cache |
                    | lock=0   |              | lock=0   |
                    +----+-----+              +----+-----+
                         |                         |
                         v                         v
                    +-----------------------------------+
                    |          Shared L2/L3 Cache       |
                    |          lock (main memory)        |
                    +-----------------------------------+

 Interlocked.CompareExchange:
   |  Generates LOCK CMPXCHG instruction (x86)
   |  Cache line locked atomically
   |  No other core can modify this memory location
   |  during the operation
   |
   |  Guarantees:
   |    - Mutual exclusion (only one CAS succeeds)
   |    - Memory barrier (full fence)
   |    - No torn reads/writes on int

 Volatile.Read:
   |  Generates acquire fence
   |  Ensures subsequent reads see fresh data
   |  Does NOT generate bus traffic (just reads cache line)

 Volatile.Write:
   |  Generates release fence
   |  Ensures prior writes are committed
   |  before this write becomes visible
```

---

## State Machine

```
                    lock field
                 +-------+-------+
                 |   0   |   1   |
                 +-------+-------+
   Acquire()  |  FREE  | HELD  |
              +-------+-------+
                  |       ^
                  |  CAS  |  CAS fails
                  |  OK   |  (already 1)
                  v       |
              +-------+-------+
              | HELD  | HELD  |
              | (ours)|(theirs)|
              +-------+-------+
                  |
                  | Release()
                  | Volatile.Write(0)
                  v
              +-------+
              | FREE  |
              +-------+

 Legend:
   CAS(0->1)  = Interlocked.CompareExchange(ref lock, 1, 0)
   Write(0)   = Volatile.Write(ref lock, 0)
```

---

## Usage Patterns in ECS

```
 Pattern 1: Protecting a shared counter
 ============================================
 struct MyJob : IJobParallelFor
 {
     public SpinLock lock;
     public NativeArray<int> counter;

     void Execute(int i)
     {
         // ... compute something ...
         lock.Acquire();
         counter[0] += result;
         lock.Release();
     }
 }


 Pattern 2: Try-lock with fallback
 ============================================
 struct MyJob : IJobParallelFor
 {
     public SpinLock lock;
     public NativeArray<int> data;

     void Execute(int i)
     {
         if (lock.TryAcquire())
         {
             // critical section
             data[0] = i;
             lock.Release();
         }
         else
         {
             // fallback path (skip, defer, use local buffer)
         }
     }
 }


 Pattern 3: Conditional spin
 ============================================
 if (spinLock.TryAcquire(shouldSpin))
 {
     // got it (always true if shouldSpin==true)
     // do work
     spinLock.Release();
 }
```

---

## Comparison: SpinLock vs System.Threading.SpinLock

```
 +-----------------------+---------------------------+---------------------------+
 | Feature               | BovineLabs SpinLock       | System.Threading.SpinLock  |
 +-----------------------+---------------------------+---------------------------+
 | Type                  | struct (blittable)        | struct (non-blittable)     |
 | Burst compatible      | YES                       | NO                         |
 | Embeddable in         | YES                       | NO                         |
 |   NativeContainer     |                           |                            |
 | Thread ownership      | Not tracked               | Optional tracking          |
 | Reentrancy detection  | None                      | Optional                   |
 | Lock stealing         | Possible (user discipline)| Detected if tracking on    |
 | Wait strategy         | Volatile.Read spin        | SpinWait + backoff         |
 | CPU pause instruction | Not used (noted as TODO)  | Uses Thread.SpinWait       |
 | Overhead (uncontended)| ~1 CAS instruction        | ~1 CAS + method calls      |
 +-----------------------+---------------------------+---------------------------+
```

---

## Why Two-Phase Spin (CAS + Volatile.Read)?

```
 Naive approach (CAS only in the loop):
 ---------------------------------------
 for (;;)
 {
     if (Interlocked.CompareExchange(ref lock, 1, 0) == 0)
         return;
 }
   Problem: Every failed CAS generates a write to the cache line
            (LOCK prefix instruction), causing cache-line
            invalidation traffic on the bus. N waiting threads
            generate O(N) bus transactions per retry.

 BovineLabs approach (Volatile.Read + CAS):
 -------------------------------------------
 for (;;)
 {
     if (Interlocked.CompareExchange(ref lock, 1, 0) == 0)
         return;

     while (Volatile.Read(ref lock) == 1)
     {
         // spin without writing
     }
 }
   Benefit: Most spins are read-only (Volatile.Read).
            No cache-line invalidation traffic.
            Only when lock transitions 1->0 do threads retry CAS.
            Reduces bus traffic from O(N) to O(1) per release.
```

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| `struct` (not `class`) | Blittable, embeddable in NativeContainers, no GC allocation |
| Two-phase spin (CAS + Volatile.Read) | Reduces cache-coherency bus traffic during contention |
| `Volatile.Write` for release | Ensures all writes in critical section are visible before lock release |
| No thread ownership tracking | Minimal overhead; user responsible for correct usage |
| No `pause` instruction | Noted as future improvement; current approach is portable |
| `TryAcquire` checks `Volatile.Read` first | Avoids unnecessary CAS (and cache miss) when lock is already held |
| Adapted from Unity Collections | Battle-tested pattern in Unity's own allocator code |

## Source

- [BovineLabs.Core/Utility/SpinLock.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/SpinLock.cs)
- [BovineLabs.Core/Utility/EntityLock.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/EntityLock.cs)
- [BovineLabs.Core/Collections/UnmanagedPool.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnmanagedPool.cs)
