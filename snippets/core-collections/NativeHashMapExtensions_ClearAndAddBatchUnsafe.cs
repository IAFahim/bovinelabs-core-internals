// Run: cat snippets/core-collections/NativeHashMapExtensions_ClearAndAddBatchUnsafe.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,Unity.Collections,System.Linq"
// Verifies: docs/NativeHashMapExtensions_ClearAndAddBatchUnsafe.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Extension method exists ---
var extType = typeof(BovineLabs.Core.Extensions.NativeParallelHashMapExtensions);
t("NativeParallelHashMapExtensions: type exists", extType != null);

var clearAddBatchMethods = extType.GetMethods().Where(m => m.Name == "ClearAndAddBatchUnsafe").ToList();
t("NativeParallelHashMapExtensions: has ClearAndAddBatchUnsafe methods", clearAddBatchMethods.Count > 0);

// --- Functional test: ClearAndAddBatchUnsafe with NativeArrays ---
var map = new NativeParallelHashMap<int, int>(16, Unity.Collections.Allocator.Temp);

// Pre-populate with some data
map.TryAdd(1, 100);
map.TryAdd(2, 200);
t("ClearAndAddBatch: pre-populated, Count == 2", map.Count() == 2);

// Now clear and batch-add new data
var keys = new NativeArray<int>(3, Unity.Collections.Allocator.Temp);
var values = new NativeArray<int>(3, Unity.Collections.Allocator.Temp);
keys[0] = 10; values[0] = 1000;
keys[1] = 20; values[1] = 2000;
keys[2] = 30; values[2] = 3000;

map.ClearAndAddBatchUnsafe(keys, values);
t("ClearAndAddBatchUnsafe: Count == 3", map.Count() == 3);

// Verify all new values are accessible
bool found = map.TryGetValue(10, out int val);
t("ClearAndAddBatchUnsafe: TryGetValue(10) found", found);
t("ClearAndAddBatchUnsafe: TryGetValue(10) == 1000", val == 1000);

found = map.TryGetValue(20, out val);
t("ClearAndAddBatchUnsafe: TryGetValue(20) found", found);
t("ClearAndAddBatchUnsafe: TryGetValue(20) == 2000", val == 2000);

found = map.TryGetValue(30, out val);
t("ClearAndAddBatchUnsafe: TryGetValue(30) found", found);
t("ClearAndAddBatchUnsafe: TryGetValue(30) == 3000", val == 3000);

// Verify old keys are gone (map was cleared)
found = map.TryGetValue(1, out val);
t("ClearAndAddBatchUnsafe: old key 1 not found", !found);
found = map.TryGetValue(2, out val);
t("ClearAndAddBatchUnsafe: old key 2 not found", !found);

// --- Test second batch replace ---
var keys2 = new NativeArray<int>(2, Unity.Collections.Allocator.Temp);
var values2 = new NativeArray<int>(2, Unity.Collections.Allocator.Temp);
keys2[0] = 50; values2[0] = 5000;
keys2[1] = 60; values2[1] = 6000;

map.ClearAndAddBatchUnsafe(keys2, values2);
t("ClearAndAddBatchUnsafe: second batch, Count == 2", map.Count() == 2);

found = map.TryGetValue(50, out val);
t("ClearAndAddBatchUnsafe: second batch TryGetValue(50) found", found && val == 5000);
found = map.TryGetValue(60, out val);
t("ClearAndAddBatchUnsafe: second batch TryGetValue(60) found", found && val == 6000);

// Verify first batch keys gone
found = map.TryGetValue(10, out val);
t("ClearAndAddBatchUnsafe: first batch key 10 not found", !found);

map.Dispose();
keys.Dispose();
values.Dispose();
keys2.Dispose();
values2.Dispose();

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
