// Run: cat snippets/core-collections/BitArray256.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
var type = typeof(BovineLabs.Core.Collections.BitArray256);

sb.AppendLine("BitArray256");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : type.IsInterface ? "interface" : type.IsAbstract && type.IsSealed ? "static class" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf<BovineLabs.Core.Collections.BitArray256>()} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf<BovineLabs.Core.Collections.BitArray256>(fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead ? "get" : "")};{(prp.CanWrite ? "set" : "")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut ? "out " : "") + px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var bits = new BovineLabs.Core.Collections.BitArray256();
sb.AppendLine($"  default: CountBits={bits.CountBits()}, AllFalse={bits.AllFalse}, AllTrue={bits.AllTrue}");
sb.AppendLine($"  Capacity={bits.Capacity}");
bits[0] = true; bits[63] = true; bits[64] = true; bits[127] = true; bits[200] = true;
sb.AppendLine($"  set[0,63,64,127,200]: CountBits={bits.CountBits()}");
var orA = new BovineLabs.Core.Collections.BitArray256(); orA[0] = true;
var orB = new BovineLabs.Core.Collections.BitArray256(); orB[1] = true;
var orR = orA.BitOr(orB);
sb.AppendLine($"  BitOr: bits[0]={orR[0]}, bits[1]={orR[1]}, CountBits={orR.CountBits()}");
var andA = new BovineLabs.Core.Collections.BitArray256(); andA[0] = true; andA[1] = true;
var andB = new BovineLabs.Core.Collections.BitArray256(); andB[0] = true;
var andR = andA.BitAnd(andB);
sb.AppendLine($"  BitAnd: bits[0]={andR[0]}, bits[1]={andR[1]}, CountBits={andR.CountBits()}");
bits = new BovineLabs.Core.Collections.BitArray256(); bits[0] = true;
var notR = bits.BitNot();
sb.AppendLine($"  BitNot({{0}}): CountBits={notR.CountBits()}");

check("Size==32", Marshal.SizeOf<BovineLabs.Core.Collections.BitArray256>() == 32);
check("Capacity==256", bits.Capacity == 256);
check("default AllFalse", new BovineLabs.Core.Collections.BitArray256().AllFalse == true);
check("BitOr", orR[0] == true && orR[1] == true);
check("BitAnd", andR[0] == true && andR[1] == false);
check("BitNot", notR.CountBits() == 255);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
