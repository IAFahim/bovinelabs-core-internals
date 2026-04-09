// Run: cat snippets/core-collections/BitArrayUtilities.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Collections.BitArrayUtilities);

sb.AppendLine("BitArrayUtilities");
sb.AppendLine($"  Kind: {(type.IsAbstract && type.IsSealed ? "static class" : type.IsValueType ? "struct" : "class")}");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
    sb.AppendLine($"  [static] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic ? "public" : "private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name + " " + px.Name))})");
sb.AppendLine();

check("Is static class", type.IsAbstract && type.IsSealed);
var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName).ToList();
check("Has methods", methods.Count > 0);
sb.AppendLine($"  Method count: {methods.Count}");

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
