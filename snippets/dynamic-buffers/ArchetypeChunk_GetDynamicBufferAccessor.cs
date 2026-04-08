// Run: cat snippets/dynamic-buffers/ArchetypeChunk_GetDynamicBufferAccessor.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/ArchetypeChunk_GetDynamicBufferAccessor.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- ArchetypeChunkExtensions type ---
var extType = typeof(BovineLabs.Core.Extensions.ArchetypeChunkExtensions);
t("ArchetypeChunkExtensions: type exists", extType != null);
t("ArchetypeChunkExtensions: is static class", extType.IsAbstract && extType.IsSealed);

// --- GetDynamicBufferAccessor method ---
var getDBAMethod = extType.GetMethods(bf).Where(m => m.Name == "GetDynamicBufferAccessor").FirstOrDefault();
t("ArchetypeChunkExtensions: has GetDynamicBufferAccessor", getDBAMethod != null);

if (getDBAMethod != null)
{
    var ps = getDBAMethod.GetParameters();
    t("GetDynamicBufferAccessor: param 0 is ArchetypeChunk",
        ps.Length > 0 && ps[0].ParameterType == typeof(ArchetypeChunk));
    t("GetDynamicBufferAccessor: param 1 is DynamicComponentTypeHandle (by ref)",
        ps.Length > 1 && ps[1].ParameterType == typeof(DynamicComponentTypeHandle).MakeByRefType());

    t("GetDynamicBufferAccessor: returns DynamicBufferAccessor",
        getDBAMethod.ReturnType.Name == "DynamicBufferAccessor");
}
else
{
    t("GetDynamicBufferAccessor: param 0 is ArchetypeChunk", false);
    t("GetDynamicBufferAccessor: param 1 is DynamicComponentTypeHandle (by ref)", false);
    t("GetDynamicBufferAccessor: returns DynamicBufferAccessor", false);
}

// --- DynamicBufferAccessor struct (from BovineLabs.Core.Collections) ---
var dbaType = typeof(BovineLabs.Core.Collections.DynamicBufferAccessor);
t("DynamicBufferAccessor: type exists", dbaType != null);
if (dbaType != null)
    t("DynamicBufferAccessor: is ValueType", dbaType.IsValueType);
else
    t("DynamicBufferAccessor: is ValueType", false);

// --- UnsafeUntypedDynamicBufferAccessor struct ---
var uubaType = typeof(UnsafeUntypedDynamicBufferAccessor);
t("UnsafeUntypedDynamicBufferAccessor: type exists", uubaType != null);
t("UnsafeUntypedDynamicBufferAccessor: is ValueType", uubaType.IsValueType);

var lengthProp = uubaType.GetProperty("Length");
t("UnsafeUntypedDynamicBufferAccessor: has Length property", lengthProp != null);

var elemSizeProp = uubaType.GetProperty("ElementSize");
t("UnsafeUntypedDynamicBufferAccessor: has ElementSize property", elemSizeProp != null);

var getUBMethod = uubaType.GetMethods(bf).Where(m => m.Name == "GetUntypedBuffer").FirstOrDefault();
t("UnsafeUntypedDynamicBufferAccessor: has GetUntypedBuffer(int)", getUBMethod != null);
if (getUBMethod != null)
    t("GetUntypedBuffer: returns UnsafeUntypedDynamicBuffer", getUBMethod.ReturnType == typeof(UnsafeUntypedDynamicBuffer));
else
    t("GetUntypedBuffer: returns UnsafeUntypedDynamicBuffer", false);

var sla = uubaType.StructLayoutAttribute;
t("UnsafeUntypedDynamicBufferAccessor: has StructLayout attribute", sla != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
