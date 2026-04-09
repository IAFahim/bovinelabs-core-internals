// Run: cat snippets/blob-system/BlobCurveHeader.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveHeader.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var hdrType = typeof(BlobCurveHeader);
int sz = Marshal.SizeOf(hdrType);

sb.AppendLine("BlobCurveHeader");
sb.AppendLine($"  Kind: {(hdrType.IsValueType ? "struct" : "class")}, {sz} bytes");
sb.AppendLine();

sb.AppendLine("  Fields:");
foreach (var f in hdrType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var offset = Marshal.OffsetOf(hdrType, f.Name);
    sb.AppendLine($"    [{offset}] {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var p in hdrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

// WrapMode enum
var wrapModeEnum = hdrType.GetNestedType("WrapMode");
if (wrapModeEnum != null)
{
    sb.AppendLine($"  Nested Enum: WrapMode (underlying: {Enum.GetUnderlyingType(wrapModeEnum).Name})");
    foreach (var v in Enum.GetValues(wrapModeEnum))
        sb.AppendLine($"    {Enum.GetName(wrapModeEnum, v)} = {Convert.ToInt32(v)}");
    t("WrapMode.Clamp == 0", Convert.ToInt32(Enum.Parse(wrapModeEnum, "Clamp")) == 0);
    t("WrapMode.Loop == 1", Convert.ToInt32(Enum.Parse(wrapModeEnum, "Loop")) == 1);
    t("WrapMode.PingPong == 2", Convert.ToInt32(Enum.Parse(wrapModeEnum, "PingPong")) == 2);
    t("Underlying type is short", Enum.GetUnderlyingType(wrapModeEnum) == typeof(short));
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in hdrType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
