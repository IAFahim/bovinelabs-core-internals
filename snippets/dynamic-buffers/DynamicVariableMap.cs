// Run: cat snippets/dynamic-buffers/DynamicVariableMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,BovineLabs.Core.Iterators.Columns,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicVariableMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- DynamicVariableMap<TKey,TValue,T,TC> type ---
var mapType = typeof(DynamicVariableMap<int, float, short, MultiHashColumn<short>>);
t("DynamicVariableMap: type exists", mapType != null);
t("DynamicVariableMap: is ValueType", mapType.IsValueType);

// --- Key methods ---
var tryAddMethod = mapType.GetMethods(bf).Where(m => m.Name == "TryAdd").FirstOrDefault();
t("DynamicVariableMap: has TryAdd", tryAddMethod != null);

var removeMethod = mapType.GetMethods(bf).Where(m => m.Name == "Remove" && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicVariableMap: has Remove(TKey)", removeMethod != null);

var removeAtMethod = mapType.GetMethods(bf).Where(m => m.Name == "RemoveAt").FirstOrDefault();
t("DynamicVariableMap: has RemoveAt", removeAtMethod != null);

var tryGetValue = mapType.GetMethods(bf).Where(m => m.Name == "TryGetValue").FirstOrDefault();
t("DynamicVariableMap: has TryGetValue", tryGetValue != null);

var replaceCol = mapType.GetMethods(bf).Where(m => m.Name == "ReplaceColumn").FirstOrDefault();
t("DynamicVariableMap: has ReplaceColumn", replaceCol != null);

// Column property
var colProp = mapType.GetProperty("Column", bf);
t("DynamicVariableMap: has Column property", colProp != null);

// Count property
var countProp = mapType.GetProperty("Count", bf);
t("DynamicVariableMap: has Count property", countProp != null);

// Capacity property
var capProp = mapType.GetProperty("Capacity", bf);
t("DynamicVariableMap: has Capacity property", capProp != null);

// --- MultiHashColumn<T> ---
var mhcType = typeof(MultiHashColumn<int>);
t("MultiHashColumn: type exists", mhcType != null);
t("MultiHashColumn: is ValueType", mhcType.IsValueType);

var tryGetFirstCol = mhcType.GetMethods(bf).Where(m => m.Name == "TryGetFirst").FirstOrDefault();
t("MultiHashColumn: has TryGetFirst", tryGetFirstCol != null);

var tryGetNextCol = mhcType.GetMethods(bf).Where(m => m.Name == "TryGetNext").FirstOrDefault();
t("MultiHashColumn: has TryGetNext", tryGetNextCol != null);

// --- OrderedListColumn<T> ---
var olcType = typeof(OrderedListColumn<int>);
t("OrderedListColumn: type exists", olcType != null);
t("OrderedListColumn: is ValueType", olcType.IsValueType);

// --- IColumn<T> interface ---
var icolType = typeof(IColumn<int>);
t("IColumn: interface exists", icolType != null);
t("IColumn: is interface", icolType.IsInterface);

// --- IDynamicVariableMap interfaces (1-col and 2-col) ---
var iface1 = typeof(IDynamicVariableMap<int, float, short, MultiHashColumn<short>>);
t("IDynamicVariableMap (1-col): interface exists", iface1 != null);
t("IDynamicVariableMap (1-col): is interface", iface1.IsInterface);
t("IDynamicVariableMap (1-col): implements IBufferElementData",
    iface1.GetInterfaces().Contains(typeof(IBufferElementData)));

var iface2 = typeof(IDynamicVariableMap<int, float, short, MultiHashColumn<short>, byte, MultiHashColumn<byte>>);
t("IDynamicVariableMap (2-col): interface exists", iface2 != null);
t("IDynamicVariableMap (2-col): is interface", iface2.IsInterface);

// --- Extension methods ---
var extType = typeof(DynamicExtensions);
var initVarMap = extType.GetMethods(bf).Where(m => m.Name == "InitializeVariableMap").ToArray();
t("DynamicExtensions: has InitializeVariableMap overloads", initVarMap.Length >= 2);

var asVarMap = extType.GetMethods(bf).Where(m => m.Name == "AsVariableMap").ToArray();
t("DynamicExtensions: has AsVariableMap overloads", asVarMap.Length >= 2);

// --- DynamicVariableMapHelper is internal, verify through assembly scan ---
var asm = typeof(DynamicVariableMap<int, float, short, MultiHashColumn<short>>).Assembly;
var helperTypeName = "BovineLabs.Core.Iterators.DynamicVariableMapHelper`4";
var helperType = asm.GetType(helperTypeName);
t("DynamicVariableMapHelper: type exists in assembly", helperType != null);
if (helperType != null)
    t("DynamicVariableMapHelper: is ValueType", helperType.IsValueType);
else
    t("DynamicVariableMapHelper: is ValueType", false);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
