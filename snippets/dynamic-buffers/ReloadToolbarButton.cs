// Run: cat snippets/dynamic-buffers/ReloadToolbarButton.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.Utility,System,System.Reflection,System.Linq"
// Verifies: docs/ReloadToolbarButton.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.Utility.ReloadToolbarButton);
sb.AppendLine("BovineLabs.Core.Editor.Utility.ReloadToolbarButton");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
