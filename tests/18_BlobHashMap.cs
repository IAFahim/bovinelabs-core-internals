// ============================================================================
// TEST: BlobBuilderExtensions + BlobHashMap + BlobHashMapData + BlobPerfectHashMap
//       + BlobMultiHashMap + BlobAllocation
// Branches: topic/BlobBuilderExtensions, topic/BlobHashMap, topic/BlobHashMapData,
//           topic/BlobPerfectHashMap, topic/BlobBuilderExtensions_Allocate,
//           topic/BlobBuilderExtensions_ConstructHashMap, topic/BlobBuilderHashMap
// Sources: BovineLabs.Core/Collections/BlobBuilderExtensions.cs, BlobHashMap.cs,
//          BlobHashMapData.cs, BlobPerfectHashMap.cs, BlobMultiHashMap.cs
// Run: cat 18_BlobHashMap.cs | unity-cli exec --usings "BovineLabs.Core.Collections,Unity.Collections,Unity.Entities,System.Linq"
// ============================================================================
// BlobBuilderExtensions: static class with extension methods on BlobBuilder.
// BlobHashMap<K,V>: read-only hash map inside BlobAssetReference.
// BlobHashMapData<K,V>: shared internal storage engine (Keys/Values/Next/Buckets).
// BlobPerfectHashMap<K,V>: zero-collision variant.
// BlobMultiHashMap<K,V>: multi-value hash map variant.
// BlobAllocation: helper struct returned by allocation methods.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- BlobBuilderExtensions: static class ---
var bbeType = typeof(BlobBuilderExtensions);
t("BlobBuilderExtensions exists", bbeType != null);
t("BlobBuilderExtensions is static", bbeType.IsAbstract && bbeType.IsSealed);
var bbeMethods = bbeType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var bbeNames = bbeMethods.Select(m => m.Name).Distinct().ToList();
t("BlobBuilderExtensions has Allocate methods", bbeNames.Contains("Allocate") || bbeMethods.Length > 5);
// Verify it extends BlobBuilder
var bbeParams = bbeMethods.Where(m => m.GetParameters().Length > 0)
    .Select(m => m.GetParameters()[0].ParameterType.Name).Distinct().ToList();
t("BlobBuilderExtensions takes BlobBuilder param", bbeParams.Contains("BlobBuilder"));

// --- BlobHashMap<K,V> ---
t("BlobHashMap<,> exists", typeof(BlobHashMap<int,int>) != null);
t("BlobHashMap<,> is struct", typeof(BlobHashMap<int,int>).IsValueType);
var bhMethods = typeof(BlobHashMap<int,int>).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var bhNames = bhMethods.Select(m => m.Name).Distinct().ToList();
t("BlobHashMap has TryGetValue", bhNames.Contains("TryGetValue"));
t("BlobHashMap contains ContainsKey", bhNames.Contains("ContainsKey") || bhNames.Contains("Contains"));

// --- BlobHashMapData<K,V>: internal storage (use NonPublic reflection) ---
var bhdType = System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .FirstOrDefault(xt => xt.Name == "BlobHashMapData`2");
t("BlobHashMapData<,> exists", bhdType != null);
t("BlobHashMapData<,> is struct", bhdType != null && bhdType.IsValueType);

// --- BlobPerfectHashMap<K,V> ---
t("BlobPerfectHashMap<,> exists", typeof(BlobPerfectHashMap<int,int>) != null);
t("BlobPerfectHashMap<,> is struct", typeof(BlobPerfectHashMap<int,int>).IsValueType);

// --- BlobMultiHashMap<K,V> ---
t("BlobMultiHashMap<,> exists", typeof(BlobMultiHashMap<int,int>) != null);
t("BlobMultiHashMap<,> is struct", typeof(BlobMultiHashMap<int,int>).IsValueType);

// --- BlobAllocation: nested struct ---
// Find BlobAllocation by searching
var blobAllocType = System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .FirstOrDefault(xt => xt.Name == "BlobAllocation");
t("BlobAllocation exists", blobAllocType != null);
t("BlobAllocation is struct", blobAllocType != null && blobAllocType.IsValueType);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
