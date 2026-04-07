# BufferAccessor.GetUnsafe

## Inner Workings Diagram

```
 BufferAccessor<T>.GetUnsafe(index)
 ═══════════════════════════════════════════════════════════════
 Bypasses per-element safety checks for faster buffer access

 PURPOSE:
 ═════════
 The standard BufferAccessor<T> indexer performs AtomicSafetyHandle
 checks on every access. GetUnsafe skips these, yielding a
 DynamicBuffer<T> directly from the raw pointer.


 ┌─────────────────────────────────────────────────────────────────────┐
 │  BufferAccessor<T>  (Unity's standard struct)                       │
 │  ┌─────────────────────────────────────────────────────────────┐   │
 │  │  byte*  m_BasePointer   ─→ BufferHeader array in chunk      │   │
 │  │  int    m_Length        ─→ count of buffers                 │   │
 │  │  int    m_Stride        ─→ bytes between BufferHeaders      │   │
 │  │  int    m_InternalCapacity                                  │   │
 │  │  AtomicSafetyHandle m_Safety0    (debug builds)             │   │
 │  │  AtomicSafetyHandle m_Safety1    (debug builds)             │   │
 │  │  byte   m_IsReadOnly           (debug builds)               │   │
 │  └─────────────────────────────────────────────────────────────┘   │
 └─────────────────────────────────────────────────────────────────────┘


 ACCESS COMPARISON
 ┌──────────────────────────────┬────────────────────────────────────┐
 │  Standard: bufferAccessor[i] │  Fast: bufferAccessor.GetUnsafe(i) │
 │  ─────────────────────────── │  ──────────────────────────────    │
 │  AtomicSafetyHandle.Check    │  NO safety handle check            │
 │  on every access             │  (only range assert in debug)      │
 │  Safe but slower in          │  Zero-overhead pointer arithmetic  │
 │  safety-enabled builds       │  in release builds                 │
 └──────────────────────────────┴────────────────────────────────────┘


 IMPLEMENTATION
 ┌───────────────────────────────────────────────────────────────────┐
 │  GetUnsafe<T>(this BufferAccessor<T> accessor, int index)        │
 │                                                                   │
 │  Step 1: Reinterpret internal layout                              │
 │  ┌───────────────────────────────────────────────────────────┐   │
 │  │  var accessor = UnsafeUtility.As<BufferAccessor<T>,       │   │
 │  │      InternalBufferAccessor>(ref bufferAccessor);          │   │
 │  │                                                           │   │
 │  │  InternalBufferAccessor mirrors BufferAccessor<T> layout: │   │
 │  │  ┌────────────────────────────────────────────────────┐   │   │
 │  │  │  byte* BasePointer;                                 │   │   │
 │  │  │  int   Length;                                      │   │   │
 │  │  │  int   Stride;                                      │   │   │
 │  │  │  int   InternalCapacity;                            │   │   │
 │  │  │  byte  IsReadOnly;     (CHECKS only)                 │   │   │
 │  │  │  AtomicSafetyHandle Safety0;  (CHECKS only)          │   │   │
 │  │  │  AtomicSafetyHandle ArrayInvalidationSafety;          │   │   │
 │  │  │  int SafetyReadOnlyCount;                             │   │   │
 │  │  │  int SafetyReadWriteCount;                            │   │   │
 │  │  └────────────────────────────────────────────────────┘   │   │
 │  └───────────────────────────────────────────────────────────┘   │
 │                     │                                             │
 │                     ▼                                             │
 │  Step 2: Bounds check (debug-only)                                │
 │  ┌───────────────────────────────────────────────────────────┐   │
 │  │  accessor.AssertIndexInRange(index)                        │   │
 │  │  → only active in ENABLE_UNITY_COLLECTIONS_CHECKS          │   │
 │  └───────────────────────────────────────────────────────────┘   │
 │                     │                                             │
 │                     ▼                                             │
 │  Step 3: Raw pointer arithmetic                                   │
 │  ┌───────────────────────────────────────────────────────────┐   │
 │  │  var hdr = (BufferHeader*)(                               │   │
 │  │      accessor.BasePointer + (index * accessor.Stride));    │   │
 │  │                                                           │   │
 │  │  BasePointer ──→ [BH0][BH1][BH2]...                       │   │
 │  │                     ↑                                     │   │
 │  │       index=1  ────┘  (stride bytes apart)                │   │
 │  └───────────────────────────────────────────────────────────┘   │
 │                     │                                             │
 │                     ▼                                             │
 │  Step 4: Return DynamicBuffer<T>                                  │
 │  ┌───────────────────────────────────────────────────────────┐   │
 │  │  return new DynamicBuffer<T>(                             │   │
 │  │      hdr,                                                 │   │
 │  │      accessor.InternalCapacity);                          │   │
 │  │  // Safety handles passed in CHECKS builds                │   │
 │  └───────────────────────────────────────────────────────────┘   │
 └───────────────────────────────────────────────────────────────────┘


 BUFFER HEADER TO DYNAMIC BUFFER
 ┌──────────────────────────────────────────────────────────────┐
 │  BufferHeader (internal Unity struct)                         │
 │  ┌────────────────────────────────────────────────────────┐  │
 │  │  byte* Pointer  ─→ actual element data in memory       │  │
 │  │  int   Length    ─→ current element count               │  │
 │  │  int   Capacity ─→ allocated capacity                  │  │
 │  └────────────────────────────────────────────────────────┘  │
 │         │                                                     │
 │         ▼                                                     │
 │  DynamicBuffer<T> wraps this header                          │
 │  ┌────────────────────────────────────────────────────────┐  │
 │  │  this[i] → *(Pointer + i)                             │  │
 │  │  Add/Remove/Resize via buffer allocator                │  │
 │  └────────────────────────────────────────────────────────┘  │
 └──────────────────────────────────────────────────────────────┘

 NOTE: There is also GetUnsafeRW() which forces IsReadOnly=false,
 ensuring the returned DynamicBuffer allows write access even when
 the accessor was created with read-only semantics.
