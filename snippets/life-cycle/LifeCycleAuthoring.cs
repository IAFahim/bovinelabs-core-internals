// Run: cat snippets/life-cycle/LifeCycleAuthoring.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Authoring.LifeCycle,System,System.Linq,System.Reflection,Unity.Entities"
// Verifies: docs/LifeCycleAuthoring.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };
var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;
var bfAll = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;

sb.AppendLine("LifeCycle");
try
{
    var t0 = typeof(BovineLabs.Core.Authoring.LifeCycle);
    var kind = t0.IsValueType ? "struct" : "class";
    var sz = System.Runtime.InteropServices.Marshal.SizeOf(t0);
    sb.AppendLine("  Kind: " + kind + ", " + sz + " bytes");
    check("LifeCycle exists", true);
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
catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("LifeCycle exists", false); }

sb.AppendLine("Authoring");
try
{
    var t1 = typeof(BovineLabs.Core.Extensions.Authoring);
    var kind = t1.IsValueType ? "struct" : "class";
    var sz = System.Runtime.InteropServices.Marshal.SizeOf(t1);
    sb.AppendLine("  Kind: " + kind + ", " + sz + " bytes");
    check("Authoring exists", true);
    sb.AppendLine("  Properties:");
    foreach (var p in t1.GetProperties(bf))
        sb.AppendLine("    " + p.PropertyType.Name + " " + p.Name);
    sb.AppendLine("  Methods:");
    foreach (var m in t1.GetMethods(bf).Where(m => !m.IsSpecialName))
        sb.AppendLine("    " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
    sb.AppendLine("  Fields:");
    foreach (var f in t1.GetFields(bfAll))
        sb.AppendLine("    " + f.FieldType.Name + " " + f.Name + " (" + (f.IsPublic ? "public" : "private") + ")");
}
catch (System.Exception) { sb.AppendLine("  TYPE NOT FOUND"); check("Authoring exists", false); }

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
