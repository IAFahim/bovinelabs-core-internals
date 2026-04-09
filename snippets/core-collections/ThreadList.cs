// Run: cat snippets/core-collections/ThreadList.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Jobs.LowLevel.Unsafe"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.ThreadList);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("ThreadList");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

// Lists nested type
var listsType = type.GetNestedType("Lists", BindingFlags.NonPublic | BindingFlags.Public);
sb.AppendLine("Nested: Lists");
if (listsType != null)
{
    sb.AppendLine($"  Kind: {(listsType.IsValueType ? "struct" : "class")}");
    sb.AppendLine($"  Size: {Marshal.SizeOf(listsType)} bytes");
    var layout = listsType.StructLayoutAttribute;
    sb.AppendLine($"  StructLayout: {(layout != null ? layout.Value.ToString() : "none")}, Size={layout?.Size}");
    sb.AppendLine($"  CacheLineSize={Unity.Jobs.LowLevel.Unsafe.JobsUtility.CacheLineSize}");
}
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var tl = new BovineLabs.Core.Collections.ThreadList(Unity.Collections.Allocator.Temp);
sb.AppendLine($"  IsCreated={tl.IsCreated}");
var list0 = tl.GetList(0);
sb.AppendLine($"  GetList(0): Capacity={list0.Capacity}, IsCreated={list0.IsCreated}");
tl.Dispose();
sb.AppendLine($"  After Dispose: IsCreated={tl.IsCreated}");

check("Is struct", type.IsValueType);
check("IsCreated after construct", tl.IsCreated);
check("Initial Capacity=512", list0.Capacity == 512);
check("Has GetList", type.GetMethod("GetList") != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
