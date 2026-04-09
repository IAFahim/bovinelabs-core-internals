# NativeListExtensions.ClearAddRange

## Inner Workings Diagram

```
 NativeListExtensions.ClearAddRange
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeListExtensions                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
NativeListExtensions
  Kind: static class
Methods:
  Void ReserveNoResize(NativeList`1, Int32, out T*&, out Int32&)
  Void ReserveNoResize(ParallelWriter, Int32, out T*&, out Int32&)
  IntPtr GetUnsafeIntPtr(NativeList`1)
  IntPtr GetUnsafeReadOnlyIntPtr(NativeList`1)
  Void Insert(NativeList`1, Int32, T)
  Void ResizeInitialized(NativeList`1, Int32, Byte)
  Void ResizeInitialized(NativeList`1, Int32)
  Void AddRange(NativeList`1, T[])
  Void AddRange(NativeList`1, IEnumerable`1)
  Void ClearAddRange(NativeList`1, IEnumerable`1)
  Void ClearAddRange(NativeList`1, NativeArray`1)
  Void ClearAddRange(NativeList`1, NativeHashSet`1)
  Boolean Compare(NativeList`1, NativeHashSet`1)
Runtime Behavior:
  Initial: Length=3
  ClearAddRange(int[]): Length=4, [10,20,30,40]
  ClearAddRange(NativeArray): Length=2, [100,200]
  ClearAddRange(NativeHashSet): Length=3, values=[5,15,25]
  ClearAddRange clears first: Length=1, [0]=99
Verified: 3 checks, 0 failures
```
NativeListExtensions
  Kind: static class
Methods:
  Void ReserveNoResize(NativeList`1, Int32, out T*&, out Int32&)
  Void ReserveNoResize(ParallelWriter, Int32, out T*&, out Int32&)
  IntPtr GetUnsafeIntPtr(NativeList`1)
  IntPtr GetUnsafeReadOnlyIntPtr(NativeList`1)
  Void Insert(NativeList`1, Int32, T)
  Void ResizeInitialized(NativeList`1, Int32, Byte)
  Void ResizeInitialized(NativeList`1, Int32)
  Void AddRange(NativeList`1, T[])
  Void AddRange(NativeList`1, IEnumerable`1)
  Void ClearAddRange(NativeList`1, IEnumerable`1)
  Void ClearAddRange(NativeList`1, NativeArray`1)
  Void ClearAddRange(NativeList`1, NativeHashSet`1)
  Boolean Compare(NativeList`1, NativeHashSet`1)
Runtime Behavior:
  Initial: Length=3
  ClearAddRange(int[]): Length=4, [10,20,30,40]
  ClearAddRange(NativeArray): Length=2, [100,200]
  ClearAddRange(NativeHashSet): Length=3, values=[5,15,25]
  ClearAddRange clears first: Length=1, [0]=99
Verified: 3 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeListExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeListExtensions.cs)
