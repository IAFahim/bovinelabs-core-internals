// Run: cat snippets/blob-system/BlobCurve2_3_4.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurve2_3_4.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

void InspectCurve(Type tp, string label)
{
    int sz = Marshal.SizeOf(tp);
    sb.AppendLine($"{label}");
    sb.AppendLine($"  Kind: {(tp.IsValueType ? "struct" : "class")}, {sz} bytes");

    var ifaces = tp.GetInterfaces();
    foreach (var i in ifaces) sb.AppendLine($"  Implements: {i.Name}");

    sb.AppendLine("  Properties:");
    foreach (var p in tp.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
        var rw = (p.CanRead ? "get" : "") + (p.CanRead && p.CanWrite ? "; " : "") + (p.CanWrite ? "set" : "");
        sb.AppendLine($"    {p.PropertyType.Name} {p.Name} {{ {rw} }}");
    }

    sb.AppendLine("  Methods:");
    foreach (var m in tp.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName))
    {
        var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
        sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
        t($"{label}.{m.Name}", true);
    }
    sb.AppendLine();
}

InspectCurve(typeof(BlobCurve2), "BlobCurve2");
InspectCurve(typeof(BlobCurve3), "BlobCurve3");
InspectCurve(typeof(BlobCurve4), "BlobCurve4");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
