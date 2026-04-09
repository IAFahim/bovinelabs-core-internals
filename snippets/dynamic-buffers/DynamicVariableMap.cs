// Run: cat snippets/dynamic-buffers/DynamicVariableMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,BovineLabs.Core.Iterators.Columns,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicVariableMap.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- DynamicVariableMap<TKey,TValue,T,TC> ---
var mapType = typeof(DynamicVariableMap<int, float, short, MultiHashColumn<short>>);
t("DynamicVariableMap type exists", mapType != null);
t("Is ValueType", mapType.IsValueType);

int mapSize = System.Runtime.InteropServices.Marshal.SizeOf(mapType);
sb.AppendLine("DynamicVariableMap<TKey,TValue,T,TC>");
sb.AppendLine("  Kind: struct");
sb.AppendLine($"  Size: {mapSize} bytes");
sb.AppendLine($"  Generic params: TKey={mapType.GetGenericArguments()[0].Name}, TValue={mapType.GetGenericArguments()[1].Name}, T={mapType.GetGenericArguments()[2].Name}, TC={mapType.GetGenericArguments()[3].Name}");
sb.AppendLine();

// Interfaces
sb.AppendLine("  Interfaces:");
foreach (var iface in mapType.GetInterfaces())
    sb.AppendLine($"    {iface.FullName}");
sb.AppendLine();

