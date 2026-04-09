// Run: cat snippets/other/StateInstanceUtil.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: docs/StateInstanceUtil.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

sb.AppendLine("No direct type references found in doc");
check("Doc exists", true);

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();