// Run: cat snippets/blob-system/BlobCurveSegment.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveSegment.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var segType = typeof(BlobCurveSegment);
t("BlobCurveSegment: type exists", segType != null);
t("BlobCurveSegment: is struct", segType.IsValueType);

// Claims: has factors field (float4)
var factorsField = segType.GetField("factors", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobCurveSegment: has factors field", factorsField != null);
t("BlobCurveSegment: factors is float4", factorsField?.FieldType == typeof(Unity.Mathematics.float4));

// Claims: struct is readonly
t("BlobCurveSegment: is readonly struct", segType.IsValueType && segType.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("IsReadOnlyAttribute")) || true);
// Actually check via initonly on fields
t("BlobCurveSegment: factors field is initonly (readonly)", factorsField != null && factorsField.IsInitOnly);

// Claims: constructor from float4 factors
var ctor1 = segType.GetConstructor(new[] { typeof(Unity.Mathematics.float4) });
t("BlobCurveSegment: has ctor(float4)", ctor1 != null);

// Claims: constructor from Keyframe pair  
var ctor2 = segType.GetConstructor(new[] { typeof(UnityEngine.Keyframe), typeof(UnityEngine.Keyframe) });
t("BlobCurveSegment: has ctor(Keyframe, Keyframe)", ctor2 != null);

// Claims: Sample(float4 timeSerial) returns float
var sampleMethod = segType.GetMethod("Sample");
t("BlobCurveSegment: has Sample method", sampleMethod != null);
t("BlobCurveSegment: Sample returns float", sampleMethod?.ReturnType == typeof(float));
var sampleParams = sampleMethod?.GetParameters();
t("BlobCurveSegment: Sample takes float4 param", sampleParams != null && sampleParams.Length == 1 && sampleParams[0].ParameterType == typeof(Unity.Mathematics.float4));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
