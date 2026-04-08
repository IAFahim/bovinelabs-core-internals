// Run: cat snippets/blob-system/BlobSpline.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobSpline.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var splineType = typeof(BlobSpline);
t("BlobSpline: type exists", splineType != null);
t("BlobSpline: is struct", splineType.IsValueType);

// Claims: Knots (BlobArray<BezierKnot>)
var knotsField = splineType.GetField("Knots");
t("BlobSpline: has Knots field", knotsField != null);

// Claims: Curves (BlobArray<BezierCurve>)
var curvesField = splineType.GetField("Curves");
t("BlobSpline: has Curves field", curvesField != null);

// Claims: SegmentLengthsLookupTable
var segmentField = splineType.GetField("SegmentLengthsLookupTable");
t("BlobSpline: has SegmentLengthsLookupTable field", segmentField != null);

// Claims: UpVectorsLookupTable
var upVectorsField = splineType.GetField("UpVectorsLookupTable");
t("BlobSpline: has UpVectorsLookupTable field", upVectorsField != null);

// Claims: Closed (bool)
var closedField = splineType.GetField("Closed");
t("BlobSpline: has Closed field", closedField != null);
t("BlobSpline: Closed is bool", closedField?.FieldType == typeof(bool));

// Claims: Length (float)
var lengthField = splineType.GetField("Length");
t("BlobSpline: has Length field", lengthField != null);
t("BlobSpline: Length is float", lengthField?.FieldType == typeof(float));

// Claims: Count (int)
var countField = splineType.GetField("Count");
t("BlobSpline: has Count field", countField != null);
t("BlobSpline: Count is int", countField?.FieldType == typeof(int));

// Claims: Origin (float3)
var originField = splineType.GetField("Origin");
t("BlobSpline: has Origin field", originField != null);

// Claims: Evaluate method(s)
var evalMethods = splineType.GetMethods().Where(m => m.Name == "Evaluate").ToArray();
t("BlobSpline: has Evaluate overloads", evalMethods.Length >= 1);

// Claims: EvaluatePosition
t("BlobSpline: has EvaluatePosition", splineType.GetMethod("EvaluatePosition") != null);

// Claims: EvaluateTangent
t("BlobSpline: has EvaluateTangent", splineType.GetMethod("EvaluateTangent") != null);

// Claims: EvaluateUpVector
t("BlobSpline: has EvaluateUpVector", splineType.GetMethod("EvaluateUpVector") != null);

// Claims: GetCurve
t("BlobSpline: has GetCurve", splineType.GetMethod("GetCurve") != null);

// Claims: ToSpline
t("BlobSpline: has ToSpline", splineType.GetMethod("ToSpline") != null);

// Claims: Create static
var createMethod = splineType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
t("BlobSpline: has static Create", createMethod != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
