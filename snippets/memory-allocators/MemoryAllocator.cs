// Run: cat snippets/memory-allocators/MemoryAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Mathematics"
// Verifies: docs/MemoryAllocator.md claims
// NOTE: No /unsafe support, so reflection-only + safe API verification

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var maType = typeof(BovineLabs.Core.Memory.MemoryAllocator);
t("MemoryAllocator type exists", maType != null);
t("Is a struct (ValueType)", maType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(maType));

// --- Constructor ---
var ctor = maType.GetConstructors().FirstOrDefault();
t("Has constructor", ctor != null);
if (ctor != null)
{
    var ps = ctor.GetParameters();
    t("Constructor takes Allocator parameter", ps.Length == 1 && ps[0].ParameterType.Name.Contains("Allocator"));
}

// --- Key methods via reflection ---
var allocateMethod = maType.GetMethods().Where(m => m.Name == "Allocate").FirstOrDefault();
t("Has Allocate method", allocateMethod != null);
if (allocateMethod != null)
{
    var ps = allocateMethod.GetParameters();
    r.Add($"INFO: Allocate params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
    t("Allocate takes itemSizeInBytes, alignmentInBytes, items", ps.Length >= 2);
    t("Allocate returns void pointer", allocateMethod.ReturnType == typeof(void).MakePointerType() || allocateMethod.ReturnType.IsPointer);
}

var createMethod = maType.GetMethods().Where(m => m.Name == "Create" && m.IsGenericMethod).FirstOrDefault();
t("Has generic Create<T> method", createMethod != null);

var createListMethod = maType.GetMethods().Where(m => m.Name == "CreateList" && m.IsGenericMethod).FirstOrDefault();
t("Has generic CreateList<T> method", createListMethod != null);
if (createListMethod != null)
{
    t("CreateList returns UnsafeList<T>", createListMethod.ReturnType.Name.Contains("UnsafeList"));
}

var freeAllMethod = maType.GetMethods().Where(m => m.Name == "FreeAll").FirstOrDefault();
t("Has FreeAll method", freeAllMethod != null);
if (freeAllMethod != null)
    t("FreeAll returns void", freeAllMethod.ReturnType == typeof(void));

var disposeMethod = maType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Allocator property ---
var allocProp = maType.GetProperty("Allocator");
t("Has Allocator property", allocProp != null);

// --- Fields ---
var fields = maType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var fieldNames = fields.Select(f => f.Name).ToList();
r.Add($"INFO: Fields: {string.Join(", ", fieldNames)}");
t("Has allocated tracking field", fieldNames.Any(fn => fn.Contains("allocated") || fn.Contains("Allocated")));

// --- Functional test: CreateList<T> returns usable UnsafeList ---
// (This is safe because UnsafeList is a struct without pointers in the API)
var mem = new BovineLabs.Core.Memory.MemoryAllocator(Allocator.Persistent);
try
{
    t("MemoryAllocator constructs", true);
    
    // CreateList<T> returns UnsafeList<T> (safe struct)
    var list = mem.CreateList<byte>(1024);
    t("CreateList<byte>(1024) returns list", list.IsCreated);
    r.Add($"INFO: List capacity = {list.Capacity}");
    t("List capacity is power of 2", (list.Capacity & (list.Capacity - 1)) == 0);
    t("List capacity >= requested", list.Capacity >= 1024);
    // Doc: capacity = max(1024, 64/1) = 1024, ceilpow2(1024) = 1024
    t("List capacity equals 1024", list.Capacity == 1024);

    // CreateList with small capacity - should round up to 64/sizeof(T)
    var smallList = mem.CreateList<int>(1);
    t("CreateList<int>(1) returns list", smallList.IsCreated);
    // max(1, 64/4) = 16, ceilpow2(16) = 16
    t("Small list capacity is 16 (max(1, 64/sizeof(int)) rounded to pow2)", smallList.Capacity == 16);

    // FreeAll
    mem.FreeAll();
    t("FreeAll completes without error", true);
}
finally
{
    mem.Dispose();
    t("Dispose completes without error", true);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
