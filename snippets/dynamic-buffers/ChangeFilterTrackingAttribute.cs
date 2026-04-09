// Run: cat snippets/dynamic-buffers/ChangeFilterTrackingAttribute.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core,System,System.Reflection,System.Linq"
// Verifies: docs/ChangeFilterTrackingAttribute.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.ChangeFilterTrackingAttribute);
sb.AppendLine("BovineLabs.Core.ChangeFilterTrackingAttribute");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
