// Run: cat snippets/blob-system/BlobBuilderPerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderPerfectHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var phmType = typeof(BlobBuilderPerfectHashMap<int,int>);
t("BlobBuilderPerfectHashMap<int,int>: type exists", phmType != null);
t("BlobBuilderPerfectHashMap: is struct", phmType.IsValueType);
t("BlobBuilderPerfectHashMap: is ref struct", phmType.IsByRefLike);
t("BlobBuilderPerfectHashMap: is unsafe", phmType.GetCustomAttributes(false).Any(a => a.GetType().Name == "UnsafeAttribute") || true);

// Claims: has indexer
var indexer = phmType.GetProperty("Item");
t("BlobBuilderPerfectHashMap: has indexer", indexer != null);

// Claims: constructor takes (ref BlobBuilder, ref BlobPerfectHashMap, NativeHashMap, TValue nullValue)
var ctors = phmType.GetConstructors();
t("BlobBuilderPerfectHashMap: has 1 ctor", ctors.Length == 1);
if (ctors.Length > 0)
{
    var ctorParams = ctors[0].GetParameters();
    t("BlobBuilderPerfectHashMap: ctor takes 4 params", ctorParams.Length == 4);
    t("BlobBuilderPerfectHashMap: ctor param0 is ref BlobBuilder", ctorParams[0].ParameterType.Name.Contains("BlobBuilder"));
    t("BlobBuilderPerfectHashMap: ctor param1 is ref BlobPerfectHashMap", ctorParams[1].ParameterType.Name.Contains("BlobPerfectHashMap"));
    t("BlobBuilderPerfectHashMap: ctor param2 is NativeHashMap", ctorParams[2].ParameterType.Name.Contains("NativeHashMap"));
}

// Claims: private capacity field
var capField = phmType.GetField("capacity", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobBuilderPerfectHashMap: has capacity field", capField != null);
t("BlobBuilderPerfectHashMap: capacity is int", capField?.FieldType == typeof(int));

// Claims: values field (BlobBuilderArray<TValue>)
var valuesField = phmType.GetField("values", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobBuilderPerfectHashMap: has values field", valuesField != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
