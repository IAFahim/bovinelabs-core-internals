// Run: cat snippets/blob-system/BlobCurve2_3_4.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurve2_3_4.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bc2Type = typeof(BlobCurve2);
t("BlobCurve2: type exists", bc2Type != null);
t("BlobCurve2: is struct", bc2Type.IsValueType);

// Header may return ref BlobCurveHeader rather than BlobCurveHeader directly
var headerProp = bc2Type.GetProperty("Header");
t("BlobCurve2: has Header property", headerProp != null);
// Header property may return ref BlobCurveHeader
t("BlobCurve2: Header is or refs BlobCurveHeader", headerProp != null && (headerProp.PropertyType == typeof(BlobCurveHeader) || headerProp.PropertyType.Name.Contains("BlobCurveHeader")));

t("BlobCurve2: has SegmentCount", bc2Type.GetProperty("SegmentCount") != null);
t("BlobCurve2: has StartTime", bc2Type.GetProperty("StartTime") != null);
t("BlobCurve2: has EndTime", bc2Type.GetProperty("EndTime") != null);
t("BlobCurve2: has Duration", bc2Type.GetProperty("Duration") != null);

var evalIgnore = bc2Type.GetMethods().Where(m => m.Name == "EvaluateIgnoreWrapMode").ToArray();
t("BlobCurve2: has EvaluateIgnoreWrapMode overloads", evalIgnore.Length >= 2);
t("BlobCurve2: EvaluateIgnoreWrapMode returns float2", evalIgnore.Length > 0 && evalIgnore[0].ReturnType == typeof(Unity.Mathematics.float2));

var evalMethods = bc2Type.GetMethods().Where(m => m.Name == "Evaluate").ToArray();
t("BlobCurve2: has Evaluate overloads", evalMethods.Length >= 2);
t("BlobCurve2: Evaluate returns float2", evalMethods.Length > 0 && evalMethods[0].ReturnType == typeof(Unity.Mathematics.float2));

var bc3Type = typeof(BlobCurve3);
t("BlobCurve3: type exists", bc3Type != null);
t("BlobCurve3: is struct", bc3Type.IsValueType);

var bc4Type = typeof(BlobCurve4);
t("BlobCurve4: type exists", bc4Type != null);
t("BlobCurve4: is struct", bc4Type.IsValueType);

var eval3 = bc3Type.GetMethods().Where(m => m.Name == "Evaluate").ToArray();
t("BlobCurve3: Evaluate returns float3", eval3.Length > 0 && eval3[0].ReturnType == typeof(Unity.Mathematics.float3));

var eval4 = bc4Type.GetMethods().Where(m => m.Name == "Evaluate").ToArray();
t("BlobCurve4: Evaluate returns float4", eval4.Length > 0 && eval4[0].ReturnType == typeof(Unity.Mathematics.float4));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
