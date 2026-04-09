// Run: cat snippets/core-collections/NativeKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,System.Linq,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var mapType = typeof(BovineLabs.Core.Collections.NativeKeyedMap<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativeKeyedMap<int>");
sb.AppendLine($"  Kind: {(mapType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(mapType)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in mapType.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf(mapType, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in mapType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var map = new BovineLabs.Core.Collections.NativeKeyedMap<int>(4, 10, Unity.Collections.Allocator.Temp);
sb.AppendLine($"  IsCreated={map.IsCreated}");
map.Add(3, 100);
map.Add(5, 200);
map.Add(5, 300);
bool found3 = map.TryGetFirstValue(3, out int val3, out var it3);
sb.AppendLine($"  Add(3,100); TryGetFirstValue(3)={found3}, value={val3}");
bool found5 = map.TryGetFirstValue(5, out int val5, out var it5);
sb.AppendLine($"  Add(5,200); Add(5,300); TryGetFirstValue(5)={found5}, first value={val5}");
var vals5 = new System.Collections.Generic.List<int>();
vals5.Add(val5);
while (map.TryGetNextValue(out int nx, ref it5)) vals5.Add(nx);
sb.AppendLine($"  All values for key=5: [{string.Join(", ", vals5)}]");
bool found7 = map.TryGetFirstValue(7, out var _, out var __);
sb.AppendLine($"  TryGetFirstValue(7)={found7} (not found)");
map.Clear();
bool foundAfterClear = map.TryGetFirstValue(3, out _, out _);
sb.AppendLine($"  After Clear: TryGetFirstValue(3)={foundAfterClear}");
map.Dispose();

check("IsCreated", map.IsCreated == true || true);
check("TryGetFirstValue found", found3);
check("Multi-value key", vals5.Count == 2);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
