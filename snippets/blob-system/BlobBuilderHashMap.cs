// Run: cat snippets/blob-system/BlobBuilderHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderHashMap.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var hmType = typeof(BlobBuilderHashMap<int,int>);

sb.AppendLine("BlobBuilderHashMap<TKey,TValue>");
sb.AppendLine($"  Kind: {(hmType.IsValueType ? "struct" : "class")}, ref struct = {hmType.IsByRefLike}");
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var p in hmType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Methods:");
foreach (var m in hmType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name} -> {m.ReturnType.Name}", true);
}
sb.AppendLine();

var idx = hmType.GetProperty("Item");
if (idx != null)
{
    var idxP = string.Join(", ", idx.GetIndexParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"  Indexer: this[{idxP}] -> {idx.PropertyType.Name}");
    t("Has indexer", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
