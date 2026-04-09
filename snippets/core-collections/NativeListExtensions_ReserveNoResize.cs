// Run: cat snippets/core-collections/NativeListExtensions_ReserveNoResize.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Extensions,System,System.Reflection,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var extType = typeof(BovineLabs.Core.Extensions.NativeListExtensions);

sb.AppendLine("NativeListExtensions");
sb.AppendLine($"  Kind: {(extType.IsAbstract && extType.IsSealed ? "static class" : "class")}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in extType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name + " " + px.Name))})");
sb.AppendLine();

var methods = extType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "ReserveNoResize").ToList();
sb.AppendLine($"ReserveNoResize overloads: {methods.Count}");
foreach (var m in methods)
{
    var ps = m.GetParameters();
    sb.AppendLine($"  ({string.Join(", ", ps.Select(px => px.ParameterType.Name + " " + px.Name))}) -> {m.ReturnType.Name}");
}

check("Is static class", extType.IsAbstract && extType.IsSealed);
check("ReserveNoResize exists", methods.Count > 0);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
