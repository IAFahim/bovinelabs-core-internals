// Run: cat snippets/core-collections/NativeParallelMultiHashMapFallback.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq,Unity.Collections,Unity.Jobs"
// Verifies: docs/NativeParallelMultiHashMapFallback.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var type = typeof(BovineLabs.Core.Collections.NativeParallelMultiHashMapFallback<int, int>);
t("NativeParallelMultiHashMapFallback<int,int> type exists", type != null);
t("Is a struct (ValueType)", type.IsValueType);

// --- Claim: Implements IDisposable ---
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(type));

// --- Claim: Has public HashMap and Fallback fields ---
var hashMapField = type.GetField("HashMap");
t("Has public HashMap field", hashMapField != null && hashMapField.IsPublic);
if (hashMapField != null)
    t("HashMap is NativeParallelMultiHashMap", hashMapField.FieldType.Name.StartsWith("NativeParallelMultiHashMap"));

var fallbackField = type.GetField("Fallback");
t("Has public Fallback field", fallbackField != null && fallbackField.IsPublic);
if (fallbackField != null)
    t("Fallback is NativeQueue", fallbackField.FieldType.Name.StartsWith("NativeQueue"));

// --- Claim: Constructor takes (int capacity, Allocator allocator) ---
var ctor = type.GetConstructor(new[] { typeof(int), typeof(Unity.Collections.Allocator) });
t("Constructor takes (int, Allocator)", ctor != null);

// --- Claim: Has AsWriter method ---
var asWriter = type.GetMethod("AsWriter", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
t("Has AsWriter() method", asWriter != null);

// --- Claim: Has Dispose() ---
var disposeNoParam = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
t("Has Dispose()", disposeNoParam != null);

// --- Claim: Has Dispose(JobHandle) ---
var disposeWithDep = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Unity.Jobs.JobHandle) }, null);
t("Has Dispose(JobHandle)", disposeWithDep != null);

// --- Claim: Has Clear() (no-param version) ---
var clearNoParam = type.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
t("Has Clear() (no params)", clearNoParam != null);

// --- Claim: Has Clear(JobHandle, ...) overload with default param ---
var clearMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "Clear").ToList();
t("Has Clear methods (at least 1)", clearMethods.Count >= 1);
r.Add($"INFO: Clear method overloads: {clearMethods.Count}");

// --- Claim: Has Apply method ---
var applyMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "Apply").ToList();
t("Has Apply() method", applyMethods.Count > 0);

// --- Claim: ParallelWriter nested type exists ---
var pwType = type.GetNestedType("ParallelWriter");
t("ParallelWriter nested type exists", pwType != null);
if (pwType != null)
{
    t("ParallelWriter is struct", pwType.IsValueType);

    var addMethods = pwType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "Add").ToList();
    t("ParallelWriter has Add methods", addMethods.Count >= 2); // Add(key, item) and Add(key, item, hash)

    var addBatchMethods = pwType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "AddBatch").ToList();
    t("ParallelWriter has AddBatch methods", addBatchMethods.Count >= 1);
}

// --- Claim: ApplyJob nested type exists ---
var applyJobType = type.GetNestedType("ApplyJob");
t("ApplyJob nested type exists", applyJobType != null);
if (applyJobType != null)
{
    t("ApplyJob is struct", applyJobType.IsValueType);
    var ijobIface = applyJobType.GetInterfaces().FirstOrDefault(i => i.Name == "IJob");
    t("ApplyJob implements IJob", ijobIface != null);
}

// --- Claim: FallbackData nested type exists ---
var fbDataType = type.GetNestedType("FallbackData");
t("FallbackData nested type exists", fbDataType != null);
if (fbDataType != null)
{
    t("FallbackData is struct", fbDataType.IsValueType);
    var fbFields = fbDataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
    t("FallbackData has Key field", fbFields.Any(f => f.Name == "Key"));
    t("FallbackData has Value field", fbFields.Any(f => f.Name == "Value"));
    t("FallbackData has Hash field", fbFields.Any(f => f.Name == "Hash"));
    t("FallbackData has exactly 3 fields", fbFields.Length == 3);
}

// --- Functional test: create, write, apply, read ---
var map = new BovineLabs.Core.Collections.NativeParallelMultiHashMapFallback<int, int>(10, Unity.Collections.Allocator.TempJob);
t("Construction succeeds", true);

var writer = map.AsWriter();
writer.Add(1, 100);
writer.Add(2, 200);
writer.Add(1, 300);
t("Added 3 entries via writer", true);

// Apply: merge fallback into hashmap
var handle = map.Apply(default(Unity.Jobs.JobHandle), out var reader);
handle.Complete();
t("Apply succeeds", true);

// Read back using TryGetFirstValue (ReadOnly uses same API as multi hash map)
// Note: multi-hashmaps iterate in reverse insertion order per key, so key=1 first returns 300, then 100
bool found = reader.TryGetFirstValue(1, out int val, out var it);
t("TryGetFirstValue(1) found", found);
t("TryGetFirstValue(1) value == 300 (last added)", found && val == 300);
bool hasNext = reader.TryGetNextValue(out int val2, ref it);
t("TryGetNextValue(1) value == 100 (first added)", hasNext && val2 == 100);

found = reader.TryGetFirstValue(2, out val, out it);
t("TryGetFirstValue(2) found", found);
t("TryGetFirstValue(2) value == 200", found && val == 200);

map.Dispose();
t("Dispose succeeds", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
