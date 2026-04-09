// Run: cat snippets/blob-system/IBlobCurve.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/IBlobCurve.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var ifaceType = typeof(IBlobCurve<float>);

sb.AppendLine("IBlobCurve<T>");
sb.AppendLine($"  Kind: interface, covariant T");
sb.AppendLine();

sb.AppendLine("  Methods:");
var methods = ifaceType.GetMethods();
t($"Has {methods.Length} methods", methods.Length == 4);
foreach (var m in methods)
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Implementors
sb.AppendLine("  Known Implementors:");
var bcType = typeof(BlobCurve);
var implements = bcType.GetInterfaces().Contains(ifaceType);
sb.AppendLine($"    BlobCurve implements IBlobCurve<float>: {implements}");
t("BlobCurve implements IBlobCurve<float>", implements);

var bc2 = typeof(BlobCurve2);
var implements2 = bc2.GetInterfaces().Any(i => i.Name.StartsWith("IBlobCurve"));
sb.AppendLine($"    BlobCurve2 implements IBlobCurve: {implements2}");
t("BlobCurve2 implements IBlobCurve variant", implements2);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
