// Run: cat snippets/core-collections/BitArray128.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
var type = typeof(BovineLabs.Core.Collections.BitArray128);

sb.AppendLine("BitArray128");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : type.IsInterface ? "interface" : type.IsAbstract && type.IsSealed ? "static class" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf<BovineLabs.Core.Collections.BitArray128>()} bytes");
sb.AppendLine();

// Fields with offsets
sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf<BovineLabs.Core.Collections.BitArray128>(fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine();

// Properties
sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead ? "get" : "")};{(prp.CanWrite ? "set" : "")} }}");
sb.AppendLine();

// Methods
sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut ? "out " : "") + px.ParameterType.Name))})");
sb.AppendLine();

// Runtime behavior
sb.AppendLine("Runtime Behavior:");
var bits = new BovineLabs.Core.Collections.BitArray128();
sb.AppendLine($"  default: CountBits={bits.CountBits()}, AllFalse={bits.AllFalse}, AllTrue={bits.AllTrue}");
sb.AppendLine($"  Capacity={bits.Capacity}");
bits[0] = true;
sb.AppendLine($"  set[0]=true: bits[0]={bits[0]}, CountBits={bits.CountBits()}");
bits[63] = true; bits[64] = true; bits[127] = true;
sb.AppendLine($"  set[63,64,127]=true: CountBits={bits.CountBits()}");
var orR = new BovineLabs.Core.Collections.BitArray128(); orR[0] = true;
var orB = new BovineLabs.Core.Collections.BitArray128(); orB[1] = true;
var orResult = orR.BitOr(orB);
sb.AppendLine($"  BitOr({{0}},{{1}}): bits[0]={orResult[0]}, bits[1]={orResult[1]}, CountBits={orResult.CountBits()}");
var andA = new BovineLabs.Core.Collections.BitArray128(); andA[0] = true; andA[1] = true;
var andB = new BovineLabs.Core.Collections.BitArray128(); andB[0] = true;
var andResult = andA.BitAnd(andB);
sb.AppendLine($"  BitAnd({{0,1}},{{0}}): bits[0]={andResult[0]}, bits[1]={andResult[1]}, CountBits={andResult.CountBits()}");
bits = new BovineLabs.Core.Collections.BitArray128(); bits[0] = true;
var notResult = bits.BitNot();
sb.AppendLine($"  BitNot({{0}}): bits[0]={notResult[0]}, CountBits={notResult.CountBits()}");

check("Size==16", Marshal.SizeOf<BovineLabs.Core.Collections.BitArray128>() == 16);
check("Capacity==128", bits.Capacity == 128);
check("default AllFalse", new BovineLabs.Core.Collections.BitArray128().AllFalse == true);
check("BitOr", orResult[0] == true && orResult[1] == true);
check("BitAnd", andResult[0] == true && andResult[1] == false);
check("BitNot", notResult[0] == false && notResult.CountBits() == 127);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
