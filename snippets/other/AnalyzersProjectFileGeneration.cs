// Run: cat snippets/other/AnalyzersProjectFileGeneration.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.Analyzers,System,System.Reflection,System.Linq"
// Verifies: docs/AnalyzersProjectFileGeneration.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Analyzers ---
sb.AppendLine("Analyzers");
try {
  var t0 = typeof(BovineLabs.Core.Editor.Analyzers);
  sb.AppendLine($"  Kind: {(t0.IsValueType ? \"struct\" : \"class\")}");
  sb.AppendLine($"  Size: {System.Runtime.InteropServices.Marshal.SizeOf(t0)} bytes");
  check("Analyzers exists", t0 != null);
  check("Analyzers is ValueType", t0.IsValueType);
  sb.AppendLine("  Properties:");
  foreach (var p in t0.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
  sb.AppendLine("  Methods:");
  foreach (var m in t0.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).Distinct())
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");
} catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("Analyzers exists", false); }

// --- Editor ---
sb.AppendLine("Editor");
try {
  var t1 = typeof(BovineLabs.Core.Extensions.Editor);
  sb.AppendLine($"  Kind: {(t1.IsValueType ? \"struct\" : \"class\")}");
  sb.AppendLine($"  Size: {System.Runtime.InteropServices.Marshal.SizeOf(t1)} bytes");
  check("Editor exists", t1 != null);
  check("Editor is ValueType", t1.IsValueType);
  sb.AppendLine("  Properties:");
  foreach (var p in t1.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
  sb.AppendLine("  Methods:");
  foreach (var m in t1.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).Distinct())
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");
} catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("Editor exists", false); }

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();