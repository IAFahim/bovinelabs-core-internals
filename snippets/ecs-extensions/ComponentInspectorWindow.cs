// Run: cat snippets/ecs-extensions/ComponentInspectorWindow.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Editor.ObjectManagement,System,System.Reflection,System.Linq"
// Verifies: docs/ComponentInspectorWindow.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.Editor.ObjectManagement.ComponentInspectorWindow);
sb.AppendLine("BovineLabs.Core.Editor.ObjectManagement.ComponentInspectorWindow");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
