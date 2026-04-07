// ============================================================================
// TEST: AppAPI + StateAPI + State types + CopyEnableable
// Branches: topic/AppAPI, topic/StateAPI, topic/StateFlagModel,
//           topic/StateModelEnableable, topic/StateModelWithHistory,
//           topic/StateInstanceUtil, topic/IState, topic/CopyEnableable
// Sources: BovineLabs.Core/States/*.cs, BovineLabs.Core/Model/CopyEnableable.cs
// Run: cat 09_States.cs | unity-cli exec --usings "BovineLabs.Core.States,BovineLabs.Core.Model,Unity.Entities,Unity.Collections,System.Linq"
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- AppAPI: static helper for main-thread ECS state access ---
var appMethods = typeof(AppAPI).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var appNames = appMethods.Select(m => m.Name).Distinct().ToList();
t("AppAPI has StateIsEnabled", appNames.Contains("StateIsEnabled"));
t("AppAPI has StateSet", appNames.Contains("StateSet"));
t("AppAPI has StateEnable", appNames.Contains("StateEnable"));
t("AppAPI has StateDisable", appNames.Contains("StateDisable"));
// Verify FixedString32Bytes parameter (not string!)
var siParams = appMethods.First(m => m.Name == "StateIsEnabled").GetParameters();
t("AppAPI.StateIsEnabled takes FixedString32Bytes", siParams.Any(p => p.ParameterType.Name.Contains("FixedString")));

// --- StateAPI: uses byte keys, not strings ---
var saMethods = typeof(StateAPI).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var saNames = saMethods.Select(m => m.Name).Distinct().ToList();
t("StateAPI has Register", saNames.Contains("Register"));

// --- IState<T> ---
var iStateType = typeof(IState<>);
t("IState<T> interface exists", iStateType != null);
t("IState<T> is interface", iStateType.IsInterface);

// --- StateFlagModel ---
t("StateFlagModel exists", typeof(StateFlagModel) != null);

// --- StateModelEnableable ---
t("StateModelEnableable exists", typeof(StateModelEnableable) != null);

// --- StateModelWithHistory ---
t("StateModelWithHistory exists", typeof(StateModelWithHistory) != null);

// --- StateInstanceUtil ---
t("StateInstanceUtil exists", typeof(StateInstanceUtil) != null);

// --- CopyEnableable<TTo,TFrom>: instance methods OnCreate/OnUpdate ---
t("CopyEnableable<,> exists", typeof(CopyEnableable<,>) != null);
var ceMethods = typeof(CopyEnableable<,>).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
var ceNames = ceMethods.Select(m => m.Name).ToList();
t("CopyEnableable has OnCreate", ceNames.Contains("OnCreate"));
t("CopyEnableable has OnUpdate", ceNames.Contains("OnUpdate"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
