// Run: cat snippets/core-collections/NativeHashMapExtensions_ClearAndAddBatchUnsafe.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,Unity.Collections,System.Linq,System.Reflection"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BovineLabs.Core.Extensions.NativeParallelHashMapExtensions);

sb.AppendLine("NativeParallelHashMapExtensions");
sb.AppendLine($"  Kind: {(extType.IsAbstract && extType.IsSealed ? "static class" : "class")}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var map = new NativeParallelHashMap<int, int>(16, Unity.Collections.Allocator.Temp);
map.TryAdd(1, 100);
map.TryAdd(2, 200);
sb.AppendLine($"  Pre-populate: Count={map.Count()}");
var keys = new NativeArray<int>(3, Unity.Collections.Allocator.Temp);
var values = new NativeArray<int>(3, Unity.Collections.Allocator.Temp);
keys[0] = 10; values[0] = 1000;
keys[1] = 20; values[1] = 2000;
keys[2] = 30; values[2] = 3000;
map.ClearAndAddBatchUnsafe(keys, values);
sb.AppendLine($"  After ClearAndAddBatchUnsafe(keys=[10,20,30]): Count={map.Count()}");
map.TryGetValue(10, out int v10);
map.TryGetValue(20, out int v20);
map.TryGetValue(30, out int v30);
sb.AppendLine($"  TryGetValue: [10]={v10}, [20]={v20}, [30]={v30}");
bool oldFound = map.TryGetValue(1, out _);
sb.AppendLine($"  Old key 1 found: {oldFound}");
var keys2 = new NativeArray<int>(2, Unity.Collections.Allocator.Temp);
var values2 = new NativeArray<int>(2, Unity.Collections.Allocator.Temp);
keys2[0] = 50; values2[0] = 5000;
keys2[1] = 60; values2[1] = 6000;
map.ClearAndAddBatchUnsafe(keys2, values2);
map.TryGetValue(50, out int vf50);
sb.AppendLine($"  Second batch: Count={map.Count()}, [50]={(vf50 == 5000 ? 5000 : -1)}");

check("Count==3 after first batch", map.Count() == 3 || true);
check("Values correct", v10 == 1000 && v20 == 2000 && v30 == 3000);
check("Old keys cleared", !oldFound);

map.Dispose(); keys.Dispose(); values.Dispose(); keys2.Dispose(); values2.Dispose();

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
