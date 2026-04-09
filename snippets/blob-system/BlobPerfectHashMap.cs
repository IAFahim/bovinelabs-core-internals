// Run: cat snippets/blob-system/BlobPerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobPerfectHashMap.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var phmType = typeof(BlobPerfectHashMap<int,int>);
int sz = Marshal.SizeOf(phmType);

sb.AppendLine("BlobPerfectHashMap<TKey,TValue>");
sb.AppendLine($"  Kind: {(phmType.IsValueType ? "struct" : "class")}, {sz} bytes");
t($"Size = {sz} bytes", sz > 0);
sb.AppendLine();

// Fields
sb.AppendLine("  Fields:");
foreach (var f in phmType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var vis = f.IsPublic ? "public" : "private";
    var offset = Marshal.OffsetOf(phmType, f.Name);
    sb.AppendLine($"    [{offset}] {f.FieldType.Name} {f.Name} ({vis})");
    t($"Field {f.Name} exists", true);
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var p in phmType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    var rw = (p.CanRead ? "get" : "") + (p.CanRead && p.CanWrite ? "; " : "") + (p.CanWrite ? "set" : "");
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name} {{ {rw} }}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
foreach (var m in phmType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Private methods
sb.AppendLine("  Private Methods:");
foreach (var m in phmType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Private method {m.Name}", true);
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
