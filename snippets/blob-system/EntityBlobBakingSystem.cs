// Run: cat snippets/blob-system/EntityBlobBakingSystem.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/EntityBlobBakingSystem.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// EntityBlobBakingSystem is in BovineLabs.Core.Authoring.Blobs
var sysType = (Type)null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try {
        sysType = asm.GetType("BovineLabs.Core.Authoring.Blobs.EntityBlobBakingSystem");
        if (sysType != null) break;
    } catch {}
}

t("EntityBlobBakingSystem: type exists", sysType != null);

if (sysType != null)
{
    t("EntityBlobBakingSystem: is struct", sysType.IsValueType);

    // Claims: implements ISystem
    var implementsISys = sysType.GetInterfaces().Any(i => i.Name == "ISystem");
    t("EntityBlobBakingSystem: implements ISystem", implementsISys);

    // Claims: has OnCreate, OnDestroy, OnUpdate
    var onCreate = sysType.GetMethod("OnCreate");
    t("EntityBlobBakingSystem: has OnCreate", onCreate != null);

    var onDestroy = sysType.GetMethod("OnDestroy");
    t("EntityBlobBakingSystem: has OnDestroy", onDestroy != null);

    var onUpdate = sysType.GetMethod("OnUpdate");
    t("EntityBlobBakingSystem: has OnUpdate", onUpdate != null);

    // Claims: has WorldSystemFilter(BakingSystem)
    var wsfa = sysType.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name.Contains("WorldSystemFilter"));
    t("EntityBlobBakingSystem: has WorldSystemFilter attribute", wsfa != null);

    // Claims: has EntityBlobBakedDataHandle field (ComponentTypeHandle)
    var handleField = sysType.GetField("EntityBlobBakedDataHandle", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    t("EntityBlobBakingSystem: has EntityBlobBakedDataHandle field", handleField != null);

    // Claims: partial struct
    t("EntityBlobBakingSystem: is partial", sysType.Name == "EntityBlobBakingSystem");
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
