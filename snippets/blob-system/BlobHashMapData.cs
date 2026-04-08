// Run: cat snippets/blob-system/BlobHashMapData.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobHashMapData.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// BlobHashMapData is internal, so we access via BlobHashMap
var hmType = typeof(BlobHashMap<int,int>);
var dataField = hmType.GetField("Data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobHashMapData: accessible via BlobHashMap.Data", dataField != null);

var dataType = dataField?.FieldType;
t("BlobHashMapData: type is BlobHashMapData", dataType != null && dataType.Name.StartsWith("BlobHashMapData"));
t("BlobHashMapData: is struct", dataType?.IsValueType ?? false);

if (dataType != null)
{
    // Claims: Values field (BlobArray<TValue>)
    var valuesField = dataType.GetField("Values");
    t("BlobHashMapData: has Values field", valuesField != null);

    // Claims: Keys field (BlobArray<TKey>)
    var keysField = dataType.GetField("Keys");
    t("BlobHashMapData: has Keys field", keysField != null);

    // Claims: Next field (BlobArray<int>)
    var nextField = dataType.GetField("Next");
    t("BlobHashMapData: has Next field", nextField != null);

    // Claims: Buckets field (BlobArray<int>)
    var bucketsField = dataType.GetField("Buckets");
    t("BlobHashMapData: has Buckets field", bucketsField != null);

    // Claims: Count field (BlobArray<int> length=1)
    var countField = dataType.GetField("Count");
    t("BlobHashMapData: has Count field", countField != null);

    // Claims: BucketCapacityMask field (int)
    var maskField = dataType.GetField("BucketCapacityMask");
    t("BlobHashMapData: has BucketCapacityMask field", maskField != null);
    t("BlobHashMapData: BucketCapacityMask is int", maskField?.FieldType == typeof(int));

    // Claims: TryGetFirstValue method
    var tryGetFirst = dataType.GetMethod("TryGetFirstValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    t("BlobHashMapData: has TryGetFirstValue", tryGetFirst != null);

    // Claims: TryGetNextValue method
    var tryGetNext = dataType.GetMethod("TryGetNextValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    t("BlobHashMapData: has TryGetNextValue", tryGetNext != null);
}

// Claims: KVPair is a public readonly unsafe struct
var kvPairType = typeof(KVPair<int,int>);
t("BlobHashMapData: KVPair type exists", kvPairType != null);
t("BlobHashMapData: KVPair is struct", kvPairType.IsValueType);
t("BlobHashMapData: KVPair has Key property", kvPairType.GetProperty("Key") != null);
t("BlobHashMapData: KVPair has Value property", kvPairType.GetProperty("Value") != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
