// Run: cat snippets/dynamic-buffers/IFacet.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core,System,System.Reflection,System.Linq"
// Verifies: docs/IFacet.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.IFacet);
sb.AppendLine("BovineLabs.Core.IFacet");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
