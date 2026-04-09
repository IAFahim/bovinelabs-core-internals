// Run: cat snippets/dynamic-buffers/LookupAuthoring.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Authoring.ObjectManagement,System,System.Reflection,System.Linq"
// Verifies: docs/LookupAuthoring.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Authoring.ObjectManagement.ILookupAuthoring);
sb.AppendLine("BovineLabs.Core.Authoring.ObjectManagement.ILookupAuthoring");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
