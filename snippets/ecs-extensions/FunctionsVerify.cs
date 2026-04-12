// Run: cat snippets/ecs-extensions/FunctionsVerify.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: Functions types surface

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

// === IFunction<T> ===
var ift = typeof(BovineLabs.Core.Functions.IFunction<>);
sb.AppendLine("IFunction<T>");
sb.AppendLine("  Kind: " + (ift.IsInterface ? "interface" : "struct/class"));
check("is interface", ift.IsInterface);
check("is generic", ift.IsGenericTypeDefinition);
check("1 generic param", ift.GetGenericArguments().Length == 1);
var iftProps = ift.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", iftProps));
var iftMethods = ift.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", iftMethods));
check("has ExecuteFunction prop", iftProps.Any(p => p.Contains("ExecuteFunction")));
check("has UpdateFunction prop", iftProps.Any(p => p.Contains("UpdateFunction")));
check("has DestroyFunction prop", iftProps.Any(p => p.Contains("DestroyFunction")));
check("has OnCreate method", iftMethods.Any(m => m.Contains("OnCreate")));

// === Delegates ===
sb.AppendLine();
sb.AppendLine("Delegates");
var updateFn = typeof(BovineLabs.Core.Functions.UpdateFunction);
sb.AppendLine("  UpdateFunction: " + (updateFn.IsSubclassOf(typeof(Delegate)) ? "delegate" : "not delegate"));
var destroyFn = typeof(BovineLabs.Core.Functions.DestroyFunction);
sb.AppendLine("  DestroyFunction: " + (destroyFn.IsSubclassOf(typeof(Delegate)) ? "delegate" : "not delegate"));
var execFn = typeof(BovineLabs.Core.Functions.ExecuteFunction);
sb.AppendLine("  ExecuteFunction: " + (execFn.IsSubclassOf(typeof(Delegate)) ? "delegate" : "not delegate"));
check("all delegates", updateFn.IsSubclassOf(typeof(Delegate)) && destroyFn.IsSubclassOf(typeof(Delegate)) && execFn.IsSubclassOf(typeof(Delegate)));

// === FunctionData (internal) ===
Type findType(string name) {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { var t = asm.GetType(name); if (t != null) return t; } catch { }
    }
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}

sb.AppendLine();
var fd = findType("BovineLabs.Core.Functions.FunctionData");
if (fd != null) {
    sb.AppendLine("FunctionData");
    sb.AppendLine("  Kind: " + (fd.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(fd) + " bytes");
    sb.AppendLine("  Fields:");
    foreach (var f in fd.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        sb.AppendLine("    " + f.FieldType.Name + " " + f.Name + " (" + (f.IsPublic ? "public" : "internal") + ")");
    check("FunctionData found", true);
} else {
    sb.AppendLine("FunctionData NOT FOUND");
    check("FunctionData found", false);
}

// === FunctionsBuilder<T,TO> ===
sb.AppendLine();
var fbt = typeof(BovineLabs.Core.Functions.FunctionsBuilder<,>);
sb.AppendLine("FunctionsBuilder<T,TO>");
sb.AppendLine("  Kind: " + (fbt.IsValueType ? "struct" : "class"));
sb.AppendLine("  Generic args: " + fbt.GetGenericArguments().Length);
check("is struct", fbt.IsValueType);
check("2 generic params", fbt.GetGenericArguments().Length == 2);
var fbtMethods = fbt.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", fbtMethods));
check("has ReflectAll", fbtMethods.Any(m => m.Contains("ReflectAll")));
check("has Build", fbtMethods.Any(m => m.Contains("Build")));
check("has Dispose", fbtMethods.Any(m => m.Contains("Dispose")));

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
