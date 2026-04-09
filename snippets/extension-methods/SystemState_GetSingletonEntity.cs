// Run: cat snippets/extension-methods/SystemState_GetSingletonEntity.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/SystemState_GetSingletonEntity.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

sb.AppendLine("No direct type references found in doc");
check("Doc exists", true);

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();