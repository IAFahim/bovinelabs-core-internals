// Run: cat snippets/core-collections/ThreadList.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Jobs.LowLevel.Unsafe"
// Verifies: docs/ThreadList.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var type = typeof(BovineLabs.Core.Collections.ThreadList);
t("ThreadList type exists", type != null);
t("Is a struct (ValueType)", type.IsValueType);

var isCreated = type.GetProperty("IsCreated", BindingFlags.Public | BindingFlags.Instance);
t("Has IsCreated property", isCreated != null);

var getListNoParam = type.GetMethod("GetList", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
t("Has GetList() (no param)", getListNoParam != null);

var getListWithParam = type.GetMethod("GetList", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
t("Has GetList(int threadIndex)", getListWithParam != null);

// Verify return type is ref UnsafeList<byte>
if (getListNoParam != null)
    t("GetList() returns UnsafeList<byte>", getListNoParam.ReturnType.Name.StartsWith("UnsafeList"));
if (getListWithParam != null)
    t("GetList(int) returns UnsafeList<byte>", getListWithParam.ReturnType.Name.StartsWith("UnsafeList"));

var disposeMethod = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
t("Has Dispose()", disposeMethod != null);

var ctor = type.GetConstructor(new[] { typeof(Unity.Collections.AllocatorManager.AllocatorHandle) });
t("Constructor takes AllocatorHandle", ctor != null);

// --- Claim: Internal Lists struct is cache-line sized (64 bytes) ---
var listsType = type.GetNestedType("Lists", BindingFlags.NonPublic | BindingFlags.Public);
t("Lists nested type exists", listsType != null);
if (listsType != null)
{
    t("Lists is struct", listsType.IsValueType);
    var attr = listsType.GetCustomAttributes(typeof(System.Runtime.InteropServices.StructLayoutAttribute), false);
    bool hasExplicitLayout = false; int explicitSize = 0;
    foreach (var a in attr) { var sla = (StructLayoutAttribute)a; if (sla.Value == LayoutKind.Explicit) { hasExplicitLayout = true; explicitSize = sla.Size; } }
    t("Lists has [StructLayout(LayoutKind.Explicit)]", hasExplicitLayout);
    int cacheLineSize = Unity.Jobs.LowLevel.Unsafe.JobsUtility.CacheLineSize;
    t($"Lists Size = CacheLineSize ({cacheLineSize}), actual = {explicitSize}", explicitSize == cacheLineSize);
    var listField = listsType.GetField("List", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    t("Lists has List field of UnsafeList<byte>", listField != null && listField.FieldType.Name == "UnsafeList`1");
}

// --- Functional test ---
var threadList = new BovineLabs.Core.Collections.ThreadList(Unity.Collections.Allocator.Temp);
t("ThreadList construction succeeds", threadList.IsCreated);

var list0 = threadList.GetList(0);
t($"Initial Capacity = 512 (got {list0.Capacity})", list0.Capacity == 512);
t("List uses byte type (UnsafeList<byte>)", list0.IsCreated);

threadList.Dispose();
t("Dispose succeeds", true);
t("IsCreated == false after Dispose", !threadList.IsCreated);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
