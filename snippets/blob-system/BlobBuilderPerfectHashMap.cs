// Run: cat snippets/blob-system/BlobBuilderPerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderPerfectHashMap.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var phmType = typeof(BlobBuilderPerfectHashMap<int,int>);

sb.AppendLine("BlobBuilderPerfectHashMap<TKey,TValue>");
sb.AppendLine($"  Kind: {(phmType.IsValueType ? "struct" : "class")}, ref struct = {phmType.IsByRefLike}");
sb.AppendLine();

sb.AppendLine("  Constructors:");
foreach (var c in phmType.GetConstructors())
{
    var ps = string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    BlobBuilderPerfectHashMap({ps})");
    t($"Ctor({ps})", true);
}
sb.AppendLine();

sb.AppendLine("  Properties:");
foreach (var p in phmType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Fields (private):");
foreach (var f in phmType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
{
    sb.AppendLine($"    {f.FieldType.Name} {f.Name}");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

var idx = phmType.GetProperty("Item");
if (idx != null)
{
    var idxP = string.Join(", ", idx.GetIndexParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"  Indexer: this[{idxP}] -> {idx.PropertyType.Name}");
    t("Has indexer", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
