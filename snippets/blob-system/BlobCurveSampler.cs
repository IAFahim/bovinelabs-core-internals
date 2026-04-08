// Run: cat snippets/blob-system/BlobCurveSampler.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveSampler.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var samplerType = typeof(BlobCurveSampler);
t("BlobCurveSampler: type exists", samplerType != null);
t("BlobCurveSampler: is struct", samplerType.IsValueType);

// Claims: has Curve field (BlobAssetReference<BlobCurve>)
var curveField = samplerType.GetField("Curve");
t("BlobCurveSampler: has Curve field", curveField != null);
t("BlobCurveSampler: Curve is BlobAssetReference<BlobCurve>", 
    curveField?.FieldType == typeof(Unity.Entities.BlobAssetReference<BlobCurve>));

// Claims: has IsCreated property (bool)
var isCreatedProp = samplerType.GetProperty("IsCreated");
t("BlobCurveSampler: has IsCreated property", isCreatedProp != null);
t("BlobCurveSampler: IsCreated is bool", isCreatedProp?.PropertyType == typeof(bool));

// Claims: Evaluate(in float time) returns float
var evalMethod = samplerType.GetMethod("Evaluate", new[] { typeof(float).MakeByRefType() });
t("BlobCurveSampler: has Evaluate(in float)", evalMethod != null);
t("BlobCurveSampler: Evaluate returns float", evalMethod?.ReturnType == typeof(float));

// Claims: EvaluateIgnoreWrapMode returns float
var evalIgnore = samplerType.GetMethod("EvaluateIgnoreWrapMode", new[] { typeof(float).MakeByRefType() });
t("BlobCurveSampler: has EvaluateIgnoreWrapMode(in float)", evalIgnore != null);
t("BlobCurveSampler: EvaluateIgnoreWrapMode returns float", evalIgnore?.ReturnType == typeof(float));

// Claims: EvaluateWithoutCache returns float
var evalNoCache = samplerType.GetMethod("EvaluateWithoutCache", new[] { typeof(float).MakeByRefType() });
t("BlobCurveSampler: has EvaluateWithoutCache(in float)", evalNoCache != null);
t("BlobCurveSampler: EvaluateWithoutCache returns float", evalNoCache?.ReturnType == typeof(float));

// Claims: EvaluateIgnoreWrapModeWithoutCache returns float
var evalIgnoreNoCache = samplerType.GetMethod("EvaluateIgnoreWrapModeWithoutCache", new[] { typeof(float).MakeByRefType() });
t("BlobCurveSampler: has EvaluateIgnoreWrapModeWithoutCache(in float)", evalIgnoreNoCache != null);
t("BlobCurveSampler: EvaluateIgnoreWrapModeWithoutCache returns float", evalIgnoreNoCache?.ReturnType == typeof(float));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
