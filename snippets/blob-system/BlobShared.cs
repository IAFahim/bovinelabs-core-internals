// Run: cat snippets/blob-system/BlobShared.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobShared.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var sharedType = typeof(BlobShared);
t("BlobShared: type exists", sharedType != null);
t("BlobShared: is static class", sharedType.IsAbstract && sharedType.IsSealed);

// Claims: PowerSerial(float t) returns float4
var powerSerial = sharedType.GetMethod("PowerSerial");
t("BlobShared: has PowerSerial", powerSerial != null);
t("BlobShared: PowerSerial returns float4", powerSerial?.ReturnType == typeof(Unity.Mathematics.float4));
t("BlobShared: PowerSerial takes float", powerSerial?.GetParameters().Length == 1 && powerSerial.GetParameters()[0].ParameterType == typeof(float));

// Claims: UnityFactor(float v0, float t0, float t1, float v1, float duration) returns float4
var unityFactor = sharedType.GetMethod("UnityFactor");
t("BlobShared: has UnityFactor", unityFactor != null);
t("BlobShared: UnityFactor returns float4", unityFactor?.ReturnType == typeof(Unity.Mathematics.float4));
t("BlobShared: UnityFactor takes 5 params", unityFactor?.GetParameters().Length == 5);

// Claims: HermiteFactor(float v0, float m0, float m1, float v1) returns float4
var hermiteFactor = sharedType.GetMethod("HermiteFactor");
t("BlobShared: has HermiteFactor", hermiteFactor != null);
t("BlobShared: HermiteFactor returns float4", hermiteFactor?.ReturnType == typeof(Unity.Mathematics.float4));
t("BlobShared: HermiteFactor takes 4 params", hermiteFactor?.GetParameters().Length == 4);

// Claims: BezierFactor(float p0, float p1, float p2, float p3) returns float4
var bezierFactor = sharedType.GetMethod("BezierFactor");
t("BlobShared: has BezierFactor", bezierFactor != null);
t("BlobShared: BezierFactor returns float4", bezierFactor?.ReturnType == typeof(Unity.Mathematics.float4));
t("BlobShared: BezierFactor takes 4 params", bezierFactor?.GetParameters().Length == 4);

// Claims: LinearFactor(float p0, float p3) returns float4
var linearFactor = sharedType.GetMethod("LinearFactor");
t("BlobShared: has LinearFactor", linearFactor != null);
t("BlobShared: LinearFactor returns float4", linearFactor?.ReturnType == typeof(Unity.Mathematics.float4));
t("BlobShared: LinearFactor takes 2 params", linearFactor?.GetParameters().Length == 2);

// Claims: ConvertWrapMode method exists
var convertWrapMode = sharedType.GetMethod("ConvertWrapMode");
t("BlobShared: has ConvertWrapMode", convertWrapMode != null);

// Functional test: PowerSerial(0.5) should be (0.125, 0.25, 0.5, 1)
var psResult = BlobShared.PowerSerial(0.5f);
t("BlobShared: PowerSerial(0.5).x == 0.125", Math.Abs(psResult.x - 0.125f) < 0.0001f);
t("BlobShared: PowerSerial(0.5).y == 0.25", Math.Abs(psResult.y - 0.25f) < 0.0001f);
t("BlobShared: PowerSerial(0.5).z == 0.5", Math.Abs(psResult.z - 0.5f) < 0.0001f);
t("BlobShared: PowerSerial(0.5).w == 1.0", Math.Abs(psResult.w - 1.0f) < 0.0001f);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
