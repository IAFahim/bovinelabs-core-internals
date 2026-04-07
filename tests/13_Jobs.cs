// ============================================================================
// TEST: IJobForThread + IJobChunkWorkerBeginEnd
// Branches: topic/IJobForThread, topic/IJobChunkWorkerBeginEnd
// Sources: BovineLabs.Core/Jobs/*.cs
// Run: cat 13_Jobs.cs | unity-cli exec --usings "BovineLabs.Core.Jobs,Unity.Collections,Unity.Burst,System.Linq"
// ============================================================================
// IJobForThread: Burst-compatible job that runs per-thread (not per-index).
// IJobChunkWorkerBeginEnd: chunk iteration job with OnWorkerBegin/OnWorkerEnd lifecycle.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- IJobForThread ---
var ijft = typeof(IJobForThread);
t("IJobForThread type exists", ijft != null);
t("IJobForThread is interface", ijft.IsInterface);
var ijftMethods = ijft.GetMethods();
var ijftNames = ijftMethods.Select(m => m.Name).ToList();
t("IJobForThread has Execute", ijftNames.Contains("Execute"));
t("IJobForThread has single Execute parameter", ijftMethods.First(m => m.Name == "Execute").GetParameters().Length == 1);

// --- IJobChunkWorkerBeginEnd ---
var ijcwe = typeof(IJobChunkWorkerBeginEnd);
t("IJobChunkWorkerBeginEnd type exists", ijcwe != null);
t("IJobChunkWorkerBeginEnd is interface", ijcwe.IsInterface);
var ijcweMethods = ijcwe.GetMethods();
var ijcweNames = ijcweMethods.Select(m => m.Name).ToList();
t("IJobChunkWorkerBeginEnd has OnWorkerBegin", ijcweNames.Contains("OnWorkerBegin"));
t("IJobChunkWorkerBeginEnd has OnWorkerEnd", ijcweNames.Contains("OnWorkerEnd"));
t("IJobChunkWorkerBeginEnd has Execute", ijcweNames.Contains("Execute"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
