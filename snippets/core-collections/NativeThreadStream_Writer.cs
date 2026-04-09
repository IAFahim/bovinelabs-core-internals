// Run: cat snippets/core-collections/NativeThreadStream_Writer.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);
var writerType = ntsType.GetNestedType("Writer", BindingFlags.Public | BindingFlags.NonPublic);

sb.AppendLine("NativeThreadStream.Writer");
sb.AppendLine($"  Kind: {(writerType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(writerType)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in writerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    sb.AppendLine($"  [{Marshal.OffsetOf(writerType, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

check("Writer exists", writerType != null);
check("Writer is struct", writerType.IsValueType);
check("Has Write", writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name == "Write"));
check("Has Allocate", writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name == "Allocate"));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
