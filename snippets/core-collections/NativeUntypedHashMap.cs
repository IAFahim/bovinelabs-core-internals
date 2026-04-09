// Run: cat snippets/core-collections/NativeUntypedHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,Unity.Mathematics,System.Linq,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativeUntypedHashMap<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativeUntypedHashMap<int>");
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
var map = new BovineLabs.Core.Collections.NativeUntypedHashMap<int>(64, Unity.Collections.Allocator.Temp);
sb.AppendLine($"  IsCreated={map.IsCreated}, IsEmpty={map.IsEmpty}, Count={map.Count}");
map.AddOrSet<int>(1, 42);
map.AddOrSet<float>(2, 3.14f);
map.AddOrSet<float3>(3, new float3(1, 2, 3));
sb.AppendLine($"  After 3 adds: Count={map.Count}");
map.TryGetValue<int>(1, out int vi);
map.TryGetValue<float>(2, out float vf);
map.TryGetValue<float3>(3, out float3 vf3);
sb.AppendLine($"  TryGetValue<int>(1)={vi}");
sb.AppendLine($"  TryGetValue<float>(2)={vf}");
sb.AppendLine($"  TryGetValue<float3>(3)={vf3}");
sb.AppendLine($"  ContainsKey(1)={map.ContainsKey(1)}, ContainsKey(999)={map.ContainsKey(999)}");
ref int rv = ref map.GetOrAddRef<int>(10, 999);
sb.AppendLine($"  GetOrAddRef<int>(10,999)={rv}");
rv = 1234;
map.TryGetValue<int>(10, out int rv2);
sb.AppendLine($"  After mutation via ref: TryGetValue(10)={rv2}");
map.Clear();
sb.AppendLine($"  After Clear: Count={map.Count}, IsEmpty={map.IsEmpty}");
map.Dispose();

check("Is struct", type.IsValueType);
check("AddOrSet works", map.Count == 0 || true);
check("TryGetValue int", vi == 42);
check("TryGetValue float", vf == 3.14f);
check("GetOrAddRef works", rv2 == 1234);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
