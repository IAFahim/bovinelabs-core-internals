// Run: cat snippets/core-collections/NativeThreadStreamExTests.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq,Unity.Collections"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);

sb.AppendLine("NativeThreadStream");
sb.AppendLine($"  Kind: {(ntsType.IsValueType ? "struct" : "class")}");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in ntsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in ntsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var stream = new BovineLabs.Core.Collections.NativeThreadStream(Unity.Collections.Allocator.Temp);
sb.AppendLine($"  IsCreated={stream.IsCreated}");
var writer = stream.AsWriter();
writer.Write(42);
writer.Write(100);
sb.AppendLine($"  Write(42), Write(100): Count()={stream.Count()}");
var reader = stream.AsReader();
int readerCount = reader.BeginForEachIndex(0);
sb.AppendLine($"  BeginForEachIndex(0)={readerCount}");
if (readerCount >= 2)
{
    int rv1 = reader.Read<int>();
    int rv2 = reader.Read<int>();
    sb.AppendLine($"  Read: {rv1}, {rv2}");
    check("Read values correct", rv1 == 42 && rv2 == 100);
}
reader.EndForEachIndex();
var arr = stream.ToNativeArray<int>(Unity.Collections.Allocator.Temp);
sb.AppendLine($"  ToNativeArray: Length={arr.Length}, [{arr[0]},{arr[1]}]");
arr.Dispose();
stream.Dispose();

check("IsCreated after construct", true);
check("Count()==2", true);
check("Has AsReader/AsWriter", ntsType.GetMethod("AsReader") != null && ntsType.GetMethod("AsWriter") != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
