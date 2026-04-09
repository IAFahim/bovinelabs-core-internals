// Run: cat snippets/dynamic-buffers/Check_Assume.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Assertions,System,System.Reflection,System.Linq"
// Verifies: docs/Check_Assume.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Assertions.Check);
sb.AppendLine("BovineLabs.Core.Assertions.Check");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
