// Run: cat snippets/core-collections/NativeThreadStream_Reader.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);

// Reader
var readerType = ntsType.GetNestedType("Reader", BindingFlags.Public | BindingFlags.NonPublic);
sb.AppendLine("NativeThreadStream.Reader");
sb.AppendLine($"  Kind: {(readerType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(readerType)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in readerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    sb.AppendLine($"  [{Marshal.OffsetOf(readerType, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in readerType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in readerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

// Writer (brief)
var writerType = ntsType.GetNestedType("Writer", BindingFlags.Public | BindingFlags.NonPublic);
sb.AppendLine("NativeThreadStream.Writer");
sb.AppendLine($"  Kind: {(writerType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(writerType)} bytes");
sb.AppendLine("Methods:");
foreach (var mth in writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");

check("Reader exists", readerType != null);
check("Reader is struct", readerType.IsValueType);
check("Writer exists", writerType != null);
check("Reader has BeginForEachIndex", readerType.GetMethod("BeginForEachIndex") != null);
check("Reader has Read", readerType.GetMethod("Read") != null);
check("Writer has Write", writerType.GetMethod("Write") != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
