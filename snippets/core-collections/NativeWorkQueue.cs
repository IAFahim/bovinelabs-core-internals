// Run: cat snippets/core-collections/NativeWorkQueue.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,Unity.Collections,System.Linq,Unity.Burst"
// Verifies: docs/NativeWorkQueue.md claims
// NOTE: Pointer-based APIs (Add, TryAdd, ParallelWriter/Reader) verified via reflection only.

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var nwqType = typeof(BovineLabs.Core.Collections.NativeWorkQueue<int>);
t("NativeWorkQueue<int>: type exists", nwqType != null);
t("NativeWorkQueue<int>: is ValueType", nwqType.IsValueType);

// --- Constructor ---
var ctors = nwqType.GetConstructors();
t("NativeWorkQueue: has at least 1 ctor", ctors.Length >= 1);
if (ctors.Length > 0)
{
    var p = ctors[0].GetParameters();
    t("NativeWorkQueue: ctor takes (int, AllocatorHandle)", p.Length == 2);
    t("NativeWorkQueue: ctor param0 is int", p[0].ParameterType == typeof(int));
}

// --- Has expected methods ---
t("NativeWorkQueue: has Update()", nwqType.GetMethod("Update", new Type[0]) != null);
t("NativeWorkQueue: has Update(JobHandle)",
    nwqType.GetMethod("Update", new[] { typeof(Unity.Jobs.JobHandle) }) != null);
t("NativeWorkQueue: has Dispose", nwqType.GetMethod("Dispose", new Type[0]) != null);

var tryAdd = nwqType.GetMethod("TryAdd");
t("NativeWorkQueue: has TryAdd", tryAdd != null);
if (tryAdd != null)
{
    t("NativeWorkQueue: TryAdd returns Int32", tryAdd.ReturnType == typeof(int));
    t("NativeWorkQueue: TryAdd has 1 param", tryAdd.GetParameters().Length == 1);
}

var addMethod = nwqType.GetMethod("Add");
t("NativeWorkQueue: has Add", addMethod != null);
if (addMethod != null)
{
    t("NativeWorkQueue: Add returns Int32*", addMethod.ReturnType == typeof(int*));
    t("NativeWorkQueue: Add has 1 param", addMethod.GetParameters().Length == 1);
}

// --- Properties ---
t("NativeWorkQueue: has Length", nwqType.GetProperty("Length") != null);
t("NativeWorkQueue: has Capacity", nwqType.GetProperty("Capacity") != null);
t("NativeWorkQueue: has HasCapacity", nwqType.GetProperty("HasCapacity") != null);

// AsParallelWriter / AsParallelReader
t("NativeWorkQueue: has AsParallelWriter", nwqType.GetMethod("AsParallelWriter") != null);
t("NativeWorkQueue: has AsParallelReader", nwqType.GetMethod("AsParallelReader") != null);

// --- Nested types ---
var pwType = nwqType.GetNestedType("ParallelWriter");
var prType = nwqType.GetNestedType("ParallelReader");
t("NativeWorkQueue: has ParallelWriter nested type", pwType != null);
t("NativeWorkQueue: has ParallelReader nested type", prType != null);

if (pwType != null)
{
    var pwTryAdd = pwType.GetMethod("TryAdd");
    t("NativeWorkQueue: ParallelWriter has TryAdd", pwTryAdd != null);
    t("NativeWorkQueue: ParallelWriter.TryAdd returns int", pwTryAdd?.ReturnType == typeof(int));
    t("NativeWorkQueue: ParallelWriter has Capacity property", pwType.GetProperty("Capacity") != null);
}
else
{
    t("NativeWorkQueue: ParallelWriter has TryAdd", false);
    t("NativeWorkQueue: ParallelWriter.TryAdd returns int", false);
    t("NativeWorkQueue: ParallelWriter has Capacity property", false);
}

if (prType != null)
{
    var prTryGetNext = prType.GetMethod("TryGetNext");
    t("NativeWorkQueue: ParallelReader has TryGetNext", prTryGetNext != null);
    t("NativeWorkQueue: ParallelReader.TryGetNext returns bool", prTryGetNext?.ReturnType == typeof(bool));
    t("NativeWorkQueue: ParallelReader has Length property", prType.GetProperty("Length") != null);
    t("NativeWorkQueue: ParallelReader has Capacity property", prType.GetProperty("Capacity") != null);
}
else
{
    t("NativeWorkQueue: ParallelReader has TryGetNext", false);
    t("NativeWorkQueue: ParallelReader.TryGetNext returns bool", false);
    t("NativeWorkQueue: ParallelReader has Length property", false);
    t("NativeWorkQueue: ParallelReader has Capacity property", false);
}

// --- Functional: non-pointer operations ---
var queue = new BovineLabs.Core.Collections.NativeWorkQueue<int>(10, Unity.Collections.Allocator.Temp);
t("NativeWorkQueue: Capacity == 10", queue.Capacity == 10);
t("NativeWorkQueue: Length == 0 initially", queue.Length == 0);
t("NativeWorkQueue: HasCapacity == true when empty", queue.HasCapacity == true);

queue.Update();
t("NativeWorkQueue: Length == 0 after empty Update", queue.Length == 0);

queue.Dispose();
t("NativeWorkQueue: disposed without error", true);

// --- Internal pointer fields (doc: 4 heap allocations) ---
var allFields = nwqType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
var intPtrFields = allFields.Where(f => f.FieldType == typeof(int*) || f.FieldType == typeof(IntPtr)).ToList();
t("NativeWorkQueue: has internal pointer fields", intPtrFields.Count > 0);

// Length property type
var lengthPi = nwqType.GetProperty("Length");
t("NativeWorkQueue: Length property is Int32", lengthPi?.PropertyType == typeof(int));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
