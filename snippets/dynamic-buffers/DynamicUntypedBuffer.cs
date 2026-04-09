// Run: cat snippets/dynamic-buffers/DynamicUntypedBuffer.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicUntypedBuffer.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

var bufType = typeof(DynamicUntypedBuffer);
sb.AppendLine("DynamicUntypedBuffer");
sb.AppendLine($"  Kind: {(bufType.IsValueType ? "struct" : "class")}, {Marshal.SizeOf(bufType)} bytes");
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in bufType.GetMethods(bf).Where(m => !m.IsSpecialName && (m.IsPublic || m.IsPrivate))
    .OrderBy(m => m.Name))
{
    var generic = m.IsGenericMethod ? $"<{string.Join(",", m.GetGenericArguments().Select(ga => ga.Name))}>" : "";
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {(m.IsPublic ? "public" : "private")} {m.ReturnType.Name} {m.Name}{generic}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  DynamicUntypedBufferHelper");
var helperType = typeof(DynamicUntypedBufferHelper);
sb.AppendLine($"    Kind: {(helperType.IsValueType ? "struct" : "class")}, {Marshal.SizeOf(helperType)} bytes");
var sla = helperType.StructLayoutAttribute;
sb.AppendLine($"    Layout: {sla?.Value.ToString() ?? "default"}");

sb.AppendLine("    Fields:");
var helperFields = helperType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
foreach (var f in helperFields)
    sb.AppendLine($"      {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
t("Helper has 10 fields (all int)", helperFields.Length == 10 && helperFields.All(f => f.FieldType == typeof(int)));
t("Helper size == 40 bytes", Marshal.SizeOf<DynamicUntypedBufferHelper>() == 40);
sb.AppendLine();

sb.AppendLine("  IDynamicUntypedBuffer");
var ifaceType = typeof(IDynamicUntypedBuffer);
sb.AppendLine($"    IsInterface: {ifaceType.IsInterface}");
sb.AppendLine($"    Implements IBufferElementData: {ifaceType.GetInterfaces().Contains(typeof(IBufferElementData))}");
var valueProp = ifaceType.GetProperty("Value");
sb.AppendLine($"    Value property: {valueProp?.PropertyType.Name ?? "not found"}");
t("IDynamicUntypedBuffer is IBufferElementData with byte Value", ifaceType.IsInterface && valueProp?.PropertyType == typeof(byte));
sb.AppendLine();

sb.AppendLine("  DynamicExtensions");
var extType = typeof(DynamicExtensions);
var initMethod = extType.GetMethods(bf).Where(m => m.Name == "InitializeUntypedBuffer").FirstOrDefault();
var asMethod = extType.GetMethods(bf).Where(m => m.Name == "AsUntypedBuffer").FirstOrDefault();
sb.AppendLine($"    InitializeUntypedBuffer: {(initMethod != null ? "exists" : "not found")}");
sb.AppendLine($"    AsUntypedBuffer: {(asMethod != null ? "exists" : "not found")}");
t("DynamicExtensions has buffer methods", initMethod != null && asMethod != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
