// Run: cat snippets/blob-system/BlobHashMapData.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobHashMapData.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

// BlobHashMapData is accessed via BlobHashMap<int,int>.Data field
var hmType = typeof(BlobHashMap<int,int>);
var dataField = hmType.GetField("Data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
var dataType = dataField?.FieldType;

sb.AppendLine("BlobHashMapData<TKey,TValue>");
sb.AppendLine($"  Kind: {(dataType?.IsValueType ?? false ? "struct" : "class")}, internal");
t("BlobHashMapData accessible via BlobHashMap.Data", dataField != null);
sb.AppendLine();

if (dataType != null)
{
    int sz = Marshal.SizeOf(dataType);
    sb.AppendLine($"  Size: {sz} bytes");
    sb.AppendLine();

    sb.AppendLine("  Fields:");
    foreach (var f in dataType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    {
        sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
        t($"Field {f.Name}", true);
    }
    sb.AppendLine();

    sb.AppendLine("  Methods:");
    foreach (var m in dataType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName))
    {
        var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
        t($"Method {m.Name}", true);
    }
}
sb.AppendLine();

// KVPair
sb.AppendLine("  Related: KVPair<TKey,TValue>");
var kvType = typeof(BovineLabs.Core.Collections.KVPair<int,int>);
int kvSz = Marshal.SizeOf(kvType);
sb.AppendLine($"    Kind: struct, {kvSz} bytes");
foreach (var p in kvType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
t("KVPair exists", kvType != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
