// Run: cat snippets/blob-system/BlobHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var hmType = typeof(BlobHashMap<int,int>);
t("BlobHashMap<int,int>: type exists", hmType != null);
t("BlobHashMap<int,int>: is struct (ValueType)", hmType.IsValueType);
t("BlobHashMap<int,int>: is generic", hmType.IsGenericTypeDefinition || hmType.IsGenericType);

// Claims: TKey : unmanaged, IEquatable<TKey>, TValue : unmanaged
var genericArgs = hmType.GetGenericArguments();
t("BlobHashMap: has 2 generic type args", genericArgs.Length == 2);

// Claims: has internal BlobHashMapData<TKey,TValue> Data field
var dataField = hmType.GetField("Data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobHashMap: has Data field", dataField != null);

// Claims: Count property returns Data.Count[0]
var countProp = hmType.GetProperty("Count");
t("BlobHashMap: has Count property", countProp != null);
t("BlobHashMap: Count returns int", countProp?.PropertyType == typeof(int));

// Claims: TryGetValue returns Ptr<TValue>
var tryGetValue = hmType.GetMethods().Where(m => m.Name == "TryGetValue").FirstOrDefault();
t("BlobHashMap: has TryGetValue", tryGetValue != null);
if (tryGetValue != null)
{
    t("BlobHashMap: TryGetValue returns bool", tryGetValue.ReturnType == typeof(bool));
    var parms = tryGetValue.GetParameters();
    t("BlobHashMap: TryGetValue takes 2 params", parms.Length == 2);
}

// Claims: ContainsKey delegates to TryGetValue
var containsKey = hmType.GetMethod("ContainsKey");
t("BlobHashMap: has ContainsKey", containsKey != null);
t("BlobHashMap: ContainsKey returns bool", containsKey?.ReturnType == typeof(bool));

// Claims: indexer (this[key]) returns ref TValue
var indexer = hmType.GetProperty("Item");
t("BlobHashMap: has indexer (Item)", indexer != null);

// Claims: GetEnumerator returns BlobHashMapEnumerator
var getEnumerator = hmType.GetMethod("GetEnumerator");
t("BlobHashMap: has GetEnumerator", getEnumerator != null);
if (getEnumerator != null)
{
    t("BlobHashMap: GetEnumerator returns BlobHashMapEnumerator", 
        getEnumerator.ReturnType.Name.StartsWith("BlobHashMapEnumerator"));
}

// Claims: nested within is BlobHashMapData
t("BlobHashMap: Data field type is BlobHashMapData", 
    dataField != null && dataField.FieldType.Name.StartsWith("BlobHashMapData"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
