// Run: cat snippets/memory-allocators/NativeArrayExtensions_WhereNoAlloc.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
// Verifies: docs/NativeArrayExtensions_WhereNoAlloc.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BovineLabs.Core.Extensions.NativeArrayExtensions);
sb.AppendLine("BovineLabs.Core.Extensions.NativeArrayExtensions");
sb.AppendLine($"  Kind: static class (Abstract={extType.IsAbstract}, Sealed={extType.IsSealed})");
sb.AppendLine();

// IPredicate<T> interface
var ipredType = typeof(BovineLabs.Core.Extensions.IPredicate<int>);
sb.AppendLine("  Nested Interface: IPredicate<T>");
sb.AppendLine($"    IsInterface: {ipredType.IsInterface}");
var checkMethod = ipredType.GetMethod("Check");
if (checkMethod != null)
{
    sb.AppendLine($"    Method: {checkMethod.ReturnType.Name} Check({checkMethod.GetParameters()[0].ParameterType.Name})");
    t("IPredicate has Check returning bool", checkMethod.ReturnType == typeof(bool));
}
sb.AppendLine();

// All public static methods
sb.AppendLine("  Methods (public static):");
foreach (var m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .OrderBy(m => m.Name))
{
    var generic = m.IsGenericMethod ? $"<{string.Join(",", m.GetGenericArguments().Select(ga => ga.Name))}>" : "";
    var pStr = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}{generic}({pStr})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Functional test: WhereNoAlloc
sb.AppendLine("  Functional Tests:");
var arr = new NativeArray<int>(6, Allocator.Persistent);
try
{
    arr[0] = 10; arr[1] = 20; arr[2] = 30; arr[3] = 40; arr[4] = 50; arr[5] = 60;
    var result = arr.WhereNoAlloc<int, BovineLabs.Core.Extensions.Equals<int>>(new BovineLabs.Core.Extensions.Equals<int>(30));
    sb.AppendLine($"    WhereNoAlloc [10,20,30,40,50,60] with Equals(30):");
    sb.AppendLine($"      Result Length: {result.Length}");
    if (result.Length > 0) sb.AppendLine($"      Result[0]: {result[0]}");
    t("WhereNoAlloc finds matching element", result.Length == 1);
    t("WhereNoAlloc returns correct value", result.Length > 0 && result[0] == 30);
}
finally
{
    arr.Dispose();
}

// Min/Max functional test
var minMaxArr = new NativeArray<int>(4, Allocator.Persistent);
try
{
    minMaxArr[0] = 5; minMaxArr[1] = 2; minMaxArr[2] = 8; minMaxArr[3] = 1;
    var minVal = minMaxArr.Min();
    var maxVal = minMaxArr.Max();
    sb.AppendLine($"    Min/Max on [5,2,8,1]:");
    sb.AppendLine($"      Min: {minVal}");
    sb.AppendLine($"      Max: {maxVal}");
    t("Min returns correct value", minVal == 1);
    t("Max returns correct value", maxVal == 8);
}
finally
{
    minMaxArr.Dispose();
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
