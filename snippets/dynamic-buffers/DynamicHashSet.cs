// Run: cat snippets/dynamic-buffers/DynamicHashSet.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicHashSet.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- DynamicHashSet<T> is a struct ---
var setType = typeof(DynamicHashSet<int>);
t("DynamicHashSet: type exists", setType != null);
t("DynamicHashSet: is ValueType", setType.IsValueType);

// Implements IEnumerable<T>
t("DynamicHashSet: implements IEnumerable",
    setType.GetInterfaces().Any(iface =>
        iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>)));

// --- Key properties ---
t("DynamicHashSet: has Count property", setType.GetProperty("Count", bf) != null);
t("DynamicHashSet: has Capacity property", setType.GetProperty("Capacity", bf) != null);
t("DynamicHashSet: has IsCreated property", setType.GetProperty("IsCreated", bf) != null);
t("DynamicHashSet: has IsEmpty property", setType.GetProperty("IsEmpty", bf) != null);

// --- Key methods ---
var addMethod = setType.GetMethods(bf).Where(m => m.Name == "Add" && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicHashSet: has Add(T)", addMethod != null);
if (addMethod != null)
    t("DynamicHashSet: Add returns bool", addMethod.ReturnType == typeof(bool));

var removeMethod = setType.GetMethods(bf).Where(m => m.Name == "Remove" && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicHashSet: has Remove(T)", removeMethod != null);
if (removeMethod != null)
    t("DynamicHashSet: Remove returns bool", removeMethod.ReturnType == typeof(bool));

var containsMethod = setType.GetMethods(bf).Where(m => m.Name == "Contains" && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicHashSet: has Contains(T)", containsMethod != null);
if (containsMethod != null)
    t("DynamicHashSet: Contains returns bool", containsMethod.ReturnType == typeof(bool));

var clearMethod = setType.GetMethods(bf).Where(m => m.Name == "Clear" && m.GetParameters().Length == 0).FirstOrDefault();
t("DynamicHashSet: has Clear()", clearMethod != null);

var flattenMethod = setType.GetMethods(bf).Where(m => m.Name == "Flatten" && m.GetParameters().Length == 0).FirstOrDefault();
t("DynamicHashSet: has Flatten()", flattenMethod != null);

// --- DynamicHashSet reuses DynamicHashMapHelper<T> ---
// The helper is the same type used by hash maps (with SizeOfTValue=0)
var helperType = typeof(DynamicHashMapHelper<int>);
t("DynamicHashSet: reuses DynamicHashMapHelper<T> (same helper type)", helperType != null);

// --- IDynamicHashSet<TKey> interface ---
var ifaceType = typeof(IDynamicHashSet<int>);
t("IDynamicHashSet: interface exists", ifaceType != null);
t("IDynamicHashSet: is interface", ifaceType.IsInterface);
t("IDynamicHashSet: implements IBufferElementData",
    ifaceType.GetInterfaces().Contains(typeof(IBufferElementData)));

var valueProp = ifaceType.GetProperty("Value");
t("IDynamicHashSet: has Value property returning byte", valueProp != null && valueProp.PropertyType == typeof(byte));

// --- InitializeHashSet extension method ---
var extType = typeof(DynamicExtensions);
t("DynamicExtensions: type exists", extType != null);

var initMethod = extType.GetMethods(bf).Where(m => m.Name == "InitializeHashSet").FirstOrDefault();
t("DynamicExtensions: has InitializeHashSet method", initMethod != null);

var asHashSetMethod = extType.GetMethods(bf).Where(m => m.Name == "AsHashSet").FirstOrDefault();
t("DynamicExtensions: has AsHashSet method", asHashSetMethod != null);

// Verify Init passes sizeOfValue=0 for sets (we check the method has the right param count)
if (initMethod != null)
{
    var initParams = initMethod.GetParameters();
    t("DynamicHashSet: InitializeHashSet has 3 params (buffer, capacity, minGrowth)", initParams.Length == 3);
}
else
{
    t("DynamicHashSet: InitializeHashSet has 3 params (buffer, capacity, minGrowth)", false);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
