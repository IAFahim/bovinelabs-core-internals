# UnsafePerfectHashMap

## Inner Workings Diagram

```
 UnsafePerfectHashMap
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ TKey*                    Keys                                      │
 │ TValue*                  Values                                    │
 │ int                      Size                                      │
 │ TValue                   NullValue                                 │
 │ bool                     IsCreated                                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Free(UnsafePerfectHashMap<TKey, TValue>* data)                     │
 │   → void                                                           │
 │ Dispose()                                                          │
 │   → void                                                           │
 │ TryGetValue(TKey key, out TValue item)                             │
 │   → bool                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
UnsafePerfectHashMap<int,int>
  Kind: struct
  Size: 32 bytes
Fields:
  [0] Int32* Keys  (private)
  [8] Int32* Values  (private)
  [16] Int32 Size  (private)
  [20] Int32 NullValue  (private)
  [24] AllocatorHandle allocator  (private)
Properties:
  Boolean IsCreated { get; }
  Int32 Item { get;set }
Methods:
  Void Dispose()
  Boolean TryGetValue(Int32, out Int32&)
Runtime Behavior:
  IsCreated=True
  TryGetValue(1)=True, value=100
  TryGetValue(2)=True, value=200
  TryGetValue(3)=True, value=300
  TryGetValue(0)=False (empty slot)
  map[1]=100
  map[1]=111 -> TryGetValue(1)=True, value=111
Verified: 3 checks, 1 failures
```
UnsafePerfectHashMap<int,int>
  Kind: struct
  Size: 32 bytes
Fields:
  [0] Int32* Keys  (private)
  [8] Int32* Values  (private)
  [16] Int32 Size  (private)
  [20] Int32 NullValue  (private)
  [24] AllocatorHandle allocator  (private)
Properties:
  Boolean IsCreated { get; }
  Int32 Item { get;set }
Methods:
  Void Dispose()
  Boolean TryGetValue(Int32, out Int32&)
Runtime Behavior:
  IsCreated=True
  TryGetValue(1)=True, value=100
  TryGetValue(2)=True, value=200
  TryGetValue(3)=True, value=300
  TryGetValue(0)=False (empty slot)
  map[1]=100
  map[1]=111 -> TryGetValue(1)=True, value=111
Verified: 3 checks, 1 failures
```

## Source

- [BovineLabs.Core/Collections/UnsafePerfectHashMap.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafePerfectHashMap.cs)
