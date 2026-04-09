// Run: cat snippets/utility/SyncEnableStateUtil.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Utility,System,System.Reflection,System.Linq"
// Verifies: docs/SyncEnableStateUtil.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Utility ---
sb.AppendLine("Utility");
try {
  var t0 = typeof(BovineLabs.Core.Utility);
  sb.AppendLine($"  Kind: {(t0.IsValueType ? \"struct\" : \"class\")}");
  sb.AppendLine($"  Size: {System.Runtime.InteropServices.Marshal.SizeOf(t0)} bytes");
  check("Utility exists", t0 != null);
  check("Utility is ValueType", t0.IsValueType);
  sb.AppendLine("  Properties:");
  foreach (var p in t0.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
    sb.AppendLine($"    {p.PropertyType.Name} {p.Name}");
  sb.AppendLine("  Methods:");
  foreach (var m in t0.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).Distinct())
    sb.AppendLine($"    {m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");
} catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("Utility exists", false); }

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();