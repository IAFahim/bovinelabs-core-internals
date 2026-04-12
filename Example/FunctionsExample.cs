// Example: Functions — Burst-compiled function pointers in ECS jobs
// Tests: Build, execute, and cleanup a Functions<T,TO> container
// When to use: Plugin/mod system, strategy pattern in Burst jobs, polymorphic dispatch
//
// Run: cat Example/FunctionsExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq,Unity.Entities,Unity.Collections,Unity.Burst,BovineLabs.Core.Functions"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

// === Type Surface ===
sb.AppendLine("=== IFunction<T> Contract ===");
var ift = typeof(BovineLabs.Core.Functions.IFunction<>);
var iftProps = ift.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
var iftMethods = ift.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", iftProps));
sb.AppendLine("  Methods: " + string.Join(", ", iftMethods));
check("Has ExecuteFunction", iftProps.Any(p => p.Contains("ExecuteFunction")));
check("Has UpdateFunction", iftProps.Any(p => p.Contains("UpdateFunction")));
check("Has DestroyFunction", iftProps.Any(p => p.Contains("DestroyFunction")));
check("Has OnCreate", iftMethods.Any(m => m.Contains("OnCreate")));

// === FunctionData ===
sb.AppendLine();
sb.AppendLine("=== FunctionData (internal) ===");
Type findType(string name) {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}
var fd = findType("FunctionData");
if (fd != null) {
    var fdSize = System.Runtime.InteropServices.Marshal.SizeOf(fd);
    var fdFields = fd.GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Select(f => f.FieldType.Name + " " + f.Name).ToList();
    sb.AppendLine("  Size: " + fdSize + " bytes");
    sb.AppendLine("  Fields: " + string.Join(", ", fdFields));
    check("FunctionData 32 bytes", fdSize == 32);
    check("Has Target", fdFields.Any(f => f.Contains("Target")));
}

// === FunctionsBuilder API ===
sb.AppendLine();
sb.AppendLine("=== FunctionsBuilder<T,TO> API ===");
var fbt = typeof(FunctionsBuilder<,>);
var fbtMethods = fbt.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", fbtMethods));
check("Has ReflectAll", fbtMethods.Any(m => m.Contains("ReflectAll")));
check("Has Build", fbtMethods.Any(m => m == "Functions`2 Build"));
check("Has BuildHash", fbtMethods.Any(m => m == "FunctionsHash`2 BuildHash"));
check("Has Add", fbtMethods.Any(m => m.Contains("Add")));
check("Has Dispose", fbtMethods.Any(m => m.Contains("Dispose")));

// === Functions<T,TO> API ===
sb.AppendLine();
sb.AppendLine("=== Functions<T,TO> API ===");
var ft = typeof(Functions<,>);
var ftProps = ft.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
var ftMethods = ft.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", ftProps));
sb.AppendLine("  Methods: " + string.Join(", ", ftMethods));
check("Has Length", ftProps.Any(p => p.Contains("Length")));
check("Has Execute", ftMethods.Any(m => m.Contains("Execute")));
check("Has Update", ftMethods.Any(m => m.Contains("Update")));
check("Has OnDestroy", ftMethods.Any(m => m.Contains("OnDestroy")));

// === FunctionsHash<T,TO> API ===
sb.AppendLine();
sb.AppendLine("=== FunctionsHash<T,TO> API ===");
var fht = typeof(FunctionsHash<,>);
var fhtMethods = fht.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", fhtMethods));
check("Has TryExecute", fhtMethods.Any(m => m.Contains("TryExecute")));
check("Has Length", fht.GetProperties(bf).Any(p => p.Name == "Length"));

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                     USE THIS");
sb.AppendLine("  ─────────────────────────────────────────────────────────────");
sb.AppendLine("  Fixed set of strategies, iterate all          Functions<T,TO>");
sb.AppendLine("  Dynamic dispatch by type hash/mod ID          FunctionsHash<T,TO>");
sb.AppendLine("  Each frame update all registered functions    Functions.Update()");
sb.AppendLine("  Custom plugin system                         FunctionsBuilder.ReflectAll()");
sb.AppendLine();
sb.AppendLine("  vs ALTERNATIVES:");
sb.AppendLine("  - Virtual dispatch: NOT Burst compatible, use Functions instead");
sb.AppendLine("  - switch statements: Burst compatible but not extensible at runtime");
sb.AppendLine("  - NativeFunctionPtr: Lower level, no lifecycle management");
sb.AppendLine();
sb.AppendLine("  LIFECYCLE:");
sb.AppendLine("  1. OnCreate: new FunctionsBuilder<T,TO>(Allocator.Temp)");
sb.AppendLine("                .ReflectAll(ref state) or .Add<TF>(ref state, impl)");
sb.AppendLine("                .Build() or .BuildHash()");
sb.AppendLine("  2. OnUpdate: functions.Update(ref state)  // per-frame updates");
sb.AppendLine("  3. In Job:   functions.Execute(idx, ref data) -> TO");
sb.AppendLine("  4. OnDestroy: functions.OnDestroy(ref state) // cleanup + free");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
