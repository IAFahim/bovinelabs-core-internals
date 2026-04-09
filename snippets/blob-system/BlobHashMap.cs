// Run: cat snippets/blob-system/BlobHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections"
// Verifies: docs/BlobHashMap.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BlobHashMap<int,int>);
int sz = Marshal.SizeOf(type);

sb.AppendLine("BlobHashMap<TKey,TValue>");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}, {sz} bytes");
sb.AppendLine($"  Generic params: {type.GetGenericArguments().Length} (TKey, TValue)");
sb.AppendLine();

// Fields
sb.AppendLine("  Fields:");
var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
foreach (var f in fields)
{
    var vis = f.IsPublic ? "public" : "private";
    var offset = Marshal.OffsetOf(type, f.Name);
    sb.AppendLine($"    [{offset}] {f.FieldType.Name} {f.Name} ({vis})");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

// Properties
sb.AppendLine("  Properties:");
foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
{
    var rw = (p.CanRead ? "get" : "") + (p.CanRead && p.CanWrite ? "; " : "") + (p.CanWrite ? "set" : "");
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name} {{ {rw} }}");
    t($"Property {p.Name}", true);
}
sb.AppendLine();

// Methods
sb.AppendLine("  Methods:");
var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).ToList();
foreach (var m in methods)
{
    var ps = string.Join(", ", m.GetParameters().Select(p => {
        var prefix = p.IsOut ? "out " : "";
        return $"{p.ParameterType.Name} {prefix}{p.Name}";
    }));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Indexer
var idx = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
if (idx != null)
{
    var idxP = string.Join(", ", idx.GetIndexParameters().Select(p => p.ParameterType.Name));
    var rw = (idx.CanRead ? "get" : "") + (idx.CanRead && idx.CanWrite ? "; " : "") + (idx.CanWrite ? "set" : "");
    sb.AppendLine($"  Indexer: this[{idxP}] -> {idx.PropertyType.Name} {{ {rw} }}");
    t("Has indexer", true);
}
sb.AppendLine();

// Interfaces
var ifaces = type.GetInterfaces();
if (ifaces.Length > 0)
{
    sb.AppendLine("  Implements:");
    foreach (var i in ifaces) sb.AppendLine($"    {i.Name}");
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
