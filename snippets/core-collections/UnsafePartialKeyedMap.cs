// Run: cat snippets/core-collections/UnsafePartialKeyedMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,Unity.Collections.LowLevel.Unsafe,System.Linq,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.UnsafePartialKeyedMap<int>);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("UnsafePartialKeyedMap<int>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf(type, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Static Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

// Constructor signature
sb.AppendLine("Constructors:");
foreach (var ctor in type.GetConstructors())
    sb.AppendLine($"  ({string.Join(", ", ctor.GetParameters().Select(px => px.ParameterType.Name + " " + px.Name))})");

check("Is struct", type.IsValueType);
check("Has TryGetFirstValue", type.GetMethod("TryGetFirstValue") != null);
check("Has TryGetNextValue", type.GetMethod("TryGetNextValue") != null);
check("Has Update", type.GetMethod("Update") != null);
check("Has Dispose", type.GetMethod("Dispose") != null);
check("Has IsCreated", type.GetProperty("IsCreated") != null);
check("Has indexer", type.GetProperty("Item") != null);
check("No Add() method", type.GetMethod("Add") == null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
