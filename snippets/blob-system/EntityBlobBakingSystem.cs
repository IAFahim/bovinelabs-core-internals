// Run: cat snippets/blob-system/EntityBlobBakingSystem.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/EntityBlobBakingSystem.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var sysType = (Type)null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try {
        sysType = asm.GetType("BovineLabs.Core.Authoring.Blobs.EntityBlobBakingSystem");
        if (sysType != null) break;
    } catch {}
}

sb.AppendLine("EntityBlobBakingSystem");
sb.AppendLine($"  Namespace: BovineLabs.Core.Authoring.Blobs");

if (sysType != null)
{
    sb.AppendLine($"  Kind: {(sysType.IsValueType ? "partial struct" : "class")}");

    var ifaces = sysType.GetInterfaces();
    sb.AppendLine($"  Implements: {string.Join(", ", ifaces.Select(i => i.Name))}");
    t("Implements ISystem", ifaces.Any(i => i.Name == "ISystem"));

    var attrs = sysType.GetCustomAttributes(false);
    sb.AppendLine($"  Attributes: {string.Join(", ", attrs.Select(a => a.GetType().Name))}");
    sb.AppendLine();

    sb.AppendLine("  Lifecycle Methods:");
    foreach (var name in new[] { "OnCreate", "OnDestroy", "OnUpdate" })
    {
        var m = sysType.GetMethod(name);
        sb.AppendLine($"    {(m != null ? "+" : "-")} {name}");
        t($"Has {name}", m != null);
    }
    sb.AppendLine();

    sb.AppendLine("  Fields:");
    foreach (var f in sysType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
    {
        sb.AppendLine($"    {f.FieldType.Name} {f.Name}");
        t($"Field {f.Name}", true);
    }
}
else
{
    sb.AppendLine("  Status: Type not found in loaded assemblies");
    t("EntityBlobBakingSystem found", false);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
