// Run: cat snippets/core-collections/NativeListExtensions_ClearAddRange.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,Unity.Collections,System.Linq"
// Verifies: docs/NativeListExtensions_ClearAddRange.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Extension method exists ---
var extType = typeof(BovineLabs.Core.Extensions.NativeListExtensions);
t("NativeListExtensions: type exists", extType != null);

var clearAddRangeMethods = extType.GetMethods().Where(m => m.Name == "ClearAddRange").ToList();
t("NativeListExtensions: has ClearAddRange methods", clearAddRangeMethods.Count > 0);

// --- Functional test: ClearAddRange with IEnumerable ---
var list = new NativeList<int>(Unity.Collections.Allocator.Temp);
list.Add(1);
list.Add(2);
list.Add(3);
t("NativeList: initial length == 3", list.Length == 3);

// ClearAddRange with array (IEnumerable)
var newArr = new int[] { 10, 20, 30, 40 };
list.ClearAddRange(newArr);
t("ClearAddRange(IEnumerable): length == 4", list.Length == 4);
t("ClearAddRange(IEnumerable): [0] == 10", list[0] == 10);
t("ClearAddRange(IEnumerable): [1] == 20", list[1] == 20);
t("ClearAddRange(IEnumerable): [2] == 30", list[2] == 30);
t("ClearAddRange(IEnumerable): [3] == 40", list[3] == 40);

// ClearAddRange with NativeArray
var nativeArr = new NativeArray<int>(2, Unity.Collections.Allocator.Temp);
nativeArr[0] = 100;
nativeArr[1] = 200;
list.ClearAddRange(nativeArr);
t("ClearAddRange(NativeArray): length == 2", list.Length == 2);
t("ClearAddRange(NativeArray): [0] == 100", list[0] == 100);
t("ClearAddRange(NativeArray): [1] == 200", list[1] == 200);

// ClearAddRange with NativeHashSet
var hashSet = new NativeHashSet<int>(4, Unity.Collections.Allocator.Temp);
hashSet.Add(5);
hashSet.Add(15);
hashSet.Add(25);
list.ClearAddRange(hashSet);
t("ClearAddRange(NativeHashSet): length == 3", list.Length == 3);
// Order from hash set is not guaranteed, but all values should be present
var listSet = new System.Collections.Generic.HashSet<int>();
for (int i = 0; i < list.Length; i++) listSet.Add(list[i]);
t("ClearAddRange(NativeHashSet): contains 5", listSet.Contains(5));
t("ClearAddRange(NativeHashSet): contains 15", listSet.Contains(15));
t("ClearAddRange(NativeHashSet): contains 25", listSet.Contains(25));

// Verify ClearAddRange actually clears first
list.ClearAddRange(new int[] { 99 });
t("ClearAddRange: clears previous content and adds new", list.Length == 1 && list[0] == 99);

list.Dispose();
nativeArr.Dispose();
hashSet.Dispose();

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
