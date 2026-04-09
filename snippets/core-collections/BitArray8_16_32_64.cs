// Run: cat snippets/core-collections/BitArray8_16_32_64.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

string[] typeNames = { "BitArray8", "BitArray16", "BitArray32", "BitArray64" };
Type[] types = {
    typeof(BovineLabs.Core.Collections.BitArray8),
    typeof(BovineLabs.Core.Collections.BitArray16),
    typeof(BovineLabs.Core.Collections.BitArray32),
    typeof(BovineLabs.Core.Collections.BitArray64)
};

for (int ti = 0; ti < types.Length; ti++)
{
    var tp = types[ti];
    sb.AppendLine(typeNames[ti]);
    sb.AppendLine($"  Kind: {(tp.IsValueType ? "struct" : "class")}");
    sb.AppendLine($"  Size: {Marshal.SizeOf(tp)} bytes");
    sb.AppendLine("Fields:");
    foreach (var fld in tp.GetFields(bf))
        sb.AppendLine($"  [{Marshal.OffsetOf(tp, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
    sb.AppendLine("Properties:");
    foreach (var prp in tp.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
    sb.AppendLine("Methods:");
    foreach (var mth in tp.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
        sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");
    sb.AppendLine();
}

// Runtime: BitArray8
sb.AppendLine("Runtime Behavior - BitArray8:");
var b8 = new BovineLabs.Core.Collections.BitArray8();
sb.AppendLine($"  Capacity={b8.Capacity}, CountBits={b8.CountBits()}, AllFalse={b8.AllFalse}");
b8[0] = true;
sb.AppendLine($"  set[0]=true: bits[0]={b8[0]}, CountBits={b8.CountBits()}");
check("BitArray8 size==1", Marshal.SizeOf(types[0]) == 1);
check("BitArray8 Cap==8", b8.Capacity == 8);

// Runtime: BitArray16
sb.AppendLine("Runtime Behavior - BitArray16:");
var b16 = new BovineLabs.Core.Collections.BitArray16();
sb.AppendLine($"  Capacity={b16.Capacity}, CountBits={b16.CountBits()}, AllFalse={b16.AllFalse}");
b16[0] = true;
sb.AppendLine($"  set[0]=true: bits[0]={b16[0]}, CountBits={b16.CountBits()}");
check("BitArray16 size==2", Marshal.SizeOf(types[1]) == 2);
check("BitArray16 Cap==16", b16.Capacity == 16);

// Runtime: BitArray32
sb.AppendLine("Runtime Behavior - BitArray32:");
var b32 = new BovineLabs.Core.Collections.BitArray32();
sb.AppendLine($"  Capacity={b32.Capacity}, CountBits={b32.CountBits()}, AllFalse={b32.AllFalse}");
b32[0] = true;
sb.AppendLine($"  set[0]=true: bits[0]={b32[0]}, CountBits={b32.CountBits()}");
check("BitArray32 size==4", Marshal.SizeOf(types[2]) == 4);
check("BitArray32 Cap==32", b32.Capacity == 32);

// Runtime: BitArray64
sb.AppendLine("Runtime Behavior - BitArray64:");
var b64 = new BovineLabs.Core.Collections.BitArray64();
sb.AppendLine($"  Capacity={b64.Capacity}, CountBits={b64.CountBits()}, AllFalse={b64.AllFalse}");
b64[0] = true;
sb.AppendLine($"  set[0]=true: bits[0]={b64[0]}, CountBits={b64.CountBits()}");
check("BitArray64 size==8", Marshal.SizeOf(types[3]) == 8);
check("BitArray64 Cap==64", b64.Capacity == 64);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
