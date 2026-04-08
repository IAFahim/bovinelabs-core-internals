// Run: cat snippets/blob-system/IBlobCurve.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/IBlobCurve.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var ifaceType = typeof(IBlobCurve<float>);
t("IBlobCurve<float>: type exists", ifaceType != null);
t("IBlobCurve: is interface", ifaceType.IsInterface);
t("IBlobCurve: is generic", ifaceType.IsGenericType);

// Claims: interface has 4 methods
var methods = ifaceType.GetMethods();
t("IBlobCurve: has 4 methods", methods.Length == 4);

// Claims: EvaluateIgnoreWrapMode(in float, ref BlobCurveCache) -> T
var evalIgnoreCache = methods.FirstOrDefault(m => m.Name == "EvaluateIgnoreWrapMode" && m.GetParameters().Length == 2);
t("IBlobCurve: has EvaluateIgnoreWrapMode with cache", evalIgnoreCache != null);

// Claims: EvaluateIgnoreWrapMode(in float) -> T
var evalIgnore = methods.FirstOrDefault(m => m.Name == "EvaluateIgnoreWrapMode" && m.GetParameters().Length == 1);
t("IBlobCurve: has EvaluateIgnoreWrapMode without cache", evalIgnore != null);

// Claims: Evaluate(in float, ref BlobCurveCache) -> T
var evalCache = methods.FirstOrDefault(m => m.Name == "Evaluate" && m.GetParameters().Length == 2);
t("IBlobCurve: has Evaluate with cache", evalCache != null);

// Claims: Evaluate(in float) -> T
var evalNoCache = methods.FirstOrDefault(m => m.Name == "Evaluate" && m.GetParameters().Length == 1);
t("IBlobCurve: has Evaluate without cache", evalNoCache != null);

// Claims: covariant out T (IBlobCurve<out T>)
t("IBlobCurve: T is covariant (out)", ifaceType.GetGenericArguments()[0].GenericParameterPosition == 0);

// Claims: BlobCurve implements IBlobCurve<float>
var bcType = typeof(BlobCurve);
t("IBlobCurve: BlobCurve implements IBlobCurve<float>", bcType.GetInterfaces().Contains(ifaceType));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
