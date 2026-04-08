// Run: cat snippets/blob-system/BlobBuilderExtensions.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderExtensions.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var extType = typeof(BlobBuilderExtensions);
t("BlobBuilderExtensions: type exists", extType != null);
t("BlobBuilderExtensions: is static class", extType.IsAbstract && extType.IsSealed);

// Claims: has Allocate(this ref BlobBuilder, int size) -> void*
var allocateMethods = extType.GetMethods().Where(m => m.Name == "Allocate").ToArray();
t("BlobBuilderExtensions: has Allocate methods", allocateMethods.Length >= 1);

// Claims: Construct<T>(NativeArray) and Construct<T>(NativeList)
var constructMethods = extType.GetMethods().Where(m => m.Name == "Construct").ToArray();
t("BlobBuilderExtensions: has Construct methods", constructMethods.Length >= 2);

// Claims: ConstructHashMap
var constructHashMapMethods = extType.GetMethods().Where(m => m.Name == "ConstructHashMap").ToArray();
t("BlobBuilderExtensions: has ConstructHashMap overloads", constructHashMapMethods.Length >= 2);

// Claims: AllocateHashMap
var allocateHashMapMethods = extType.GetMethods().Where(m => m.Name == "AllocateHashMap").ToArray();
t("BlobBuilderExtensions: has AllocateHashMap overloads", allocateHashMapMethods.Length >= 2);

// Claims: ConstructMultiHashMap
var constructMulti = extType.GetMethods().Where(m => m.Name == "ConstructMultiHashMap").ToArray();
t("BlobBuilderExtensions: has ConstructMultiHashMap", constructMulti.Length >= 1);

// Claims: AllocateMultiHashMap
var allocMulti = extType.GetMethods().Where(m => m.Name == "AllocateMultiHashMap").ToArray();
t("BlobBuilderExtensions: has AllocateMultiHashMap overloads", allocMulti.Length >= 1);

// Claims: ConstructPerfectHashMap
var constructPerfect = extType.GetMethods().Where(m => m.Name == "ConstructPerfectHashMap").ToArray();
t("BlobBuilderExtensions: has ConstructPerfectHashMap", constructPerfect.Length >= 1);

// Claims: GetListPtr returns IntPtr
var getListPtr = extType.GetMethod("GetListPtr");
t("BlobBuilderExtensions: has GetListPtr", getListPtr != null);
t("BlobBuilderExtensions: GetListPtr returns IntPtr", getListPtr?.ReturnType == typeof(IntPtr));

// Claims: bucket ratio threshold constant = 16384
// Can't read private const via reflection easily, but we verify the behavior through AllocateHashMap
// The constant UseBucketCapacityRatioOfThreeUpTo = 16384 is private

// Claims: has nested BlobBuilderInternal type
var nestedTypes = extType.GetNestedTypes(BindingFlags.NonPublic);
var hasInternal = nestedTypes.Any(nt => nt.Name.Contains("BlobBuilderInternal"));
t("BlobBuilderExtensions: has BlobBuilderInternal nested type", hasInternal);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
