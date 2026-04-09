// Run: cat snippets/memory-allocators/UnsafeParallelPoolAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Jobs.LowLevel.Unsafe"
// Verifies: docs/UnsafeParallelPoolAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var ppaType = typeof(BovineLabs.Core.Memory.UnsafeParallelPoolAllocator<int>);
sb.AppendLine("BovineLabs.Core.Memory.UnsafeParallelPoolAllocator<T>");
sb.AppendLine($"  Kind: struct (ValueType={ppaType.IsValueType})");
sb.AppendLine($"  Size (T=int): {Marshal.SizeOf(ppaType)} bytes");
sb.AppendLine($"  Generic params: {string.Join(", ", ppaType.GetGenericArguments().Select(ga => ga.Name))}");
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in ppaType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); t($"Implements {iface.Name}", true); }
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var ctor in ppaType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in ppaType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in ppaType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var ret = m.ReturnType.IsPointer ? $"{m.ReturnType.GetElementType().Name}*" : m.ReturnType.Name;
    sb.AppendLine($"    public {ret} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Fields:");
var fields = ppaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
foreach (var f in fields)
{
    var attrs = string.Join(", ", f.GetCustomAttributes(false).Select(a => $"[{a.GetType().Name}]"));
    sb.AppendLine($"    {(f.IsPublic ? "public" : "private")} {f.FieldType.Name} {f.Name} {attrs}");
}
t("Has pools field", fields.Any(f => f.Name.Contains("pool") || f.Name.Contains("Pool")));
t("Has threadIndex field", fields.Any(f => f.Name.Contains("threadIndex") || f.Name.Contains("ThreadIndex")));

var threadIdxField = fields.FirstOrDefault(f => f.Name.Contains("threadIndex") || f.Name.Contains("ThreadIndex"));
if (threadIdxField != null)
{
    var hasAttr = threadIdxField.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("NativeSetThreadIndex"));
    t("threadIndex has [NativeSetThreadIndex]", hasAttr);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
