// Run: cat snippets/blob-system/BlobBuilderHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var hmType = typeof(BlobBuilderHashMap<int,int>);
t("BlobBuilderHashMap<int,int>: type exists", hmType != null);
t("BlobBuilderHashMap: is struct", hmType.IsValueType);
t("BlobBuilderHashMap: is ref struct", hmType.IsByRefLike);

// Claims: Capacity property
var capProp = hmType.GetProperty("Capacity");
t("BlobBuilderHashMap: has Capacity property", capProp != null);
t("BlobBuilderHashMap: Capacity is int", capProp?.PropertyType == typeof(int));

// Claims: Count property
var countProp = hmType.GetProperty("Count");
t("BlobBuilderHashMap: has Count property", countProp != null);
t("BlobBuilderHashMap: Count is int", countProp?.PropertyType == typeof(int));

// Claims: Add(TKey key, TValue item) -> void
var addMethod = hmType.GetMethod("Add");
t("BlobBuilderHashMap: has Add method", addMethod != null);
t("BlobBuilderHashMap: Add returns void", addMethod?.ReturnType == typeof(void));
t("BlobBuilderHashMap: Add takes 2 params", addMethod?.GetParameters().Length == 2);

// Claims: TryAdd(TKey key, TValue value) -> bool
var tryAddMethod = hmType.GetMethod("TryAdd");
t("BlobBuilderHashMap: has TryAdd method", tryAddMethod != null);
t("BlobBuilderHashMap: TryAdd returns bool", tryAddMethod?.ReturnType == typeof(bool));

// Claims: ContainsKey(TKey key) -> bool
var containsKeyMethod = hmType.GetMethod("ContainsKey");
t("BlobBuilderHashMap: has ContainsKey method", containsKeyMethod != null);
t("BlobBuilderHashMap: ContainsKey returns bool", containsKeyMethod?.ReturnType == typeof(bool));

// Claims: AddUnique method exists
var addUniqueMethod = hmType.GetMethod("AddUnique");
t("BlobBuilderHashMap: has AddUnique method", addUniqueMethod != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
