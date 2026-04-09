# NativeListExtensions.ReserveNoResize

## Inner Workings Diagram

```
NativeListExtensions.ReserveNoResize
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
  Void ReserveNoResize(NativeList`1 nativeList, Int32 length, out T*& ptr, out Int32& idx)
  Void ReserveNoResize(ParallelWriter nativeList, Int32 length, out T*& ptr, out Int32& idx)
  IntPtr GetUnsafeIntPtr(NativeList`1 list)
  IntPtr GetUnsafeReadOnlyIntPtr(NativeList`1 list)
  Void Insert(NativeList`1 list, Int32 index, T item)
  Void ResizeInitialized(NativeList`1 list, Int32 length, Byte value)
  Void ResizeInitialized(NativeList`1 list, Int32 length)
  Void AddRange(NativeList`1 list, T[] array)
  Void AddRange(NativeList`1 list, IEnumerable`1 enumerable)
  Void ClearAddRange(NativeList`1 list, IEnumerable`1 enumerable)
  Void ClearAddRange(NativeList`1 list, NativeArray`1 array)
  Void ClearAddRange(NativeList`1 list, NativeHashSet`1 hashSet)
  Boolean Compare(NativeList`1 list, NativeHashSet`1 hashSet)
ReserveNoResize overloads: 2
  (NativeList`1 nativeList, Int32 length, T*& ptr, Int32& idx) -> Void
  (ParallelWriter nativeList, Int32 length, T*& ptr, Int32& idx) -> Void
Verified: 2 checks, 0 failures
```
NativeListExtensions
  Kind: static class
Methods:
  Void ReserveNoResize(NativeList`1 nativeList, Int32 length, out T*& ptr, out Int32& idx)
  Void ReserveNoResize(ParallelWriter nativeList, Int32 length, out T*& ptr, out Int32& idx)
  IntPtr GetUnsafeIntPtr(NativeList`1 list)
  IntPtr GetUnsafeReadOnlyIntPtr(NativeList`1 list)
  Void Insert(NativeList`1 list, Int32 index, T item)
  Void ResizeInitialized(NativeList`1 list, Int32 length, Byte value)
  Void ResizeInitialized(NativeList`1 list, Int32 length)
  Void AddRange(NativeList`1 list, T[] array)
  Void AddRange(NativeList`1 list, IEnumerable`1 enumerable)
  Void ClearAddRange(NativeList`1 list, IEnumerable`1 enumerable)
  Void ClearAddRange(NativeList`1 list, NativeArray`1 array)
  Void ClearAddRange(NativeList`1 list, NativeHashSet`1 hashSet)
  Boolean Compare(NativeList`1 list, NativeHashSet`1 hashSet)
ReserveNoResize overloads: 2
  (NativeList`1 nativeList, Int32 length, T*& ptr, Int32& idx) -> Void
  (ParallelWriter nativeList, Int32 length, T*& ptr, Int32& idx) -> Void
Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeListExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeListExtensions.cs)