// Fields
sb.AppendLine("  Fields:");
foreach (var fld in mapType.GetFields(bf))
{
    try
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(mapType, fld.Name).ToInt32();
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
foreach (var prop in mapType.GetProperties(bf))
{
    t($"Property {prop.Name} exists", true);
    sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name} (read={prop.CanRead}, write={prop.CanWrite})");
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
var keyMethods = new[] { "TryAdd", "Remove", "RemoveAt", "TryGetValue", "ReplaceColumn", "Clear" };
foreach (var mn in keyMethods)
{
    var meths = mapType.GetMethods(bf).Where(m => m.Name == mn).ToArray();
    foreach (var meth in meths)
    {
        var paramStr = string.Join(", ", meth.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        t($"Method {mn} exists", true);
        sb.AppendLine($"    {meth.ReturnType.Name} {mn}({paramStr})");
    }
}
sb.AppendLine();

// --- MultiHashColumn<T> ---
var mhcType = typeof(MultiHashColumn<int>);
t("MultiHashColumn type exists", mhcType != null);
t("MultiHashColumn is ValueType", mhcType.IsValueType);
int mhcSize = System.Runtime.InteropServices.Marshal.SizeOf(mhcType);
sb.AppendLine("MultiHashColumn<T>");
sb.AppendLine($"  Kind: struct");
sb.AppendLine($"  Size: {mhcSize} bytes");
sb.AppendLine("  Fields:");
foreach (var fld in mhcType.GetFields(bf))
{
    try
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(mhcType, fld.Name).ToInt32();
        sb.AppendLine($"    [{offset}] {fld.FieldType.Name} {fld.Name}");
    }
    catch
    {
        sb.AppendLine($"    [?] {fld.FieldType.Name} {fld.Name}");
    }
}
sb.AppendLine("  Methods:");
foreach (var meth in mhcType.GetMethods(bf).Where(m => !m.IsSpecialName))
{
    var paramStr = string.Join(", ", meth.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {meth.ReturnType.Name} {meth.Name}({paramStr})");
}
sb.AppendLine();

// --- OrderedListColumn<T> ---
var olcType = typeof(OrderedListColumn<int>);
t("OrderedListColumn type exists", olcType != null);
t("OrderedListColumn is ValueType", olcType.IsValueType);
int olcSize = System.Runtime.InteropServices.Marshal.SizeOf(olcType);
sb.AppendLine("OrderedListColumn<T>");
sb.AppendLine($"  Kind: struct");
sb.AppendLine($"  Size: {olcSize} bytes");
sb.AppendLine("  Fields:");
foreach (var fld in olcType.GetFields(bf))
{
    try
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(olcType, fld.Name).ToInt32();
        sb.AppendLine($"    [{offset}] {fld.FieldType.Name} {fld.Name}");
    }
    catch
    {
        sb.AppendLine($"    [?] {fld.FieldType.Name} {fld.Name}");
    }
}
sb.AppendLine("  Methods:");
foreach (var meth in olcType.GetMethods(bf).Where(m => !m.IsSpecialName))
{
    var paramStr = string.Join(", ", meth.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {meth.ReturnType.Name} {meth.Name}({paramStr})");
}
sb.AppendLine();

// --- IColumn<T> ---
var icolType = typeof(IColumn<int>);
t("IColumn interface exists", icolType != null);
t("IColumn is interface", icolType.IsInterface);
sb.AppendLine("IColumn<T>");
sb.AppendLine("  Kind: interface");
sb.AppendLine("  Methods:");
foreach (var meth in icolType.GetMethods())
{
    var paramStr = string.Join(", ", meth.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {meth.ReturnType.Name} {meth.Name}({paramStr})");
}
sb.AppendLine();

// --- IDynamicVariableMap ---
var iface1 = typeof(IDynamicVariableMap<int, float, short, MultiHashColumn<short>>);
t("IDynamicVariableMap (1-col) exists", iface1 != null);
t("IDynamicVariableMap (1-col) is interface", iface1.IsInterface);
t("Implements IBufferElementData", iface1.GetInterfaces().Contains(typeof(IBufferElementData)));

sb.AppendLine("IDynamicVariableMap<TKey,TValue,T,TC>");
sb.AppendLine("  Kind: interface");
sb.AppendLine("  Interfaces:");
foreach (var ii in iface1.GetInterfaces())
    sb.AppendLine($"    {ii.FullName}");
sb.AppendLine();

// --- 2-col variant ---
var iface2 = typeof(IDynamicVariableMap<int, float, short, MultiHashColumn<short>, byte, MultiHashColumn<byte>>);
t("IDynamicVariableMap (2-col) exists", iface2 != null);
sb.AppendLine("IDynamicVariableMap<TKey,TValue,T1,TC1,T2,TC2>");
sb.AppendLine("  Kind: interface");
sb.AppendLine($"  Generic params: {iface2.GetGenericArguments().Length}");
sb.AppendLine();

// --- DynamicVariableMapHelper (internal) ---
var asm = mapType.Assembly;
var helperType1 = asm.GetType("BovineLabs.Core.Iterators.DynamicVariableMapHelper`4");
var helperType2 = asm.GetType("BovineLabs.Core.Iterators.DynamicVariableMapHelper2`6");
t("DynamicVariableMapHelper`4 exists", helperType1 != null);
t("DynamicVariableMapHelper2`6 exists", helperType2 != null);

if (helperType1 != null)
{
    sb.AppendLine("DynamicVariableMapHelper<TKey,TValue,T,TC> (internal)");
    sb.AppendLine($"  Kind: {(helperType1.IsValueType ? "struct" : "class")}");
    sb.AppendLine("  Fields:");
    foreach (var fld in helperType1.GetFields(bf))
    {
        sb.AppendLine($"    {fld.FieldType.Name} {fld.Name}");
    }
}
if (helperType2 != null)
{
    sb.AppendLine("DynamicVariableMapHelper2<TKey,TValue,T1,TC1,T2,TC2> (internal)");
    sb.AppendLine($"  Kind: {(helperType2.IsValueType ? "struct" : "class")}");
    sb.AppendLine("  Fields:");
    foreach (var fld in helperType2.GetFields(bf))
    {
        sb.AppendLine($"    {fld.FieldType.Name} {fld.Name}");
    }
}
sb.AppendLine();

// --- Extension methods ---
var extType = typeof(DynamicExtensions);
var initVarMap = extType.GetMethods(bf).Where(m => m.Name == "InitializeVariableMap").ToArray();
var asVarMap = extType.GetMethods(bf).Where(m => m.Name == "AsVariableMap").ToArray();
t("InitializeVariableMap overloads >= 2", initVarMap.Length >= 2);
t("AsVariableMap overloads >= 2", asVarMap.Length >= 2);

sb.AppendLine("DynamicExtensions (relevant methods)");
sb.AppendLine($"  InitializeVariableMap overloads: {initVarMap.Length}");
foreach (var m in initVarMap)
{
    var paramStr = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {m.ReturnType.Name} InitializeVariableMap({paramStr})");
}
sb.AppendLine($"  AsVariableMap overloads: {asVarMap.Length}");
foreach (var m in asVarMap)
{
    var paramStr = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {m.ReturnType.Name} AsVariableMap({paramStr})");
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
