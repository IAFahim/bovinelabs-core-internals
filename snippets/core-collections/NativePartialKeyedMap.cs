// Run: cat snippets/core-collections/NativePartialKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,Unity.Collections.LowLevel.Unsafe,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativePartialKeyedMap<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativePartialKeyedMap<int>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

var unsafeType = typeof(BovineLabs.Core.Collections.UnsafePartialKeyedMap<int>);
sb.AppendLine("UnsafePartialKeyedMap<int>");
sb.AppendLine($"  Kind: {(unsafeType.IsValueType ? "struct" : "class")}");
sb.AppendLine("Methods:");
foreach (var mth in unsafeType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

check("Is struct", type.IsValueType);
check("Has IsCreated", type.GetProperty("IsCreated") != null);
check("Has TryGetFirstValue", type.GetMethod("TryGetFirstValue") != null);
check("Has TryGetNextValue", type.GetMethod("TryGetNextValue") != null);
check("Has Update", type.GetMethod("Update") != null);
check("Has Dispose", type.GetMethod("Dispose") != null);
check("No Add() method", unsafeType.GetMethod("Add") == null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
