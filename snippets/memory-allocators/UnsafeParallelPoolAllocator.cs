// Run: cat snippets/memory-allocators/UnsafeParallelPoolAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Jobs.LowLevel.Unsafe"
// Verifies: docs/UnsafeParallelPoolAllocator.md claims
// NOTE: No /unsafe support, reflection-only verification

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var ppaType = typeof(BovineLabs.Core.Memory.UnsafeParallelPoolAllocator<int>);
t("UnsafeParallelPoolAllocator<T> type exists", ppaType != null);
t("Is a struct (ValueType)", ppaType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(ppaType));

// --- Generic constraint: T : unmanaged ---
var gParams = ppaType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Constructor ---
var ctor = ppaType.GetConstructors().FirstOrDefault();
t("Has constructor", ctor != null);
if (ctor != null)
{
    var ps = ctor.GetParameters();
    r.Add($"INFO: Constructor params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
    t("Constructor takes int countPerChunk", ps.Length >= 1 && ps[0].ParameterType == typeof(int));
    t("Constructor takes Allocator", ps.Length >= 2 && ps[1].ParameterType.Name.Contains("Allocator"));
}

// --- Key properties ---
var isCreatedProp = ppaType.GetProperty("IsCreated");
t("Has IsCreated property", isCreatedProp != null);

// --- Key methods ---
var allocMethod = ppaType.GetMethods().Where(m => m.Name == "Alloc").FirstOrDefault();
t("Has Alloc method", allocMethod != null);
if (allocMethod != null)
    t("Alloc returns pointer", allocMethod.ReturnType.IsPointer);

var freeMethod = ppaType.GetMethods().Where(m => m.Name == "Free").FirstOrDefault();
t("Has Free method", freeMethod != null);

var allocatedMethod = ppaType.GetMethods().Where(m => m.Name == "Allocated").FirstOrDefault();
t("Has Allocated method", allocatedMethod != null);
if (allocatedMethod != null)
    t("Allocated returns int", allocatedMethod.ReturnType == typeof(int));

var disposeMethod = ppaType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Fields ---
var fields = ppaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var fieldNames = fields.Select(f => f.Name).ToList();
r.Add($"INFO: Fields: {string.Join(", ", fieldNames)}");

// Doc: has pools field (array of UnsafePoolAllocator)
t("Has pools field", fieldNames.Any(fn => fn.Contains("pool") || fn.Contains("Pool")));

// Doc: has threadIndex field with [NativeSetThreadIndex]
t("Has threadIndex field", fieldNames.Any(fn => fn.Contains("threadIndex") || fn.Contains("ThreadIndex")));

// Doc: has allocator field
t("Has allocator field", fieldNames.Any(fn => fn.Contains("allocator") || fn.Contains("Allocator")));

// --- Verify threadIndex field has [NativeSetThreadIndex] attribute ---
var threadIdxField = fields.FirstOrDefault(f => f.Name.Contains("threadIndex") || f.Name.Contains("ThreadIndex"));
if (threadIdxField != null)
{
    var attrs = threadIdxField.GetCustomAttributes(false);
    var hasThreadIdxAttr = attrs.Any(a => a.GetType().Name.Contains("NativeSetThreadIndex"));
    t("threadIndex has [NativeSetThreadIndex] attribute", hasThreadIdxAttr);
    r.Add($"INFO: threadIndex attributes: {string.Join(", ", attrs.Select(a => a.GetType().Name))}");
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
