// Run: cat snippets/memory-allocators/NativeSlabAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NativeSlabAllocator.md claims
// NOTE: No /unsafe support, reflection-only verification

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var nsaType = typeof(BovineLabs.Core.Memory.NativeSlabAllocator<int>);
t("NativeSlabAllocator<T> type exists", nsaType != null);
t("Is a struct (ValueType)", nsaType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(nsaType));

// --- [NativeContainer] attribute ---
var ncAttr = nsaType.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("NativeContainer"));
t("Has [NativeContainer] attribute", ncAttr);

// --- Generic constraint: T : unmanaged ---
var gParams = nsaType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Constructor ---
var ctor = nsaType.GetConstructors().FirstOrDefault();
t("Has constructor", ctor != null);
if (ctor != null)
{
    var ps = ctor.GetParameters();
    r.Add($"INFO: Constructor params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
    t("Constructor takes int countPerSlab", ps.Length >= 1 && ps[0].ParameterType == typeof(int));
}

// --- Key properties ---
var isCreatedProp = nsaType.GetProperty("IsCreated");
t("Has IsCreated property", isCreatedProp != null);
if (isCreatedProp != null)
    t("IsCreated returns bool", isCreatedProp.PropertyType == typeof(bool));

var allocCountProp = nsaType.GetProperty("AllocationCount");
t("Has AllocationCount property", allocCountProp != null);
if (allocCountProp != null)
    t("AllocationCount returns int", allocCountProp.PropertyType == typeof(int));

// --- Key methods ---
var allocMethod = nsaType.GetMethods().Where(m => m.Name == "Alloc").FirstOrDefault();
t("Has Alloc method", allocMethod != null);
if (allocMethod != null)
    t("Alloc returns pointer", allocMethod.ReturnType.IsPointer);

var clearMethod = nsaType.GetMethods().Where(m => m.Name == "Clear").FirstOrDefault();
t("Has Clear method", clearMethod != null);

var disposeMethod = nsaType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Fields: verify safety handle exists ---
var fields = nsaType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var fieldNames = fields.Select(f => f.Name).ToList();
r.Add($"INFO: Fields: {string.Join(", ", fieldNames)}");

// Doc: wraps UnsafeSlabAllocator<T>
t("Has slabAllocator field (wraps UnsafeSlabAllocator)", fieldNames.Any(fn => fn.Contains("slab") || fn.Contains("Slab")));

// Safety handle fields (behind #if)
t("Has safety handle field (m_Safety)", fieldNames.Any(fn => fn.Contains("Safety") || fn.Contains("safety")));

// --- Static safety ID ---
var staticFields = nsaType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
var staticFieldNames = staticFields.Select(f => f.Name).ToList();
r.Add($"INFO: Static fields: {string.Join(", ", staticFieldNames)}");
t("Has static safety ID field", staticFieldNames.Any(fn => fn.Contains("s_staticSafetyId") || fn.Contains("safetyId")));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
