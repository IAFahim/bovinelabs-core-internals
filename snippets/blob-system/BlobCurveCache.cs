// Run: cat snippets/blob-system/BlobCurveCache.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveCache.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var cacheType = typeof(BlobCurveCache);
int sz = Marshal.SizeOf(cacheType);

sb.AppendLine("BlobCurveCache");
sb.AppendLine($"  Kind: {(cacheType.IsValueType ? "struct" : "class")}, {sz} bytes");
sb.AppendLine();

sb.AppendLine("  Fields:");
foreach (var f in cacheType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
{
    sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
    t($"Field {f.Name}", true);
}
sb.AppendLine();

sb.AppendLine("  Static Fields:");
foreach (var f in cacheType.GetFields(BindingFlags.Static | BindingFlags.Public))
{
    sb.AppendLine($"    {f.FieldType.Name} {f.Name} (static)");
    t($"Static field {f.Name}", true);
}
sb.AppendLine();

// Runtime: verify Empty sentinel values
var emptyVal = (BlobCurveCache)cacheType.GetField("Empty").GetValue(null);
sb.AppendLine("  Runtime Constants:");
sb.AppendLine($"    Empty.Index = {emptyVal.Index} (expect int.MinValue = {int.MinValue})");
sb.AppendLine($"    Empty.NeighborhoodTimes = ({emptyVal.NeighborhoodTimes.x}, {emptyVal.NeighborhoodTimes.y}) (expect NaN, NaN)");
t("Empty.Index == int.MinValue", emptyVal.Index == int.MinValue);
t("Empty.NeighborhoodTimes is (NaN, NaN)", float.IsNaN(emptyVal.NeighborhoodTimes.x) && float.IsNaN(emptyVal.NeighborhoodTimes.y));

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
