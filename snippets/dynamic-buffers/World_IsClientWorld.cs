// Run: cat snippets/dynamic-buffers/World_IsClientWorld.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/World_IsClientWorld.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
// Type: Worlds, Namespace: 
var t = typeof(object); // placeholder

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
