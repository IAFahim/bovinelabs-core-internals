     1|NativeSlabAllocator - Safety-Wrapped Slab Allocator
     2|====================================================
     3|
     4|Source: BovineLabs.Core/Memory/NativeSlabAllocator.cs
     5|
     6|OVERVIEW
     7|--------
     8|NativeSlabAllocator<T> is a [NativeContainer]-decorated wrapper around
     9|UnsafeSlabAllocator<T>. It adds Unity's AtomicSafetyHandle system, enabling
    10|editor-time detection of use-after-free, write-during-read, and other safety
    11|violations. The underlying allocation mechanics are identical to
    12|UnsafeSlabAllocator - bump allocation within fixed-size slabs.
    13|
    14|ARCHITECTURE - WRAPPER PATTERN
    15|===============================
    16|
    17|    ┌──────────────────────────────────────────────────────────┐
    18|    │  [NativeContainer]                                       │
    19|    │  NativeSlabAllocator<T>                                  │
    20|    │                                                          │
    21|    │  ┌────────────────────────────────────────────────────┐  │
    22|    │  │  AtomicSafetyHandle m_Safety                       │  │
    23|    │  │  SharedStatic<int> s_staticSafetyId                │  │
    24|    │  │                                                    │  │
    25|    │  │  (only exist in ENABLE_UNITY_COLLECTIONS_CHECKS)   │  │
    26|    │  └────────────────────────────────────────────────────┘  │
    27|    │                                                          │
    28|    │  ┌────────────────────────────────────────────────────┐  │
    29|    │  │  slabAllocator: UnsafeSlabAllocator<T>             │  │
    30|    │  │                                                    │  │
    31|    │  │  ┌──────────────────────────────────────────────┐  │  │
    32|    │  │  │ countPerSlab                                 │  │  │
    33|    │  │  │ allocator: AllocatorHandle                   │  │  │
    34|    │  │  │ slabs: UnsafeList<Ptr>*                      │  │  │
    35|    │  │  │ count: int*                                  │  │  │
    36|    │  │  └──────────────────────────────────────────────┘  │  │
    37|    │  └────────────────────────────────────────────────────┘  │
    38|    │                                                          │
    39|    └──────────────────────────────────────────────────────────┘
    40|
    41|    Every public method checks AtomicSafetyHandle BEFORE delegating:
    42|
    43|
    44|OPERATION FLOW WITH SAFETY CHECKS
    45|==================================
    46|
    47|    Alloc():
    48|    ┌───────────────────────────────────────────────┐
    49|    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    50|    │   AtomicSafetyHandle.CheckWriteAndThrow(      │
    51|    │       m_Safety)                               │
    52|    │   // Throws if:                               │
    53|    │   //   - Already disposed                     │
    54|    │   //   - Another job is reading               │
    55|    │   //   - Safety handle invalidated            │
    56|    │ #endif                                        │
    57|    │                                               │
    58|    │ return slabAllocator.Alloc()  ──────────────► │
    59|    │                              ┌──────────────┐ │
    60|    │                              │ Bump allocate │ │
    61|    │                              │ from current  │ │
    62|    │                              │ slab or add   │ │
    63|    │                              │ new slab      │ │
    64|    │                              └──────────────┘ │
    65|    └───────────────────────────────────────────────┘
    66|
    67|    Clear():
    68|    ┌───────────────────────────────────────────────┐
    69|    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    70|    │   AtomicSafetyHandle.CheckWriteAndThrow(      │
    71|    │       m_Safety)                               │
    72|    │ #endif                                        │
    73|    │                                               │
    74|    │ slabAllocator.Clear()                         │
    75|    │   └── Free all slabs, reset to empty state    │
    76|    └───────────────────────────────────────────────┘
    77|
    78|    AllocationCount (get):
    79|    ┌───────────────────────────────────────────────┐
    80|    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    81|    │   AtomicSafetyHandle.CheckReadAndThrow(       │
    82|    │       m_Safety)                               │
    83|    │   // Write check NOT needed for reads         │
    84|    │ #endif                                        │
    85|    │                                               │
    86|    │ return slabAllocator.AllocationCount          │
    87|    └───────────────────────────────────────────────┘
    88|
    89|    Dispose():
    90|    ┌───────────────────────────────────────────────┐
    91|    │ #if ENABLE_UNITY_COLLECTIONS_CHECKS           │
    92|    │   CollectionHelper                            │
    93|    │     .DisposeSafetyHandle(ref m_Safety)        │
    94|    │   // Invalidates the handle so any future     │
    95|    │   // access throws immediately                │
    96|    │ #endif                                        │
    97|    │                                               │
    98|    │ slabAllocator.Dispose()                       │
    99|    │   └── Full teardown of all slabs              │
   100|    └───────────────────────────────────────────────┘
   101|
   102|
   103|SAFETY SYSTEM INTEGRATION
   104|==========================
   105|
   106|    Editor (ENABLE_UNITY_COLLECTIONS_CHECKS):
   107|
   108|    ┌───────────────────────────┐
   109|    │   NativeSlabAllocator<T>  │
   110|    │         │                 │
   111|    │         ▼                 │
   112|    │   ┌─────────────┐        │
   113|    │   │ SafetyId    │───────►│ Static type ID
   114|    │   └─────────────┘        │ registered once
   115|    │         │                 │
   116|    │         ▼                 │
   117|    │   ┌──────────────────┐   │
   118|    │   │ AtomicSafety     │   │
   119|    │   │ Handle           │   │
   120|    │   │                  │   │
   121|    │   │ • Version check  │   │
   122|    │   │ • Read/write     │   │
   123|    │   │   tracking       │   │
   124|    │   │ • Dispose detect │   │
   125|    │   └──────────────────┘   │
   126|    └───────────────────────────┘
   127|
   128|    ┌────────────────────────────────────────────────────┐
   129|    │ Safety Handle Lifecycle:                           │
   130|    │                                                    │
   131|    │ Constructor:                                       │
   132|    │   CreateSafetyHandle(allocator)                    │
   133|    │   InitNativeContainer<T>(handle)                   │
   134|    │   SetStaticSafetyId<T>(ref handle, ref id)         │
   135|    │   SetBumpSecondaryVersionOnScheduleWrite(true)     │
   136|    │                                                    │
   137|    │ Each Access:                                       │
   138|    │   CheckReadAndThrow  ← for reads (AllocationCount)│
   139|    │   CheckWriteAndThrow ← for writes (Alloc, Clear)  │
   140|    │                                                    │
   141|    │ Dispose:                                           │
   142|    │   DisposeSafetyHandle(ref handle)                  │
   143|    │   → Invalidates handle, future access throws       │
   144|    └────────────────────────────────────────────────────┘
   145|
   146|    Release builds: All safety code compiled out (zero overhead).
   147|
   148|
   149|UNDERLYING SLAB MEMORY LAYOUT
   150|==============================
   151|
   152|    (Identical to UnsafeSlabAllocator - shown here for completeness)
   153|
   154|    Slabs grow on demand:
   155|    ┌──────────────────────────────────────────────────┐
   156|    │                                                  │
   157|    │  Slab 0 (FULL)        Slab 1 (ACTIVE)            │
   158|    │  ┌──┬──┬──┬──┐      ┌──┬──┬──┬──┐              │
   159|    │  │T0│T1│T2│T3│      │T4│T5│??│??│  count=2      │
   160|    │  └──┴──┴──┴──┘      └──┴──┴──┴──┘              │
   161|    │                                                  │
   162|    │  countPerSlab = 4  in this example               │
   163|    │                                                  │
   164|    │  AllocationCount = (4 × (2-1)) + 2 = 6          │
   165|    └──────────────────────────────────────────────────┘
   166|
   167|
   168|COMPARISON: Native vs Unsafe
   169|=============================
   170|
   171|    ┌──────────────────────┬────────────────────┬───────────────────┐
   172|    │ Feature              │ NativeSlab         │ UnsafeSlab        │
   173|    ├──────────────────────┼────────────────────┼───────────────────┤
   174|    │ [NativeContainer]    │ Yes                │ No                │
   175|    │ Safety Handle        │ Yes (editor)       │ No                │
   176|    │ Dispose Detection    │ Automatic          │ Manual            │
   177|    │ Read/Write Tracking  │ Yes                │ No                │
   178|    │ Job System Integration│ Properly tracked  │ Untracked         │
   179|    │ Runtime Overhead     │ Zero (editor only) │ Zero              │
   180|    │ Burst Compatible     │ Yes                │ Yes               │
   181|    │ Use Case             │ Shared containers  │ Internal use      │
   182|    └──────────────────────┴────────────────────┴───────────────────┘
   183|
   184|
   185|KEY PROPERTIES
   186|==============
   187|
   188|  * Type:              struct NativeSlabAllocator<T> : IDisposable
   189|  * Attribute:         [NativeContainer]
   190|  * Thread Safety:     Safety-checked (editor); AtomicSafetyHandle
   191|  * Underlying:        UnsafeSlabAllocator<T> (same bump allocation)
   192|  * Safety:            AtomicSafetyHandle + static safety ID
   193|  * Write Checks:      Alloc(), Clear()
   194|  * Read Checks:       AllocationCount
   195|  * BumpSecondary:     SetBumpSecondaryVersionOnScheduleWrite(true)
   196|  * Allocator Check:   CollectionHelper.CheckAllocator in constructor
   197|  * Burst Compatible:  Yes
   198|  * Zero Runtime Cost: All checks behind #if ENABLE_UNITY_COLLECTIONS_CHECKS
   199|
   200|## Verified Data

## Verified Data

```
BovineLabs.Core.Memory.NativeSlabAllocator<T>
  Kind: struct (ValueType=True)
  Size (T=int): 40 bytes
  Attributes:
    [NativeContainerAttribute]
  Interfaces:
    System.IDisposable
  Constructors:
    .ctor(Int32 countPerSlab, AllocatorHandle allocator)
  Properties:
    public Int32 AllocationCount
    public Boolean IsCreated
  Methods:
    public Int32* Alloc()
    public Void Clear()
    public Void Dispose()
  Fields:
    private UnsafeSlabAllocator`1 slabAllocator
    private AtomicSafetyHandle m_Safety
  Static Fields:
    private static SharedStatic`1 s_staticSafetyId
Verified: 9 checks, 0 failures
```

