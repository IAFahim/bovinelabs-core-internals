// Run: cat snippets/dynamic-buffers/CopyEnableable.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/CopyEnableable.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
// Type: CopyEnableable, Namespace: 
var t = typeof(object); // placeholder

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
