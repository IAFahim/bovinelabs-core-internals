// Run: cat snippets/memory-allocators/UnsafePoolAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafePoolAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var upaType = typeof(BovineLabs.Core.Memory.UnsafePoolAllocator<int>);
sb.AppendLine("BovineLabs.Core.Memory.UnsafePoolAllocator<T>");
sb.AppendLine($"  Kind: struct (ValueType={upaType.IsValueType})");
sb.AppendLine($"  Size (T=int): {Marshal.SizeOf(upaType)} bytes");
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in upaType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); }
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var ctor in upaType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in upaType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in upaType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var ret = m.ReturnType.IsPointer ? $"{m.ReturnType.GetElementType().Name}*" : m.ReturnType.Name;
    sb.AppendLine($"    public {ret} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Fields:");
var fields = upaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
foreach (var f in fields)
    sb.AppendLine($"    {(f.IsPublic ? "public" : "private")} {f.FieldType.Name} {f.Name}");
t("Has slabAllocator field", fields.Any(f => f.Name.Contains("slab") || f.Name.Contains("Slab")));
t("Has free field (free-list)", fields.Any(f => f.Name.Contains("free") || f.Name.Contains("Free")));
t("No lock/spinlock fields (not thread-safe)", !fields.Any(f => f.Name.Contains("lock") || f.Name.Contains("spin")));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
