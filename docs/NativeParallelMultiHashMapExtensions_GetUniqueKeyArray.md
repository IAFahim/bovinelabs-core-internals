# NativeParallelMultiHashMapExtensions.GetUniqueKeyArray

## Inner Workings Diagram

```
 NativeParallelMultiHashMapExtensions.GetUniqueKeyArray
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    NativeParallelMultiHashMapExtensions      │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
NativeParallelMultiHashMapExtensions
  Kind: static class
Methods:
  Void GetUniqueKeyArray(NativeParallelMultiHashMap`2, NativeList`1)
  Void GetUniqueKeyArray(ReadOnly, NativeList`1)
  Int32 Reserve(ParallelWriter, Int32)
  Boolean TryReserve(ParallelWriter, Int32, out Int32&)
  UnsafeParallelHashMapBucketData GetUnsafeBucketData(ParallelWriter)
  Void ClearLengthBuckets(NativeParallelMultiHashMap`2)
  Void ClearAndAddBatch(NativeParallelMultiHashMap`2, NativeArray`1, NativeArray`1)
  Void ClearAndAddBatch(NativeParallelMultiHashMap`2, NativeSlice`1, NativeSlice`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, TKey*, TValue*, Int32)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeSlice`1, NativeSlice`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, TKey, TValue*, Int32)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeArray`1, TValue)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeSlice`1, TValue)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, TKey, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, NativeSlice`1)
  Void AddBatchUnsafe(ParallelWriter, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, TKey*, TValue*, Int32)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, TKey*, Int32)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, TValue)
  Void RecalculateBuckets(NativeParallelMultiHashMap`2)
  Void Add(NativeParallelMultiHashMap`2, TKey, TValue, Int32)
  Void RecalculateBucketsCached(NativeParallelMultiHashMap`2)
  Void SetAllocatedIndexLength(NativeParallelMultiHashMap`2, Int32)
  TKey FirstKey(NativeParallelMultiHashMap`2)
  Boolean TryGetFirstKeyValue(NativeParallelMultiHashMap`2, TKey, out TKey&, out TValue&, out NativeParallelMultiHashMapIterator`1&)
  Boolean TryGetNextKeyValue(NativeParallelMultiHashMap`2, out TKey&, out TValue&, NativeParallelMultiHashMapIterator`1&)
Runtime Behavior:
  Map populated: keys=[1,1,2,3,2] (5 entries)
  GetUniqueKeyArray: Count=3
  Manual unique key count: 3
Verified: 5 checks, 0 failures
```
NativeParallelMultiHashMapExtensions
  Kind: static class
Methods:
  Void GetUniqueKeyArray(NativeParallelMultiHashMap`2, NativeList`1)
  Void GetUniqueKeyArray(ReadOnly, NativeList`1)
  Int32 Reserve(ParallelWriter, Int32)
  Boolean TryReserve(ParallelWriter, Int32, out Int32&)
  UnsafeParallelHashMapBucketData GetUnsafeBucketData(ParallelWriter)
  Void ClearLengthBuckets(NativeParallelMultiHashMap`2)
  Void ClearAndAddBatch(NativeParallelMultiHashMap`2, NativeArray`1, NativeArray`1)
  Void ClearAndAddBatch(NativeParallelMultiHashMap`2, NativeSlice`1, NativeSlice`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, TKey*, TValue*, Int32)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeSlice`1, NativeSlice`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, TKey, TValue*, Int32)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeArray`1, TValue)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, NativeSlice`1, TValue)
  Void AddBatchUnsafe(NativeParallelMultiHashMap`2, TKey, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, NativeSlice`1)
  Void AddBatchUnsafe(ParallelWriter, NativeSlice`1, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, TKey*, TValue*, Int32)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1)
  Void AddBatchUnsafe(ParallelWriter, TKey*, Int32)
  Void AddBatchUnsafe(ParallelWriter, NativeArray`1, TValue)
  Void RecalculateBuckets(NativeParallelMultiHashMap`2)
  Void Add(NativeParallelMultiHashMap`2, TKey, TValue, Int32)
  Void RecalculateBucketsCached(NativeParallelMultiHashMap`2)
  Void SetAllocatedIndexLength(NativeParallelMultiHashMap`2, Int32)
  TKey FirstKey(NativeParallelMultiHashMap`2)
  Boolean TryGetFirstKeyValue(NativeParallelMultiHashMap`2, TKey, out TKey&, out TValue&, out NativeParallelMultiHashMapIterator`1&)
  Boolean TryGetNextKeyValue(NativeParallelMultiHashMap`2, out TKey&, out TValue&, NativeParallelMultiHashMapIterator`1&)
Runtime Behavior:
  Map populated: keys=[1,1,2,3,2] (5 entries)
  GetUniqueKeyArray: Count=3
  Manual unique key count: 3
Verified: 5 checks, 0 failures
```

## Source

- [BovineLabs.Core/Extensions/NativeParallelMultiHashMapExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/NativeParallelMultiHashMapExtensions.cs)
