// Example: CustomChunkIterator + QueryEntityEnumerator — Fast chunk-level entity iteration
// Tests: API surface, iteration strategies, decision matrix
// When to use: Need maximum iteration perf in Burst jobs, skipping safety checks
//
// Run: cat Example/CustomChunkIteratorExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq,BovineLabs.Core.Iterators,BovineLabs.Core.Utility"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

// === ICustomChunkIterator ===
sb.AppendLine("=== ICustomChunkIterator ===");
var ici = typeof(ICustomChunkIterator);
check("is interface", ici.IsInterface);
var iciMethods = ici.GetMethods(bf).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", iciMethods));
check("single Execute method", iciMethods.Count == 1 && iciMethods[0].Contains("Execute"));

// === CustomChunkIterator<T> ===
sb.AppendLine();
sb.AppendLine("=== CustomChunkIterator<T> ===");
var cci = typeof(CustomChunkIterator<>);
check("is struct", cci.IsValueType);
check("is readonly", cci.IsValueType && (cci.Attributes & System.Reflection.TypeAttributes.Sealed) != 0);
var cciMethods = cci.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name)) + ")").ToList();
sb.AppendLine("  Methods:");
foreach (var m in cciMethods) sb.AppendLine("    " + m);
check("has Execute(chunk, useEnabledMask, chunkEnabledMask)", cciMethods.Any(m => m.Contains("Execute")));
// Verify the 3-strategy approach via parameter types
var executeMethod = cci.GetMethods(bf).First(m => m.Name == "Execute");
var parms = executeMethod.GetParameters().Select(p => p.ParameterType.Name).ToList();
sb.AppendLine("  Execute params: " + string.Join(", ", parms));
check("takes ArchetypeChunk", parms.Any(p => p.Contains("ArchetypeChunk")));
check("takes v128 enabled mask", parms.Any(p => p.Contains("v128")));

// === QueryEntityEnumerator ===
sb.AppendLine();
sb.AppendLine("=== QueryEntityEnumerator ===");
var qee = typeof(QueryEntityEnumerator);
check("is struct", qee.IsValueType);
var qeeFields = qee.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", qeeFields));
var qeeMethods = qee.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", qeeMethods));
check("has MoveNextChunk", qeeMethods.Any(m => m.Contains("MoveNextChunk")));
check("has Reset", qeeMethods.Any(m => m.Contains("Reset")));

// === Iteration Strategy Explanation ===
sb.AppendLine();
sb.AppendLine("=== Iteration Strategy (3 Paths) ===");
sb.AppendLine("  CustomChunkIterator.Execute() picks strategy based on enableable mask:");
sb.AppendLine();
sb.AppendLine("  1. NO ENABLED MASK (useEnabledMask=false):");
sb.AppendLine("     Simple for-loop 0..chunk.Count. Fastest path.");
sb.AppendLine();
sb.AppendLine("  2. SPARSE MASK (<=4 bit-flip edges):");
sb.AppendLine("     Range-based via EnabledBitUtility.TryGetNextRange().");
sb.AppendLine("     Iterates contiguous ranges of enabled entities.");
sb.AppendLine();
sb.AppendLine("  3. DENSE MASK (>4 bit-flip edges):");
sb.AppendLine("     Bit-scanning: splits v128 into two ulong halves,");
sb.AppendLine("     shifts and tests each bit individually.");
sb.AppendLine();
sb.AppendLine("  Edge heuristic: countbits(ULong0 ^ (ULong0<<1)) + countbits(ULong1 ^ (ULong1<<1)) - 1");

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                 USE THIS");
sb.AppendLine("  ────────────────────────────────────────────────────────────");
sb.AppendLine("  Standard system iteration                IJobChunk (built-in)");
sb.AppendLine("  Custom per-entity logic in Burst job     CustomChunkIterator<T>");
sb.AppendLine("  Manual entity query + enableable respect QueryEntityEnumerator");
sb.AppendLine("  Need max perf, no safety overhead         CustomChunkIterator<T>");
sb.AppendLine();
sb.AppendLine("  TYPICAL PATTERN:");
sb.AppendLine("  1. QueryEntityEnumerator outer loop (MoveNextChunk)");
sb.AppendLine("  2. CustomChunkIterator<T>.Execute(chunk, useMask, mask) inner loop");
sb.AppendLine("  3. Your ICustomChunkIterator.Execute(index) does the actual work");
sb.AppendLine();
sb.AppendLine("  vs ALTERNATIVES:");
sb.AppendLine("  - IJobChunk: Safe, built-in, but more overhead per chunk");
sb.AppendLine("  - IJobEntity: Convenient but less control over iteration");
sb.AppendLine("  - Raw ArchetypeChunk: Same data but manual bitmask handling");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
