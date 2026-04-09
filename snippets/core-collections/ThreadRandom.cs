// Run: cat snippets/core-collections/ThreadRandom.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,Unity.Jobs.LowLevel.Unsafe,Unity.Mathematics"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.ThreadRandom);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("ThreadRandom");
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

// Randoms nested type
var randomsType = type.GetNestedType("Randoms", BindingFlags.NonPublic | BindingFlags.Public);
sb.AppendLine("Nested: Randoms");
if (randomsType != null)
{
    sb.AppendLine($"  Kind: {(randomsType.IsValueType ? "struct" : "class")}");
    sb.AppendLine($"  Size: {Marshal.SizeOf(randomsType)} bytes");
    sb.AppendLine($"  CacheLineSize={Unity.Jobs.LowLevel.Unsafe.JobsUtility.CacheLineSize}");
    foreach (var fld in randomsType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        sb.AppendLine($"  {fld.FieldType.Name} {fld.Name}");
}
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var tr = new BovineLabs.Core.Collections.ThreadRandom(42, Unity.Collections.Allocator.Temp);
sb.AppendLine($"  IsCreated={tr.IsCreated}");
ref var rng = ref tr.GetRandomRef();
int v1 = rng.NextInt();
int v2 = rng.NextInt();
float f1 = rng.NextFloat();
sb.AppendLine($"  NextInt()={v1}, NextInt()={v2}");
sb.AppendLine($"  NextFloat()={f1}");
tr.Dispose();

// Determinism
var tr2 = new BovineLabs.Core.Collections.ThreadRandom(42, Unity.Collections.Allocator.Temp);
ref var rng2 = ref tr2.GetRandomRef();
int d1 = rng2.NextInt();
int d2 = rng2.NextInt();
sb.AppendLine($"  Same seed again: {d1}, {d2} (deterministic={v1==d1 && v2==d2})");
tr2.Dispose();

check("Is struct", type.IsValueType);
check("IsCreated", tr.IsCreated);
check("Has GetRandomRef", type.GetMethod("GetRandomRef") != null);
check("Deterministic", v1 == d1 && v2 == d2);
check("NextFloat in range", f1 >= 0f && f1 < 1f);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
