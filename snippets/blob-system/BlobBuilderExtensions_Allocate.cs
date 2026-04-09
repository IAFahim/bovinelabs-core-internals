// Run: cat snippets/blob-system/BlobBuilderExtensions_Allocate.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderExtensions_Allocate.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BlobBuilderExtensions);

sb.AppendLine("BlobBuilderExtensions.Allocate (internals)");
sb.AppendLine();

// Nested types
var nestedBlobAlloc = extType.GetNestedType("BlobBuilderInternal", BindingFlags.NonPublic);
if (nestedBlobAlloc != null)
{
    sb.AppendLine($"  BlobBuilderInternal ({(nestedBlobAlloc.IsValueType ? "struct" : "class")})");
    t("BlobBuilderInternal exists", true);

    var subTypes = nestedBlobAlloc.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public);
    foreach (var st in subTypes)
    {
        sb.AppendLine($"    {st.Name} ({(st.IsValueType ? "struct" : "class")})");
        var fields = st.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var f in fields)
            sb.AppendLine($"      {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
        t($"Nested type {st.Name}", true);
    }
}
else
{
    sb.AppendLine("  BlobBuilderInternal: NOT FOUND");
    t("BlobBuilderInternal exists", false);
}
sb.AppendLine();

// Allocate overloads
var allocOverloads = extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => m.Name == "Allocate").ToArray();
sb.AppendLine($"  Allocate overloads: {allocOverloads.Length}");
foreach (var m in allocOverloads)
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} Allocate({ps})");
}
t("Allocate has overloads", allocOverloads.Length >= 1);

// GetListPtr
var getListPtr = extType.GetMethod("GetListPtr");
if (getListPtr != null)
{
    var ps = string.Join(", ", getListPtr.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"  GetListPtr: {getListPtr.ReturnType.Name} GetListPtr({ps})");
    t("GetListPtr returns IntPtr", getListPtr.ReturnType == typeof(IntPtr));
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
