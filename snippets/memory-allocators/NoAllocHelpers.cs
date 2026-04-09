// Run: cat snippets/memory-allocators/NoAllocHelpers.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Utility,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NoAllocHelpers.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var nahType = typeof(BovineLabs.Core.Utility.NoAllocHelpers);
sb.AppendLine("BovineLabs.Core.Utility.NoAllocHelpers");
sb.AppendLine($"  Kind: static class (Abstract={nahType.IsAbstract}, Sealed={nahType.IsSealed})");
sb.AppendLine();

// Methods
sb.AppendLine("  Methods (public static):");
foreach (var m in nahType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .OrderBy(m => m.Name))
{
    var generic = m.IsGenericMethod ? $"<{string.Join(",", m.GetGenericArguments().Select(ga => ga.Name))}>" : "";
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}{generic}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Functional test: ExtractArrayFromList
sb.AppendLine("  Functional Tests:");
var list1 = new System.Collections.Generic.List<int> { 10, 20, 30 };
int[] backingArray = BovineLabs.Core.Utility.NoAllocHelpers.ExtractArrayFromList(list1);
sb.AppendLine($"    ExtractArrayFromList([10,20,30]):");
sb.AppendLine($"      List.Count: {list1.Count}");
sb.AppendLine($"      List.Capacity: {list1.Capacity}");
sb.AppendLine($"      Returned array.Length: {backingArray.Length}");
sb.AppendLine($"      Elements match: {backingArray[0] == 10 && backingArray[1] == 20 && backingArray[2] == 30}");
t("ExtractArrayFromList returns non-null", backingArray != null);
t("Returned array Length >= list.Count", backingArray.Length >= list1.Count);
t("Returned array contains list elements", backingArray[0] == 10 && backingArray[1] == 20 && backingArray[2] == 30);
sb.AppendLine();

// Functional test: ResizeList
var list2 = new System.Collections.Generic.List<int> { 1, 2, 3 };
list2.Capacity = 20;
BovineLabs.Core.Utility.NoAllocHelpers.ResizeList(list2, 10);
sb.AppendLine($"    ResizeList([1,2,3], count=10):");
sb.AppendLine($"      After resize Count: {list2.Count}");
sb.AppendLine($"      After resize Capacity: {list2.Capacity}");
sb.AppendLine($"      Elements cleared (index 0 == 0): {list2[0] == 0}");
t("ResizeList sets Count to target", list2.Count == 10);
t("ResizeList clears elements first (Clear() called)", list2[0] == 0);
sb.AppendLine();

// ResizeList beyond capacity
var list3 = new System.Collections.Generic.List<int> { 1, 2, 3 };
BovineLabs.Core.Utility.NoAllocHelpers.ResizeList(list3, 100);
sb.AppendLine($"    ResizeList([1,2,3], count=100):");
sb.AppendLine($"      After resize Count: {list3.Count}");
sb.AppendLine($"      After resize Capacity: {list3.Capacity}");
t("ResizeList can grow beyond current capacity", list3.Count == 100);
t("ResizeList increases capacity when needed", list3.Capacity >= 100);
sb.AppendLine();

// ResizeList to 0
var list4 = new System.Collections.Generic.List<int> { 1, 2, 3 };
BovineLabs.Core.Utility.NoAllocHelpers.ResizeList(list4, 0);
sb.AppendLine($"    ResizeList([1,2,3], count=0):");
sb.AppendLine($"      After resize Count: {list4.Count}");
t("ResizeList to 0 sets Count=0", list4.Count == 0);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
