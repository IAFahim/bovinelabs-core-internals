// Run: cat snippets/core-collections/NativePerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,Unity.Collections,System.Linq,Unity.Burst"
// Verifies: docs/NativePerfectHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var hmType = typeof(BovineLabs.Core.Collections.NativePerfectHashMap<int,int>);
t("NativePerfectHashMap<int,int>: type exists", hmType != null);
t("NativePerfectHashMap<int,int>: is ValueType", hmType.IsValueType);

// --- Has expected methods ---
var tryGetValue = hmType.GetMethod("TryGetValue");
t("NativePerfectHashMap: has TryGetValue", tryGetValue != null);
if (tryGetValue != null)
{
    t("NativePerfectHashMap: TryGetValue returns bool", tryGetValue.ReturnType == typeof(bool));
    t("NativePerfectHashMap: TryGetValue takes 2 params", tryGetValue.GetParameters().Length == 2);
}

t("NativePerfectHashMap: has Dispose", hmType.GetMethod("Dispose", new Type[0]) != null);

var isCreatedProp = hmType.GetProperty("IsCreated");
t("NativePerfectHashMap: has IsCreated", isCreatedProp != null);
t("NativePerfectHashMap: IsCreated is bool", isCreatedProp?.PropertyType == typeof(bool));
t("NativePerfectHashMap: has indexer (Item)", hmType.GetProperty("Item") != null);

// --- Constructor signature ---
var ctors = hmType.GetConstructors();
t("NativePerfectHashMap: has at least 1 ctor", ctors.Length >= 1);
if (ctors.Length > 0)
{
    var cp = ctors[0].GetParameters();
    t("NativePerfectHashMap: ctor has 4 params", cp.Length == 4);
    t("NativePerfectHashMap: ctor param0 is NativeArray", cp[0].ParameterType.Name.StartsWith("NativeArray"));
    t("NativePerfectHashMap: ctor param1 is NativeArray", cp[1].ParameterType.Name.StartsWith("NativeArray"));
    t("NativePerfectHashMap: ctor param2 is int (nullValue)", cp[2].ParameterType == typeof(int));
}

// --- Functional test ---
var keys = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
var values = new NativeArray<int>(new[] { 100, 200, 300 }, Allocator.Temp);
var map = new BovineLabs.Core.Collections.NativePerfectHashMap<int,int>(keys, values, -1, Allocator.Temp);
t("NativePerfectHashMap: IsCreated after construction", map.IsCreated);

bool found = map.TryGetValue(10, out int val);
t("NativePerfectHashMap: TryGetValue(10) found", found == true);
t("NativePerfectHashMap: TryGetValue(10) == 100", val == 100);

found = map.TryGetValue(20, out val);
t("NativePerfectHashMap: TryGetValue(20) found", found == true);
t("NativePerfectHashMap: TryGetValue(20) == 200", val == 200);

found = map.TryGetValue(30, out val);
t("NativePerfectHashMap: TryGetValue(30) found", found == true);
t("NativePerfectHashMap: TryGetValue(30) == 300", val == 300);

found = map.TryGetValue(999, out val);
t("NativePerfectHashMap: TryGetValue(999) not found", found == false);

int idxVal = map[10];
t("NativePerfectHashMap: map[10] == 100", idxVal == 100);
idxVal = map[20];
t("NativePerfectHashMap: map[20] == 200", idxVal == 200);

map[20] = 250;
t("NativePerfectHashMap: map[20] == 250 after set", map[20] == 250);
t("NativePerfectHashMap: map[10] still == 100", map[10] == 100);

// --- Test with 4 keys ---
var keys2 = new NativeArray<int>(new[] { 5, 15, 25, 35 }, Allocator.Temp);
var vals2 = new NativeArray<int>(new[] { 50, 150, 250, 350 }, Allocator.Temp);
var map2 = new BovineLabs.Core.Collections.NativePerfectHashMap<int,int>(keys2, vals2, -1, Allocator.Temp);
t("NativePerfectHashMap: 4-key map created", map2.IsCreated);
found = map2.TryGetValue(25, out val);
t("NativePerfectHashMap: TryGetValue(25) == 250", found && val == 250);
found = map2.TryGetValue(35, out val);
t("NativePerfectHashMap: TryGetValue(35) == 350", found && val == 350);
// Key 100 maps to an empty slot (different hash from 5,15,25,35)
found = map2.TryGetValue(100, out val);
t("NativePerfectHashMap: TryGetValue(100) not found (empty slot)", found == false);

map2.Dispose();
map.Dispose();
keys.Dispose();
values.Dispose();
keys2.Dispose();
vals2.Dispose();
t("NativePerfectHashMap: all disposed ok", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
