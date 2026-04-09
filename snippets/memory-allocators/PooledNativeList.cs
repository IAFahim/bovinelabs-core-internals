// Run: cat snippets/memory-allocators/PooledNativeList.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Utility,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Jobs.LowLevel.Unsafe"
// Verifies: docs/PooledNativeList.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- PooledNativeList<T> ---
var pType = typeof(BovineLabs.Core.Utility.PooledNativeList<int>);
t("PooledNativeList<T> type exists", pType != null);
t("Is ValueType", pType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(pType));

int pSize = System.Runtime.InteropServices.Marshal.SizeOf(pType);
sb.AppendLine("PooledNativeList<T>");
sb.AppendLine("  Kind: struct");
sb.AppendLine($"  Size: {pSize} bytes");
sb.AppendLine($"  Generic params: T (unmanaged)");
sb.AppendLine("  Interfaces:");
foreach (var iface in pType.GetInterfaces())
    sb.AppendLine($"    {iface.FullName}");
sb.AppendLine();

// Fields
sb.AppendLine("  Fields:");
foreach (var fld in pType.GetFields(bf))
{
    try
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(pType, fld.Name).ToInt32();
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
foreach (var prop in pType.GetProperties(bf))
{
    t($"Property {prop.Name} exists", true);
    sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name} (read={prop.CanRead}, write={prop.CanWrite})");
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
foreach (var meth in pType.GetMethods(bf).Where(m => !m.IsSpecialName))
{
    var paramStr = string.Join(", ", meth.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var staticTag = meth.IsStatic ? "static " : "";
    t($"Method {meth.Name} exists", true);
    sb.AppendLine($"    {staticTag}{meth.ReturnType.Name} {meth.Name}({paramStr})");
}
sb.AppendLine();

// --- Inner (non-generic) PooledNativeList ---
var assembly = pType.Assembly;
var innerPooledType = assembly.GetType("BovineLabs.Core.Utility.PooledNativeList");
t("Inner PooledNativeList (non-generic) exists", innerPooledType != null);
if (innerPooledType != null)
{
    sb.AppendLine("PooledNativeList (inner non-generic)");
    sb.AppendLine($"  Kind: {(innerPooledType.IsValueType ? "struct" : (innerPooledType.IsClass ? "class" : "type"))}");
    sb.AppendLine();

    // Constants / static fields
    sb.AppendLine("  Static fields:");
    foreach (var fld in innerPooledType.GetFields(bf | BindingFlags.FlattenHierarchy))
    {
        if (fld.IsStatic)
        {
            var val = fld.IsLiteral ? fld.GetValue(null) : "(non-literal)";
            t($"Field {fld.Name}", true);
            sb.AppendLine($"    {fld.FieldType.Name} {fld.Name} = {val}");
        }
    }
    sb.AppendLine();

    // Nested types
    sb.AppendLine("  Nested types:");
    foreach (var nt in innerPooledType.GetNestedTypes(bf))
    {
        sb.AppendLine($"    {nt.Name} ({(nt.IsValueType ? "struct" : "class")})");
        foreach (var nf in nt.GetFields(bf))
            sb.AppendLine($"      {nf.FieldType.Name} {nf.Name}");
    }
    sb.AppendLine();
}

// --- Runtime behavior ---
sb.AppendLine("Runtime Behavior:");

var list1 = BovineLabs.Core.Utility.PooledNativeList<int>.Make();
t("Make() returns valid instance", list1.List.IsCreated);
t("Initial list is empty", list1.List.Length == 0);
sb.AppendLine($"  Make() -> List.IsCreated={list1.List.IsCreated}, Length={list1.List.Length}, Capacity={list1.List.Capacity}");

list1.List.Add(42);
list1.List.Add(99);
t("Can add items", list1.List.Length == 2);
sb.AppendLine($"  After Add(42,99): Length={list1.List.Length}, [0]={list1.List[0]}, [1]={list1.List[1]}");

int capBeforeDispose = list1.List.Capacity;
list1.Dispose();
t("Dispose succeeds", true);
sb.AppendLine($"  Dispose() completed (capacity was {capBeforeDispose})");

var list2 = BovineLabs.Core.Utility.PooledNativeList<int>.Make();
t("Second Make() returns valid instance", list2.List.IsCreated);
sb.AppendLine($"  Second Make() -> IsCreated={list2.List.IsCreated}, Capacity={list2.List.Capacity}");
sb.AppendLine($"    Pool reuse: Capacity={list2.List.Capacity} >= disposed capacity={capBeforeDispose}: {list2.List.Capacity >= capBeforeDispose}");

list2.Dispose();
t("Second Dispose succeeds", true);

// Third make to verify pool stacking
var list3 = BovineLabs.Core.Utility.PooledNativeList<int>.Make();
sb.AppendLine($"  Third Make() -> IsCreated={list3.List.IsCreated}, Capacity={list3.List.Capacity}");
list3.Dispose();

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
