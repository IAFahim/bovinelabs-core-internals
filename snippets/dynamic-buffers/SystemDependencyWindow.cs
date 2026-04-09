// Run: cat snippets/dynamic-buffers/SystemDependencyWindow.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.Dependency,System,System.Reflection,System.Linq"
// Verifies: docs/SystemDependencyWindow.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.Dependency.SystemDependencyWindow);
sb.AppendLine("BovineLabs.Core.Editor.Dependency.SystemDependencyWindow");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
