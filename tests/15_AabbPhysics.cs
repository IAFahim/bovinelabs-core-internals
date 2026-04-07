// ============================================================================
// TEST: AabbExtensions + AlwaysUpdatePhysicsWorld
// Branches: topic/AabbExtensions, topic/AlwaysUpdatePhysicsWorld
// Sources: BovineLabs.Core/Extensions/AabbExtensions.cs,
//          BovineLabs.Core/PhysicsUpdate/AlwaysUpdatePhysicsWorld.cs
// Run: cat 15_AabbPhysics.cs | unity-cli exec --usings "BovineLabs.Core.Extensions,BovineLabs.Core.PhysicsUpdate,Unity.Mathematics,Unity.Physics,System.Linq"
// ============================================================================
// AabbExtensions: static class with Shrink/ShrinkSafe/ExpandX/Y/Z for Aabb.
// AlwaysUpdatePhysicsWorld: internal IComponentData — verify type existence only.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- AabbExtensions: static class ---
var aabbExtType = typeof(AabbExtensions);
t("AabbExtensions exists", aabbExtType != null);
t("AabbExtensions is static", aabbExtType.IsAbstract && aabbExtType.IsSealed);
var aabbMethods = aabbExtType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var aabbNames = aabbMethods.Select(m => m.Name).Distinct().ToList();
t("AabbExtensions has Shrink", aabbNames.Contains("Shrink"));
t("AabbExtensions has ShrinkSafe", aabbNames.Contains("ShrinkSafe"));
t("AabbExtensions has ExpandX", aabbNames.Contains("ExpandX"));
t("AabbExtensions has ExpandY", aabbNames.Contains("ExpandY"));
t("AabbExtensions has ExpandZ", aabbNames.Contains("ExpandZ"));

// Test ShrinkSafe on a known Aabb
var aabb = new Aabb { Min = new float3(-2, -2, -2), Max = new float3(2, 2, 2) };
AabbExtensions.ShrinkSafe(ref aabb, 0.5f);
t("ShrinkSafe reduces extents", aabb.Min.x > -2f && aabb.Max.x < 2f);

// Test ExpandX
var aabb2 = new Aabb { Min = float3.zero, Max = new float3(1, 1, 1) };
AabbExtensions.ExpandX(ref aabb2, 1f);
t("ExpandX only affects X axis", aabb2.Min.x < 0f && aabb2.Min.y == 0f && aabb2.Min.z == 0f);

// --- AlwaysUpdatePhysicsWorld: internal struct, verify by type lookup ---
var auType = System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .FirstOrDefault(xt => xt.Name == "AlwaysUpdatePhysicsWorld");
t("AlwaysUpdatePhysicsWorld exists", auType != null);
t("AlwaysUpdatePhysicsWorld is struct", auType != null && auType.IsValueType);
// Check fields via reflection (internal)
var auFields = auType?.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var auFieldNames = (auFields ?? new System.Reflection.FieldInfo[0]).Select(f => f.Name).ToList();
t("AlwaysUpdatePhysicsWorld has FixedStepUpdatedThisFrame", auFieldNames.Contains("FixedStepUpdatedThisFrame"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
