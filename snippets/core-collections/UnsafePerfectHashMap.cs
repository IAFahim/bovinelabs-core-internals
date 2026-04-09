// Run: cat snippets/core-collections/UnsafePerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,System.Linq,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.UnsafePerfectHashMap<int, int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("UnsafePerfectHashMap<int,int>");
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
var keys = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
var values = new NativeArray<int>(new[] { 100, 200, 300 }, Allocator.Temp);
var map = new BovineLabs.Core.Collections.UnsafePerfectHashMap<int, int>(keys, values, -1, Allocator.Temp);
sb.AppendLine($"  IsCreated={map.IsCreated}");
sb.AppendLine($"  TryGetValue(1)={map.TryGetValue(1, out var v1)}, value={v1}");
sb.AppendLine($"  TryGetValue(2)={map.TryGetValue(2, out var v2)}, value={v2}");
sb.AppendLine($"  TryGetValue(3)={map.TryGetValue(3, out var v3)}, value={v3}");
sb.AppendLine($"  TryGetValue(0)={map.TryGetValue(0, out var vmiss)} (empty slot)");
sb.AppendLine($"  map[1]={map[1]}");
map[1] = 111;
sb.AppendLine($"  map[1]=111 -> TryGetValue(1)={map.TryGetValue(1, out v1)}, value={v1}");
map.Dispose(); keys.Dispose(); values.Dispose();

check("Is struct", type.IsValueType);
check("TryGetValue(1)==100", v1 == 111);
check("TryGetValue(2)==200", v2 == 200);
check("IsCreated", map.IsCreated);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
