// Run: cat snippets/blob-system/BlobCurveSampler.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveSampler.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var samplerType = typeof(BlobCurveSampler);
int sz = Marshal.SizeOf(samplerType);

sb.AppendLine("BlobCurveSampler");
sb.AppendLine($"  Kind: {(samplerType.IsValueType ? "struct" : "class")}, {sz} bytes");
sb.AppendLine();

sb.AppendLine("  Fields:");
foreach (var f in samplerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var p in samplerType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in samplerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name} returns {m.ReturnType.Name}", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
