// Run: cat snippets/blob-system/EntityBlobBakedData.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/EntityBlobBakedData.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// EntityBlobBakedData is in BovineLabs.Core.Authoring.Blobs
var dataAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
var ebbType = (Type)null;
foreach (var asm in dataAssemblies)
{
    try {
        ebbType = asm.GetType("BovineLabs.Core.Authoring.Blobs.EntityBlobBakedData");
        if (ebbType != null) break;
    } catch {}
}

t("EntityBlobBakedData: type exists", ebbType != null);

if (ebbType != null)
{
    t("EntityBlobBakedData: is struct", ebbType.IsValueType);

    // Claims: implements IComponentData
    var implementsICD = ebbType.GetInterfaces().Any(i => i.Name.StartsWith("IComponentData"));
    t("EntityBlobBakedData: implements IComponentData", implementsICD);

    // Claims: has BakingType attribute
    var hasBakingType = ebbType.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("BakingType"));
    t("EntityBlobBakedData: has BakingType attribute", hasBakingType);

    // Claims: Target field (Entity)
    var targetField = ebbType.GetField("Target");
    t("EntityBlobBakedData: has Target field", targetField != null);
    t("EntityBlobBakedData: Target is Entity", targetField?.FieldType.Name == "Entity");

    // Claims: Key field (int)
    var keyField = ebbType.GetField("Key");
    t("EntityBlobBakedData: has Key field", keyField != null);
    t("EntityBlobBakedData: Key is int", keyField?.FieldType == typeof(int));

    // Claims: Blob field (BlobAssetReference<byte>)
    var blobField = ebbType.GetField("Blob");
    t("EntityBlobBakedData: has Blob field", blobField != null);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
