// Run: cat snippets/dynamic-buffers/DynamicMultiHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicMultiHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- DynamicMultiHashMap<TKey,TValue> is a struct ---
var mmapType = typeof(DynamicMultiHashMap<int, byte>);
t("DynamicMultiHashMap: type exists", mmapType != null);
t("DynamicMultiHashMap: is ValueType", mmapType.IsValueType);

// --- Implements IEnumerable<KVPair<TKey,TValue>> ---
t("DynamicMultiHashMap: implements IEnumerable",
    mmapType.GetInterfaces().Any(iface =>
        iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>)));

// --- Key properties ---
var countProp = mmapType.GetProperty("Count", bf);
t("DynamicMultiHashMap: has Count property", countProp != null);

var capProp = mmapType.GetProperty("Capacity", bf);
t("DynamicMultiHashMap: has Capacity property", capProp != null);

var isCreatedProp = mmapType.GetProperty("IsCreated", bf);
t("DynamicMultiHashMap: has IsCreated property", isCreatedProp != null);

var isEmptyProp = mmapType.GetProperty("IsEmpty", bf);
t("DynamicMultiHashMap: has IsEmpty property", isEmptyProp != null);

// --- Key methods ---
var addMethod = mmapType.GetMethods(bf).Where(m => m.Name == "Add" && m.GetParameters().Length == 2).FirstOrDefault();
t("DynamicMultiHashMap: has Add(TKey, TValue)", addMethod != null);

var removeMethod = mmapType.GetMethods(bf).Where(m => m.Name == "Remove" && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicMultiHashMap: has Remove(TKey)", removeMethod != null);

var clearMethod = mmapType.GetMethods(bf).Where(m => m.Name == "Clear" && m.GetParameters().Length == 0).FirstOrDefault();
t("DynamicMultiHashMap: has Clear()", clearMethod != null);

var tryGetFirst = mmapType.GetMethods(bf).Where(m => m.Name == "TryGetFirstValue").FirstOrDefault();
t("DynamicMultiHashMap: has TryGetFirstValue", tryGetFirst != null);

var tryGetNext = mmapType.GetMethods(bf).Where(m => m.Name == "TryGetNextValue").FirstOrDefault();
t("DynamicMultiHashMap: has TryGetNextValue", tryGetNext != null);

var containsKey = mmapType.GetMethods(bf).Where(m => m.Name == "ContainsKey").FirstOrDefault();
t("DynamicMultiHashMap: has ContainsKey", containsKey != null);

var flattenMethod = mmapType.GetMethods(bf).Where(m => m.Name == "Flatten" && m.GetParameters().Length == 0).FirstOrDefault();
t("DynamicMultiHashMap: has Flatten()", flattenMethod != null);

var countValuesForKey = mmapType.GetMethods(bf).Where(m => m.Name == "CountValuesForKey").FirstOrDefault();
t("DynamicMultiHashMap: has CountValuesForKey", countValuesForKey != null);

// --- DynamicHashMapHelper<TKey> header struct ---
var helperType = typeof(DynamicHashMapHelper<int>);
t("DynamicHashMapHelper: type exists", helperType != null);
t("DynamicHashMapHelper: is ValueType", helperType.IsValueType);

// Check Sequential layout
var sla = helperType.StructLayoutAttribute;
t("DynamicHashMapHelper: has StructLayout(LayoutKind.Sequential)", sla != null && sla.Value == LayoutKind.Sequential);

// Check fields: 11 ints = 44 bytes
var fields = helperType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
t("DynamicHashMapHelper: has 11 fields", fields.Length == 11);

var fieldNames = fields.Select(f => f.Name).ToHashSet();
t("DynamicHashMapHelper: has ValuesOffset", fieldNames.Contains("ValuesOffset"));
t("DynamicHashMapHelper: has KeysOffset", fieldNames.Contains("KeysOffset"));
t("DynamicHashMapHelper: has NextOffset", fieldNames.Contains("NextOffset"));
t("DynamicHashMapHelper: has BucketsOffset", fieldNames.Contains("BucketsOffset"));
t("DynamicHashMapHelper: has Count", fieldNames.Contains("Count"));
t("DynamicHashMapHelper: has Capacity", fieldNames.Contains("Capacity"));
t("DynamicHashMapHelper: has BucketCapacityMask", fieldNames.Contains("BucketCapacityMask"));
t("DynamicHashMapHelper: has Log2MinGrowth", fieldNames.Contains("Log2MinGrowth"));
t("DynamicHashMapHelper: has AllocatedIndex", fieldNames.Contains("AllocatedIndex"));
t("DynamicHashMapHelper: has FirstFreeIdx", fieldNames.Contains("FirstFreeIdx"));
t("DynamicHashMapHelper: has SizeOfTValue", fieldNames.Contains("SizeOfTValue"));

// All fields are int
bool allInt = fields.All(f => f.FieldType == typeof(int));
t("DynamicHashMapHelper: all 11 fields are int", allInt);

// Size check: 11 * 4 = 44 bytes
int helperSize = Marshal.SizeOf<DynamicHashMapHelper<int>>();
t("DynamicHashMapHelper: Marshal.SizeOf == 44 bytes", helperSize == 44);

// --- IDynamicMultiHashMap<TKey, TValue> interface ---
var ifaceType = typeof(IDynamicMultiHashMap<int, byte>);
t("IDynamicMultiHashMap: interface exists", ifaceType != null);
t("IDynamicMultiHashMap: is interface", ifaceType.IsInterface);
t("IDynamicMultiHashMap: implements IBufferElementData",
    ifaceType.GetInterfaces().Contains(typeof(IBufferElementData)));

var valueProp = ifaceType.GetProperty("Value");
t("IDynamicMultiHashMap: has Value property returning byte", valueProp != null && valueProp.PropertyType == typeof(byte));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
