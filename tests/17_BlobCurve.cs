// ============================================================================
// TEST: BlobCurve + BlobCurve2/3/4 + BlobCurveHeader + BlobCurveSampler + 
//       BlobCurveSegment + BlobShared
// Branches: topic/BlobCurve, topic/BlobCurve2_3_4, topic/BlobCurveHeader,
//           topic/BlobCurveSampler, topic/BlobCurveSegment, topic/BlobShared,
//           topic/BlobSpline
// Sources: BovineLabs.Core/Collections/BlobCurve*.cs, BlobShared.cs
// Run: cat 17_BlobCurve.cs | unity-cli exec --usings "BovineLabs.Core.Collections,Unity.Collections,Unity.Mathematics,System.Linq"
// ============================================================================
// BlobCurve: bakes AnimationCurve into BlobAsset for Burst evaluation.
// BlobCurve2/3/4: multi-dimensional variants (float2/3/4 output).
// BlobCurveHeader: stores WrapMode, SegmentCount, StartTime, EndTime.
// BlobCurveSampler: wraps BlobAssetReference<BlobCurve> with caching.
// BlobCurveSegment: cubic polynomial segment for evaluation.
// BlobShared: static class with PowerSerial helper.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- BlobCurve: struct ---
var bcType = typeof(BlobCurve);
t("BlobCurve exists", bcType != null);
t("BlobCurve is struct", bcType.IsValueType);
var bcProps = bcType.GetProperties();
var bcPropNames = bcProps.Select(p => p.Name).ToList();
t("BlobCurve has Header", bcPropNames.Contains("Header"));
t("BlobCurve has SegmentCount", bcPropNames.Contains("SegmentCount"));
t("BlobCurve has StartTime", bcPropNames.Contains("StartTime"));
t("BlobCurve has EndTime", bcPropNames.Contains("EndTime"));
t("BlobCurve has Duration", bcPropNames.Contains("Duration"));
t("BlobCurve has IsCreated", bcPropNames.Contains("IsCreated"));
var bcMethods = bcType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var bcMethodNames = bcMethods.Select(m => m.Name).Distinct().ToList();
t("BlobCurve has Evaluate", bcMethodNames.Contains("Evaluate"));
t("BlobCurve has EvaluateIgnoreWrapMode", bcMethodNames.Contains("EvaluateIgnoreWrapMode"));

// --- BlobCurve2: struct with float2 output ---
var bc2Type = typeof(BlobCurve2);
t("BlobCurve2 exists", bc2Type != null);
t("BlobCurve2 is struct", bc2Type.IsValueType);

// --- BlobCurve3: struct with float3 output ---
var bc3Type = typeof(BlobCurve3);
t("BlobCurve3 exists", bc3Type != null);

// --- BlobCurve4: struct with float4 output ---
var bc4Type = typeof(BlobCurve4);
t("BlobCurve4 exists", bc4Type != null);

// --- BlobCurveHeader: struct with wrap modes and segment info ---
var bchType = typeof(BlobCurveHeader);
t("BlobCurveHeader exists", bchType != null);
t("BlobCurveHeader is struct", bchType.IsValueType);
var bchFields = bchType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
var bchFieldNames = bchFields.Select(f => f.Name).ToList();
t("BlobCurveHeader has WrapModePrev", bchFieldNames.Contains("WrapModePrev"));
t("BlobCurveHeader has WrapModePost", bchFieldNames.Contains("WrapModePost"));
t("BlobCurveHeader has SegmentCount", bchFieldNames.Contains("SegmentCount"));
t("BlobCurveHeader has StartTime", bchFieldNames.Contains("StartTime"));
t("BlobCurveHeader has EndTime", bchFieldNames.Contains("EndTime"));
var bchProps = bchType.GetProperties();
t("BlobCurveHeader has Duration property", bchProps.Any(p => p.Name == "Duration"));

// --- BlobCurveSampler: wraps BlobAssetReference with caching ---
var bcsType = typeof(BlobCurveSampler);
t("BlobCurveSampler exists", bcsType != null);
t("BlobCurveSampler is struct", bcsType.IsValueType);
var bcsFields = bcsType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
t("BlobCurveSampler has Curve field", bcsFields.Any(f => f.Name == "Curve"));
var bcsMethods = bcsType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var bcsNames = bcsMethods.Select(m => m.Name).Distinct().ToList();
t("BlobCurveSampler has Evaluate", bcsNames.Contains("Evaluate"));
t("BlobCurveSampler has EvaluateWithoutCache", bcsNames.Contains("EvaluateWithoutCache"));

// --- BlobCurveSegment: cubic polynomial segment ---
var bcsegType = typeof(BlobCurveSegment);
t("BlobCurveSegment exists", bcsegType != null);
t("BlobCurveSegment is struct", bcsegType.IsValueType);
var bcsegMethods = bcsegType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
t("BlobCurveSegment has Sample", bcsegMethods.Any(m => m.Name == "Sample"));

// --- BlobShared: static class ---
var bsType = typeof(BlobShared);
t("BlobShared exists", bsType != null);
t("BlobShared is class", bsType.IsClass);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
