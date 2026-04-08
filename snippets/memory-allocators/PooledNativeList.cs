// Run: cat snippets/memory-allocators/PooledNativeList.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Utility,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Jobs.LowLevel.Unsafe"
// Verifies: docs/PooledNativeList.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists and is struct ---
var pType = typeof(BovineLabs.Core.Utility.PooledNativeList<int>);
t("PooledNativeList<T> type exists", pType != null);
t("Is a struct (ValueType)", pType.IsValueType);
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(pType));

// --- Generic constraint: T : unmanaged ---
var gParams = pType.GetGenericArguments();
t("Has 1 generic parameter", gParams.Length == 1);

// --- Key property: List ---
var listProp = pType.GetProperty("List");
t("Has List property", listProp != null);
if (listProp != null)
{
    t("List property returns NativeList<T>", listProp.PropertyType.Name.StartsWith("NativeList"));
    t("List property is readable", listProp.CanRead);
}

// --- Key method: Make ---
var makeMethod = pType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "Make").FirstOrDefault();
t("Has static Make method", makeMethod != null);
if (makeMethod != null)
{
    t("Make returns PooledNativeList<T>", makeMethod.ReturnType.Name.StartsWith("PooledNativeList"));
    t("Make takes no parameters", makeMethod.GetParameters().Length == 0);
}

// --- Key method: Dispose ---
var disposeMethod = pType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
    .Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has instance Dispose method", disposeMethod != null);

// --- Check internal nested class via reflection ---
// PooledNativeList (non-generic) is internal - access via reflection
var innerPooledType = pType.DeclaringType;
if (innerPooledType == null)
{
    // Check if it's a nested type or if the static holder class is separate
    var assembly = pType.Assembly;
    innerPooledType = assembly.GetType("BovineLabs.Core.Utility.PooledNativeList");
}
r.Add($"INFO: Inner PooledNativeList type: {(innerPooledType != null ? innerPooledType.FullName : "not found as declaring type")}");

// --- MaxPoolSizePerThread = 8 (internal const, access via reflection) ---
if (innerPooledType != null)
{
    var maxPoolField = innerPooledType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        .Where(f => f.Name.Contains("MaxPoolSize")).FirstOrDefault();
    if (maxPoolField != null)
    {
        int val = (int)maxPoolField.GetValue(null);
        t("MaxPoolSizePerThread = 8", val == 8);
        r.Add($"INFO: MaxPoolSizePerThread = {val}");
    }
    else
    {
        t("MaxPoolSizePerThread field found", false);
    }
}

// --- Usage pattern test ---
var list1 = BovineLabs.Core.Utility.PooledNativeList<int>.Make();
t("Make() returns valid instance", list1.List.IsCreated);
t("Initial list is empty", list1.List.Length == 0);

// Add items
list1.List.Add(42);
list1.List.Add(99);
t("Can add items to pooled list", list1.List.Length == 2);
t("Items readable", list1.List[0] == 42 && list1.List[1] == 99);

// Dispose returns to pool
list1.Dispose();
t("Dispose succeeds without error", true);

// Second make should reuse from pool
var list2 = BovineLabs.Core.Utility.PooledNativeList<int>.Make();
t("Second Make() returns valid instance", list2.List.IsCreated);
list2.Dispose();

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
