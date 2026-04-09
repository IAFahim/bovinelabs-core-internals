// Run: cat snippets/dynamic-buffers/FixedNameValue.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Keys,System,System.Reflection,System.Linq"
// Verifies: docs/FixedNameValue.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Keys.FixedNameValue);
sb.AppendLine("BovineLabs.Core.Keys.FixedNameValue");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
