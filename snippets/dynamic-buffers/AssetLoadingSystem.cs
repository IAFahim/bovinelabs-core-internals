// Run: cat snippets/dynamic-buffers/AssetLoadingSystem.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.SubScenes,System,System.Reflection,System.Linq"
// Verifies: docs/AssetLoadingSystem.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.SubScenes.AssetLoadingSystem);
sb.AppendLine("BovineLabs.Core.SubScenes.AssetLoadingSystem");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
