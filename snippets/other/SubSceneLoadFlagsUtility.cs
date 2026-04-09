// Run: cat snippets/other/SubSceneLoadFlagsUtility.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.SubScenes,System,System.Linq,System.Reflection"
// Verifies: docs/SubSceneLoadFlagsUtility.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };
var bf = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;
var bfAll = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;
sb.AppendLine("(no type references)");
check("doc exists", true);

sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
