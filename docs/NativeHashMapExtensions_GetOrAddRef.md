# NativeHashMapExtensions.GetOrAddRef

## Inner Workings Diagram

```
 NativeHashMapExtensions.GetOrAddRef
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeHashMapExtensions                   │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
NativeHashMapExtensions
  Kind: static class
Methods:
  TValue& GetOrAddRef(NativeHashMap`2, TKey, TValue)
  TValue& GetRef(NativeHashMap`2, TKey)
  TValue& GetRefNoSafety(NativeHashMap`2, TKey)
  Boolean Remove(NativeHashMap`2, TKey, out TValue&)
  Boolean TryGetIndex(NativeHashMap`2, TKey, out Int32&)
  TValue ReadIndexUnsafe(NativeHashMap`2, Int32)
  Boolean TryGetIndex(ReadOnly, TKey, out Int32&)
  TValue ReadIndexUnsafe(ReadOnly, Int32)
NativeParallelHashMapExtensions
  Kind: static class
Methods:
  Int32 Reserve(ParallelWriter, Int32)
  UnsafeParallelHashMapBucketData GetUnsafeBucketData(ParallelWriter)
  TValue& GetOrAddRef(NativeParallelHashMap`2, TKey, TValue)
  TValue& GetOrAddRef(ParallelWriter, TKey, TValue)
  TValue& GetRef(ParallelWriter, TKey)
  Void ClearLengthBuckets(NativeParallelHashMap`2)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, NativeArray`1, NativeArray`1)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, TKey[], TValue[], Int32)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, TKey*, TValue*, Int32)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, TKey*, TValue*, Int32)
  Void AddBatchUnsafe(NativeParallelHashMap`2, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, TKey*, Int32)
  Void ClearAndAddKeyBatchUnsafe(NativeParallelHashMap`2, NativeArray`1)
  Void ClearAndAddKeyBatchUnsafe(NativeParallelHashMap`2, TKey*, Int32)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, TKey*, TValue*, Int32)
  Void RecalculateBuckets(NativeParallelHashMap`2)
  Void SetLengthUnsafe(NativeParallelHashMap`2, Int32)
  Int32 GetLengthUnsafe(NativeParallelHashMap`2)
  TValue& GetValueByRef(NativeParallelHashMap`2, TKey)
  TKey FirstKey(NativeParallelHashMap`2)
  Boolean TryGetFirstKeyValue(NativeParallelHashMap`2, out TKey&, out TValue&)
  Boolean TryGetFirstKeyValue(NativeParallelHashMap`2, out TKey&, out TValue&, Int32&)
Runtime Behavior:
  GetOrAddRef(1, 42) on empty: returns 42
  Mutate via ref: TryGetValue(1)=100
  GetOrAddRef(1, 999) on existing: returns 100 (not overwritten)
  GetOrAddRef(2) default: returns 0
  Count=2
Verified: 4 checks, 0 failures
```
NativeHashMapExtensions
  Kind: static class
Methods:
  TValue& GetOrAddRef(NativeHashMap`2, TKey, TValue)
  TValue& GetRef(NativeHashMap`2, TKey)
  TValue& GetRefNoSafety(NativeHashMap`2, TKey)
  Boolean Remove(NativeHashMap`2, TKey, out TValue&)
  Boolean TryGetIndex(NativeHashMap`2, TKey, out Int32&)
  TValue ReadIndexUnsafe(NativeHashMap`2, Int32)
  Boolean TryGetIndex(ReadOnly, TKey, out Int32&)
  TValue ReadIndexUnsafe(ReadOnly, Int32)
NativeParallelHashMapExtensions
  Kind: static class
Methods:
  Int32 Reserve(ParallelWriter, Int32)
  UnsafeParallelHashMapBucketData GetUnsafeBucketData(ParallelWriter)
  TValue& GetOrAddRef(NativeParallelHashMap`2, TKey, TValue)
  TValue& GetOrAddRef(ParallelWriter, TKey, TValue)
  TValue& GetRef(ParallelWriter, TKey)
  Void ClearLengthBuckets(NativeParallelHashMap`2)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, NativeArray`1, NativeArray`1)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, TKey[], TValue[], Int32)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, TKey*, TValue*, Int32)
  Void ClearAndAddBatchUnsafe(NativeParallelHashMap`2, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, TKey*, TValue*, Int32)
  Void AddBatchUnsafe(NativeParallelHashMap`2, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelHashMap`2, TKey*, Int32)
  Void ClearAndAddKeyBatchUnsafe(NativeParallelHashMap`2, NativeArray`1)
  Void ClearAndAddKeyBatchUnsafe(NativeParallelHashMap`2, TKey*, Int32)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, TKey*, TValue*, Int32)
  Void RecalculateBuckets(NativeParallelHashMap`2)
  Void SetLengthUnsafe(NativeParallelHashMap`2, Int32)
  Int32 GetLengthUnsafe(NativeParallelHashMap`2)
  TValue& GetValueByRef(NativeParallelHashMap`2, TKey)
  TKey FirstKey(NativeParallelHashMap`2)
  Boolean TryGetFirstKeyValue(NativeParallelHashMap`2, out TKey&, out TValue&)
  Boolean TryGetFirstKeyValue(NativeParallelHashMap`2, out TKey&, out TValue&, Int32&)
Runtime Behavior:
  GetOrAddRef(1, 42) on empty: returns 42
  Mutate via ref: TryGetValue(1)=100
  GetOrAddRef(1, 999) on existing: returns 100 (not overwritten)
  GetOrAddRef(2) default: returns 0
  Count=2
Verified: 4 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeHashMapExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeHashMapExtensions.cs)
