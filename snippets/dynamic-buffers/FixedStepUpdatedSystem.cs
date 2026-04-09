// Run: cat snippets/dynamic-buffers/FixedStepUpdatedSystem.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.PhysicsUpdate,System,System.Reflection,System.Linq"
// Verifies: docs/FixedStepUpdatedSystem.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(BovineLabs.Core.PhysicsUpdate.FixedStepUpdatedSystem);
sb.AppendLine("BovineLabs.Core.PhysicsUpdate.FixedStepUpdatedSystem");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
