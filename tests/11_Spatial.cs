// ============================================================================
// TEST: SpatialMap + SpatialMap3 + SpatialKeyedMap + PositionBuilder
// Branches: topic/SpatialMap, topic/SpatialMap3, topic/SpatialKeyedMap,
//           topic/PositionBuilder
// Sources: BovineLabs.Core/Spatial/*.cs
// Run: cat 11_Spatial.cs | unity-cli exec --usings "BovineLabs.Core.Spatial,Unity.Mathematics,Unity.Collections,System.Linq"
// ============================================================================
// SpatialMap: static class for 2D spatial operations (not instantiable).
// SpatialMap3: static class for 3D spatial operations.
// SpatialKeyedMap: class for spatial key-value lookup.
// PositionBuilder: has Gather() method for building position data.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- SpatialMap: static class ---
var smType = typeof(SpatialMap);
t("SpatialMap type exists", smType != null);
t("SpatialMap is static class", smType.IsAbstract && smType.IsSealed);
var smMethods = smType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var smNames = smMethods.Select(m => m.Name).ToList();
t("SpatialMap has static methods", smNames.Count > 0);

// --- SpatialMap3: static class ---
var sm3Type = typeof(SpatialMap3);
t("SpatialMap3 type exists", sm3Type != null);
t("SpatialMap3 is static class", sm3Type.IsAbstract && sm3Type.IsSealed);
var sm3Methods = sm3Type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var sm3Names = sm3Methods.Select(m => m.Name).ToList();
t("SpatialMap3 has static methods", sm3Names.Count > 0);

// --- SpatialKeyedMap: static class ---
var skmType = typeof(SpatialKeyedMap);
t("SpatialKeyedMap type exists", skmType != null);
t("SpatialKeyedMap is static class", skmType.IsAbstract && skmType.IsSealed);

// --- PositionBuilder ---
var pbType = typeof(PositionBuilder);
t("PositionBuilder type exists", pbType != null);
var pbMethods = pbType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
var pbNames = pbMethods.Select(m => m.Name).ToList();
t("PositionBuilder has Gather", pbNames.Contains("Gather"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
