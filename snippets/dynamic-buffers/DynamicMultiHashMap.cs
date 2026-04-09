// Run: cat snippets/dynamic-buffers/DynamicMultiHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicMultiHashMap.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

var mmapType = typeof(DynamicMultiHashMap<int, byte>);
sb.AppendLine("DynamicMultiHashMap<TKey,TValue>");
sb.AppendLine($"  Kind: {(mmapType.IsValueType ? "struct" : "class")}, {Marshal.SizeOf(mmapType)} bytes");
sb.AppendLine();

sb.AppendLine("  Interfaces:");
foreach (var iface in mmapType.GetInterfaces()) { sb.AppendLine($"    {iface.FullName}"); }
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var prop in mmapType.GetProperties(bf))
{
    sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in mmapType.GetMethods(bf).Where(m => !m.IsSpecialName && (m.IsPublic || m.IsPrivate))
    .OrderBy(m => m.Name))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {(m.IsPublic ? "public" : "private")} {m.ReturnType.Name} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  DynamicHashMapHelper<TKey>");
var helperType = typeof(DynamicHashMapHelper<int>);
sb.AppendLine($"    Kind: {(helperType.IsValueType ? "struct" : "class")}, {Marshal.SizeOf(helperType)} bytes");
var sla = helperType.StructLayoutAttribute;
sb.AppendLine($"    Layout: {sla?.Value.ToString() ?? "default"}");

sb.AppendLine("    Fields:");
var fields = helperType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
foreach (var f in fields)
    sb.AppendLine($"      {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
t("Helper has 11 fields (all int)", fields.Length == 11 && fields.All(f => f.FieldType == typeof(int)));
t("Helper size == 44 bytes", Marshal.SizeOf<DynamicHashMapHelper<int>>() == 44);
sb.AppendLine();

sb.AppendLine("  IDynamicMultiHashMap<TKey,TValue>");
var ifaceType = typeof(IDynamicMultiHashMap<int, byte>);
sb.AppendLine($"    IsInterface: {ifaceType.IsInterface}");
sb.AppendLine($"    Implements IBufferElementData: {ifaceType.GetInterfaces().Contains(typeof(IBufferElementData))}");
var valueProp = ifaceType.GetProperty("Value");
sb.AppendLine($"    Value property: {valueProp?.PropertyType.Name ?? "not found"}");
t("IDynamicMultiHashMap is IBufferElementData with byte Value", ifaceType.IsInterface && valueProp?.PropertyType == typeof(byte));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
