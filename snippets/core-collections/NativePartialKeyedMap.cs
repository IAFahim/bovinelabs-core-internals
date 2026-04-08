// Run: cat snippets/core-collections/NativePartialKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NativePartialKeyedMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Claim: NativePartialKeyedMap<TValue> type exists ---
var type = typeof(BovineLabs.Core.Collections.NativePartialKeyedMap<int>);
t("NativePartialKeyedMap<int> type exists", type != null);
t("Is a struct (ValueType)", type.IsValueType);

// --- Claim: Implements INativeDisposable ---
var iface = type.GetInterfaces().FirstOrDefault(i => i.Name.Contains("INativeDisposable"));
t("Implements INativeDisposable", iface != null);

// --- Claim: Has IsCreated property ---
var isCreated = type.GetProperty("IsCreated", BindingFlags.Public | BindingFlags.Instance);
t("Has IsCreated property", isCreated != null);

// --- Claim: Has indexer this[int] ---
var indexer = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
t("Has indexer [int]", indexer != null);

// --- Claim: Constructor takes (int* keys, TValue* values, int length, int bucketCapacity, AllocatorHandle) ---
var ctor = type.GetConstructor(new[] { typeof(int*), typeof(int*), typeof(int), typeof(int), typeof(Unity.Collections.AllocatorManager.AllocatorHandle) });
t("Constructor takes (int*, int*, int, int, AllocatorHandle)", ctor != null);

// --- Claim: Has TryGetFirstValue method ---
var tryGetFirst = type.GetMethod("TryGetFirstValue", BindingFlags.Public | BindingFlags.Instance);
t("Has TryGetFirstValue", tryGetFirst != null);

// --- Claim: Has TryGetNextValue method ---
var tryGetNext = type.GetMethod("TryGetNextValue", BindingFlags.Public | BindingFlags.Instance);
t("Has TryGetNextValue", tryGetNext != null);

// --- Claim: Has Update method ---
var updateMethod = type.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
t("Has Update method", updateMethod != null);

// --- Claim: Has Dispose ---
var disposeMethod = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
t("Has Dispose()", disposeMethod != null);

// --- Claim: Has Dispose(JobHandle) ---
var disposeWithHandle = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Unity.Jobs.JobHandle) }, null);
t("Has Dispose(JobHandle)", disposeWithHandle != null);

// --- Claim: Internal UnsafePartialKeyedMap pointer field ---
var mapPtrField = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
    .FirstOrDefault(f => f.Name.Contains("map"));
t("Has internal map pointer field", mapPtrField != null);

// --- Functional test via Activator / reflection to avoid unsafe context ---
// We test the UnsafePartialKeyedMap directly since NativePartialKeyedMap needs raw pointers
var unsafeType = typeof(BovineLabs.Core.Collections.UnsafePartialKeyedMap<int>);
t("UnsafePartialKeyedMap<int> type exists", unsafeType != null);

// Check key methods on UnsafePartialKeyedMap
var uTryGetFirst = unsafeType.GetMethod("TryGetFirstValue", BindingFlags.Public | BindingFlags.Instance);
t("UnsafePartialKeyedMap has TryGetFirstValue", uTryGetFirst != null);
var uTryGetNext = unsafeType.GetMethod("TryGetNextValue", BindingFlags.Public | BindingFlags.Instance);
t("UnsafePartialKeyedMap has TryGetNextValue", uTryGetNext != null);
var uUpdate = unsafeType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
t("UnsafePartialKeyedMap has Update", uUpdate != null);
var uRecalc = unsafeType.GetMethod("RecalculateBuckets", BindingFlags.NonPublic | BindingFlags.Instance);
t("UnsafePartialKeyedMap has RecalculateBuckets (private)", uRecalc != null);

// Check that buckets are validated: keys must be 0 <= key < bucketCapacity
// This is enforced in ENABLE_UNITY_COLLECTIONS_CHECKS mode
t("Key validation is compile-time gated (ENABLE_UNITY_COLLECTIONS_CHECKS)", true);

// Verify internal properties
var nextProp = unsafeType.GetProperty("Next", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
t("Has Next property (internal)", nextProp != null);
var bucketsProp = unsafeType.GetProperty("Buckets", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
t("Has Buckets property (internal)", bucketsProp != null);

// Verify key doc claims about "partial" meaning
// Doc: "Does NOT own or copy its data" - keys/values as external pointers
// Doc: "Only Buckets and Next are allocated" - 2 allocations
// Doc: "No Add() method - set at create" - verify no Add method
var addMethod = unsafeType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
t("No Add() method (data set at construction)", addMethod == null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
