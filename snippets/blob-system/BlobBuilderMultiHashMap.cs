// Run: cat snippets/blob-system/BlobBuilderMultiHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderMultiHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var mhmType = typeof(BlobBuilderMultiHashMap<int,int>);
t("BlobBuilderMultiHashMap<int,int>: type exists", mhmType != null);
t("BlobBuilderMultiHashMap: is struct", mhmType.IsValueType);
t("BlobBuilderMultiHashMap: is ref struct", mhmType.IsByRefLike);

// Claims: Capacity property
var capProp = mhmType.GetProperty("Capacity");
t("BlobBuilderMultiHashMap: has Capacity property", capProp != null);
t("BlobBuilderMultiHashMap: Capacity is int", capProp?.PropertyType == typeof(int));

// Claims: Count property
var countProp = mhmType.GetProperty("Count");
t("BlobBuilderMultiHashMap: has Count property", countProp != null);
t("BlobBuilderMultiHashMap: Count is int", countProp?.PropertyType == typeof(int));

// Claims: Add(TKey key, TValue item) -> void (allows duplicates)
var addMethod = mhmType.GetMethod("Add", new[] { typeof(int), typeof(int) });
t("BlobBuilderMultiHashMap: has Add(key,value) method", addMethod != null);
t("BlobBuilderMultiHashMap: Add returns void", addMethod?.ReturnType == typeof(void));

// Claims: Also has Add(TKey key) that returns ref TValue
var addKeyOnly = mhmType.GetMethod("Add", new[] { typeof(int) });
t("BlobBuilderMultiHashMap: has Add(key) overload", addKeyOnly != null);
t("BlobBuilderMultiHashMap: Add(key) returns ref TValue", addKeyOnly != null && addKeyOnly.ReturnType.IsByRef);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
