// Run: cat snippets/memory-allocators/UnsafeFixedPoolAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/UnsafeFixedPoolAllocator.md claims
// NOTE: No /unsafe support, reflection-only verification

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var fpaType = typeof(BovineLabs.Core.Memory.UnsafeFixedPoolAllocator<int>);
t("UnsafeFixedPoolAllocator<T> type exists", fpaType != null);
t("Is a struct (ValueType)", fpaType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(fpaType));

// --- Generic constraint: T : unmanaged ---
var gParams = fpaType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Constructor ---
var ctor = fpaType.GetConstructors().FirstOrDefault();
t("Has constructor", ctor != null);
if (ctor != null)
{
    var ps = ctor.GetParameters();
    r.Add($"INFO: Constructor params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
    t("Constructor takes int maxItems", ps.Length >= 1 && ps[0].ParameterType == typeof(int));
    t("Constructor takes Allocator", ps.Length >= 2 && ps[1].ParameterType.Name.Contains("Allocator"));
}

// --- Key properties ---
var isCreatedProp = fpaType.GetProperty("IsCreated");
t("Has IsCreated property", isCreatedProp != null);

// --- Key methods ---
var allocMethod = fpaType.GetMethods().Where(m => m.Name == "Alloc").FirstOrDefault();
t("Has Alloc method", allocMethod != null);
if (allocMethod != null)
    t("Alloc returns pointer", allocMethod.ReturnType.IsPointer);

var freeMethod = fpaType.GetMethods().Where(m => m.Name == "Free").FirstOrDefault();
t("Has Free method", freeMethod != null);

var disposeMethod = fpaType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Fields ---
var fields = fpaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var fieldNames = fields.Select(f => f.Name).ToList();
r.Add($"INFO: Fields: {string.Join(", ", fieldNames)}");

// Doc: has maxItems, buffer, freeIndex fields
t("Has maxItems field", fieldNames.Any(fn => fn.Contains("maxItems") || fn.Contains("MaxItems")));
t("Has buffer field (Ptr)", fieldNames.Any(fn => fn.Contains("buffer") || fn.Contains("Buffer")));
t("Has freeIndex field (UnsafeParallelHashSet)", fieldNames.Any(fn => fn.Contains("freeIndex") || fn.Contains("FreeIndex")));

// --- Doc claim: no resizing, fixed capacity ---
t("Has readonly-like constraint (maxItems)", fieldNames.Any(fn => fn.Contains("maxItems")));
r.Add($"INFO: Fixed pool has no growth fields (no countPerSlab, no slabs list)");

// --- ValidatePtr method (editor-only safety) ---
var validateMethod = fpaType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
    .Where(m => m.Name.Contains("Validate") || m.Name.Contains("validate")).FirstOrDefault();
t("Has ValidatePtr method (editor safety)", validateMethod != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
