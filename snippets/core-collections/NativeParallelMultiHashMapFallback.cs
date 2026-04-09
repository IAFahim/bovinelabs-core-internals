// Run: cat snippets/core-collections/NativeParallelMultiHashMapFallback.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq,Unity.Collections,Unity.Jobs,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativeParallelMultiHashMapFallback<int, int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativeParallelMultiHashMapFallback<int,int>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  [{Marshal.OffsetOf(type, fld.Name)}] {fld.FieldType.Name} {fld.Name}  (public)");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

// Nested types
var pwType = type.GetNestedType("ParallelWriter");
var fbDataType = type.GetNestedType("FallbackData");
sb.AppendLine("Nested: ParallelWriter");
if (pwType != null)
{
    sb.AppendLine($"  Kind: {(pwType.IsValueType ? "struct" : "class")}");
    foreach (var mth in pwType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
        sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
}
sb.AppendLine("Nested: FallbackData");
if (fbDataType != null)
{
    sb.AppendLine($"  Kind: {(fbDataType.IsValueType ? "struct" : "class")}");
    foreach (var fld in fbDataType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        sb.AppendLine($"  {fld.FieldType.Name} {fld.Name}");
}
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var map = new BovineLabs.Core.Collections.NativeParallelMultiHashMapFallback<int, int>(10, Unity.Collections.Allocator.TempJob);
var writer = map.AsWriter();
writer.Add(1, 100);
writer.Add(2, 200);
writer.Add(1, 300);
sb.AppendLine($"  Added 3 entries via ParallelWriter: (1,100),(2,200),(1,300)");
var handle = map.Apply(default(Unity.Jobs.JobHandle), out var reader);
handle.Complete();
bool found1 = reader.TryGetFirstValue(1, out int v1, out var it1);
sb.AppendLine($"  After Apply: TryGetFirstValue(1)={found1}, value={v1}");
bool hasNext = reader.TryGetNextValue(out int v1b, ref it1);
sb.AppendLine($"  TryGetNextValue(1)={hasNext}, value={v1b}");
bool found2 = reader.TryGetFirstValue(2, out int v2, out _);
sb.AppendLine($"  TryGetFirstValue(2)={found2}, value={v2}");
map.Dispose();

check("Is struct", type.IsValueType);
check("Has HashMap field", type.GetField("HashMap") != null);
check("Has Fallback field", type.GetField("Fallback") != null);
check("TryGetFirstValue works", found1 && found2);
check("Multi-value iteration", hasNext);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
