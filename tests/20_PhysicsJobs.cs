// ============================================================================
// TEST: CalculateEventMapBucketsJob + CollectEventsJob
// Branches: topic/CalculateEventMapBucketsJob, topic/CollectEventsJob
// Sources: BovineLabs.Core/PhysicsStates/*.cs
// Run: cat 20_PhysicsJobs.cs | unity-cli exec --usings "BovineLabs.Core.PhysicsStates,Unity.Collections,Unity.Entities,Unity.Jobs,System.Linq"
// ============================================================================
// CalculateEventMapBucketsJob<T,TC>: IJob that calls RecalculateBuckets on NativeMultiHashMap.
// CollectEventsJob<T,TC,TI>: IJobParallelForDeferBatch that collects physics events.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- CalculateEventMapBucketsJob<T,TC>: IJob ---
// Can't instantiate generic with concrete types easily, so verify via reflection
var cembType = System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .FirstOrDefault(xt => xt.Name.StartsWith("CalculateEventMapBucketsJob"));
t("CalculateEventMapBucketsJob exists", cembType != null);
t("CalculateEventMapBucketsJob is struct", cembType != null && cembType.IsValueType);
// Verify it implements IJob
var cembIfaces = cembType?.GetInterfaces() ?? System.Type.EmptyTypes;
t("CalculateEventMapBucketsJob implements IJob", cembIfaces.Any(i => i.Name == "IJob"));
// Check fields
var cembFields = cembType?.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ?? new System.Reflection.FieldInfo[0];
var cembFieldNames = cembFields.Select(f => f.Name).ToList();
t("CalculateEventMapBucketsJob has CurrentEventMap", cembFieldNames.Contains("CurrentEventMap"));
// Check Execute method
var cembMethods = cembType?.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly) ?? new System.Reflection.MethodInfo[0];
t("CalculateEventMapBucketsJob has Execute", cembMethods.Any(m => m.Name == "Execute"));

// --- CollectEventsJob<T,TC,TI>: IJobParallelForDeferBatch ---
var cejType = System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .FirstOrDefault(xt => xt.Name.StartsWith("CollectEventsJob`"));
t("CollectEventsJob exists", cejType != null);
t("CollectEventsJob is struct", cejType != null && cejType.IsValueType);
var cejIfaces = cejType?.GetInterfaces() ?? System.Type.EmptyTypes;
t("CollectEventsJob implements IJobParallelForDeferBatch", cejIfaces.Any(i => i.Name == "IJobParallelForDeferBatch"));
var cejFields = cejType?.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ?? new System.Reflection.FieldInfo[0];
var cejFieldNames = cejFields.Select(f => f.Name).ToList();
t("CollectEventsJob has CurrentEventMap", cejFieldNames.Contains("CurrentEventMap"));
t("CollectEventsJob has CurrentEvents", cejFieldNames.Contains("CurrentEvents"));
t("CollectEventsJob has EventsPerRead", cejFieldNames.Contains("EventsPerRead"));
var cejMethods = cejType?.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly) ?? new System.Reflection.MethodInfo[0];
t("CollectEventsJob has Execute", cejMethods.Any(m => m.Name == "Execute"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
