// Run: cat snippets/other/SettingsAuthoring.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Authoring.Settings,System,System.Linq,System.Reflection"
// Verifies: docs/SettingsAuthoring.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };
var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;
var bfAll = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;

sb.AppendLine("Settings");
try
{
    var t0 = typeof(BovineLabs.Core.Authoring.Settings);
    var kind = t0.IsValueType ? "struct" : "class";
    var sz = System.Runtime.InteropServices.Marshal.SizeOf(t0);
    sb.AppendLine("  Kind: " + kind + ", " + sz + " bytes");
    check("Settings exists", true);
    sb.AppendLine("  Properties:");
    foreach (var p in t0.GetProperties(bf))
        sb.AppendLine("    " + p.PropertyType.Name + " " + p.Name);
    sb.AppendLine("  Methods:");
    foreach (var m in t0.GetMethods(bf).Where(m => !m.IsSpecialName))
        sb.AppendLine("    " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
    sb.AppendLine("  Fields:");
    foreach (var f in t0.GetFields(bfAll))
        sb.AppendLine("    " + f.FieldType.Name + " " + f.Name + " (" + (f.IsPublic ? "public" : "private") + ")");
}
catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("Settings exists", false); }

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
