// Run: cat snippets/dynamic-buffers/ArchetypeChunk_GetDynamicBufferAccessor.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/ArchetypeChunk_GetDynamicBufferAccessor.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- ArchetypeChunkExtensions ---
var extType = typeof(BovineLabs.Core.Extensions.ArchetypeChunkExtensions);
t("ArchetypeChunkExtensions type exists", extType != null);

sb.AppendLine("ArchetypeChunkExtensions");
sb.AppendLine("  Kind: static class");
sb.AppendLine();

var getDBAMethod = extType.GetMethods(bf).Where(m => m.Name == "GetDynamicBufferAccessor").FirstOrDefault();
t("GetDynamicBufferAccessor method exists", getDBAMethod != null);

sb.AppendLine("Methods:");
if (getDBAMethod != null)
{
    var ps = getDBAMethod.GetParameters();
    t("Param 0 is ArchetypeChunk", ps.Length > 0 && ps[0].ParameterType == typeof(ArchetypeChunk));
    t("Param 1 is ref DynamicComponentTypeHandle", ps.Length > 1 && ps[1].ParameterType == typeof(DynamicComponentTypeHandle).MakeByRefType());
    t("Returns DynamicBufferAccessor", getDBAMethod.ReturnType.Name == "DynamicBufferAccessor");

    sb.AppendLine($"  GetDynamicBufferAccessor(this ArchetypeChunk, ref DynamicComponentTypeHandle) -> {getDBAMethod.ReturnType.Name}");
    sb.AppendLine($"    Param0: {ps[0].ParameterType.Name}");
    sb.AppendLine($"    Param1: {ps[1].ParameterType.Name} (by-ref={ps[1].ParameterType.IsByRef})");
}
else
{
    t("GetDynamicBufferAccessor method exists", false);
}
sb.AppendLine();

// --- DynamicBufferAccessor struct ---
var dbaType = typeof(BovineLabs.Core.Collections.DynamicBufferAccessor);
t("DynamicBufferAccessor type exists", dbaType != null);
if (dbaType != null)
{
    t("Is ValueType", dbaType.IsValueType);
    int dbaSize = System.Runtime.InteropServices.Marshal.SizeOf(dbaType);
    sb.AppendLine("DynamicBufferAccessor");
    sb.AppendLine($"  Kind: struct");
    sb.AppendLine($"  Size: {dbaSize} bytes");

    sb.AppendLine("  Fields:");
    foreach (var fld in dbaType.GetFields(bf))
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(dbaType, fld.Name).ToInt32();
        sb.AppendLine($"    [{offset}] {fld.FieldType.Name} {fld.Name}");
    }

    sb.AppendLine("  Properties:");
    foreach (var prop in dbaType.GetProperties(bf))
    {
        sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name} (canRead={prop.CanRead}, canWrite={prop.CanWrite})");
    }

    sb.AppendLine("  Methods:");
    foreach (var meth in dbaType.GetMethods(bf).Where(m => !m.IsSpecialName))
    {
        var paramStr = string.Join(", ", meth.GetParameters().Select(p => p.ParameterType.Name));
        sb.AppendLine($"    {meth.ReturnType.Name} {meth.Name}({paramStr})");
    }
}
sb.AppendLine();

// --- UnsafeUntypedDynamicBufferAccessor struct ---
var uubaType = typeof(UnsafeUntypedDynamicBufferAccessor);
t("UnsafeUntypedDynamicBufferAccessor type exists", uubaType != null);
if (uubaType != null)
{
    t("Is ValueType", uubaType.IsValueType);
    int uubaSize = System.Runtime.InteropServices.Marshal.SizeOf(uubaType);
    sb.AppendLine("UnsafeUntypedDynamicBufferAccessor");
    sb.AppendLine($"  Kind: struct");
    sb.AppendLine($"  Size: {uubaSize} bytes");

    sb.AppendLine("  Fields:");
    foreach (var fld in uubaType.GetFields(bf))
    {
        int offset = System.Runtime.InteropServices.Marshal.OffsetOf(uubaType, fld.Name).ToInt32();
        sb.AppendLine($"    [{offset}] {fld.FieldType.Name} {fld.Name}");
    }

    sb.AppendLine("  Properties:");
    foreach (var prop in uubaType.GetProperties(bf))
    {
        sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name}");
    }

    sb.AppendLine("  Methods:");
    foreach (var meth in uubaType.GetMethods(bf).Where(m => !m.IsSpecialName))
    {
        var paramStr = string.Join(", ", meth.GetParameters().Select(p => p.ParameterType.Name));
        sb.AppendLine($"    {meth.ReturnType.Name} {meth.Name}({paramStr})");
    }
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
