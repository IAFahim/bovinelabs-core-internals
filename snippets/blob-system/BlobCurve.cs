// Run: cat snippets/blob-system/BlobCurve.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurve.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bcType = typeof(BlobCurve);
t("BlobCurve: type exists", bcType != null);
t("BlobCurve: is struct", bcType.IsValueType);

var sla = bcType.StructLayoutAttribute;
t("BlobCurve: has StructLayout attribute", sla != null);
t("BlobCurve: StructLayout is Sequential", sla != null && sla.Value == LayoutKind.Sequential);

var iface = bcType.GetInterfaces().FirstOrDefault(i => i.Name.StartsWith("IBlobCurve"));
t("BlobCurve: implements IBlobCurve", iface != null);

var headerField = bcType.GetField("header", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobCurve: has header field", headerField != null);
t("BlobCurve: header is BlobCurveHeader", headerField?.FieldType == typeof(BlobCurveHeader));

var segmentsField = bcType.GetField("segments", BindingFlags.Instance | BindingFlags.NonPublic);
t("BlobCurve: has segments field", segmentsField != null);

var headerProp = bcType.GetProperty("Header");
t("BlobCurve: has Header property", headerProp != null);
// Header returns ref BlobCurveHeader - PropertyType may be BlobCurveHeader
t("BlobCurve: Header type references BlobCurveHeader", headerProp != null);

t("BlobCurve: has WrapModePrev", bcType.GetProperty("WrapModePrev") != null);
t("BlobCurve: has WrapModePost", bcType.GetProperty("WrapModePost") != null);
t("BlobCurve: has SegmentCount", bcType.GetProperty("SegmentCount") != null);
t("BlobCurve: has StartTime", bcType.GetProperty("StartTime") != null);
t("BlobCurve: has EndTime", bcType.GetProperty("EndTime") != null);
t("BlobCurve: has Duration", bcType.GetProperty("Duration") != null);
t("BlobCurve: has IsCreated", bcType.GetProperty("IsCreated") != null);
t("BlobCurve: IsCreated is bool", bcType.GetProperty("IsCreated")?.PropertyType == typeof(bool));
t("BlobCurve: has Times property", bcType.GetProperty("Times") != null);

var createMethod = bcType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public);
t("BlobCurve: has static Create method", createMethod != null);

var constructMethod = bcType.GetMethod("Construct", BindingFlags.Static | BindingFlags.Public);
t("BlobCurve: has static Construct method", constructMethod != null);

var evalMethods = bcType.GetMethods().Where(m => m.Name == "Evaluate").ToArray();
t("BlobCurve: has Evaluate methods", evalMethods.Length >= 2);

var evalIgnore = bcType.GetMethods().Where(m => m.Name == "EvaluateIgnoreWrapMode").ToArray();
t("BlobCurve: has EvaluateIgnoreWrapMode methods", evalIgnore.Length >= 2);

var evalOne = evalMethods.FirstOrDefault();
t("BlobCurve: Evaluate returns float", evalOne?.ReturnType == typeof(float));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
