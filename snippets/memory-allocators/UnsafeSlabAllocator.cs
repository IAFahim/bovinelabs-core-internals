// Run: cat snippets/memory-allocators/UnsafeSlabAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafeSlabAllocator.md claims
// NOTE: No /unsafe support, so reflection-only verification for pointer APIs

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var slabType = typeof(BovineLabs.Core.Memory.UnsafeSlabAllocator<int>);
t("UnsafeSlabAllocator<T> type exists", slabType != null);
t("Is a struct (ValueType)", slabType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(slabType));

// --- Generic constraint: T : unmanaged ---
var gParams = slabType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Constructor ---
var ctor = slabType.GetConstructors().FirstOrDefault();
t("Has constructor", ctor != null);
if (ctor != null)
{
    var ps = ctor.GetParameters();
    r.Add($"INFO: Constructor params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
    t("Constructor takes int countPerSlab", ps.Length >= 1 && ps[0].ParameterType == typeof(int));
}

// --- IsCreated property ---
var isCreatedProp = slabType.GetProperty("IsCreated");
t("Has IsCreated property", isCreatedProp != null);
if (isCreatedProp != null)
    t("IsCreated returns bool", isCreatedProp.PropertyType == typeof(bool));

// --- AllocationCount property ---
var allocCountProp = slabType.GetProperty("AllocationCount");
t("Has AllocationCount property", allocCountProp != null);
if (allocCountProp != null)
    t("AllocationCount returns int", allocCountProp.PropertyType == typeof(int));

// --- Methods: Alloc returns T* (pointer) ---
var allocMethod = slabType.GetMethods().Where(m => m.Name == "Alloc").FirstOrDefault();
t("Has Alloc method", allocMethod != null);
if (allocMethod != null)
{
    t("Alloc returns a pointer", allocMethod.ReturnType.IsPointer);
}

// --- Clear method ---
var clearMethod = slabType.GetMethods().Where(m => m.Name == "Clear").FirstOrDefault();
t("Has Clear method", clearMethod != null);
if (clearMethod != null)
    t("Clear returns void", clearMethod.ReturnType == typeof(void));

// --- Allocated method ---
var allocatedMethod = slabType.GetMethods().Where(m => m.Name == "Allocated").FirstOrDefault();
t("Has Allocated method", allocatedMethod != null);
if (allocatedMethod != null)
    t("Allocated returns int", allocatedMethod.ReturnType == typeof(int));

// --- Dispose method ---
var disposeMethod = slabType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Fields (private, verify internal structure) ---
var fields = slabType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var fieldNames = fields.Select(f => f.Name).ToList();
r.Add($"INFO: Fields: {string.Join(", ", fieldNames)}");
t("Has countPerSlab field", fieldNames.Any(fn => fn.Contains("countPerSlab") || fn.Contains("CountPerSlab")));
t("Has slabs field", fieldNames.Any(fn => fn.Contains("slab") || fn.Contains("Slab")));
t("Has count field", fieldNames.Any(fn => fn.Contains("count") && !fn.Contains("countPer")));

// --- Doc claim: no thread safety ---
t("No SyncContext or lock fields", !fieldNames.Any(fn => fn.Contains("lock") || fn.Contains("mutex") || fn.Contains("sync")));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
