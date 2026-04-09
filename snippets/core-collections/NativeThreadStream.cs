// Run: cat snippets/core-collections/NativeThreadStream.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var asm = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "BovineLabs.Core");

// UnsafeThreadStream
var utsType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStream");
sb.AppendLine("UnsafeThreadStream");
sb.AppendLine($"  Kind: {(utsType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(utsType)} bytes");
sb.AppendLine("Fields:");
foreach (var fld in utsType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    sb.AppendLine($"  [{Marshal.OffsetOf(utsType, fld.Name)}] {fld.FieldType.Name} {fld.Name}");
sb.AppendLine();

// UnsafeThreadStreamBlockData
var bdataType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStreamBlockData");
sb.AppendLine("UnsafeThreadStreamBlockData");
sb.AppendLine($"  Kind: {(bdataType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(bdataType)} bytes");
sb.AppendLine();

// UnsafeThreadStreamRange
var rangeType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStreamRange");
sb.AppendLine("UnsafeThreadStreamRange");
sb.AppendLine($"  Kind: {(rangeType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(rangeType)} bytes");
sb.AppendLine("Fields with offsets:");
foreach (var fld in rangeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    sb.AppendLine($"  [{Marshal.OffsetOf(rangeType, fld.Name)}] {fld.FieldType.Name} {fld.Name}");
sb.AppendLine();

// UnsafeThreadStreamBlock
var blockType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStreamBlock");
sb.AppendLine("UnsafeThreadStreamBlock");
sb.AppendLine($"  Size: {Marshal.SizeOf(blockType)} bytes");
sb.AppendLine();

// NativeThreadStream
var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);
sb.AppendLine("NativeThreadStream");
sb.AppendLine($"  Kind: {(ntsType.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(ntsType)} bytes");
sb.AppendLine("Properties:");
foreach (var prp in ntsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name}");
sb.AppendLine("Methods:");
foreach (var mth in ntsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => px.ParameterType.Name))})");

check("UnsafeThreadStream size==16", Marshal.SizeOf(utsType) == 16);
check("UnsafeThreadStreamBlockData size==24", Marshal.SizeOf(bdataType) == 24);
check("UnsafeThreadStreamRange size==48", Marshal.SizeOf(rangeType) == 48);
check("UnsafeThreadStreamBlock size==16", Marshal.SizeOf(blockType) == 16);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
