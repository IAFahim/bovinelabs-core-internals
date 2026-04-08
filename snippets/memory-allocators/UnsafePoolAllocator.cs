// Run: cat snippets/memory-allocators/UnsafePoolAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafePoolAllocator.md claims
// NOTE: No /unsafe support, reflection-only verification

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var upaType = typeof(BovineLabs.Core.Memory.UnsafePoolAllocator<int>);
t("UnsafePoolAllocator<T> type exists", upaType != null);
t("Is a struct (ValueType)", upaType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(upaType));

// --- Generic constraint: T : unmanaged ---
var gParams = upaType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Constructor ---
var ctor = upaType.GetConstructors().FirstOrDefault();
t("Has constructor", ctor != null);
if (ctor != null)
{
    var ps = ctor.GetParameters();
    r.Add($"INFO: Constructor params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
    t("Constructor takes int countPerChunk", ps.Length >= 1 && ps[0].ParameterType == typeof(int));
    t("Constructor takes Allocator", ps.Length >= 2 && ps[1].ParameterType.Name.Contains("Allocator"));
}

// --- Key properties ---
var isCreatedProp = upaType.GetProperty("IsCreated");
t("Has IsCreated property", isCreatedProp != null);
if (isCreatedProp != null)
    t("IsCreated returns bool", isCreatedProp.PropertyType == typeof(bool));

// --- Key methods ---
var allocMethod = upaType.GetMethods().Where(m => m.Name == "Alloc").FirstOrDefault();
t("Has Alloc method", allocMethod != null);
if (allocMethod != null)
    t("Alloc returns pointer", allocMethod.ReturnType.IsPointer);

var freeMethod = upaType.GetMethods().Where(m => m.Name == "Free").FirstOrDefault();
t("Has Free method", freeMethod != null);
if (freeMethod != null)
{
    var ps = freeMethod.GetParameters();
    t("Free takes pointer parameter", ps.Length == 1 && ps[0].ParameterType.IsPointer);
}

var allocatedMethod = upaType.GetMethods().Where(m => m.Name == "Allocated").FirstOrDefault();
t("Has Allocated method", allocatedMethod != null);
if (allocatedMethod != null)
    t("Allocated returns int", allocatedMethod.ReturnType == typeof(int));

var disposeMethod = upaType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Fields: slab + free-list hybrid ---
var fields = upaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var fieldNames = fields.Select(f => f.Name).ToList();
r.Add($"INFO: Fields: {string.Join(", ", fieldNames)}");

// Doc: has slabAllocator field
t("Has slabAllocator field (UnsafeSlabAllocator<T>)", fieldNames.Any(fn => fn.Contains("slab") || fn.Contains("Slab")));

// Doc: has free field (UnsafeParallelHashSet<Ptr>)
t("Has free field (UnsafeParallelHashSet<Ptr>)", fieldNames.Any(fn => fn.Contains("free") || fn.Contains("Free")));

// --- No thread safety fields ---
t("No lock/spinlock fields (not thread-safe)", !fieldNames.Any(fn => fn.Contains("lock") || fn.Contains("mutex") || fn.Contains("spin")));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
