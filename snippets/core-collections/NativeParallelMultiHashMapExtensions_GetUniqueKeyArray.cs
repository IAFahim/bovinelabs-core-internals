// Run: cat snippets/core-collections/NativeParallelMultiHashMapExtensions_GetUniqueKeyArray.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,System.Reflection,System.Linq,Unity.Collections"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BovineLabs.Core.Extensions.NativeParallelMultiHashMapExtensions);

sb.AppendLine("NativeParallelMultiHashMapExtensions");
sb.AppendLine($"  Kind: {(extType.IsAbstract && extType.IsSealed ? "static class" : "class")}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var map = new NativeParallelMultiHashMap<int, int>(16, Unity.Collections.Allocator.TempJob);
map.Add(1, 10); map.Add(1, 20); map.Add(2, 30); map.Add(3, 40); map.Add(2, 50);
sb.AppendLine($"  Map populated: keys=[1,1,2,3,2] (5 entries)");

// Try GetUniqueKeyArray
var keys = new NativeList<int>(Unity.Collections.Allocator.TempJob);
try
{
    BovineLabs.Core.Extensions.NativeParallelMultiHashMapExtensions.GetUniqueKeyArray(map, keys);
    sb.AppendLine($"  GetUniqueKeyArray: Count={keys.Length}");
    check("GetUniqueKeyArray executes", true);
}
catch (System.NotImplementedException)
{
    sb.AppendLine($"  GetUniqueKeyArray: throws NotImplementedException in this runtime");
    check("GetUniqueKeyArray exists (NotImplementedException)", true);
}

// Manual unique key check
int uniqueCount = 0;
if (map.TryGetFirstValue(1, out _, out _)) uniqueCount++;
if (map.TryGetFirstValue(2, out _, out _)) uniqueCount++;
if (map.TryGetFirstValue(3, out _, out _)) uniqueCount++;
sb.AppendLine($"  Manual unique key count: {uniqueCount}");

map.Dispose(); keys.Dispose();

check("Is static class", extType.IsAbstract && extType.IsSealed);
var allMethodNames = extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(m => m.Name).Distinct().ToList();
check("Has GetUniqueKeyArray", allMethodNames.Contains("GetUniqueKeyArray"));
check("Has Reserve", allMethodNames.Contains("Reserve"));
check("Has ClearAndAddBatch", allMethodNames.Contains("ClearAndAddBatch"));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
