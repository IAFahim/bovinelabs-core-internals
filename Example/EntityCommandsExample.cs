// Example: EntityCommands — When to use which implementation
// Tests: Compare EntityManagerCommands vs CommandBufferCommands vs CommandBufferParallelCommands
// When to use each:
//   EntityManagerCommands - Baking, editor scripts, immediate validation needed
//   CommandBufferCommands - Deferred structural changes in system OnUpdate
//   CommandBufferParallelCommands - Parallel jobs that need structural changes
//
// Run: cat Example/EntityCommandsExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq,Unity.Entities,Unity.Collections,BovineLabs.Core.EntityCommands"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

// === Type Verification ===
var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

sb.AppendLine("=== Interface Contract ===");
var iec = typeof(IEntityCommands);
var methods = iec.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.Name).OrderBy(n => n).ToList();
sb.AppendLine("  IEntityCommands methods: " + string.Join(", ", methods));
check("Has CreateEntity", methods.Contains("CreateEntity"));
check("Has Instantiate", methods.Contains("Instantiate"));
check("Has AddComponent", methods.Contains("AddComponent"));
check("Has SetComponent", methods.Contains("SetComponent"));
check("Has AddBuffer", methods.Contains("AddBuffer"));
check("Has SetBuffer", methods.Contains("SetBuffer"));
check("Has AppendToBuffer", methods.Contains("AppendToBuffer"));
check("Has SetComponentEnabled", methods.Contains("SetComponentEnabled"));
check("Has AddSharedComponent", methods.Contains("AddSharedComponent"));
check("Has SetSharedComponent", methods.Contains("SetSharedComponent"));

// === Size Comparison ===
sb.AppendLine();
sb.AppendLine("=== Size Comparison ===");
var emcSize = System.Runtime.InteropServices.Marshal.SizeOf<EntityManagerCommands>();
var cbcSize = System.Runtime.InteropServices.Marshal.SizeOf<CommandBufferCommands>();
var cbpcSize = System.Runtime.InteropServices.Marshal.SizeOf<CommandBufferParallelCommands>();
sb.AppendLine("  EntityManagerCommands:         " + emcSize + " bytes");
sb.AppendLine("  CommandBufferCommands:          " + cbcSize + " bytes");
sb.AppendLine("  CommandBufferParallelCommands:  " + cbpcSize + " bytes");
check("EMC smallest (immediate is leanest)", emcSize < cbcSize && emcSize < cbpcSize);

// === Implementation Differences ===
sb.AppendLine();
sb.AppendLine("=== Implementation Differences ===");

// EntityManagerCommands fields
var emcFields = typeof(EntityManagerCommands).GetFields(bfAll)
    .Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  EntityManagerCommands fields: " + string.Join(", ", emcFields));
check("EMC has EntityManager", emcFields.Any(f => f.Contains("entityManager")));

// CommandBufferParallelCommands fields
var cbpcFields = typeof(CommandBufferParallelCommands).GetFields(bfAll)
    .Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  ParallelCommands fields: " + string.Join(", ", cbpcFields));
check("Parallel has sortKey", cbpcFields.Any(f => f.Contains("sortKey")));
check("Parallel has ParallelWriter", cbpcFields.Any(f => f.Contains("commandBuffer")));

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                              USE THIS");
sb.AppendLine("  ─────────────────────────────────────────────────────────");
sb.AppendLine("  Baker needs to create entity NOW       EntityManagerCommands");
sb.AppendLine("  Editor script immediate changes        EntityManagerCommands");
sb.AppendLine("  System OnUpdate structural changes     CommandBufferCommands");
sb.AppendLine("  Need changes batched for perf          CommandBufferCommands");
sb.AppendLine("  Parallel job creates entities          CommandBufferParallelCommands");
sb.AppendLine("  IJobChunk writes + creates entities    CommandBufferParallelCommands");
sb.AppendLine();
sb.AppendLine("  KEY RULES:");
sb.AppendLine("  - EntityManagerCommands = instant, no playback needed");
sb.AppendLine("  - CommandBufferCommands = recorded, playback at sync point");
sb.AppendLine("  - CommandBufferParallelCommands = recorded with sortKey, deterministic playback");
sb.AppendLine("  - All 3 implement same interface — write code once, swap at construction");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
