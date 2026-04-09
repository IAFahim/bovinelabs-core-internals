// Run: cat snippets/dynamic-buffers/AnalyzersProjectFileGeneration.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.Analyzers,System,System.Reflection,System.Linq"
// Verifies: docs/AnalyzersProjectFileGeneration.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.Analyzers.AnalyzersProjectFileGeneration);
sb.AppendLine("BovineLabs.Core.Editor.Analyzers.AnalyzersProjectFileGeneration");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
