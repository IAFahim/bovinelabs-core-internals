// Run: cat snippets/core-collections/NativeCounter.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,Unity.Collections,Unity.Burst,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
// Verifies: docs/NativeCounter.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type existence and structure ---
var ncType = typeof(BovineLabs.Core.Collections.NativeCounter);
t("NativeCounter type exists", ncType != null);
t("NativeCounter is a struct", ncType.IsValueType && !ncType.IsEnum);

int ncSize = System.Runtime.InteropServices.Marshal.SizeOf(ncType);
r.Add($"INFO: NativeCounter struct size = {ncSize} bytes (doc says ~10, actual includes safety handles)");

// --- Methods ---
var methods = ncType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).Distinct().ToList();
t("Has Increment() method", methods.Contains("Increment"));
t("Has Dispose() method", methods.Contains("Dispose"));

var incrementMethod = ncType.GetMethod("Increment", BindingFlags.Public | BindingFlags.Instance);
t("Increment returns int", incrementMethod != null && incrementMethod.ReturnType == typeof(int));

// --- Properties ---
var props = ncType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();
t("Has Count property", props.Contains("Count"));

var countProp = ncType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
t("Count has getter", countProp != null && countProp.CanRead);
t("Count has setter", countProp != null && countProp.CanWrite);

// --- AsParallelWriter ---
t("Has AsParallelWriter()", methods.Contains("AsParallelWriter"));

// --- ParallelWriter nested type ---
var pwType = ncType.GetNestedType("ParallelWriter", BindingFlags.Public);
t("ParallelWriter nested type exists", pwType != null);
t("ParallelWriter is struct", pwType != null && pwType.IsValueType);

if (pwType != null)
{
    var pwMethods = pwType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).Distinct().ToList();
    t("ParallelWriter has Increment()", pwMethods.Contains("Increment"));
    
    var pwInc = pwType.GetMethod("Increment", BindingFlags.Public | BindingFlags.Instance);
    t("ParallelWriter.Increment returns int", pwInc != null && pwInc.ReturnType == typeof(int));
    
    // Check field types (use reflection to check for pointer types)
    var pwFields = pwType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    var countField = pwFields.FirstOrDefault(f => f.Name == "count");
    t("ParallelWriter has 'count' field", countField != null);
    if (countField != null)
    {
        t("ParallelWriter 'count' is a pointer type", countField.FieldType.IsPointer);
    }
}

// --- Constructor ---
var ctor = ncType.GetConstructor(new[] { typeof(Unity.Collections.AllocatorManager.AllocatorHandle) });
t("Constructor takes AllocatorHandle", ctor != null);

// --- Functional test ---
var counter = new BovineLabs.Core.Collections.NativeCounter(Unity.Collections.Allocator.Temp);
t("Construction succeeds", true);

int inc1 = counter.Increment();
t("First Increment returns 1", inc1 == 1);
int inc2 = counter.Increment();
t("Second Increment returns 2", inc2 == 2);
t("Count property matches", counter.Count == 2);

counter.Count = 10;
t("Count setter works (set to 10)", counter.Count == 10);

// ParallelWriter
var pw = counter.AsParallelWriter();
t("AsParallelWriter succeeds", true);
int pwIncrement = pw.Increment();
t("ParallelWriter.Increment works (returns 11)", pwIncrement == 11);

counter.Dispose();
t("Dispose succeeds", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
