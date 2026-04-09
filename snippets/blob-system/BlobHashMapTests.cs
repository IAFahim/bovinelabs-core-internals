// Run: cat snippets/blob-system/BlobHashMapTests.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobHashMapTests.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

sb.AppendLine("BlobHashMapTests - Dependency Verification");
sb.AppendLine();

var bhmt = typeof(BlobHashMap<int,int>);
int bhmtSz = Marshal.SizeOf(bhmt);
sb.AppendLine($"  BlobHashMap<int,int>");
sb.AppendLine($"    Kind: struct, {bhmtSz} bytes");
var fields = bhmt.GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
sb.AppendLine($"    Fields: {string.Join(", ", fields.Select(f => $"{f.FieldType.Name} {f.Name}"))}");
var methods = bhmt.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m=>!m.IsSpecialName);
sb.AppendLine($"    Methods: {string.Join(", ", methods.Select(m => $"{m.ReturnType.Name} {m.Name}"))}");
t("BlobHashMap type resolved", bhmt != null);

var bbType = typeof(Unity.Entities.BlobBuilder);
t("BlobBuilder type resolved", bbType != null);

var barType = typeof(Unity.Entities.BlobAssetReference<BlobHashMap<int,int>>);
int barSz = Marshal.SizeOf(barType);
sb.AppendLine($"  BlobAssetReference<BlobHashMap<int,int>>: {barSz} bytes");
t("BlobAssetReference resolved", barType != null);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
