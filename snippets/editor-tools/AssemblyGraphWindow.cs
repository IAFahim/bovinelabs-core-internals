// Run: cat snippets/editor-tools/AssemblyGraphWindow.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/AssemblyGraphWindow.md claims

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


// --- Dependency ---
var v0 = findType("BovineLabs.Core.Editor.Dependency");
if (v0 != null)
{
    var kind = v0.IsValueType ? "struct" : (v0.IsInterface ? "interface" : (v0.IsAbstract && v0.IsSealed ? "static class" : "class"));
    sb.AppendLine("Dependency");
    sb.AppendLine("  Kind: " + kind);
    try { sb.AppendLine("  Size: " + System.Runtime.InteropServices.Marshal.SizeOf(v0) + " bytes"); } catch {}
    check("Dependency exists", true);
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
    sb.AppendLine("Dependency: TYPE NOT FOUND");
    check("Dependency exists", false);
}

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
