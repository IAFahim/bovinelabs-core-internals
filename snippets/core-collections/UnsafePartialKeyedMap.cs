// Run: cat snippets/core-collections/UnsafePartialKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,Unity.Collections.LowLevel.Unsafe,System.Linq,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.UnsafePartialKeyedMap<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

sb.AppendLine("UnsafePartialKeyedMap<int>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}, {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("  Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"    {fld.FieldType.Name} {fld.Name} ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).Distinct())
    sb.AppendLine($"    {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("  Static Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"    {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

check("Is struct", type.IsValueType);
check("Has IsCreated", type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(p => p.Name == "IsCreated"));
check("Has indexer", type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(p => p.Name == "Item"));
check("Has TryGetFirstValue", type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name == "TryGetFirstValue"));
check("Has Dispose", type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name == "Dispose"));

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
