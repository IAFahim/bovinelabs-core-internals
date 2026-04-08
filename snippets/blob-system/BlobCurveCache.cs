// Run: cat snippets/blob-system/BlobCurveCache.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveCache.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var cacheType = typeof(BlobCurveCache);
t("BlobCurveCache: type exists", cacheType != null);
t("BlobCurveCache: is struct", cacheType.IsValueType);

// Claims: has static readonly Empty field
var emptyField = cacheType.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
t("BlobCurveCache: has static Empty field", emptyField != null);
t("BlobCurveCache: Empty is BlobCurveCache", emptyField?.FieldType == typeof(BlobCurveCache));

// Claims: NeighborhoodTimes is float2
var neighborhoodField = cacheType.GetField("NeighborhoodTimes");
t("BlobCurveCache: has NeighborhoodTimes field", neighborhoodField != null);
t("BlobCurveCache: NeighborhoodTimes is float2", neighborhoodField?.FieldType == typeof(Unity.Mathematics.float2));

// Claims: Index is int
var indexField = cacheType.GetField("Index");
t("BlobCurveCache: has Index field", indexField != null);
t("BlobCurveCache: Index is int", indexField?.FieldType == typeof(int));

// Claims: Empty sentinel Index = int.MinValue, NeighborhoodTimes = NaN
var emptyVal = (BlobCurveCache)emptyField.GetValue(null);
t("BlobCurveCache: Empty.Index == int.MinValue", emptyVal.Index == int.MinValue);
t("BlobCurveCache: Empty.NeighborhoodTimes is NaN", float.IsNaN(emptyVal.NeighborhoodTimes.x) && float.IsNaN(emptyVal.NeighborhoodTimes.y));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
