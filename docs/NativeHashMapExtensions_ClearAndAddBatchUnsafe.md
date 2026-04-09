# NativeHashMapExtensions.ClearAndAddBatchUnsafe

## Inner Workings Diagram

```
NativeHashMapExtensions.ClearAndAddBatchUnsafe
======================================================================
Namespace:  BovineLabs.Core.Extensions

Structure:
┌────────────────────────────────────────────────────────────────────┐
│ class                    NativeHashMapExtensions                   │
└────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
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
  Pre-populate: Count=2
  After ClearAndAddBatchUnsafe(keys=[10,20,30]): Count=3
  TryGetValue: [10]=1000, [20]=2000, [30]=3000
  Old key 1 found: False
  Second batch: Count=2, [50]=5000
Verified: 3 checks, 0 failures
```
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
  Pre-populate: Count=2
  After ClearAndAddBatchUnsafe(keys=[10,20,30]): Count=3
  TryGetValue: [10]=1000, [20]=2000, [30]=3000
  Old key 1 found: False
  Second batch: Count=2, [50]=5000
Verified: 3 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeHashMapExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeHashMapExtensions.cs)
