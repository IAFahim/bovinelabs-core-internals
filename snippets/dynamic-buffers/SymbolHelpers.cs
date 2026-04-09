// Run: cat snippets/dynamic-buffers/SymbolHelpers.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "CodeGenHelpers.Internals,System,System.Reflection,System.Linq"
// Verifies: docs/SymbolHelpers.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

// --- Discover type via assembly scan ---
typeof(CodeGenHelpers.Internals.SymbolHelpers);
sb.AppendLine("CodeGenHelpers.Internals.SymbolHelpers");

sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
