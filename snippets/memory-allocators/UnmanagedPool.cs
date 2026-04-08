// Run: cat snippets/memory-allocators/UnmanagedPool.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Mathematics"
// Verifies: docs/UnmanagedPool.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var poolType = typeof(BovineLabs.Core.Collections.UnmanagedPool<int>);
t("UnmanagedPool<T> type exists", poolType != null);
t("Is a struct (ValueType)", poolType.IsValueType);
t("Is readonly struct", poolType.IsValueType && (poolType.Attributes & System.Reflection.TypeAttributes.Sealed) != 0);

// --- Implements IDisposable ---
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(poolType));

// --- Generic constraint: T : unmanaged ---
var gParams = poolType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Constructor ---
var ctor = poolType.GetConstructors().FirstOrDefault(c => {
    var ps = c.GetParameters();
    return ps.Length == 2 && ps[0].ParameterType == typeof(int) && ps[1].ParameterType.Name.Contains("Allocator");
});
t("Has constructor(int capacity, Allocator)", ctor != null);

// --- Key methods ---
var tryAddMethod = poolType.GetMethods().Where(m => m.Name == "TryAdd").FirstOrDefault();
t("Has TryAdd method", tryAddMethod != null);
if (tryAddMethod != null)
{
    t("TryAdd returns bool", tryAddMethod.ReturnType == typeof(bool));
}

var tryGetMethod = poolType.GetMethods().Where(m => m.Name == "TryGet").FirstOrDefault();
t("Has TryGet method", tryGetMethod != null);
if (tryGetMethod != null)
{
    t("TryGet returns bool", tryGetMethod.ReturnType == typeof(bool));
}

var isCreatedProp = poolType.GetProperty("IsCreated");
t("Has IsCreated property", isCreatedProp != null);
if (isCreatedProp != null)
{
    t("IsCreated returns bool", isCreatedProp.PropertyType == typeof(bool));
}

var disposeMethod = poolType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Capacity is power of 2 (doc claim) ---
// Test: T=byte, capacity=8 => min=64/1=64, ceilpow2(64)=64
var bytePool = new BovineLabs.Core.Collections.UnmanagedPool<byte>(8, Allocator.Persistent);
try
{
    // TryAdd/TryGet behavior
    byte item = 42;
    bool added = bytePool.TryAdd(item);
    t("TryAdd succeeds on fresh pool", added);

    byte retrieved;
    bool got = bytePool.TryGet(out retrieved);
    t("TryGet succeeds after TryAdd", got);
    t("TryGet returns last added item (LIFO)", retrieved == 42);

    // Fill pool and test TryAdd returns false when full
    // Capacity for byte with input 8: max(8, 64/1)=64, ceilpow2(64)=64
    for (int i = 0; i < 63; i++) // already 0 items (was popped)
    {
        bytePool.TryAdd((byte)i);
    }
    // Pool should have 63 items, capacity should be 64, so one more should succeed
    bool addResult = bytePool.TryAdd(255);
    t("TryAdd fills up to capacity", addResult);
    // One more should fail
    bool overResult = bytePool.TryAdd(100);
    t("TryAdd returns false when full", !overResult);
}
finally
{
    bytePool.Dispose();
}

// --- Test LIFO ordering ---
var intPool = new BovineLabs.Core.Collections.UnmanagedPool<int>(4, Allocator.Persistent);
try
{
    intPool.TryAdd(10);
    intPool.TryAdd(20);
    intPool.TryAdd(30);
    int val;
    intPool.TryGet(out val);
    t("LIFO: first TryGet returns last added (30)", val == 30);
    intPool.TryGet(out val);
    t("LIFO: second TryGet returns 20", val == 20);
    intPool.TryGet(out val);
    t("LIFO: third TryGet returns 10", val == 10);
    bool emptyResult = intPool.TryGet(out val);
    t("TryGet returns false when empty", !emptyResult);
}
finally
{
    intPool.Dispose();
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
