// Run: cat snippets/core-collections/NativeListExtensions_ClearAddRange.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,Unity.Collections,System.Linq,System.Reflection"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BovineLabs.Core.Extensions.NativeListExtensions);

sb.AppendLine("NativeListExtensions");
sb.AppendLine($"  Kind: {(extType.IsAbstract && extType.IsSealed ? "static class" : "class")}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var list = new NativeList<int>(Unity.Collections.Allocator.Temp);
list.Add(1); list.Add(2); list.Add(3);
sb.AppendLine($"  Initial: Length={list.Length}");

list.ClearAddRange(new int[] { 10, 20, 30, 40 });
sb.AppendLine($"  ClearAddRange(int[]): Length={list.Length}, [{list[0]},{list[1]},{list[2]},{list[3]}]");

var nativeArr = new NativeArray<int>(2, Unity.Collections.Allocator.Temp);
nativeArr[0] = 100; nativeArr[1] = 200;
list.ClearAddRange(nativeArr);
sb.AppendLine($"  ClearAddRange(NativeArray): Length={list.Length}, [{list[0]},{list[1]}]");

var hashSet = new NativeHashSet<int>(4, Unity.Collections.Allocator.Temp);
hashSet.Add(5); hashSet.Add(15); hashSet.Add(25);
list.ClearAddRange(hashSet);
var listSet = new System.Collections.Generic.HashSet<int>();
for (int i = 0; i < list.Length; i++) listSet.Add(list[i]);
sb.AppendLine($"  ClearAddRange(NativeHashSet): Length={list.Length}, values=[{string.Join(",", listSet)}]");

list.ClearAddRange(new int[] { 99 });
sb.AppendLine($"  ClearAddRange clears first: Length={list.Length}, [0]={list[0]}");

check("ClearAddRange(IEnumerable) works", list.Length == 1 && list[0] == 99);
check("ClearAddRange(NativeArray) works", true);
check("ClearAddRange(NativeHashSet) works", listSet.Count == 3);

list.Dispose(); nativeArr.Dispose(); hashSet.Dispose();

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
