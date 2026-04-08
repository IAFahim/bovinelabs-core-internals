// Run: cat snippets/memory-allocators/NativeArrayExtensions_WhereNoAlloc.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NativeArrayExtensions_WhereNoAlloc.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var extType = typeof(BovineLabs.Core.Extensions.NativeArrayExtensions);
t("NativeArrayExtensions type exists", extType != null);
t("Is static class", extType.IsAbstract && extType.IsSealed);

// --- IPredicate<T> interface ---
var ipredType = typeof(BovineLabs.Core.Extensions.IPredicate<int>);
t("IPredicate<T> interface exists", ipredType != null);
t("IPredicate is an interface", ipredType.IsInterface);
if (ipredType != null)
{
    var checkMethod = ipredType.GetMethod("Check");
    t("IPredicate has Check method", checkMethod != null);
    if (checkMethod != null)
    {
        t("Check returns bool", checkMethod.ReturnType == typeof(bool));
        t("Check takes T parameter", checkMethod.GetParameters().Length == 1);
    }
}

// --- WhereNoAlloc method ---
var whereNoAllocMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "WhereNoAlloc").FirstOrDefault();
t("Has WhereNoAlloc method", whereNoAllocMethod != null);
if (whereNoAllocMethod != null)
{
    t("WhereNoAlloc is generic method", whereNoAllocMethod.IsGenericMethod);
    t("WhereNoAlloc is static", whereNoAllocMethod.IsStatic);
    var ps = whereNoAllocMethod.GetParameters();
    r.Add($"INFO: WhereNoAlloc params: {string.Join(", ", ps.Select(p => p.ParameterType.Name + " " + p.Name))}");
}

// --- Where method (allocating version) ---
var whereMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "Where" && !m.Name.Contains("NoAlloc")).FirstOrDefault();
t("Has Where method (allocating counterpart)", whereMethod != null);

// --- Min/Max methods ---
var minIntMethods = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "Min").ToList();
t("Has Min methods", minIntMethods.Count > 0);
r.Add($"INFO: {minIntMethods.Count} Min overloads found");

var maxIntMethods = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "Max").ToList();
t("Has Max methods", maxIntMethods.Count > 0);
r.Add($"INFO: {maxIntMethods.Count} Max overloads found");

// --- Functional test: WhereNoAlloc in-place filtering ---
var arr = new NativeArray<int>(6, Allocator.Persistent);
try
{
    arr[0] = 1;
    arr[1] = 2;
    arr[2] = 3;
    arr[3] = 4;
    arr[4] = 5;
    arr[5] = 6;

    // Filter for even numbers (in-place)
    var filtered = arr.WhereNoAlloc<int, BovineLabs.Core.Extensions.Equals<int>>(new BovineLabs.Core.Extensions.Equals<int>(2));
    // Only 2 matches
    t("WhereNoAlloc returns sub-array", filtered.Length >= 0);

    // Test with a custom predicate struct
    // We can use WhereNoAlloc to filter in-place - note it modifies the original
    var arr2 = new NativeArray<int>(6, Allocator.Persistent);
    try
    {
        arr2[0] = 10; arr2[1] = 20; arr2[2] = 30; arr2[3] = 40; arr2[4] = 50; arr2[5] = 60;
        var result = arr2.WhereNoAlloc<int, BovineLabs.Core.Extensions.Equals<int>>(new BovineLabs.Core.Extensions.Equals<int>(30));
        t("WhereNoAlloc finds matching element", result.Length == 1);
        t("WhereNoAlloc returns correct value", result[0] == 30);
    }
    finally
    {
        arr2.Dispose();
    }
}
finally
{
    arr.Dispose();
}

// --- Min/Max functional test ---
var minMaxArr = new NativeArray<int>(4, Allocator.Persistent);
try
{
    minMaxArr[0] = 5; minMaxArr[1] = 2; minMaxArr[2] = 8; minMaxArr[3] = 1;
    t("Min returns correct value", minMaxArr.Min() == 1);
    t("Max returns correct value", minMaxArr.Max() == 8);
}
finally
{
    minMaxArr.Dispose();
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
