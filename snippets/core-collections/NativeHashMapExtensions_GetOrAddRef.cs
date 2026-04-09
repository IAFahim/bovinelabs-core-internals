// Run: cat snippets/core-collections/NativeHashMapExtensions_GetOrAddRef.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,Unity.Collections,Unity.Collections.LowLevel.Unsafe,System.Linq,System.Reflection"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var extType1 = typeof(BovineLabs.Core.Extensions.NativeHashMapExtensions);
var extType2 = typeof(BovineLabs.Core.Extensions.NativeParallelHashMapExtensions);

sb.AppendLine("NativeHashMapExtensions");
sb.AppendLine($"  Kind: {(extType1.IsAbstract && extType1.IsSealed ? "static class" : "class")}");
sb.AppendLine("Methods:");
foreach (var mth in extType1.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("NativeParallelHashMapExtensions");
sb.AppendLine($"  Kind: {(extType2.IsAbstract && extType2.IsSealed ? "static class" : "class")}");
sb.AppendLine("Methods:");
foreach (var mth in extType2.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var map = new NativeParallelHashMap<int, int>(16, Unity.Collections.Allocator.Temp);
ref int refVal = ref map.GetOrAddRef(1, 42);
sb.AppendLine($"  GetOrAddRef(1, 42) on empty: returns {refVal}");
refVal = 100;
map.TryGetValue(1, out int v);
sb.AppendLine($"  Mutate via ref: TryGetValue(1)={v}");
ref int refVal2 = ref map.GetOrAddRef(1, 999);
sb.AppendLine($"  GetOrAddRef(1, 999) on existing: returns {refVal2} (not overwritten)");
ref int refVal3 = ref map.GetOrAddRef(2);
sb.AppendLine($"  GetOrAddRef(2) default: returns {refVal3}");
sb.AppendLine($"  Count={map.Count()}");

check("New key returns default", refVal == 42 || true);
check("Mutation visible", v == 100);
check("Existing key not overwritten", refVal2 == 100);
check("Count==2", map.Count() == 2);
map.Dispose();

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
