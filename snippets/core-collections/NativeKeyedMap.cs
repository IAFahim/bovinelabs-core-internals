// Run: cat snippets/core-collections/NativeKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,System.Linq"
// Verifies: docs/NativeKeyedMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists and is a struct ---
var mapType = typeof(BovineLabs.Core.Collections.NativeKeyedMap<int>);
t("NativeKeyedMap<int>: type exists", mapType != null);
t("NativeKeyedMap<int>: is a struct (ValueType)", mapType.IsValueType);

// --- Has expected methods ---
var tryGetFirst = mapType.GetMethod("TryGetFirstValue");
t("NativeKeyedMap: has TryGetFirstValue method", tryGetFirst != null);

var tryGetNext = mapType.GetMethod("TryGetNextValue");
t("NativeKeyedMap: has TryGetNextValue method", tryGetNext != null);

var addMethod = mapType.GetMethod("Add");
t("NativeKeyedMap: has Add method", addMethod != null);

var clearMethod = mapType.GetMethod("Clear");
t("NativeKeyedMap: has Clear method", clearMethod != null);

var recalcMethod = mapType.GetMethod("RecalculateBuckets");
t("NativeKeyedMap: has RecalculateBuckets method", recalcMethod != null);

var disposeMethod = mapType.GetMethods().FirstOrDefault(m => m.Name == "Dispose");
t("NativeKeyedMap: has Dispose method", disposeMethod != null);

// --- Properties ---
var isCreatedProp = mapType.GetProperty("IsCreated");
t("NativeKeyedMap: has IsCreated property", isCreatedProp != null);

var capacityProp = mapType.GetProperty("Capacity");
t("NativeKeyedMap: has Capacity property", capacityProp != null);

// --- Constructor: tested functionally below ---
t("NativeKeyedMap: ctor exists (tested functionally)", true);

// --- Functional test: create, add, try-get ---
var map = new BovineLabs.Core.Collections.NativeKeyedMap<int>(4, 10, Unity.Collections.Allocator.Temp);
t("NativeKeyedMap: IsCreated after construction", map.IsCreated);

map.Add(3, 100);
map.Add(5, 200);

// TryGetFirstValue for existing key
bool found = map.TryGetFirstValue(3, out int val, out var it);
t("NativeKeyedMap: TryGetFirstValue(3) found", found == true);
t("NativeKeyedMap: TryGetFirstValue(3) value == 100", val == 100);

// TryGetFirstValue for non-existing key
found = map.TryGetFirstValue(7, out val, out it);
t("NativeKeyedMap: TryGetFirstValue(7) not found", found == false);

// Multi-value per key (supports multiple values per key like NativeMultiHashMap)
map.Add(5, 300);

found = map.TryGetFirstValue(5, out val, out it);
t("NativeKeyedMap: TryGetFirstValue(5) found (multi)", found == true);

// Iterate all values for key=5
var values5 = new System.Collections.Generic.List<int>();
if (found)
{
    values5.Add(val);
    while (map.TryGetNextValue(out val, ref it))
    {
        values5.Add(val);
    }
}
t("NativeKeyedMap: key=5 has 2 values", values5.Count == 2);
t("NativeKeyedMap: key=5 values contain 200", values5.Contains(200));
t("NativeKeyedMap: key=5 values contain 300", values5.Contains(300));

// Clear
map.Clear();
found = map.TryGetFirstValue(3, out val, out it);
t("NativeKeyedMap: Clear() -> TryGetFirstValue(3) not found", found == false);
found = map.TryGetFirstValue(5, out val, out it);
t("NativeKeyedMap: Clear() -> TryGetFirstValue(5) not found", found == false);

map.Dispose();

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
