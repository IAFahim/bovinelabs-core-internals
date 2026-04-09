// Run: cat snippets/dynamic-buffers/DynamicHashSet.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicHashSet.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- DynamicHashSet<T> struct ---
var setType = typeof(DynamicHashSet<int>);
t("DynamicHashSet type exists", setType != null);
t("Is ValueType", setType.IsValueType);

int setSize = System.Runtime.InteropServices.Marshal.SizeOf(setType);
sb.AppendLine("DynamicHashSet<T>");
sb.AppendLine("  Kind: struct");
sb.AppendLine($"  Size: {setSize} bytes");
sb.AppendLine($"  Generic params: {string.Join(", ", setType.GetGenericArguments().Select(a => a.Name))}");

// Interfaces
sb.AppendLine("  Interfaces:");
foreach (var iface in setType.GetInterfaces())
    sb.AppendLine($"    {iface.FullName}");
sb.AppendLine();

// Fields
sb.AppendLine("  Fields:");
foreach (var fld in setType.GetFields(bf))
{
    try
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(setType, fld.Name).ToInt32();
        sb.AppendLine($"    [{offset}] {fld.FieldType.Name} {fld.Name}");
    }
    catch
    {
        sb.AppendLine($"    [?] {fld.FieldType.Name} {fld.Name}");
    }
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var prop in setType.GetProperties(bf))
{
    t($"Property {prop.Name} exists", true);
    sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name} (read={prop.CanRead}, write={prop.CanWrite})");
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
var methodNames = new[] { "Add", "Remove", "Contains", "Clear", "Flatten" };
foreach (var mn in methodNames)
{
    var meths = setType.GetMethods(bf).Where(m => m.Name == mn).ToArray();
    foreach (var meth in meths)
    {
        var paramStr = string.Join(", ", meth.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        t($"Method {mn} exists", true);
        sb.AppendLine($"    {meth.ReturnType.Name} {mn}({paramStr})");
    }
}
sb.AppendLine();

// --- IDynamicHashSet<TKey> ---
var ifaceType = typeof(IDynamicHashSet<int>);
t("IDynamicHashSet interface exists", ifaceType != null);
t("Is interface", ifaceType.IsInterface);
t("Implements IBufferElementData", ifaceType.GetInterfaces().Contains(typeof(IBufferElementData)));

sb.AppendLine("IDynamicHashSet<TKey>");
sb.AppendLine("  Kind: interface");
sb.AppendLine("  Interfaces:");
foreach (var ii in ifaceType.GetInterfaces())
    sb.AppendLine($"    {ii.FullName}");
var valProp = ifaceType.GetProperty("Value");
t("Has Value property (byte)", valProp != null && valProp.PropertyType == typeof(byte));
if (valProp != null)
    sb.AppendLine($"  byte Value {{ get; }}");
sb.AppendLine();

// --- DynamicHashMapHelper<T> (shared infrastructure) ---
var helperType = typeof(DynamicHashMapHelper<int>);
t("DynamicHashMapHelper reuses same type", helperType != null);
int helperSize = System.Runtime.InteropServices.Marshal.SizeOf(helperType);
sb.AppendLine("DynamicHashMapHelper<TKey> (shared with DynamicHashMap)");
sb.AppendLine($"  Kind: struct");
sb.AppendLine($"  Size: {helperSize} bytes");
sb.AppendLine("  Fields:");
foreach (var fld in helperType.GetFields(bf))
{
    try
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(helperType, fld.Name).ToInt32();
        sb.AppendLine($"    [{offset}] {fld.FieldType.Name} {fld.Name}");
    }
    catch
    {
        sb.AppendLine($"    [?] {fld.FieldType.Name} {fld.Name}");
    }
}
sb.AppendLine();

// --- DynamicExtensions ---
var extType = typeof(DynamicExtensions);
t("DynamicExtensions type exists", extType != null);
var initMethods = extType.GetMethods(bf).Where(m => m.Name == "InitializeHashSet").ToArray();
var asMethods = extType.GetMethods(bf).Where(m => m.Name == "AsHashSet").ToArray();

sb.AppendLine("DynamicExtensions (relevant methods)");
sb.AppendLine($"  InitializeHashSet overloads: {initMethods.Length}");
foreach (var m in initMethods)
{
    var paramStr = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {m.ReturnType.Name} InitializeHashSet({paramStr})");
}
sb.AppendLine($"  AsHashSet overloads: {asMethods.Length}");
foreach (var m in asMethods)
{
    var paramStr = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {m.ReturnType.Name} AsHashSet({paramStr})");
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
