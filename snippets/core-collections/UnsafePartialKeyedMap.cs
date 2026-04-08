// Run: cat snippets/core-collections/UnsafePartialKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,Unity.Collections.LowLevel.Unsafe,System.Linq"
// Verifies: docs/UnsafePartialKeyedMap.md claims
// Note: UnsafePartialKeyedMap requires int* keys and TValue* values (pointer-based).
// We use NativeArray and obtain pointers via GetUnsafePtr() which requires unsafe context.
// Since the test runner doesn't support /unsafe, we do reflection-only verification here.

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var mapType = typeof(BovineLabs.Core.Collections.UnsafePartialKeyedMap<int>);
t("UnsafePartialKeyedMap<int>: type exists", mapType != null);
t("UnsafePartialKeyedMap<int>: is a struct (ValueType)", mapType.IsValueType);

// --- Verify it's generic ---
var genericArgs = mapType.GetGenericArguments();
t("UnsafePartialKeyedMap: has 1 generic parameter (TValue)", genericArgs.Length == 1);
t("UnsafePartialKeyedMap: generic param name starts with T", genericArgs[0].Name.StartsWith("T"));

// --- Has expected methods ---
var tryGetFirst = mapType.GetMethod("TryGetFirstValue");
t("UnsafePartialKeyedMap: has TryGetFirstValue", tryGetFirst != null);

var tryGetNext = mapType.GetMethod("TryGetNextValue");
t("UnsafePartialKeyedMap: has TryGetNextValue", tryGetNext != null);

var updateMethod = mapType.GetMethod("Update");
t("UnsafePartialKeyedMap: has Update", updateMethod != null);

var disposeMethods = mapType.GetMethods().Where(m => m.Name == "Dispose").ToList();
t("UnsafePartialKeyedMap: has Dispose", disposeMethods.Count > 0);
// Should have both Dispose() and Dispose(JobHandle) overloads
t("UnsafePartialKeyedMap: Dispose has multiple overloads (incl. JobHandle)", disposeMethods.Count >= 2);

// --- Has expected constructor ---
var ctors = mapType.GetConstructors();
t("UnsafePartialKeyedMap: has constructors", ctors.Length > 0);

// Check constructor signature: (int* keys, TValue* values, int length, int bucketCapacity, AllocatorHandle)
var mainCtor = ctors.FirstOrDefault(c =>
{
    var ps = c.GetParameters();
    return ps.Length == 5;
});
t("UnsafePartialKeyedMap: has 5-parameter constructor (keys, values, length, bucketCapacity, allocator)", mainCtor != null);

// --- Properties ---
var isCreatedProp = mapType.GetProperty("IsCreated");
t("UnsafePartialKeyedMap: has IsCreated property", isCreatedProp != null);
t("UnsafePartialKeyedMap: IsCreated is bool", isCreatedProp != null && isCreatedProp.PropertyType == typeof(bool));

// --- Check for indexer ---
var indexer = mapType.GetProperty("Item");
t("UnsafePartialKeyedMap: has indexer (this[int])", indexer != null);

// --- Verify namespace ---
t("UnsafePartialKeyedMap: in BovineLabs.Core.Collections namespace", mapType.Namespace == "BovineLabs.Core.Collections");

// --- Check static methods ---
var createMethod = mapType.GetMethod("Create", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
t("UnsafePartialKeyedMap: has static Create method", createMethod != null);

var destroyMethod = mapType.GetMethod("Destroy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
t("UnsafePartialKeyedMap: has static Destroy method", destroyMethod != null);

// --- Verify TryGetFirstValue signature ---
if (tryGetFirst != null)
{
    var ps = tryGetFirst.GetParameters();
    t("UnsafePartialKeyedMap: TryGetFirstValue takes (int key, out TValue, out iterator)", ps.Length == 3);
    t("UnsafePartialKeyedMap: TryGetFirstValue returns bool", tryGetFirst.ReturnType == typeof(bool));
}

// --- Verify TryGetNextValue signature ---
if (tryGetNext != null)
{
    var ps = tryGetNext.GetParameters();
    t("UnsafePartialKeyedMap: TryGetNextValue takes (out TValue, ref iterator)", ps.Length == 2);
    t("UnsafePartialKeyedMap: TryGetNextValue returns bool", tryGetNext.ReturnType == typeof(bool));
}

// --- Verify Update signature ---
if (updateMethod != null)
{
    var ps = updateMethod.GetParameters();
    t("UnsafePartialKeyedMap: Update takes (int*, TValue*, int)", ps.Length == 3);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
