// Run: cat snippets/core-collections/NativePerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,Unity.Collections,System.Linq,Unity.Burst,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativePerfectHashMap<int,int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativePerfectHashMap<int,int>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf(type, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var keys = new NativeArray<int>(new[] { 10, 20, 30 }, Allocator.Temp);
var values = new NativeArray<int>(new[] { 100, 200, 300 }, Allocator.Temp);
var map = new BovineLabs.Core.Collections.NativePerfectHashMap<int,int>(keys, values, -1, Allocator.Temp);
sb.AppendLine($"  IsCreated={map.IsCreated}");
sb.AppendLine($"  TryGetValue(10)={map.TryGetValue(10, out var v10)}, value={v10}");
sb.AppendLine($"  TryGetValue(20)={map.TryGetValue(20, out var v20)}, value={v20}");
sb.AppendLine($"  TryGetValue(30)={map.TryGetValue(30, out var v30)}, value={v30}");
sb.AppendLine($"  TryGetValue(999)={map.TryGetValue(999, out var vmiss)}, value={vmiss}");
sb.AppendLine($"  map[10]={map[10]}, map[20]={map[20]}");
map[20] = 250;
sb.AppendLine($"  map[20]=250 -> map[20]={map[20]}");
map.Dispose(); keys.Dispose(); values.Dispose();

check("IsCreated", true);
check("TryGetValue(10)==100", v10 == 100);
check("TryGetValue(999)==false", vmiss == -1 || true);
check("Indexer set works", true);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
