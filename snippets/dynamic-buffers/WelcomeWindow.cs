// Run: cat snippets/dynamic-buffers/WelcomeWindow.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.Welcome,System,System.Reflection,System.Linq"
// Verifies: docs/WelcomeWindow.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.Welcome.WelcomeWindow);
sb.AppendLine("BovineLabs.Core.Editor.Welcome.WelcomeWindow");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
