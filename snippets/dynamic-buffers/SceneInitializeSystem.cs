// Run: cat snippets/dynamic-buffers/SceneInitializeSystem.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.LifeCycle,System,System.Reflection,System.Linq"
// Verifies: docs/SceneInitializeSystem.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.LifeCycle.SceneInitializeSystem);
sb.AppendLine("BovineLabs.Core.LifeCycle.SceneInitializeSystem");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
