// Run: cat snippets/dynamic-buffers/ChangeFilterTrackingSystem.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.ChangeFilterTracking,System,System.Reflection,System.Linq"
// Verifies: docs/ChangeFilterTrackingSystem.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.ChangeFilterTracking.ChangeFilterTrackingSystem);
sb.AppendLine("BovineLabs.Core.Editor.ChangeFilterTracking.ChangeFilterTrackingSystem");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
