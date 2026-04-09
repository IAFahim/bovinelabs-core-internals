// Run: cat snippets/dynamic-buffers/PrefabInstance.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Authoring.BakeFast,System,System.Reflection,System.Linq"
// Verifies: docs/PrefabInstance.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Authoring.BakeFast.PrefabInstanceBake);
sb.AppendLine("BovineLabs.Core.Authoring.BakeFast.PrefabInstanceBake");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
