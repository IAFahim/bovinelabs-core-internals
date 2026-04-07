// ============================================================================
// TEST: DynamicHashMapHelper + DynamicHashSet + UnsafeDynamicHashMapExtensions
// Branches: topic/DynamicHashMapHelper, topic/DynamicHashMapBatch,
//           topic/DynamicHashMapMulti, topic/DynamicHashSet,
//           topic/DynamicHashSetExtensions
// Sources: BovineLabs.Core/Iterators/*.cs
// Run: cat 12_Iterators.cs | unity-cli exec --usings "BovineLabs.Core.Iterators,Unity.Collections,Unity.Mathematics,System.Linq"
// ============================================================================
// DynamicHashMapHelper<T>: unsafe struct for low-level DynamicBuffer-backed hash map.
// Internal layout fields: ValuesOffset, KeysOffset, NextOffset, BucketsOffset,
//   Count, Capacity, BucketCapacityMask, Log2MinGrowth, AllocatedIndex, FirstFreeIdx.
// DynamicHashSet<T>: hash set backed by DynamicBuffer.
// UnsafeDynamicHashMapExtensions: static helper for untyped iteration.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- DynamicHashMapHelper<T> ---
var dhmh = typeof(DynamicHashMapHelper<>);
t("DynamicHashMapHelper<> exists", dhmh != null);
t("DynamicHashMapHelper is struct", dhmh.IsValueType);
var dhmhFields = dhmh.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var fieldNames = dhmhFields.Select(f => f.Name).ToList();
t("DynamicHashMapHelper has ValuesOffset", fieldNames.Contains("ValuesOffset"));
t("DynamicHashMapHelper has KeysOffset", fieldNames.Contains("KeysOffset"));
t("DynamicHashMapHelper has NextOffset", fieldNames.Contains("NextOffset"));
t("DynamicHashMapHelper has BucketsOffset", fieldNames.Contains("BucketsOffset"));
t("DynamicHashMapHelper has Capacity", fieldNames.Contains("Capacity"));
t("DynamicHashMapHelper has Count", fieldNames.Contains("Count"));
t("DynamicHashMapHelper has BucketCapacityMask", fieldNames.Contains("BucketCapacityMask"));
t("DynamicHashMapHelper has FirstFreeIdx", fieldNames.Contains("FirstFreeIdx"));
t("DynamicHashMapHelper has SizeOfTValue", fieldNames.Contains("SizeOfTValue"));

// --- DynamicHashMap<K,V> ---
t("DynamicHashMap<,> exists", typeof(DynamicHashMap<,>) != null);

// --- DynamicHashSet<T> ---
t("DynamicHashSet<> exists", typeof(DynamicHashSet<>) != null);
t("DynamicHashSet is struct", typeof(DynamicHashSet<>).IsValueType);

// --- IDynamicHashMap<K,V> ---
t("IDynamicHashMap<,> exists", typeof(IDynamicHashMap<,>) != null);

// --- IDynamicHashSet<T> ---
t("IDynamicHashSet<> exists", typeof(IDynamicHashSet<>) != null);

// --- UnsafeDynamicHashMapExtensions ---
var udhmeMethods = typeof(UnsafeDynamicHashMapExtensions).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
t("UnsafeDynamicHashMapExtensions has GetUntypedIterator", udhmeMethods.Any(m => m.Name == "GetUntypedIterator"));

// --- UntypedDynamicHashMapHelper ---
t("UntypedDynamicHashMapHelper exists", typeof(UntypedDynamicHashMapHelper) != null);

// --- UntypedDynamicHashMapIterator ---
t("UntypedDynamicHashMapIterator exists", typeof(UntypedDynamicHashMapIterator) != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
