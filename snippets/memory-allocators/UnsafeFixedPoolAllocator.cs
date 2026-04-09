// Run: cat snippets/memory-allocators/UnsafeFixedPoolAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafeFixedPoolAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var fpaType = typeof(BovineLabs.Core.Memory.UnsafeFixedPoolAllocator<int>);
sb.AppendLine("BovineLabs.Core.Memory.UnsafeFixedPoolAllocator<T>");
sb.AppendLine($"  Kind: struct (ValueType={fpaType.IsValueType})");
sb.AppendLine($"  Size (T=int): {Marshal.SizeOf(fpaType)} bytes");
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in fpaType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); }
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var ctor in fpaType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in fpaType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in fpaType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var ret = m.ReturnType.IsPointer ? $"{m.ReturnType.GetElementType().Name}*" : m.ReturnType.Name;
    sb.AppendLine($"    public {ret} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Fields:");
var fields = fpaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
foreach (var f in fields)
    sb.AppendLine($"    {(f.IsPublic ? "public" : "private")} {f.FieldType.Name} {f.Name}");
t("Has maxItems field", fields.Any(f => f.Name.Contains("maxItems") || f.Name.Contains("MaxItems")));
t("Has buffer field", fields.Any(f => f.Name.Contains("buffer") || f.Name.Contains("Buffer")));
t("Has freeIndex field", fields.Any(f => f.Name.Contains("freeIndex") || f.Name.Contains("FreeIndex")));

var validateMethod = fpaType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
    .Where(m => m.Name.Contains("Validate")).FirstOrDefault();
t("Has ValidatePtr method (editor safety)", validateMethod != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
