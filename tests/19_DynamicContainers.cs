// ============================================================================
// TEST: DynamicMultiHashMap + DynamicUntypedBuffer + DynamicVariableMap
// Branches: topic/DynamicMultiHashMap, topic/DynamicUntypedBuffer,
//           topic/DynamicVariableMap
// Sources: BovineLabs.Core/Iterators/DynamicMultiHashMap.cs,
//          DynamicUntypedBuffer.cs, DynamicVariableMap.cs
// Run: cat 19_DynamicContainers.cs | unity-cli exec --usings "BovineLabs.Core.Iterators,Unity.Collections,Unity.Entities,System.Linq"
// ============================================================================
// DynamicMultiHashMap<K,V>: multi-value hash map inside DynamicBuffer<byte>.
// DynamicUntypedBuffer: type-erased list of mixed unmanaged types in DynamicBuffer<byte>.
// DynamicVariableMap<K,V,>: hash map with secondary column indexes in DynamicBuffer<byte>.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- DynamicMultiHashMap<K,V>: multi-value hash map in DynamicBuffer ---
var dmhmType = typeof(DynamicMultiHashMap<int,int>);
t("DynamicMultiHashMap<,> exists", dmhmType != null);
t("DynamicMultiHashMap is struct", dmhmType.IsValueType);
var dmhmMethods = dmhmType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var dmhmNames = dmhmMethods.Select(m => m.Name).Distinct().ToList();
t("DynamicMultiHashMap has Add", dmhmNames.Contains("Add"));
t("DynamicMultiHashMap has TryGetFirstValue", dmhmNames.Contains("TryGetFirstValue"));
t("DynamicMultiHashMap has TryGetNextValue", dmhmNames.Contains("TryGetNextValue"));
t("DynamicMultiHashMap has ContainsKey", dmhmNames.Contains("ContainsKey"));
t("DynamicMultiHashMap has CountValuesForKey", dmhmNames.Contains("CountValuesForKey"));
t("DynamicMultiHashMap has Clear", dmhmNames.Contains("Clear"));
t("DynamicMultiHashMap has IsCreated", dmhmNames.Contains("get_IsCreated"));
t("DynamicMultiHashMap has Capacity", dmhmNames.Contains("get_Capacity") || dmhmNames.Contains("set_Capacity"));
t("DynamicMultiHashMap has AddBatchUnsafe", dmhmNames.Contains("AddBatchUnsafe"));
// Implements IEnumerable
var dmhmIfaces = dmhmType.GetInterfaces();
t("DynamicMultiHashMap implements IEnumerable", dmhmIfaces.Any(i => i.Name.StartsWith("IEnumerable")));

// --- DynamicUntypedBuffer: type-erased mixed-type list ---
var dubType = typeof(DynamicUntypedBuffer);
t("DynamicUntypedBuffer exists", dubType != null);
t("DynamicUntypedBuffer is struct", dubType.IsValueType);
var dubMethods = dubType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
var dubNames = dubMethods.Select(m => m.Name).Distinct().ToList();
t("DynamicUntypedBuffer has Add", dubNames.Contains("Add"));
t("DynamicUntypedBuffer has ElementAt", dubNames.Contains("ElementAt"));
t("DynamicUntypedBuffer has ElementAtRO", dubNames.Contains("ElementAtRO"));
t("DynamicUntypedBuffer has Set", dubNames.Contains("Set"));
t("DynamicUntypedBuffer has RemoveAt", dubNames.Contains("RemoveAt"));
t("DynamicUntypedBuffer has Clear", dubNames.Contains("Clear"));
t("DynamicUntypedBuffer has IsCreated", dubNames.Contains("get_IsCreated"));
t("DynamicUntypedBuffer has Length", dubNames.Contains("get_Length"));

// --- DynamicVariableMap<K,V,T,TC>: hash map with column indexes ---
// TC must implement IColumn<T>. Use reflection to test the open type.
var dvmOpenType = System.AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.GetName().Name.Contains("BovineLabs"))
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .FirstOrDefault(xt => xt.Name == "DynamicVariableMap`4");
t("DynamicVariableMap`4 exists", dvmOpenType != null);
t("DynamicVariableMap is struct", dvmOpenType != null && dvmOpenType.IsValueType);
var dvmMethods = dvmOpenType?.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly) ?? new System.Reflection.MethodInfo[0];
var dvmNames = dvmMethods.Select(m => m.Name).Distinct().ToList();
t("DynamicVariableMap has TryAdd", dvmNames.Contains("TryAdd"));
t("DynamicVariableMap has TryGetValue", dvmNames.Contains("TryGetValue"));
t("DynamicVariableMap has AddOrReplace", dvmNames.Contains("AddOrReplace"));
t("DynamicVariableMap has Remove", dvmNames.Contains("Remove"));
t("DynamicVariableMap has ReplaceColumn", dvmNames.Contains("ReplaceColumn"));
t("DynamicVariableMap has Column property", dvmNames.Contains("get_Column"));
t("DynamicVariableMap has IsCreated", dvmNames.Contains("get_IsCreated"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
