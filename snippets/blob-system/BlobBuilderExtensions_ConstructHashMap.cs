// Run: cat snippets/blob-system/BlobBuilderExtensions_ConstructHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderExtensions_ConstructHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var extType = typeof(BlobBuilderExtensions);

// Claims: ConstructHashMap from NativeParallelHashMap
var constructHM = extType.GetMethods().Where(m => m.Name == "ConstructHashMap").ToArray();
t("ConstructHashMap: has overloads", constructHM.Length >= 2);

// Verify the first overload accepts NativeParallelHashMap
var nativeOverload = constructHM.FirstOrDefault(m => {
    var p = m.GetParameters();
    return p.Any(pp => pp.ParameterType.Name.Contains("NativeParallelHashMap"));
});
t("ConstructHashMap: has NativeParallelHashMap overload", nativeOverload != null);

// Verify the second overload accepts Dictionary
var dictOverload = constructHM.FirstOrDefault(m => {
    var p = m.GetParameters();
    return p.Any(pp => pp.ParameterType.Name.Contains("Dictionary"));
});
t("ConstructHashMap: has Dictionary overload", dictOverload != null);

// Claims: AllocateHashMap returns BlobBuilderHashMap
var allocHM = extType.GetMethods().Where(m => m.Name == "AllocateHashMap").ToArray();
t("ConstructHashMap: AllocateHashMap has overloads", allocHM.Length >= 2);

// Verify return type is BlobBuilderHashMap
var hmReturnType = allocHM[0].ReturnType;
t("ConstructHashMap: AllocateHashMap returns BlobBuilderHashMap", hmReturnType.Name.StartsWith("BlobBuilderHashMap"));

// Claims: ConstructMultiHashMap exists
var constructMulti = extType.GetMethods().Where(m => m.Name == "ConstructMultiHashMap").ToArray();
t("ConstructHashMap: ConstructMultiHashMap exists", constructMulti.Length >= 1);

// Claims: AllocateMultiHashMap returns BlobBuilderMultiHashMap
var allocMulti = extType.GetMethods().Where(m => m.Name == "AllocateMultiHashMap").ToArray();
t("ConstructHashMap: AllocateMultiHashMap has overloads", allocMulti.Length >= 1);
t("ConstructHashMap: AllocateMultiHashMap returns BlobBuilderMultiHashMap", allocMulti[0].ReturnType.Name.StartsWith("BlobBuilderMultiHashMap"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
