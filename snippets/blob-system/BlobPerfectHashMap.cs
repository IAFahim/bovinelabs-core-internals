// Run: cat snippets/blob-system/BlobPerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobPerfectHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var phmType = typeof(BlobPerfectHashMap<int,int>);
t("BlobPerfectHashMap<int,int>: type exists", phmType != null);
t("BlobPerfectHashMap: is struct", phmType.IsValueType);

// Claims: TValue must implement IEquatable<TValue>
var genericArgs = phmType.GetGenericArguments();
t("BlobPerfectHashMap: has 2 generic type args", genericArgs.Length == 2);

// Claims: has BlobArray<TValue> Values field
var valuesField = phmType.GetField("Values", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobPerfectHashMap: has Values field", valuesField != null);

// Claims: has int Capacity field
var capField = phmType.GetField("Capacity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobPerfectHashMap: has Capacity field", capField != null);
t("BlobPerfectHashMap: Capacity is int", capField?.FieldType == typeof(int));

// Claims: has TValue NullValue field
var nullField = phmType.GetField("NullValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobPerfectHashMap: has NullValue field", nullField != null);

// Claims: TryGetValue returns Ptr<TValue>
var tryGetValue = phmType.GetMethods().Where(m => m.Name == "TryGetValue").FirstOrDefault();
t("BlobPerfectHashMap: has TryGetValue", tryGetValue != null);
t("BlobPerfectHashMap: TryGetValue returns bool", tryGetValue?.ReturnType == typeof(bool));

// Claims: ContainsKey
var containsKey = phmType.GetMethod("ContainsKey");
t("BlobPerfectHashMap: has ContainsKey", containsKey != null);

// Claims: indexer
var indexer = phmType.GetProperty("Item");
t("BlobPerfectHashMap: has indexer (Item)", indexer != null);

// Claims: IndexFor uses key.GetHashCode() & (Capacity - 1) - verify via private method existence
var indexFor = phmType.GetMethod("IndexFor", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobPerfectHashMap: has private IndexFor method", indexFor != null);

// Claims: TryGetIndex is private
var tryGetIndex = phmType.GetMethod("TryGetIndex", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobPerfectHashMap: has private TryGetIndex method", tryGetIndex != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
