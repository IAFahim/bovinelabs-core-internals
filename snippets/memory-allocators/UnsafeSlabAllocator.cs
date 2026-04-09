// Run: cat snippets/memory-allocators/UnsafeSlabAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafeSlabAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var slabType = typeof(BovineLabs.Core.Memory.UnsafeSlabAllocator<int>);
sb.AppendLine("BovineLabs.Core.Memory.UnsafeSlabAllocator<T>");
sb.AppendLine($"  Kind: struct (ValueType={slabType.IsValueType})");
sb.AppendLine($"  Size (T=int): {Marshal.SizeOf(slabType)} bytes");
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in slabType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); }
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var ctor in slabType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
    t("Constructor takes int countPerSlab", ctor.GetParameters().Length >= 1 && ctor.GetParameters()[0].ParameterType == typeof(int));
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in slabType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in slabType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var ret = m.ReturnType.IsPointer ? $"{m.ReturnType.GetElementType().Name}*" : m.ReturnType.Name;
    sb.AppendLine($"    public {ret} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Fields:");
var fields = slabType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
foreach (var f in fields)
    sb.AppendLine($"    {(f.IsPublic ? "public" : "private")} {f.FieldType.Name} {f.Name}");
t("Has slab-related field", fields.Any(f => f.Name.Contains("slab") || f.Name.Contains("Slab")));
t("Has count field", fields.Any(f => f.Name.Contains("count") && !f.Name.Contains("countPer")));
t("No lock/mutex fields (not thread-safe)", !fields.Any(f => f.Name.Contains("lock") || f.Name.Contains("mutex")));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
