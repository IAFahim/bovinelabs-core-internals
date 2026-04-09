// Run: cat snippets/core-collections/NativeWorkQueue.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,Unity.Collections,System.Linq,Unity.Burst,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativeWorkQueue<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativeWorkQueue<int>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

// Nested types
var pwType = type.GetNestedType("ParallelWriter");
var prType = type.GetNestedType("ParallelReader");
sb.AppendLine("Nested: ParallelWriter");
if (pwType != null)
{
    sb.AppendLine($"  Kind: {(pwType.IsValueType ? "struct" : "class")}");
    foreach (var mth in pwType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
        sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
    foreach (var prp in pwType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
}
sb.AppendLine("Nested: ParallelReader");
if (prType != null)
{
    sb.AppendLine($"  Kind: {(prType.IsValueType ? "struct" : "class")}");
    foreach (var mth in prType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
        sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
    foreach (var prp in prType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
}
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var queue = new BovineLabs.Core.Collections.NativeWorkQueue<int>(10, Unity.Collections.Allocator.Temp);
sb.AppendLine($"  Capacity={queue.Capacity}, Length={queue.Length}, HasCapacity={queue.HasCapacity}");
queue.Update();
sb.AppendLine($"  After empty Update: Length={queue.Length}");
queue.Dispose();

check("Is struct", type.IsValueType);
check("Has Length", type.GetProperty("Length") != null);
check("Has Capacity", type.GetProperty("Capacity") != null);
check("Has ParallelWriter", pwType != null);
check("Has ParallelReader", prType != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
