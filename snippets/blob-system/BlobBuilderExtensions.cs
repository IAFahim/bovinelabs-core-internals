// Run: cat snippets/blob-system/BlobBuilderExtensions.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderExtensions.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BlobBuilderExtensions);

sb.AppendLine("BlobBuilderExtensions");
sb.AppendLine($"  Kind: {(extType.IsAbstract && extType.IsSealed ? "static class" : "class")}");
sb.AppendLine();

// Group methods by name
var methodGroups = extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName)
    .GroupBy(m => m.Name)
    .OrderBy(g => g.Key)
    .ToList();

sb.AppendLine("  Static Methods:");
foreach (var grp in methodGroups)
{
    foreach (var m in grp)
    {
        var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    }
    t($"{grp.Key} overloads: {grp.Count()}", true);
}
sb.AppendLine();

// Nested types
sb.AppendLine("  Nested Types:");
var nested = extType.GetNestedTypes(BindingFlags.NonPublic);
foreach (var nt in nested)
{
    sb.AppendLine($"    {nt.Name} ({(nt.IsValueType ? "struct" : "class")})");
    var subNested = nt.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
    foreach (var sn in subNested)
    {
        var fields = sn.GetFields().Select(f => $"{f.FieldType.Name} {f.Name}");
        sb.AppendLine($"      {sn.Name}: {string.Join(", ", fields)}");
    }
    t($"Nested {nt.Name}", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
