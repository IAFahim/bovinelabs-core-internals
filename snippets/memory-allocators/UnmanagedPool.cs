// Run: cat snippets/memory-allocators/UnmanagedPool.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Mathematics"
// Verifies: docs/UnmanagedPool.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var poolType = typeof(BovineLabs.Core.Collections.UnmanagedPool<int>);
sb.AppendLine("BovineLabs.Core.Collections.UnmanagedPool<T>");
sb.AppendLine($"  Kind: struct (ValueType={poolType.IsValueType})");
sb.AppendLine($"  Size (T=int): {System.Runtime.InteropServices.Marshal.SizeOf(poolType)} bytes");
sb.AppendLine($"  Generic params: {string.Join(", ", poolType.GetGenericArguments().Select(ga => ga.Name))}");
sb.AppendLine();

// Interfaces
sb.AppendLine("  Interfaces:");
foreach (var iface in poolType.GetInterfaces())
{
    sb.AppendLine($"    {iface.FullName}");
    t($"Implements {iface.Name}", true);
}
sb.AppendLine();

// Constructors
sb.AppendLine("  Constructors:");
foreach (var ctor in poolType.GetConstructors())
{
    var pStr = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    .ctor({pStr})");
    t($"Constructor ({pStr})", true);
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var prop in poolType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
    t($"Property {prop.Name}", true);
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
foreach (var m in poolType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    public {m.ReturnType.Name} {m.Name}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Functional tests
sb.AppendLine("  Functional Tests:");

var bytePool = new BovineLabs.Core.Collections.UnmanagedPool<byte>(8, Allocator.Persistent);
try
{
    byte item = 42;
    bool added = bytePool.TryAdd(item);
    sb.AppendLine($"    byte pool (capacity=8): TryAdd(42) = {added}");
    t("TryAdd succeeds on fresh pool", added);

    byte retrieved;
    bool got = bytePool.TryGet(out retrieved);
    sb.AppendLine($"    byte pool: TryGet = {got}, value = {retrieved}");
    t("TryGet succeeds after TryAdd", got);
    t("TryGet returns last added item (LIFO)", retrieved == 42);

    for (int i = 0; i < 63; i++) bytePool.TryAdd((byte)i);
    bool addResult = bytePool.TryAdd(255);
    bool overResult = bytePool.TryAdd(100);
    sb.AppendLine($"    byte pool full test: last add={addResult}, overflow={overResult}");
    t("TryAdd fills up to capacity", addResult);
    t("TryAdd returns false when full", !overResult);
}
finally
{
    bytePool.Dispose();
}

var intPool = new BovineLabs.Core.Collections.UnmanagedPool<int>(4, Allocator.Persistent);
try
{
    intPool.TryAdd(10);
    intPool.TryAdd(20);
    intPool.TryAdd(30);
    int val;
    intPool.TryGet(out val);
    sb.AppendLine($"    int pool LIFO: pop1={val}");
    t("LIFO: first TryGet returns last added (30)", val == 30);
    intPool.TryGet(out val);
    sb.AppendLine($"    int pool LIFO: pop2={val}");
    t("LIFO: second TryGet returns 20", val == 20);
    intPool.TryGet(out val);
    sb.AppendLine($"    int pool LIFO: pop3={val}");
    t("LIFO: third TryGet returns 10", val == 10);
    bool emptyResult = intPool.TryGet(out val);
    sb.AppendLine($"    int pool: empty TryGet={emptyResult}");
    t("TryGet returns false when empty", !emptyResult);
}
finally
{
    intPool.Dispose();
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
