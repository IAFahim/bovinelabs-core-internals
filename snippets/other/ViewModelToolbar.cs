// Run: cat snippets/other/ViewModelToolbar.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/ViewModelToolbar.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Find types by scanning loaded assemblies ---
Type findType(string name)
{
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        catch { }
    }
    // Try partial match on class name
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.Name == name) return t;
            }
        }
        catch { }
    }
    return null;
}


// --- UI ---
var v0 = findType("BovineLabs.Core.Editor.UI");
if (v0 != null)
{
    var kind = v0.IsValueType ? "struct" : (v0.IsInterface ? "interface" : (v0.IsAbstract && v0.IsSealed ? "static class" : "class"));
    sb.AppendLine("UI");
    sb.AppendLine("  Kind: " + kind);
    try { sb.AppendLine("  Size: " + System.Runtime.InteropServices.Marshal.SizeOf(v0) + " bytes"); } catch {}
    check("UI exists", true);
    sb.AppendLine("  Interfaces:");
    foreach (var i in v0.GetInterfaces())
        sb.AppendLine("    " + i.Name);
    sb.AppendLine("  Properties:");
    foreach (var p in v0.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        sb.AppendLine("    " + p.PropertyType.Name + " " + p.Name);
    sb.AppendLine("  Methods:");
    foreach (var m in v0.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
        sb.AppendLine("    " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
    sb.AppendLine("  Fields:");
    foreach (var f in v0.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        sb.AppendLine("    " + f.FieldType.Name + " " + f.Name + " (" + (f.IsPublic ? "public" : "private") + ")");
}
else
{
    sb.AppendLine("UI: TYPE NOT FOUND");
    check("UI exists", false);
}

// --- Editor ---
var v1 = findType("BovineLabs.Core.Extensions.Editor");
if (v1 != null)
{
    var kind = v1.IsValueType ? "struct" : (v1.IsInterface ? "interface" : (v1.IsAbstract && v1.IsSealed ? "static class" : "class"));
    sb.AppendLine("Editor");
    sb.AppendLine("  Kind: " + kind);
    try { sb.AppendLine("  Size: " + System.Runtime.InteropServices.Marshal.SizeOf(v1) + " bytes"); } catch {}
    check("Editor exists", true);
    sb.AppendLine("  Interfaces:");
    foreach (var i in v1.GetInterfaces())
        sb.AppendLine("    " + i.Name);
    sb.AppendLine("  Properties:");
    foreach (var p in v1.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        sb.AppendLine("    " + p.PropertyType.Name + " " + p.Name);
    sb.AppendLine("  Methods:");
    foreach (var m in v1.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
        sb.AppendLine("    " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")");
    sb.AppendLine("  Fields:");
    foreach (var f in v1.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        sb.AppendLine("    " + f.FieldType.Name + " " + f.Name + " (" + (f.IsPublic ? "public" : "private") + ")");
}
else
{
    sb.AppendLine("Editor: TYPE NOT FOUND");
    check("Editor exists", false);
}

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
