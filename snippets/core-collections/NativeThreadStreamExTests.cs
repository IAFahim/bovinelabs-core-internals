// Run: cat snippets/core-collections/NativeThreadStreamExTests.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq"
// Verifies: docs/NativeThreadStreamExTests.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);

// --- Public API surface ---
var methods = ntsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
    .Select(m => m.Name).Distinct().ToList();
r.Add($"INFO: NativeThreadStream methods: {string.Join(", ", methods)}");

t("Has Dispose", methods.Contains("Dispose"));
t("Has AsReader", methods.Contains("AsReader"));
t("Has AsWriter", methods.Contains("AsWriter"));
t("Has Count (method or property)", methods.Contains("Count") || ntsType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance) != null);
t("Has ToNativeArray", methods.Contains("ToNativeArray"));

var isCreatedProp = ntsType.GetProperty("IsCreated", BindingFlags.Public | BindingFlags.Instance);
t("Has IsCreated property", isCreatedProp != null);

// IsEmpty and ForEachCount might be methods in the newer API
bool hasIsEmpty = methods.Contains("get_IsEmpty") || methods.Contains("IsEmpty") || 
    ntsType.GetProperty("IsEmpty", BindingFlags.Public | BindingFlags.Instance) != null;
t("Has IsEmpty (method or property)", hasIsEmpty);

bool hasForEachCount = methods.Any(m => m.Contains("ForEachCount")) || 
    ntsType.GetProperty("ForEachCount", BindingFlags.Public | BindingFlags.Instance) != null ||
    ntsType.GetNestedType("Reader", BindingFlags.Public | BindingFlags.NonPublic)?
        .GetProperty("ForEachCount", BindingFlags.Public | BindingFlags.Instance) != null;
t("Has ForEachCount (on Reader)", hasForEachCount);

// --- Functional: complete write-read lifecycle ---
var stream = new BovineLabs.Core.Collections.NativeThreadStream(Unity.Collections.Allocator.Temp);
t("Construction succeeds", stream.IsCreated);

var writer = stream.AsWriter();
writer.Write(42);
writer.Write(100);
t("Write 2 items succeeds", true);

// Count() is a method on NativeThreadStream
int count = stream.Count();
t($"Count() = {count} (expected 2)", count == 2);

var reader = stream.AsReader();
int readerCount = reader.BeginForEachIndex(0);
t($"Reader.BeginForEachIndex(0) = {readerCount}", readerCount == 2);
if (readerCount >= 2)
{
    int val1 = reader.Read<int>();
    int val2 = reader.Read<int>();
    t($"Read values: {val1}, {val2} (expected 42, 100)", val1 == 42 && val2 == 100);
}
reader.EndForEachIndex();

// ToNativeArray
var arr = stream.ToNativeArray<int>(Unity.Collections.Allocator.Temp);
t($"ToNativeArray length = {arr.Length}", arr.Length == 2);
if (arr.Length >= 2)
{
    t($"arr[0]={arr[0]}, arr[1]={arr[1]}", arr[0] == 42 && arr[1] == 100);
}
arr.Dispose();

stream.Dispose();
t("Dispose succeeds", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
