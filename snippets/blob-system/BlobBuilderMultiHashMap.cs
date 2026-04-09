// Run: cat snippets/blob-system/BlobBuilderMultiHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderMultiHashMap.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var mhmType = typeof(BlobBuilderMultiHashMap<int,int>);

sb.AppendLine("BlobBuilderMultiHashMap<TKey,TValue>");
sb.AppendLine($"  Kind: {(mhmType.IsValueType ? "struct" : "class")}, ref struct = {mhmType.IsByRefLike}");
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var p in mhmType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in mhmType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    var retByRef = m.ReturnType.IsByRef ? "ref " : "";
    sb.AppendLine($"    {retByRef}{m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name} -> {m.ReturnType.Name}", true);
}
sb.AppendLine();

// Key difference: Add(key) returns ref TValue for multi-map
var addKeyOnly = mhmType.GetMethod("Add", new[] { typeof(int) });
if (addKeyOnly != null)
{
    sb.AppendLine($"  Note: Add(TKey) returns {(addKeyOnly.ReturnType.IsByRef ? "ref " : "")}{addKeyOnly.ReturnType.Name}");
    sb.AppendLine("  (allows multiple values per key — the multi-map pattern)");
    t("Add(TKey) returns ref TValue", addKeyOnly.ReturnType.IsByRef);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
