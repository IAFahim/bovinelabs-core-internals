// Run: cat snippets/core-collections/FixedArray.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,Unity.Mathematics,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

// FixedArray<float, float4>
var faFloat4 = typeof(BovineLabs.Core.Collections.FixedArray<float, Unity.Mathematics.float4>);
sb.AppendLine("FixedArray<float, float4>");
sb.AppendLine($"  Kind: {(faFloat4.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(faFloat4)} bytes");
sb.AppendLine("Fields:");
foreach (var fld in faFloat4.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf(faFloat4, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine("Properties:");
foreach (var prp in faFloat4.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine("Methods:");
foreach (var mth in faFloat4.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var fa = Activator.CreateInstance(faFloat4);
var lenProp = faFloat4.GetProperty("Length");
int flen = (int)lenProp.GetValue(fa);
sb.AppendLine($"  Length = {flen} (sizeof(float4)/sizeof(float) = 16/4)");
var indexer = faFloat4.GetProperty("Item");
indexer.GetSetMethod().Invoke(fa, new object[] { 0, 1.5f });
indexer.GetSetMethod().Invoke(fa, new object[] { 3, 42.0f });
float v0 = (float)indexer.GetGetMethod().Invoke(fa, new object[] { 0 });
float v3 = (float)indexer.GetGetMethod().Invoke(fa, new object[] { 3 });
sb.AppendLine($"  [0]={v0}, [3]={v3}");
check("FixedArray<float,float4> Length==4", flen == 4);
check("[0]=1.5 after set", v0 == 1.5f);
check("[3]=42.0 after set", v3 == 42.0f);
sb.AppendLine();

// FixedArray<int, ulong>
var faIntUL = typeof(BovineLabs.Core.Collections.FixedArray<int, ulong>);
sb.AppendLine("FixedArray<int, ulong>");
sb.AppendLine($"  Size: {Marshal.SizeOf(faIntUL)} bytes");
var fa2 = Activator.CreateInstance(faIntUL);
int len2 = (int)faIntUL.GetProperty("Length").GetValue(fa2);
sb.AppendLine($"  Length = {len2} (sizeof(ulong)/sizeof(int) = 8/4)");
check("FixedArray<int,ulong> Length==2", len2 == 2);
sb.AppendLine();

// FixedArray<byte, float4>
var faByteF4 = typeof(BovineLabs.Core.Collections.FixedArray<byte, Unity.Mathematics.float4>);
sb.AppendLine("FixedArray<byte, float4>");
sb.AppendLine($"  Size: {Marshal.SizeOf(faByteF4)} bytes");
var fa3 = Activator.CreateInstance(faByteF4);
int len3 = (int)faByteF4.GetProperty("Length").GetValue(fa3);
sb.AppendLine($"  Length = {len3} (sizeof(float4)/sizeof(byte) = 16/1)");
check("FixedArray<byte,float4> Length==16", len3 == 16);
sb.AppendLine();

// FixedArray<byte, float4x4>
var faByteF4x4 = typeof(BovineLabs.Core.Collections.FixedArray<byte, Unity.Mathematics.float4x4>);
sb.AppendLine("FixedArray<byte, float4x4>");
sb.AppendLine($"  Size: {Marshal.SizeOf(faByteF4x4)} bytes");
var fa4 = Activator.CreateInstance(faByteF4x4);
int len4 = (int)faByteF4x4.GetProperty("Length").GetValue(fa4);
sb.AppendLine($"  Length = {len4} (sizeof(float4x4)/sizeof(byte) = 64/1)");
check("FixedArray<byte,float4x4> Length==64", len4 == 64);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
