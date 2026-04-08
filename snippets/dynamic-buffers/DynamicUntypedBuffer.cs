// Run: cat snippets/dynamic-buffers/DynamicUntypedBuffer.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicUntypedBuffer.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

// --- DynamicUntypedBuffer is a struct ---
var bufType = typeof(DynamicUntypedBuffer);
t("DynamicUntypedBuffer: type exists", bufType != null);
t("DynamicUntypedBuffer: is ValueType", bufType.IsValueType);

// --- Key methods ---
var addMethod = bufType.GetMethods(bf).Where(m => m.Name == "Add" && m.IsGenericMethod && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicUntypedBuffer: has Add<T>(T)", addMethod != null);

var elementAtRO = bufType.GetMethods(bf).Where(m => m.Name == "ElementAtRO" && m.IsGenericMethod).FirstOrDefault();
t("DynamicUntypedBuffer: has ElementAtRO<T>(int)", elementAtRO != null);

var removeAtMethod = bufType.GetMethods(bf).Where(m => m.Name == "RemoveAt" && m.GetParameters().Length == 1).FirstOrDefault();
t("DynamicUntypedBuffer: has RemoveAt(int)", removeAtMethod != null);

var clearMethod = bufType.GetMethods(bf).Where(m => m.Name == "Clear" && m.GetParameters().Length == 0).FirstOrDefault();
t("DynamicUntypedBuffer: has Clear()", clearMethod != null);

// --- DynamicUntypedBufferHelper header struct ---
var helperType = typeof(DynamicUntypedBufferHelper);
t("DynamicUntypedBufferHelper: type exists", helperType != null);
t("DynamicUntypedBufferHelper: is ValueType", helperType.IsValueType);

var helperSla = helperType.StructLayoutAttribute;
t("DynamicUntypedBufferHelper: has StructLayout(LayoutKind.Sequential)", helperSla != null && helperSla.Value == LayoutKind.Sequential);

// Check fields: 10 ints = 40 bytes
var helperFields = helperType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
t("DynamicUntypedBufferHelper: has 10 fields", helperFields.Length == 10);

var hfn = helperFields.Select(f => f.Name).ToHashSet();
t("DynamicUntypedBufferHelper: has OffsetsOffset", hfn.Contains("OffsetsOffset"));
t("DynamicUntypedBufferHelper: has SizesOffset", hfn.Contains("SizesOffset"));
t("DynamicUntypedBufferHelper: has TypesOffset", hfn.Contains("TypesOffset"));
t("DynamicUntypedBufferHelper: has AlignmentsOffset", hfn.Contains("AlignmentsOffset"));
t("DynamicUntypedBufferHelper: has DataOffset", hfn.Contains("DataOffset"));
t("DynamicUntypedBufferHelper: has Count", hfn.Contains("Count"));
t("DynamicUntypedBufferHelper: has Capacity", hfn.Contains("Capacity"));
t("DynamicUntypedBufferHelper: has DataCapacity", hfn.Contains("DataCapacity"));
t("DynamicUntypedBufferHelper: has DataAllocatedIndex", hfn.Contains("DataAllocatedIndex"));
t("DynamicUntypedBufferHelper: has Log2MinGrowth", hfn.Contains("Log2MinGrowth"));

bool allHelperFieldsInt = helperFields.All(f => f.FieldType == typeof(int));
t("DynamicUntypedBufferHelper: all 10 fields are int", allHelperFieldsInt);

int helperSize = Marshal.SizeOf<DynamicUntypedBufferHelper>();
t("DynamicUntypedBufferHelper: Marshal.SizeOf == 40 bytes", helperSize == 40);

// --- IDynamicUntypedBuffer interface ---
var ifaceType = typeof(IDynamicUntypedBuffer);
t("IDynamicUntypedBuffer: interface exists", ifaceType != null);
t("IDynamicUntypedBuffer: is interface", ifaceType.IsInterface);
t("IDynamicUntypedBuffer: implements IBufferElementData",
    ifaceType.GetInterfaces().Contains(typeof(IBufferElementData)));

var valueProp = ifaceType.GetProperty("Value");
t("IDynamicUntypedBuffer: has Value property returning byte", valueProp != null && valueProp.PropertyType == typeof(byte));

// --- Extension methods ---
var extType = typeof(DynamicExtensions);
var initUBMethod = extType.GetMethods(bf).Where(m => m.Name == "InitializeUntypedBuffer").FirstOrDefault();
t("DynamicExtensions: has InitializeUntypedBuffer", initUBMethod != null);

var asUBMethod = extType.GetMethods(bf).Where(m => m.Name == "AsUntypedBuffer").FirstOrDefault();
t("DynamicExtensions: has AsUntypedBuffer", asUBMethod != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
