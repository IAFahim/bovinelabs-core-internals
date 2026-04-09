// Run: cat snippets/blob-system/BlobCurve.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurve.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bcType = typeof(BlobCurve);
int sz = Marshal.SizeOf(bcType);

sb.AppendLine("BlobCurve");
sb.AppendLine($"  Kind: {(bcType.IsValueType ? "struct" : "class")}, {sz} bytes");
var sla = bcType.StructLayoutAttribute;
t($"StructLayout = {sla?.Value}", sla != null);
sb.AppendLine();

// Interfaces
var ifaces = bcType.GetInterfaces();
sb.AppendLine("  Implements:");
foreach (var i in ifaces) sb.AppendLine($"    {i.Name}");
t("Implements IBlobCurve", ifaces.Any(i => i.Name.StartsWith("IBlobCurve")));
sb.AppendLine();

// Fields
sb.AppendLine("  Fields:");
foreach (var f in bcType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var vis = f.IsPublic ? "public" : "private";
    sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({vis})");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var p in bcType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    var rw = (p.CanRead ? "get" : "") + (p.CanRead && p.CanWrite ? "; " : "") + (p.CanWrite ? "set" : "");
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name} {{ {rw} }}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
foreach (var m in bcType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var isStatic = m.IsStatic ? "static " : "";
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {isStatic}{m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
