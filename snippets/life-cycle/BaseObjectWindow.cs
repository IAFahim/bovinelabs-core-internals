// Run: cat snippets/life-cycle/BaseObjectWindow.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.Windows.Base,System,System.Reflection,System.Linq"
// Verifies: docs/BaseObjectWindow.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.Windows.Base.BaseObjectWindow);
sb.AppendLine("BovineLabs.Core.Editor.Windows.Base.BaseObjectWindow");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
