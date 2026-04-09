// Run: cat snippets/dynamic-buffers/TransformAuthoring.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Authoring,System,System.Reflection,System.Linq"
// Verifies: docs/TransformAuthoring.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Authoring.TransformAuthoring);
sb.AppendLine("BovineLabs.Core.Authoring.TransformAuthoring");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
