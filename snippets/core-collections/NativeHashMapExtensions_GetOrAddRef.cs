// Run: cat snippets/core-collections/NativeHashMapExtensions_GetOrAddRef.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,Unity.Collections,Unity.Collections.LowLevel.Unsafe,System.Linq,System.Reflection"
// Verifies: docs/NativeHashMapExtensions_GetOrAddRef.md claims
// Note: NativeHashMap is an alias for NativeParallelHashMap in current Unity versions.
// GetOrAddRef is available on both NativeHashMapExtensions (extends NativeHashMap)
// and NativeParallelHashMapExtensions (extends NativeParallelHashMap).
// NativeHashMap throws NotImplementedException in this test environment, so we test
// the NativeParallelHashMapExtensions.GetOrAddRef variant which works.

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- NativeHashMapExtensions type exists ---
var extType1 = typeof(BovineLabs.Core.Extensions.NativeHashMapExtensions);
t("NativeHashMapExtensions: type exists", extType1 != null);

var getOrAddRef1 = extType1.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.Name == "GetOrAddRef");
t("NativeHashMapExtensions: has GetOrAddRef", getOrAddRef1 != null);

// --- NativeParallelHashMapExtensions also has GetOrAddRef ---
var extType2 = typeof(BovineLabs.Core.Extensions.NativeParallelHashMapExtensions);
t("NativeParallelHashMapExtensions: type exists", extType2 != null);

var getOrAddRef2 = extType2.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.Name == "GetOrAddRef");
t("NativeParallelHashMapExtensions: has GetOrAddRef", getOrAddRef2 != null);

// --- Functional test using NativeParallelHashMap ---
var map = new NativeParallelHashMap<int, int>(16, Unity.Collections.Allocator.Temp);
t("NativeParallelHashMap: created", true);

// GetOrAddRef for new key - should add with default value
ref int refVal = ref map.GetOrAddRef(1, 42);
t("GetOrAddRef: new key returns default value", refVal == 42);
t("GetOrAddRef: map has key after add", map.ContainsKey(1));
t("GetOrAddRef: map Count == 1", map.Count() == 1);

// Mutate through ref
refVal = 100;
t("GetOrAddRef: mutation visible via TryGetValue", map.TryGetValue(1, out int v) && v == 100);

// GetOrAddRef for existing key - should return current value (not overwrite)
ref int refVal2 = ref map.GetOrAddRef(1, 999);
t("GetOrAddRef: existing key returns current value", refVal2 == 100);
t("GetOrAddRef: map Count still 1", map.Count() == 1);

// GetOrAddRef with default default value (0)
ref int refVal3 = ref map.GetOrAddRef(2);
t("GetOrAddRef: new key with default defaultValue returns 0", refVal3 == 0);
t("GetOrAddRef: map Count == 2", map.Count() == 2);

// Mutate second key
refVal3 = 200;
t("GetOrAddRef: second mutation visible", map.TryGetValue(2, out v) && v == 200);

map.Dispose();
t("GetOrAddRef: Dispose completed", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
