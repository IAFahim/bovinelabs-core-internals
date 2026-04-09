// Run: cat snippets/core-collections/NativeLinearCongruentialGenerator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.NativeLinearCongruentialGenerator);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("NativeLinearCongruentialGenerator");
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

sb.AppendLine("Runtime Behavior:");
var rng = new BovineLabs.Core.Collections.NativeLinearCongruentialGenerator(42, Unity.Collections.Allocator.Temp);
int val1 = rng.Next();
int val2 = rng.Next();
int val3 = rng.Next();
long exp1 = ((long)134775813 * 42 + 1) & 0x7FFFFFFF;
long exp2 = ((long)134775813 * val1 + 1) & 0x7FFFFFFF;
sb.AppendLine($"  Seed=42: Next()={val1} (formula: {exp1})");
sb.AppendLine($"  Next()={val2} (formula: {exp2})");
sb.AppendLine($"  Next()={val3}");
sb.AppendLine($"  Formula: (134775813 * x + 1) & 0x7FFFFFFF");

rng.Dispose();
var rng2 = new BovineLabs.Core.Collections.NativeLinearCongruentialGenerator(42, Unity.Collections.Allocator.Temp);
int det1 = rng2.Next();
int det2 = rng2.Next();
sb.AppendLine($"  Deterministic: seed=42 again: {det1}, {det2}");
rng2.Dispose();

check("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(type));
check("Next() returns int", type.GetMethod("Next", BindingFlags.Public | BindingFlags.Instance) != null);
check("Deterministic", det1 == val1 && det2 == val2);
check("Value matches formula", val1 == (int)exp1);
check("Values non-negative", val1 >= 0 && val2 >= 0 && val3 >= 0);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
