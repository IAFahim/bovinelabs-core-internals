// Run: cat snippets/memory-allocators/NoAllocHelpers.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Utility,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NoAllocHelpers.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var nahType = typeof(BovineLabs.Core.Utility.NoAllocHelpers);
t("NoAllocHelpers type exists", nahType != null);
t("Is static class", nahType.IsAbstract && nahType.IsSealed);

// --- ExtractArrayFromList method ---
var extractMethod = nahType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "ExtractArrayFromList" && m.IsGenericMethod).FirstOrDefault();
t("Has ExtractArrayFromList<T> method", extractMethod != null);
if (extractMethod != null)
{
    t("ExtractArrayFromList is static", extractMethod.IsStatic);
    var ps = extractMethod.GetParameters();
    t("Takes List<T> parameter", ps.Length == 1 && ps[0].ParameterType.Name.StartsWith("List"));
    t("Returns T[]", extractMethod.ReturnType.IsArray);
}

// --- ResizeList method ---
var resizeMethod = nahType.GetMethods(BindingFlags.Public | BindingFlags.Static)
    .Where(m => m.Name == "ResizeList" && m.IsGenericMethod).FirstOrDefault();
t("Has ResizeList<T> method", resizeMethod != null);
if (resizeMethod != null)
{
    t("ResizeList is static", resizeMethod.IsStatic);
    var ps = resizeMethod.GetParameters();
    t("Takes List<T> and int count", ps.Length == 2 && ps[0].ParameterType.Name.StartsWith("List") && ps[1].ParameterType == typeof(int));
    t("Returns void", resizeMethod.ReturnType == typeof(void));
}

// --- Functional test: ExtractArrayFromList ---
var list = new System.Collections.Generic.List<int> { 10, 20, 30 };
int[] backingArray = BovineLabs.Core.Utility.NoAllocHelpers.ExtractArrayFromList(list);
t("ExtractArrayFromList returns non-null", backingArray != null);
t("Returned array Length >= list.Count", backingArray.Length >= list.Count);
t("Returned array contains list elements", backingArray[0] == 10 && backingArray[1] == 20 && backingArray[2] == 30);
r.Add($"INFO: List.Count={list.Count}, Array.Length={backingArray.Length} (capacity={list.Capacity})");

// --- Functional test: ResizeList ---
// Doc says: Clear() is called first, then size is set directly
// So elements become default(0) after ResizeList
var list2 = new System.Collections.Generic.List<int> { 1, 2, 3 };
list2.Capacity = 20; // Ensure enough capacity
BovineLabs.Core.Utility.NoAllocHelpers.ResizeList(list2, 10);
t("ResizeList sets Count to target", list2.Count == 10);
t("ResizeList clears elements first (Clear() called)", list2[0] == 0); // Clear() resets elements to default
r.Add($"INFO: After ResizeList, Count={list2.Count}, Capacity={list2.Capacity}");

// --- ResizeList with capacity insufficient ---
var list3 = new System.Collections.Generic.List<int> { 1, 2, 3 };
BovineLabs.Core.Utility.NoAllocHelpers.ResizeList(list3, 100);
t("ResizeList can grow beyond current capacity", list3.Count == 100);
t("ResizeList increases capacity when needed", list3.Capacity >= 100);

// --- ResizeList to 0 ---
var list4 = new System.Collections.Generic.List<int> { 1, 2, 3 };
BovineLabs.Core.Utility.NoAllocHelpers.ResizeList(list4, 0);
t("ResizeList to 0 sets Count=0", list4.Count == 0);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
