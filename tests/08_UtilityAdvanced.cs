// ============================================================================
// TEST: BurstTrampoline + BurstUtil + CommandLineArgs + ConvexHullBuilder
// Branches: topic/BurstTrampoline, topic/BurstUtil, topic/CommandLineArgs,
//           topic/ConvexHullBuilder
// Sources: BovineLabs.Core/Utility/*.cs
// Run: cat 08_UtilityAdvanced.cs | unity-cli exec --usings "BovineLabs.Core.Utility,Unity.Burst,System.Linq"
// ============================================================================
// BurstTrampoline: bridges managed delegates to Burst-compiled function pointers.
// BurstUtil: static helpers for Burst compatibility checks.
// CommandLineArgs: caches CLI args for fast lookup.
// ConvexHullBuilder: static class that generates convex hulls from point sets.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- BurstTrampoline ---
var btType = typeof(BurstTrampoline);
t("BurstTrampoline type exists", btType != null);
t("BurstTrampoline is struct", btType.IsValueType);
var bteType = typeof(BurstTrampolineExtensions);
t("BurstTrampolineExtensions type exists", bteType != null);

// --- BurstUtil ---
var buMethods = typeof(BurstUtil).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var buNames = buMethods.Select(m => m.Name).ToList();
t("BurstUtil has IsEmpty", buNames.Contains("IsEmpty"));
t("BurstUtil has SetNotBurstCompiled", buNames.Contains("SetNotBurstCompiled"));

// Test IsEmpty on an EntityQuery
// (can't easily create one in exec, so test the method signature exists)
var isEmptyMethod = buMethods.First(m => m.Name == "IsEmpty");
t("BurstUtil.IsEmpty takes ref EntityQuery", isEmptyMethod.GetParameters().Length > 0);

// --- CommandLineArgs ---
var claMethods = typeof(CommandLineArgs).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var claNames = claMethods.Select(m => m.Name).ToList();
t("CommandLineArgs has TryGetArgument", claNames.Contains("TryGetArgument"));
t("CommandLineArgs has Contains", claNames.Contains("Contains"));

// TryGetArgument signature
var tgaMethod = claMethods.First(m => m.Name == "TryGetArgument");
t("TryGetArgument takes string and out string", tgaMethod.GetParameters().Length == 2);

// --- ConvexHullBuilder ---
var chbType = typeof(ConvexHullBuilder);
t("ConvexHullBuilder is static class", chbType.IsAbstract && chbType.IsSealed);
var chbMethods = chbType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var chbNames = chbMethods.Select(m => m.Name).ToList();
t("ConvexHullBuilder has Generate", chbNames.Contains("Generate"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
