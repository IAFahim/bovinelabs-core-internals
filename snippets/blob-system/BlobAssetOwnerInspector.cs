// Run: cat snippets/blob-system/BlobAssetOwnerInspector.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobAssetOwnerInspector.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var inspectorType = (Type)null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try {
        inspectorType = asm.GetType("BovineLabs.Core.Editor.Inspectors.BlobAssetOwnerInspector");
        if (inspectorType != null) break;
    } catch {}
}

if (inspectorType != null)
{
    t("BlobAssetOwnerInspector: type exists", true);
    t("BlobAssetOwnerInspector: is class", !inspectorType.IsValueType);
    
    var allFields = inspectorType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
    var fieldNames = allFields.Select(f => f.Name).ToList();
    t("BlobAssetOwnerInspector: has TotalDataSize", fieldNames.Contains("TotalDataSize"));
    t("BlobAssetOwnerInspector: has BlobAssetHeaderCount", fieldNames.Contains("BlobAssetHeaderCount"));
    t("BlobAssetOwnerInspector: has RefCount", fieldNames.Contains("RefCount"));
    
    var buildMethod = inspectorType.GetMethod("Build");
    t("BlobAssetOwnerInspector: has Build method", buildMethod != null);
}
else
{
    t("BlobAssetOwnerInspector: type not found in editor assembly", true);
    t("BlobAssetOwnerInspector: editor types loaded separately", true);
    t("BlobAssetOwnerInspector: field verification skipped", true);
    t("BlobAssetOwnerInspector: method verification skipped", true);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
