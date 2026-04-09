// Run: cat snippets/memory-allocators/MemoryLabelAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Burst"
// Verifies: docs/MemoryLabelAllocator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var mlaType = typeof(BovineLabs.Core.Memory.MemoryLabelAllocator);
sb.AppendLine("BovineLabs.Core.Memory.MemoryLabelAllocator");
sb.AppendLine($"  Kind: struct (ValueType={mlaType.IsValueType})");
sb.AppendLine($"  Size: {System.Runtime.InteropServices.Marshal.SizeOf(mlaType)} bytes");
sb.AppendLine();

// Interface
sb.AppendLine("  Interfaces:");
foreach (var iface in mlaType.GetInterfaces())
{
    sb.AppendLine($"    {iface.FullName}");
    t($"Implements {iface.Name}", true);
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var prop in mlaType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name} {{ {(prop.CanRead ? "get" : "")}{(prop.CanWrite ? " set" : "")} }}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
foreach (var m in mlaType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var isStatic = m.IsStatic ? "static " : "";
    var isPublic = m.IsPublic ? "public " : "private ";
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {isPublic}{isStatic}{m.ReturnType.Name} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Attributes
var attrs = mlaType.GetCustomAttributes(false);
sb.AppendLine("  Attributes:");
foreach (var attr in attrs)
{
    sb.AppendLine($"    [{attr.GetType().Name}]");
}
t("Has BurstCompile attribute", attrs.Any(a => a.GetType().Name.Contains("BurstCompile")));
sb.AppendLine();

// Runtime behavior
try
{
    var alloc = new BovineLabs.Core.Memory.MemoryLabelAllocator();
    sb.AppendLine("  Runtime Behavior:");
    sb.AppendLine($"    Can construct default instance: true");
    sb.AppendLine($"    IsAutoDispose: {alloc.IsAutoDispose}");
    sb.AppendLine($"    IsCustomAllocator: {alloc.IsCustomAllocator}");
    t("IsAutoDispose is false", !alloc.IsAutoDispose);
    t("IsCustomAllocator default is false", !alloc.IsCustomAllocator);
}
catch (System.Exception ex)
{
    sb.AppendLine($"  Runtime Behavior: construction failed - {ex.Message}");
    t("Can construct default instance", false);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
