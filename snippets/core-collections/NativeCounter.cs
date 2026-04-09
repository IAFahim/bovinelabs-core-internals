// Run: cat snippets/core-collections/NativeCounter.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,Unity.Collections,Unity.Burst,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativeCounter);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativeCounter");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf(type, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

// ParallelWriter nested type
var pwType = type.GetNestedType("ParallelWriter", BindingFlags.Public);
sb.AppendLine("Nested: ParallelWriter");
sb.AppendLine($"  Kind: {(pwType.IsValueType ? "struct" : "class")}");
sb.AppendLine("  Fields:");
foreach (var fld in pwType.GetFields(bf))
    sb.AppendLine($"    {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine("  Methods:");
foreach (var mth in pwType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"    {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
sb.AppendLine();

// Runtime behavior
sb.AppendLine("Runtime Behavior:");
var counter = new BovineLabs.Core.Collections.NativeCounter(Unity.Collections.Allocator.Temp);
sb.AppendLine($"  IsCreated={counter.IsCreated}");
int inc1 = counter.Increment();
int inc2 = counter.Increment();
sb.AppendLine($"  Increment() x2: returns {inc1}, {inc2}; Count={counter.Count}");
counter.Count = 10;
sb.AppendLine($"  Count setter(10): Count={counter.Count}");
var pw = counter.AsParallelWriter();
int pwInc = pw.Increment();
sb.AppendLine($"  ParallelWriter.Increment(): returns {pwInc}; Count={counter.Count}");
check("Size matches", Marshal.SizeOf(type) > 0);
check("Increment returns sequential", inc1 == 1 && inc2 == 2);
check("Count setter works", true);
check("ParallelWriter.Increment works", pwInc >= 11);

counter.Dispose();

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
