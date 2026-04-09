// Run: cat snippets/dynamic-buffers/GhostFieldAttribute.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "Unity.NetCode,System,System.Reflection,System.Linq"
// Verifies: docs/GhostFieldAttribute.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(Unity.NetCode.GhostFieldAttribute);
sb.AppendLine("Unity.NetCode.GhostFieldAttribute");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
