// ============================================================================
// TEST: DebugUtil + BakerCommands + ChangeFilterTrackingSystem + BovineLabsBootstrap
// Branches: topic/DebugUtil.SplitInt, topic/BakerCommands,
//           topic/ChangeFilterTrackingSystem, topic/BovineLabsBootstrap,
//           topic/BovineLabsBootstrap_NetCode
// Sources: BovineLabs.Core/Utility/DebugUtil.cs,
//          BovineLabs.Core/Authoring/EntityCommands/BakerCommands.cs,
//          BovineLabs.Core/Editor/ChangeFilterTracking/ChangeFilterTrackingSystem.cs,
//          BovineLabs.Core/BovineLabsBootstrap.cs
// Run: cat 16_UtilityAndBootstrap.cs | unity-cli exec --usings "BovineLabs.Core.Utility,BovineLabs.Core.Authoring.EntityCommands,BovineLabs.Core.Editor.ChangeFilterTracking,BovineLabs.Core,Unity.Entities,System.Linq"
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- DebugUtil: static class with SplitInt for Burst-safe float decomposition ---
var duType = typeof(DebugUtil);
t("DebugUtil exists", duType != null);
t("DebugUtil is static", duType.IsAbstract && duType.IsSealed);
var duMethods = duType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var duNames = duMethods.Select(m => m.Name).ToList();
t("DebugUtil has SplitInt", duNames.Contains("SplitInt"));

// Test SplitInt: decompose 3.14159 into integer=3, decimal part
DebugUtil.SplitInt(3.14159f, 5, out int integer, out int decimals);
t("SplitInt: integer part = 3", integer == 3);
t("SplitInt: decimal part > 0", decimals > 0);

DebugUtil.SplitInt(-7.5f, 2, out int negInt, out int negDec);
t("SplitInt: negative integer = -7", negInt == -7);

DebugUtil.SplitInt(0f, 3, out int zeroInt, out int zeroDec);
t("SplitInt: zero value = 0", zeroInt == 0);

// --- BakerCommands: struct with entity command methods ---
var bcType = typeof(BakerCommands);
t("BakerCommands exists", bcType != null);
t("BakerCommands is struct", bcType.IsValueType);
var bcMethods = bcType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var bcNames = bcMethods.Select(m => m.Name).Distinct().ToList();
t("BakerCommands has CreateEntity", bcNames.Contains("CreateEntity"));
t("BakerCommands has AddComponent", bcNames.Contains("AddComponent"));
t("BakerCommands has SetComponent", bcNames.Contains("SetComponent"));
t("BakerCommands has AddBuffer", bcNames.Contains("AddBuffer"));
t("BakerCommands has AppendToBuffer", bcNames.Contains("AppendToBuffer"));
t("BakerCommands has SetComponentEnabled", bcNames.Contains("SetComponentEnabled"));
t("BakerCommands has Instantiate", bcNames.Contains("Instantiate"));

// --- ChangeFilterTrackingSystem: editor system ---
var cftType = typeof(ChangeFilterTrackingSystem);
t("ChangeFilterTrackingSystem exists", cftType != null);
t("ChangeFilterTrackingSystem is struct (SystemHandle)", cftType.IsValueType);

// --- BovineLabsBootstrap: singleton with instance methods ---
var blbType = typeof(BovineLabsBootstrap);
t("BovineLabsBootstrap exists", blbType != null);
t("BovineLabsBootstrap is class", blbType.IsClass && !blbType.IsValueType);
var blbMethods = blbType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var blbNames = blbMethods.Select(m => m.Name).ToList();
t("BovineLabsBootstrap has Initialize", blbNames.Contains("Initialize"));
t("BovineLabsBootstrap has CreateGameWorld", blbNames.Contains("CreateGameWorld"));
t("BovineLabsBootstrap has DestroyGameWorld", blbNames.Contains("DestroyGameWorld"));
// Static properties
var blbStatic = blbType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);
var blbStaticNames = blbStatic.Select(m => m.Name).ToList();
t("BovineLabsBootstrap has Instance getter", blbStaticNames.Contains("get_Instance"));
t("BovineLabsBootstrap has GameWorldCreated event", blbStaticNames.Contains("add_GameWorldCreated"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
