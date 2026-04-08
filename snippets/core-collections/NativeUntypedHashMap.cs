// Run: cat snippets/core-collections/NativeUntypedHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,Unity.Mathematics,System.Linq"
// Verifies: docs/NativeUntypedHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists, is a struct, generic on TKey ---
var mapType = typeof(BovineLabs.Core.Collections.NativeUntypedHashMap<int>);
t("NativeUntypedHashMap<int>: type exists", mapType != null);
t("NativeUntypedHashMap<int>: is a struct (ValueType)", mapType.IsValueType);

// --- Has expected methods ---
var addOrSet = mapType.GetMethod("AddOrSet");
t("NativeUntypedHashMap: has AddOrSet<TValue>", addOrSet != null);

var getOrAddRef = mapType.GetMethod("GetOrAddRef");
t("NativeUntypedHashMap: has GetOrAddRef<TValue>", getOrAddRef != null);

var tryGetValue = mapType.GetMethod("TryGetValue");
t("NativeUntypedHashMap: has TryGetValue<TValue>", tryGetValue != null);

var containsKey = mapType.GetMethod("ContainsKey");
t("NativeUntypedHashMap: has ContainsKey", containsKey != null);

var clear = mapType.GetMethod("Clear");
t("NativeUntypedHashMap: has Clear", clear != null);

// --- Properties ---
var isCreatedProp = mapType.GetProperty("IsCreated");
t("NativeUntypedHashMap: has IsCreated property", isCreatedProp != null);

var countProp = mapType.GetProperty("Count");
t("NativeUntypedHashMap: has Count property", countProp != null);

var capacityProp = mapType.GetProperty("Capacity");
t("NativeUntypedHashMap: has Capacity property", capacityProp != null);

var isEmptyProp = mapType.GetProperty("IsEmpty");
t("NativeUntypedHashMap: has IsEmpty property", isEmptyProp != null);

// --- Functional tests ---
// Constructor: NativeUntypedHashMap<TKey>(int capacity, AllocatorManager.AllocatorHandle allocator, int minGrowth = 256)
var map = new BovineLabs.Core.Collections.NativeUntypedHashMap<int>(64, Unity.Collections.Allocator.Temp);
t("NativeUntypedHashMap: IsCreated after construction", map.IsCreated);
t("NativeUntypedHashMap: IsEmpty after construction", map.IsEmpty);
t("NativeUntypedHashMap: Count == 0 after construction", map.Count == 0);

// AddOrSet with small value (int) - stored directly in Values array
map.AddOrSet<int>(1, 42);
t("NativeUntypedHashMap: Count == 1 after AddOrSet<int>", map.Count == 1);
t("NativeUntypedHashMap: IsEmpty false after add", !map.IsEmpty);

// AddOrSet with different type (float)
map.AddOrSet<float>(2, 3.14f);
t("NativeUntypedHashMap: Count == 2 after AddOrSet<float>", map.Count == 2);

// AddOrSet with larger type (float3)
map.AddOrSet<float3>(3, new float3(1, 2, 3));
t("NativeUntypedHashMap: Count == 3 after AddOrSet<float3>", map.Count == 3);

// TryGetValue for int
bool found = map.TryGetValue<int>(1, out int intVal);
t("NativeUntypedHashMap: TryGetValue<int>(1) found", found);
t("NativeUntypedHashMap: TryGetValue<int>(1) value == 42", intVal == 42);

// TryGetValue for float
found = map.TryGetValue<float>(2, out float floatVal);
t("NativeUntypedHashMap: TryGetValue<float>(2) found", found);
t("NativeUntypedHashMap: TryGetValue<float>(2) value == 3.14f", floatVal == 3.14f);

// TryGetValue for float3
found = map.TryGetValue<float3>(3, out float3 f3Val);
t("NativeUntypedHashMap: TryGetValue<float3>(3) found", found);
t("NativeUntypedHashMap: TryGetValue<float3>(3) value correct", f3Val.Equals(new float3(1, 2, 3)));

// ContainsKey
t("NativeUntypedHashMap: ContainsKey(1) true", map.ContainsKey(1));
t("NativeUntypedHashMap: ContainsKey(2) true", map.ContainsKey(2));
t("NativeUntypedHashMap: ContainsKey(3) true", map.ContainsKey(3));
t("NativeUntypedHashMap: ContainsKey(999) false", !map.ContainsKey(999));

// TryGetValue for missing key
found = map.TryGetValue<int>(999, out intVal);
t("NativeUntypedHashMap: TryGetValue<int>(999) not found", !found);

// AddOrSet updates existing key (same type)
map.AddOrSet<int>(1, 100);
found = map.TryGetValue<int>(1, out intVal);
t("NativeUntypedHashMap: AddOrSet updates existing key", found && intVal == 100);
t("NativeUntypedHashMap: Count unchanged after update", map.Count == 3);

// GetOrAddRef - add new
ref int refVal = ref map.GetOrAddRef<int>(10, 999);
t("NativeUntypedHashMap: GetOrAddRef new key returns correct value", refVal == 999);
refVal = 1234;
found = map.TryGetValue<int>(10, out intVal);
t("NativeUntypedHashMap: GetOrAddRef mutation visible", found && intVal == 1234);

// GetOrAddRef - existing key
ref int refVal2 = ref map.GetOrAddRef<int>(10, 0);
t("NativeUntypedHashMap: GetOrAddRef existing key returns current value", refVal2 == 1234);

// Clear
map.Clear();
t("NativeUntypedHashMap: Count == 0 after Clear", map.Count == 0);
t("NativeUntypedHashMap: IsEmpty after Clear", map.IsEmpty);
t("NativeUntypedHashMap: ContainsKey(1) false after Clear", !map.ContainsKey(1));

map.Dispose();
t("NativeUntypedHashMap: Dispose completed", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
