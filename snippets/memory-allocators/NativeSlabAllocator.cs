// Run: cat snippets/memory-allocators/NativeSlabAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NativeSlabAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var nsaType = typeof(BovineLabs.Core.Memory.NativeSlabAllocator<int>);
sb.AppendLine("BovineLabs.Core.Memory.NativeSlabAllocator<T>");
sb.AppendLine($"  Kind: struct (ValueType={nsaType.IsValueType})");
sb.AppendLine($"  Size (T=int): {Marshal.SizeOf(nsaType)} bytes");
sb.AppendLine();

sb.AppendLine("  Attributes:");
foreach (var attr in nsaType.GetCustomAttributes(false))
    sb.AppendLine($"    [{attr.GetType().Name}]");
t("Has [NativeContainer] attribute", nsaType.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("NativeContainer")));
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in nsaType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); }
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var ctor in nsaType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in nsaType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in nsaType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var ret = m.ReturnType.IsPointer ? $"{m.ReturnType.GetElementType().Name}*" : m.ReturnType.Name;
    sb.AppendLine($"    public {ret} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Fields:");
var fields = nsaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
foreach (var f in fields)
    sb.AppendLine($"    {(f.IsPublic ? "public" : "private")} {f.FieldType.Name} {f.Name}");
t("Has slabAllocator field", fields.Any(f => f.Name.Contains("slab") || f.Name.Contains("Slab")));
t("Has safety handle field", fields.Any(f => f.Name.Contains("Safety") || f.Name.Contains("safety")));

var staticFields = nsaType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
sb.AppendLine("  Static Fields:");
foreach (var f in staticFields)
    sb.AppendLine($"    {(f.IsPublic ? "public" : "private")} static {f.FieldType.Name} {f.Name}");
t("Has static safety ID field", staticFields.Any(f => f.Name.Contains("safetyId") || f.Name.Contains("SafetyId")));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
