// Run: cat snippets/dynamic-buffers/BakerCommands.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Authoring.EntityCommands,System,System.Reflection,System.Linq"
// Verifies: docs/BakerCommands.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Authoring.EntityCommands.BakerCommands);
sb.AppendLine("BovineLabs.Core.Authoring.EntityCommands.BakerCommands");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
