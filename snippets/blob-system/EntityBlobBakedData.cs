// Run: cat snippets/blob-system/EntityBlobBakedData.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/EntityBlobBakedData.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var ebbType = (Type)null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try {
        ebbType = asm.GetType("BovineLabs.Core.Authoring.Blobs.EntityBlobBakedData");
        if (ebbType != null) break;
    } catch {}
}

sb.AppendLine("EntityBlobBakedData");
sb.AppendLine($"  Namespace: BovineLabs.Core.Authoring.Blobs");

if (ebbType != null)
{
    int sz = Marshal.SizeOf(ebbType);
    sb.AppendLine($"  Kind: {(ebbType.IsValueType ? "struct" : "class")}, {sz} bytes");

    var ifaces = ebbType.GetInterfaces();
    sb.AppendLine($"  Implements: {string.Join(", ", ifaces.Select(i => i.Name))}");
    t("Implements IComponentData", ifaces.Any(i => i.Name.StartsWith("IComponentData")));

    var attrs = ebbType.GetCustomAttributes(false);
    sb.AppendLine($"  Attributes: {string.Join(", ", attrs.Select(a => a.GetType().Name))}");
    t("Has BakingType attribute", attrs.Any(a => a.GetType().Name.Contains("BakingType")));
    sb.AppendLine();

    sb.AppendLine("  Fields:");
    foreach (var f in ebbType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    {
        sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
        t($"Field {f.Name}", true);
    }
}
else
{
    sb.AppendLine("  Status: Type not found in loaded assemblies");
    t("EntityBlobBakedData found", false);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
