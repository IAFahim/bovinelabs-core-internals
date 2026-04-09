// Run: cat snippets/core-collections/UnsafeArray.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.UnsafeArray<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("UnsafeArray<int>");
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

sb.AppendLine("Static Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var arr = new BovineLabs.Core.Collections.UnsafeArray<int>(5, Unity.Collections.Allocator.Temp, Unity.Collections.NativeArrayOptions.ClearMemory);
sb.AppendLine($"  IsCreated={arr.IsCreated}, Length={arr.Length}");
sb.AppendLine($"  Default cleared: [0]={arr[0]}, [4]={arr[4]}");
arr[0] = 42; arr[2] = 99; arr[4] = -1;
sb.AppendLine($"  After writes: [0]={arr[0]}, [2]={arr[2]}, [4]={arr[4]}");
int[] managed = new int[5];
arr.CopyTo(managed);
sb.AppendLine($"  CopyTo: [{managed[0]},{managed[1]},{managed[2]},{managed[3]},{managed[4]}]");
int[] asArr = arr.ToArray();
sb.AppendLine($"  ToArray: Length={asArr.Length}");
var arr2 = new BovineLabs.Core.Collections.UnsafeArray<int>(5, Unity.Collections.Allocator.Temp, Unity.Collections.NativeArrayOptions.ClearMemory);
BovineLabs.Core.Collections.UnsafeArray<int>.Copy(arr, arr2);
sb.AppendLine($"  Static Copy: arr2[0]={arr2[0]}, arr2[2]={arr2[2]}");
arr2.Dispose(); arr.Dispose();

check("Is struct", type.IsValueType);
check("Length==5", arr.Length == 5);
check("CopyTo works", managed[0] == 42 && managed[2] == 99);
check("Static Copy works", arr2[0] == 42);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
