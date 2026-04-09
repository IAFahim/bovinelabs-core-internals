// Run: cat snippets/other/InspectorSearch.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor,System,System.Reflection,System.Linq"
// Verifies: docs/InspectorSearch.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Editor ---
sb.AppendLine("Editor");
try {
  var t0 = typeof(BovineLabs.Core.Editor);
  sb.AppendLine($"  Kind: {(t0.IsValueType ? \"struct\" : \"class\")}");
  sb.AppendLine($"  Size: {System.Runtime.InteropServices.Marshal.SizeOf(t0)} bytes");
  check("Editor exists", t0 != null);
  check("Editor is ValueType", t0.IsValueType);
  sb.AppendLine("  Properties:");
  foreach (var p in t0.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
  sb.AppendLine("  Methods:");
  foreach (var m in t0.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).Distinct())
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");
} catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("Editor exists", false); }

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();