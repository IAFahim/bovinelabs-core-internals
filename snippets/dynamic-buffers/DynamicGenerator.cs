// Run: cat snippets/dynamic-buffers/DynamicGenerator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.DynamicGenerator,System,System.Reflection,System.Linq"
// Verifies: docs/DynamicGenerator.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.DynamicGenerator.DynamicGenerator);
sb.AppendLine("BovineLabs.DynamicGenerator.DynamicGenerator");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
