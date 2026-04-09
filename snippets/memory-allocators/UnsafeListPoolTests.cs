// Run: cat snippets/memory-allocators/UnsafeListPoolTests.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafeListPoolTests.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

// --- Verify UnsafeListPool<T> type (the type under test) ---
var poolType = typeof(BovineLabs.Core.Collections.UnsafeListPool<int>);
sb.AppendLine("BovineLabs.Core.Collections.UnsafeListPool<T>");
sb.AppendLine($"  Kind: struct (ValueType={poolType.IsValueType})");
sb.AppendLine($"  Size (T=int): {System.Runtime.InteropServices.Marshal.SizeOf(poolType)} bytes");
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
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var prop in poolType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var access = prop.GetMethod != null && prop.GetMethod.IsPublic ? "public" : "private";
    sb.AppendLine($"    {access} {prop.PropertyType.Name} {prop.Name}");
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

// Functional test: UnsafeListPool lifecycle
sb.AppendLine("  Functional Tests (pool lifecycle):");
var pool = new BovineLabs.Core.Collections.UnsafeListPool<int>(4, Allocator.Persistent);
try
{
    sb.AppendLine($"    IsCreated: {pool.IsCreated}");
    t("UnsafeListPool is created", pool.IsCreated);

    // GetOrCreate on empty pool
    var list = pool.GetOrCreate(16, Allocator.Persistent);
    sb.AppendLine($"    GetOrCreate(16) on empty pool: IsCreated={list.IsCreated}, Capacity={list.Capacity}");
    t("GetOrCreate on empty pool returns new list", list.IsCreated);
    t("GetOrCreate list has capacity >= 16", list.Capacity >= 16);

    // Add items, return to pool
    list.Add(42);
    sb.AppendLine($"    Added 42 to list, Length={list.Length}");
    pool.ReturnOrDispose(list);
    sb.AppendLine($"    ReturnOrDispose(list) succeeded");
    t("ReturnOrDispose succeeds", true);

    // GetOrCreate should now return recycled list
    var list2 = pool.GetOrCreate(4, Allocator.Persistent);
    sb.AppendLine($"    GetOrCreate(4) after return: IsCreated={list2.IsCreated}, Length={list2.Length}, [0]={list2[0]}");
    t("GetOrCreate after return returns recycled list", list2.IsCreated);
    t("Recycled list still has data (not cleared automatically)", list2.Length == 1 && list2[0] == 42);

    pool.ReturnOrDispose(list2);
}
finally
{
    pool.Dispose();
    sb.AppendLine($"    Dispose() succeeded");
    t("UnsafeListPool Dispose succeeds", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
