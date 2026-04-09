// Run: cat snippets/source-generators/AssemblyBuilderWindow.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.AssemblyBuilder,System,System.Reflection,System.Linq"
// Verifies: docs/AssemblyBuilderWindow.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.AssemblyBuilder.AssemblyBuilderWindow);
sb.AppendLine("BovineLabs.Core.Editor.AssemblyBuilder.AssemblyBuilderWindow");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
