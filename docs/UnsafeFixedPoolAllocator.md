     1|UnsafeFixedPoolAllocator - Pre-Allocated Fixed-Size Pool
     2|=========================================================
     3|
     4|Source: BovineLabs.Core/Memory/UnsafeFixedPoolAllocator.cs
     5|
     6|OVERVIEW
     7|--------
     8|UnsafeFixedPoolAllocator<T> pre-allocates exactly maxItems contiguous slots
     9|at construction time. A UnsafeParallelHashSet<Ptr> tracks which slots are free.
    10|No resizing ever occurs. When the pool is exhausted, Alloc() returns null.
    11|This provides stable, predictable memory usage and pointer stability.
    12|
    13|MEMORY LAYOUT
    14|=============
    15|
    16|    UnsafeFixedPoolAllocator<T> Instance
    17|    ┌──────────────────────────────────────────────────────────┐
    18|    │                                                          │
    19|    │  maxItems: int            (fixed capacity, e.g. 16)      │
    20|    │  allocator: AllocatorHandle                              │
    21|    │                                                          │
    22|    │  buffer: Ptr ─────────────────────────────────┐          │
    23|    │        (single contiguous allocation)         │          │
    24|    │                                                │          │
    25|    │  freeIndex: UnsafeParallelHashSet<Ptr>         │          │
    26|    │    (tracks which slots are free)               │          │
    27|    │                                                │          │
    28|    └────────────────────────────────────────────────┼──────────┘
    29|                                                     │
    30|                                                     ▼
    31|    buffer (contiguous T[maxItems] block)
    32|    ┌──────┬──────┬──────┬──────┬──────┬──────┬──────┬──────┐
    33|    │ T[0] │ T[1] │ T[2] │ T[3] │ T[4] │ T[5] │ T[6] │ T[7] │
    34|    └──────┴──────┴──────┴──────┴──────┴──────┴──────┴──────┘
    35|      ▲      ▲      ▲      ▲      ▲      ▲      ▲      ▲
    36|      │      │      │      │      │      │      │      │
    37|    ptrs: buffer+0, buffer+1, buffer+2, ... buffer+7
    38|
    39|    Construction: ALL slots added to freeIndex
    40|    ┌────────────────────────────────────────────────────┐
    41|    │ freeIndex = { &T[0], &T[1], &T[2], ..., &T[15] } │
    42|    └────────────────────────────────────────────────────┘
    43|
    44|
    45|ALLOCATION FLOW
    46|================
    47|
    48|    T* Alloc():
    49|    ┌───────────────────────────────────────────────────┐
    50|    │ if freeIndex.Count() == 0:                        │
    51|    │   ┌─────────────────────────────────────────┐     │
    52|    │   │ Pool exhausted! Return null             │     │
    53|    │   │ (caller must handle failure)            │     │
    54|    │   └─────────────────────────────────────────┘     │
    55|    │                                                   │
    56|    │ e = freeIndex.GetEnumerator()                     │
    57|    │ ptr = e.Current    (arbitrary free slot pointer)  │
    58|    │ freeIndex.Remove(ptr)                             │
    59|    │ return (T*)ptr                                    │
    60|    └───────────────────────────────────────────────────┘
    61|
    62|    Example: Alloc() removes &T[3] from free set
    63|
    64|    Before:                               After:
    65|    ┌──────┬──────┬──────┬──────┐        ┌──────┬──────┬──────┐
    66|    │ T[0] │ T[1] │ T[2] │ T[3] │ ...    │ T[0] │ T[1] │ T[2] │ ...
    67|    │ FREE │ FREE │ FREE │ FREE │        │ FREE │ FREE │ FREE │ USED│
    68|    └──────┴──────┴──────┴──────┘        └──────┴──────┴──────┘
    69|    free={0,1,2,3,...}                    free={0,1,2,...}
    70|          ^^^^ removed 3                        ^^^ no 3
    71|
    72|
    73|FREE FLOW
    74|==========
    75|
    76|    Free(T* p):
    77|    ┌───────────────────────────────────────────────────┐
    78|    │ ValidatePtr(p)  ←── safety checks (editor only)   │
    79|    │                                                   │
    80|    │ freeIndex.Add(p)                                  │
    81|    └───────────────────────────────────────────────────┘
    82|
    83|    Validation checks (ENABLE_UNITY_COLLECTIONS_CHECKS):
    84|    ┌───────────────────────────────────────────────┐
    85|    │ 1. p != null                                  │
    86|    │ 2. buffer <= p < buffer + maxItems            │
    87|    │    (pointer must be from this pool)           │
    88|    │ 3. !freeIndex.Contains(p)                     │
    89|    │    (double-free detection)                    │
    90|    │ 4. freeIndex.Count() < maxItems               │
    91|    │    (corruption check)                         │
    92|    └───────────────────────────────────────────────┘
    93|
    94|
    95|POOL STATE LIFECYCLE
    96|====================
    97|
    98|    State 1: Just constructed (all free)
    99|    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
   100|    │F │F │F │F │F │F │F │F │F │F │F │F │F │F │F │F │  maxItems=16
   101|    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
   102|    freeIndex.Count() = 16
   103|
   104|    State 2: After 10 allocations
   105|    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
   106|    │U │U │U │U │U │U │U │U │U │U │F │F │F │F │F │F │
   107|    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
   108|    freeIndex.Count() = 6
   109|
   110|    State 3: After freeing 3 slots
   111|    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
   112|    │U │F │U │F │U │U │U │U │U │U │F │F │F │U │F │F │
   113|    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
   114|    freeIndex.Count() = 9
   115|
   116|    State 4: Fully exhausted
   117|    ┌──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┬──┐
   118|    │U │U │U │U │U │U │U │U │U │U │U │U │U │U │U │U │
   119|    └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──┘
   120|    freeIndex.Count() = 0  →  next Alloc() returns null!
   121|
   122|
   123|POINTER VALIDATION DETAIL
   124|==========================
   125|
   126|    Valid pointer check (editor only):
   127|
   128|    buffer                                         buffer + maxItems
   129|      │                                                   │
   130|      ▼                                                   ▼
   131|    ┌──────────────────────────────────────────────────────┐
   132|    │  ✓ p inside this range                               │
   133|    └──────────────────────────────────────────────────────┘
   134|
   135|    ┌───────────────────┐
   136|    │ p == null?        │──► YES → ArgumentException
   137|    │                   │
   138|    │ p < buffer?       │──► YES → "Ptr not from this allocator"
   139|    │                   │
   140|    │ p >= buffer+N?    │──► YES → "Ptr not from this allocator"
   141|    │                   │
   142|    │ freeIndex.Has(p)? │──► YES → "Ptr already returned" (double free!)
   143|    │                   │
   144|    │ All OK            │──► proceed with free
   145|    └───────────────────┘
   146|
   147|
   148|DISPOSE
   149|=======
   150|
   151|    Dispose():
   152|    ┌────────────────────────────────────────────────────┐
   153|    │ Unmanaged.Free(buffer)    ← free the T[] block    │
   154|    │ freeIndex.Dispose()       ← free the hash set     │
   155|    │ buffer = Ptr.Zero                                │
   156|    └────────────────────────────────────────────────────┘
   157|
   158|
   159|KEY PROPERTIES
   160|==============
   161|
   162|  * Type:              struct UnsafeFixedPoolAllocator<T> : IDisposable
   163|  * Thread Safety:     Partial (UnsafeParallelHashSet is concurrent-read)
   164|  * Capacity:          Fixed at construction (maxItems), never resizes
   165|  * Alloc:             O(1) amortized (hashset enumerate + remove)
   166|  * Free:              O(1) (hashset add)
   167|  * Overflow:          Returns null (caller must handle)
   168|  * Safety:            Double-free, null, bounds checking (editor only)
   169|  * Burst Compatible:  Yes
   170|  * Memory Layout:     Single contiguous block for all items
   171|  * Overhead:          UnsafeParallelHashSet<Ptr> with maxItems entries
   172|  * Use Case:          Fixed-capacity pools where max count is known upfront
   173|
   174|## Verified Data

## Verified Data

```
BovineLabs.Core.Memory.UnsafeFixedPoolAllocator<T>
  Kind: struct (ValueType=True)
  Size (T=int): 32 bytes
  Interfaces:
    System.IDisposable
  Constructors:
    .ctor(Int32 maxItems, Allocator allocator)
  Properties:
    public Boolean IsCreated
  Methods:
    public Int32* Alloc()
    public Void Free(Int32* p)
    public Void Dispose()
  Fields:
    private Int32 maxItems
    private AllocatorHandle allocator
    private Ptr buffer
    private UnsafeParallelHashSet`1 freeIndex
Verified: 8 checks, 0 failures
```

