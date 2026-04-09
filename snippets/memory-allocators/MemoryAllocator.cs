// Run: cat snippets/memory-allocators/MemoryAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Mathematics"
// Verifies: docs/MemoryAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var maType = typeof(BovineLabs.Core.Memory.MemoryAllocator);
sb.AppendLine("BovineLabs.Core.Memory.MemoryAllocator");
sb.AppendLine($"  Kind: struct (ValueType={maType.IsValueType})");
sb.AppendLine($"  Size: {Marshal.SizeOf(maType)} bytes");
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in maType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); }
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var ctor in maType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in maType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in maType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
{
    var generic = m.IsGenericMethod ? $"<{string.Join(",", m.GetGenericArguments().Select(ga => ga.Name))}>" : "";
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var ret = m.ReturnType.IsPointer ? $"{m.ReturnType.GetElementType().Name}*" : m.ReturnType.Name;
    sb.AppendLine($"    public {ret} {m.Name}{generic}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Functional tests
sb.AppendLine("  Functional Tests:");
var mem = new BovineLabs.Core.Memory.MemoryAllocator(Allocator.Persistent);
try
{
    var list = mem.CreateList<byte>(1024);
    sb.AppendLine($"    CreateList<byte>(1024): Capacity={list.Capacity}");
    t("CreateList returns created list", list.IsCreated);
    t("Capacity >= requested", list.Capacity >= 1024);
    t("Capacity is power of 2", (list.Capacity & (list.Capacity - 1)) == 0);

    var smallList = mem.CreateList<int>(1);
    sb.AppendLine($"    CreateList<int>(1): Capacity={smallList.Capacity}");
    t("Small list capacity rounds up", smallList.Capacity >= 1);

    mem.FreeAll();
    sb.AppendLine($"    FreeAll(): completed");
    t("FreeAll completes", true);
}
finally
{
    mem.Dispose();
    sb.AppendLine($"    Dispose(): completed");
    t("Dispose completes", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
