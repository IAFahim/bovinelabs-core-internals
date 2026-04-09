// Run: cat snippets/blob-system/BlobCurveSegment.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveSegment.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var segType = typeof(BlobCurveSegment);
int sz = Marshal.SizeOf(segType);

sb.AppendLine("BlobCurveSegment");
sb.AppendLine($"  Kind: {(segType.IsValueType ? "readonly struct" : "class")}, {sz} bytes");
sb.AppendLine();

sb.AppendLine("  Fields:");
foreach (var f in segType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var ro = f.IsInitOnly ? " (initonly)" : "";
    sb.AppendLine($"    {f.FieldType.Name} {f.Name}{ro} ({(f.IsPublic ? "public" : "private")})");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var c in segType.GetConstructors())
{
    var ps = string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    BlobCurveSegment({ps})");
    t($"Ctor({ps})", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in segType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
