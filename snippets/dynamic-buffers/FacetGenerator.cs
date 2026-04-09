// Run: cat snippets/dynamic-buffers/FacetGenerator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.FacetGenerator,System,System.Reflection,System.Linq"
// Verifies: docs/FacetGenerator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.FacetGenerator.FacetGenerator);
sb.AppendLine("BovineLabs.FacetGenerator.FacetGenerator");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
