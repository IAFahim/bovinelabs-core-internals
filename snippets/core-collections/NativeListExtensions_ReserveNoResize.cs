// Run: cat snippets/core-collections/NativeListExtensions_ReserveNoResize.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,System.Reflection,System.Linq"
// Verifies: docs/NativeListExtensions_ReserveNoResize.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var extType = typeof(BovineLabs.Core.Extensions.NativeListExtensions);
t("NativeListExtensions type exists", extType != null);
t("Is static class", extType.IsAbstract && extType.IsSealed);

// --- ReserveNoResize method exists ---
var methods = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "ReserveNoResize").ToList();
t("ReserveNoResize method exists", methods.Count > 0);
r.Add($"INFO: {methods.Count} overloads of ReserveNoResize found");

// --- Has NativeList overload ---
var listOverload = methods.FirstOrDefault(m => {
    var ps = m.GetParameters();
    return ps.Length == 4 && ps[0].ParameterType.Name.StartsWith("NativeList");
});
t("Has NativeList<T> overload (4 params)", listOverload != null);

// --- Has ParallelWriter overload ---
var writerOverload = methods.FirstOrDefault(m => {
    var ps = m.GetParameters();
    return ps.Length == 4 && ps[0].ParameterType.Name.Contains("Writer");
});
t("Has ParallelWriter overload (4 params)", writerOverload != null);

// --- Parameter signatures ---
if (listOverload != null)
{
    var ps = listOverload.GetParameters();
    t("Param 0: NativeList<T>", ps[0].ParameterType.Name.StartsWith("NativeList"));
    t("Param 1: int length", ps[1].ParameterType == typeof(int));
    t("Param 2: T* ptr (unsafe pointer)", ps[2].ParameterType.IsPointer || ps[2].ParameterType.Name == "T*&");
    t("Param 3: int& idx (ref int)", ps[3].ParameterType.IsByRef);
    t("Returns void", listOverload.ReturnType == typeof(void));
}

// --- Doc claim: grows list without zeroing new elements ---
// This is an unsafe pointer-based API, verify it exists as extension method
t("Is ExtensionMethod (has ExtensionAttribute)", 
    listOverload != null && listOverload.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
