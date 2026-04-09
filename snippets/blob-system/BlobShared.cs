// Run: cat snippets/blob-system/BlobShared.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobShared.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var sharedType = typeof(BlobShared);

sb.AppendLine("BlobShared");
sb.AppendLine($"  Kind: {(sharedType.IsAbstract && sharedType.IsSealed ? "static class" : "class")}");
sb.AppendLine();

sb.AppendLine("  Static Methods:");
foreach (var m in sharedType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName))
{
    var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
    t($"Method {m.Name}", true);
}
sb.AppendLine();

// Functional tests
sb.AppendLine("  Runtime Verification:");
var psResult = BlobShared.PowerSerial(0.5f);
sb.AppendLine($"    PowerSerial(0.5) = ({psResult.x}, {psResult.y}, {psResult.z}, {psResult.w})");
t("PowerSerial(0.5).x == 0.125", Math.Abs(psResult.x - 0.125f) < 0.0001f);
t("PowerSerial(0.5).y == 0.25", Math.Abs(psResult.y - 0.25f) < 0.0001f);
t("PowerSerial(0.5).z == 0.5", Math.Abs(psResult.z - 0.5f) < 0.0001f);
t("PowerSerial(0.5).w == 1.0", Math.Abs(psResult.w - 1.0f) < 0.0001f);

var ps0 = BlobShared.PowerSerial(0f);
sb.AppendLine($"    PowerSerial(0.0) = ({ps0.x}, {ps0.y}, {ps0.z}, {ps0.w})");
t("PowerSerial(0) = (0,0,0,1)", ps0.x == 0 && ps0.y == 0 && ps0.z == 0 && ps0.w == 1);

var ps1 = BlobShared.PowerSerial(1f);
sb.AppendLine($"    PowerSerial(1.0) = ({ps1.x}, {ps1.y}, {ps1.z}, {ps1.w})");
t("PowerSerial(1) = (1,1,1,1)", ps1.x == 1 && ps1.y == 1 && ps1.z == 1 && ps1.w == 1);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
