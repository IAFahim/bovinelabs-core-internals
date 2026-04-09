// Run: cat snippets/blob-system/BlobBuilderExtensions_ConstructHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderExtensions_ConstructHashMap.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BlobBuilderExtensions);

sb.AppendLine("BlobBuilderExtensions — HashMap Construction Methods");
sb.AppendLine();

string[] methodNames = { "ConstructHashMap", "AllocateHashMap", "ConstructMultiHashMap", "AllocateMultiHashMap", "ConstructPerfectHashMap", "AllocatePerfectHashMap" };

foreach (var mn in methodNames)
{
    var overloads = extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => m.Name == mn).ToArray();
    sb.AppendLine($"  {mn}: {overloads.Length} overload(s)");
    foreach (var m in overloads)
    {
        var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        sb.AppendLine($"    {m.ReturnType.Name} {mn}({ps})");
    }
    t($"{mn} has overloads", overloads.Length >= 1);
}
sb.AppendLine();

// Verify return types
var allocHM = extType.GetMethods().Where(m => m.Name == "AllocateHashMap").ToArray();
if (allocHM.Length > 0)
{
    sb.AppendLine($"  AllocateHashMap returns: {allocHM[0].ReturnType.Name}");
    t("AllocateHashMap returns BlobBuilderHashMap", allocHM[0].ReturnType.Name.StartsWith("BlobBuilderHashMap"));
}

var allocMHM = extType.GetMethods().Where(m => m.Name == "AllocateMultiHashMap").ToArray();
if (allocMHM.Length > 0)
{
    sb.AppendLine($"  AllocateMultiHashMap returns: {allocMHM[0].ReturnType.Name}");
    t("AllocateMultiHashMap returns BlobBuilderMultiHashMap", allocMHM[0].ReturnType.Name.StartsWith("BlobBuilderMultiHashMap"));
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
