     1|MemoryAllocator - Tracked Multi-Allocation Manager
     2|====================================================
     3|
     4|Source: BovineLabs.Core/Memory/MemoryAllocator.cs
     5|
     6|OVERVIEW
     7|--------
     8|MemoryAllocator wraps Unity's AllocatorManager to provide simple "allocate many,
     9|free all" semantics. Every allocation is tracked in a NativeHashSet<Ptr>.
    10|Individual frees are NOT supported - call FreeAll() to release everything at once.
    11|This is ideal for scoped subsystems that create many temporary unmanaged buffers.
    12|
    13|ARCHITECTURE
    14|============
    15|
    16|     MemoryAllocator Instance
    17|    ┌─────────────────────────────────────────────────────┐
    18|    │                                                     │
    19|    │  Allocator: Allocator (e.g. Allocator.Persistent)   │
    20|    │                                                     │
    21|    │  allocated: NativeHashSet<Ptr>                      │──────┐
    22|    │    (tracks every live pointer)                      │      │
    23|    │                                                     │      │
    24|    └─────────────────────────────────────────────────────┘      │
    25|                                                                 │
    26|         allocated (NativeHashSet<Ptr>)                          │
    27|        ┌───────────────────────────────────────┐                │
    28|        │  Buckets (hash-based)                 │                │
    29|        │  ┌─────────┬──┐ → Ptr 0xDEADBEEF     │                │
    30|        │  │ bucket 0│──┤ → Ptr 0x12340000     │                │
    31|        │  ├─────────┼──┤ → Ptr 0xAAAA0000     │                │
    32|        │  │ bucket 1│──┤                       │                │
    33|        │  ├─────────┼──┤ → Ptr 0xBBBB0000     │                │
    34|        │  │ bucket 2│──┤ → Ptr 0xCCCC0000     │                │
    35|        │  ├─────────┼──┤                       │                │
    36|        │  │ ...     │  │                       │                │
    37|        │  └─────────┴──┘                       │                │
    38|        └───────────────────────────────────────┘                │
    39|                                                                 │
    40|    Actual Allocations (via AllocatorManager)                    │
    41|    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
    42|    │ Ptr 0xDEAD... │  │ Ptr 0x1234...│  │ Ptr 0xAAAA...│◄───────┘
    43|    │ sizeof(X)×N  │  │ sizeof(Y)×M  │  │ sizeof(Z)×K  │  (each tracked)
    44|    └──────────────┘  └──────────────┘  └──────────────┘
    45|
    46|
    47|ALLOCATION FLOW
    48|===============
    49|
    50|    Allocate(itemSizeInBytes, alignmentInBytes, items=1):
    51|
    52|    ┌───────────────────────────────────────────────────┐
    53|    │ ptr = AllocatorManager.Allocate(                  │
    54|    │         allocator, itemSizeInBytes,               │
    55|    │         alignmentInBytes, items)                  │
    56|    └───────────────────────┬───────────────────────────┘
    57|                            │
    58|                            ▼
    59|                ┌─────────────────────┐
    60|                │ allocated.Add(ptr)  │
    61|                └─────────────────────┘
    62|                            │
    63|                            ▼
    64|                    return ptr;
    65|
    66|
    67|    Create<T>(count=1):
    68|
    69|    ┌──────────────────────────────────────────────┐
    70|    │ return (T*)Allocate(                         │
    71|    │     sizeof(T),                               │
    72|    │     alignof(T),                              │
    73|    │     count)                                   │
    74|    └──────────────────────────────────────────────┘
    75|
    76|
    77|    CreateList<T>(capacity):
    78|
    79|    ┌───────────────────────────────────────────────────┐
    80|    │ capacity = max(capacity, 64/sizeof(T))            │
    81|    │ capacity = ceilpow2(capacity)                     │
    82|    │ buffer = Create<T>(capacity)                      │
    83|    │                                                   │
    84|    │ return UnsafeList<T> {                            │
    85|    │     Ptr       = buffer,                           │
    86|    │     Capacity  = capacity,                         │
    87|    │     Allocator = Allocator.None  (we own it!)      │
    88|    │ }                                                 │
    89|    └───────────────────────────────────────────────────┘
    90|
    91|    NOTE: Allocator.None means the list itself won't free the buffer.
    92|    MemoryAllocator.FreeAll() handles that.
    93|
    94|
    95|FREEALL FLOW
    96|============
    97|
    98|    FreeAll():
    99|
   100|    ┌─────────────────────────────────────────────┐
   101|    │ array = allocated.ToNativeArray(Temp)        │
   102|    │                                             │
   103|    │ ┌───────────────────────────────────────┐   │
   104|    │ │ foreach ptr in array:                 │   │
   105|    │ │   AllocatorManager.Free(allocator,ptr)│   │
   106|    │ │                                       │   │
   107|    │ │   ┌──────────┐  ┌──────────┐         │   │
   108|    │ │   │ Ptr 0xA  │  │ Ptr 0xB  │  ...    │   │
   109|    │ │   │  FREE    │  │  FREE    │         │   │
   110|    │ │   └──────────┘  └──────────┘         │   │
   111|    │ └───────────────────────────────────────┘   │
   112|    │                                             │
   113|    │ allocated.Clear()                           │
   114|    └─────────────────────────────────────────────┘
   115|
   116|
   117|DISPOSE FLOW
   118|============
   119|
   120|    Dispose():
   121|
   122|    ┌──────────────────────────────┐
   123|    │ FreeAll()                    │
   124|    │   └─ frees all tracked ptrs │
   125|    │   └─ clears the hashset     │
   126|    │                              │
   127|    │ allocated.Dispose()          │
   128|    │   └─ frees the hashset itself│
   129|    └──────────────────────────────┘
   130|
   131|
   132|USAGE PATTERN
   133|=============
   134|
   135|    ┌────────────────────────────────────────────────────┐
   136|    │  // Setup phase                                    │
   137|    │  var mem = new MemoryAllocator(Allocator.Persistent)│
   138|    │                                                    │
   139|    │  // Allocate many buffers                          │
   140|    │  int* a = mem.Create<int>(100);                    │
   141|    │  float* b = mem.Create<float>(256);                │
   142|    │  var list = mem.CreateList<byte>(1024);            │
   143|    │  void* c = mem.Allocate(64, 8);                    │
   144|    │                                                    │
   145|    │  // Use them...                                    │
   146|    │  // ...                                            │
   147|    │                                                    │
   148|    │  // Tear down everything at once                   │
   149|    │  mem.Dispose();                                    │
   150|    └────────────────────────────────────────────────────┘
   151|
   152|
   153|KEY PROPERTIES
   154|==============
   155|
   156|  * Type:              struct MemoryAllocator : IDisposable
   157|  * Thread Safety:     NONE (not thread-safe)
   158|  * Individual Free:   NOT supported (use FreeAll or Dispose)
   159|  * Tracking:          NativeHashSet<Ptr> - O(1) add, O(n) free-all
   160|  * Alloc Speed:       O(1) + hashset insert
   161|  * FreeAll Speed:     O(n) where n = number of allocations
   162|  * Burst Compatible:  Yes
   163|  * List Capacity:     Minimum 64/sizeof(T), always power-of-2
   164|  * List Ownership:    Buffer tracked by MemoryAllocator, not by list
   165|
   166|## Verified Data

## Verified Data

```
BovineLabs.Core.Memory.MemoryAllocator
  Kind: struct (ValueType=True)
  Size: 32 bytes
  Interfaces:
    System.IDisposable
  Constructors:
    .ctor(Allocator allocator)
  Properties:
    Allocator Allocator
  Methods:
    public Void* Allocate(Int32 itemSizeInBytes, Int32 alignmentInBytes, Int32 items)
    public T* Create<T>(Int32 count)
    public UnsafeList`1 CreateList<T>(Int32 capacity)
    public Void FreeAll()
    public Void Dispose()
  Functional Tests:
    CreateList<byte>(1024): Capacity=1024
    CreateList<int>(1): Capacity=16
    FreeAll(): completed
    Dispose(): completed
Verified: 12 checks, 0 failures
```

