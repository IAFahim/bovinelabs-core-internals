// Run: cat snippets/core-collections/ArchetypeChunk_GetNativeArrayReadOnly.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/ArchetypeChunk_GetNativeArrayReadOnly.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
// Type: ArchetypeChunkExtensions, Namespace: 
var t = typeof(object); // placeholder

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
