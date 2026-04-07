// ============================================================================
// TEST: EntityQueryExtensions + ArchetypeChunkExtensions + EntityQueryBuilderExtensions
// Branches: topic/EntityQuery_*, topic/ArchetypeChunk_*, topic/EntityQueryBuilder_*
// Sources: BovineLabs.Core/Extensions/*.cs
// Run: cat 14_Extensions.cs | unity-cli exec --usings "BovineLabs.Core.Extensions,Unity.Entities,Unity.Collections,System.Linq"
// ============================================================================
// EntityQueryExtensions: static helpers for EntityQuery (GetFirstEntity, etc).
// ArchetypeChunkExtensions: static helpers for ArchetypeChunk iteration.
// EntityQueryBuilderExtensions: fluent builder for EntityQueryBuilder (WithAllRW).
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- EntityQueryExtensions (from BovineLabs.Core.Extensions) ---
var eqType = typeof(BovineLabs.Core.Extensions.EntityQueryExtensions);
var eqMethods = eqType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var eqNames = eqMethods.Select(m => m.Name).Distinct().ToList();
t("EntityQueryExtensions has methods", eqNames.Count > 0);
t("EntityQueryExtensions has GetFirstEntity", eqNames.Contains("GetFirstEntity"));
t("EntityQueryExtensions has GetSingletonBufferNoSync", eqNames.Contains("GetSingletonBufferNoSync"));
t("EntityQueryExtensions has QueryHasSharedFilter", eqNames.Contains("QueryHasSharedFilter"));
t("EntityQueryExtensions has ReplaceSharedComponentFilter", eqNames.Contains("ReplaceSharedComponentFilter"));

// --- ArchetypeChunkExtensions ---
var acType = typeof(BovineLabs.Core.Extensions.ArchetypeChunkExtensions);
var acMethods = acType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var acNames = acMethods.Select(m => m.Name).Distinct().ToList();
t("ArchetypeChunkExtensions has methods", acNames.Count > 0);
t("ArchetypeChunkExtensions has DidChange", acNames.Contains("DidChange"));
t("ArchetypeChunkExtensions has GetDynamicBufferAccessor", acNames.Contains("GetDynamicBufferAccessor"));
t("ArchetypeChunkExtensions has GetNativeArrayReadOnly", acNames.Contains("GetNativeArrayReadOnly"));

// --- EntityQueryBuilderExtensions (disambiguate to Extensions namespace) ---
var eqbType = typeof(BovineLabs.Core.Extensions.EntityQueryBuilderExtensions);
var eqbMethods = eqbType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var eqbNames = eqbMethods.Select(m => m.Name).Distinct().ToList();
t("EntityQueryBuilderExtensions has methods", eqbNames.Count > 0);
t("EntityQueryBuilderExtensions has WithAllRW", eqbNames.Contains("WithAllRW"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
