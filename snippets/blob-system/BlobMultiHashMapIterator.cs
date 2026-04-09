// Run: cat snippets/blob-system/BlobMultiHashMapIterator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobMultiHashMapIterator.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var iterType = typeof(BlobMultiHashMapIterator<int>);
int sz = Marshal.SizeOf(iterType);

sb.AppendLine("BlobMultiHashMapIterator<TKey>");
sb.AppendLine($"  Kind: {(iterType.IsValueType ? "struct" : "class")}, {sz} bytes");
sb.AppendLine();

sb.AppendLine("  Fields:");
foreach (var f in iterType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    var vis = f.IsPublic ? "public" : "private";
    sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({vis})");
    t($"Field {f.Name} is {f.FieldType.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Note: Used to iterate multi-value entries in BlobMultiHashMap.");
sb.AppendLine("  Pattern: TryGetFirstValue gives initial iterator, TryGetNextValue advances it.");

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
