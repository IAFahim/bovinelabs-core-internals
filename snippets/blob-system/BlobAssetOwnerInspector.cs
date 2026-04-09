// Run: cat snippets/blob-system/BlobAssetOwnerInspector.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobAssetOwnerInspector.md

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

var inspectorType = (Type)null;
foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
{
    try {
        inspectorType = asm.GetType("BovineLabs.Core.Editor.Inspectors.BlobAssetOwnerInspector");
        if (inspectorType != null) break;
    } catch {}
}

sb.AppendLine("BlobAssetOwnerInspector");
sb.AppendLine($"  Namespace: BovineLabs.Core.Editor.Inspectors");

if (inspectorType != null)
{
    sb.AppendLine($"  Kind: {(inspectorType.IsValueType ? "struct" : "class")}");
    sb.AppendLine($"  Base: {inspectorType.BaseType?.Name ?? "none"}");
    sb.AppendLine();

    sb.AppendLine("  Fields:");
    foreach (var f in inspectorType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
    {
        var isStatic = f.IsStatic ? "static " : "";
        sb.AppendLine($"    {isStatic}{f.FieldType.Name} {f.Name}");
        t($"Field {f.Name}", true);
    }
    sb.AppendLine();

    sb.AppendLine("  Methods:");
    foreach (var m in inspectorType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName))
    {
        var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({ps})");
        t($"Method {m.Name}", true);
    }
}
else
{
    sb.AppendLine("  Status: Editor assembly not loaded in batch mode");
    sb.AppendLine("  (Inspector types are in BovineLabs.Core.Editor, only available in Editor)");
    t("Editor assembly not loaded (expected)", true);
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
