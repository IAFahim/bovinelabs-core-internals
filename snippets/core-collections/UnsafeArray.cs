// Run: cat snippets/core-collections/UnsafeArray.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections"
// Verifies: docs/UnsafeArray.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Claim: UnsafeArray<T> type exists ---
var type = typeof(BovineLabs.Core.Collections.UnsafeArray<int>);
t("UnsafeArray<int> type exists", type != null);
t("Is a struct (ValueType)", type.IsValueType);

// --- Claim: Implements IDisposable ---
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(type));

// --- Claim: Implements IEnumerable<T> ---
t("Implements IEnumerable<int>", typeof(System.Collections.Generic.IEnumerable<int>).IsAssignableFrom(type));

// --- Claim: Implements IEquatable<UnsafeArray<T>> ---
t("Implements IEquatable<UnsafeArray<int>>", typeof(System.IEquatable<BovineLabs.Core.Collections.UnsafeArray<int>>).IsAssignableFrom(type));

// --- Claim: Has Length property ---
var lengthProp = type.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
t("Has Length property", lengthProp != null);
if (lengthProp != null)
    t("Length has only getter (no setter)", lengthProp.SetMethod == null || lengthProp.SetMethod.IsPrivate);

// --- Claim: Has IsCreated property ---
var isCreatedProp = type.GetProperty("IsCreated", BindingFlags.Public | BindingFlags.Instance);
t("Has IsCreated property", isCreatedProp != null);

// --- Claim: Has indexer [int] ---
var indexer = type.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
t("Has indexer [int]", indexer != null);

// --- Claim: Constructor takes (int length, Allocator allocator, NativeArrayOptions options) ---
var ctor1 = type.GetConstructor(new[] { typeof(int), typeof(Unity.Collections.Allocator), typeof(Unity.Collections.NativeArrayOptions) });
t("Constructor takes (int, Allocator, NativeArrayOptions)", ctor1 != null);

var ctor2 = type.GetConstructor(new[] { typeof(int), typeof(Unity.Collections.Allocator) });
t("Constructor takes (int, Allocator) - default options", ctor2 != null);

// --- Claim: Has Dispose(JobHandle) for deferred disposal ---
var disposeWithHandle = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Unity.Jobs.JobHandle) }, null);
t("Has Dispose(JobHandle)", disposeWithHandle != null);

// --- Claim: Has GetUnsafePtr method ---
var getPtr = type.GetMethod("GetUnsafePtr", BindingFlags.Public | BindingFlags.Instance);
t("Has GetUnsafePtr()", getPtr != null);

// --- Claim: Has CopyFrom/CopyTo methods ---
var copyFromArray = type.GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int[]) }, null);
t("Has CopyFrom(int[])", copyFromArray != null);
var copyFromArray2 = type.GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Instance, null, new[] { type }, null);
t("Has CopyFrom(UnsafeArray<int>)", copyFromArray2 != null);
var copyToArray = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int[]) }, null);
t("Has CopyTo(int[])", copyToArray != null);
var copyToArray2 = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance, null, new[] { type }, null);
t("Has CopyTo(UnsafeArray<int>)", copyToArray2 != null);

// --- Claim: Has ToArray method ---
var toArray = type.GetMethod("ToArray", BindingFlags.Public | BindingFlags.Instance);
t("Has ToArray()", toArray != null);

// --- Claim: Has GetEnumerator (foreach support) ---
var getEnum = type.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
t("Has GetEnumerator()", getEnum != null);

// --- Claim: Static Copy methods ---
var staticCopy = type.GetMethod("Copy", BindingFlags.Public | BindingFlags.Static, null, new[] { type, type }, null);
t("Has static Copy(UnsafeArray, UnsafeArray)", staticCopy != null);

var staticCopyArr = type.GetMethod("Copy", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int[]), type }, null);
t("Has static Copy(int[], UnsafeArray)", staticCopyArr != null);

// --- Claim: Struct size ~16 bytes (void* buffer + int Length + Allocator label) ---
int structSize = System.Runtime.InteropServices.Marshal.SizeOf(type);
r.Add($"INFO: UnsafeArray<int> struct size = {structSize} bytes (doc: ~16 bytes)");
// On 64-bit: void* (8) + int (4) + Allocator enum (4) + padding = ~16-20
t("Struct size is reasonable (~16-24 bytes on 64-bit)", structSize >= 12 && structSize <= 32);

// --- Claim: allocatorLabel is Allocator enum (not AllocatorHandle) ---
var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
var allocField = fields.FirstOrDefault(f => f.Name.Contains("allocatorLabel"));
t("Has allocatorLabel field", allocField != null);
if (allocField != null)
    t("allocatorLabel is Allocator enum", allocField.FieldType == typeof(Unity.Collections.Allocator));

// --- Functional test: create, read, write, copy ---
var arr = new BovineLabs.Core.Collections.UnsafeArray<int>(5, Unity.Collections.Allocator.Temp);
t("Construction succeeds", arr.IsCreated);
t("Length == 5", arr.Length == 5);

// Default cleared to 0
t("Default cleared: arr[0] == 0", arr[0] == 0);
t("Default cleared: arr[4] == 0", arr[4] == 0);

// Write
arr[0] = 42;
arr[2] = 99;
arr[4] = -1;
t("arr[0] == 42 after write", arr[0] == 42);
t("arr[2] == 99 after write", arr[2] == 99);
t("arr[4] == -1 after write", arr[4] == -1);

// Copy to managed array
int[] managed = new int[5];
arr.CopyTo(managed);
t("CopyTo: managed[0] == 42", managed[0] == 42);
t("CopyTo: managed[2] == 99", managed[2] == 99);
t("CopyTo: managed[4] == -1", managed[4] == -1);

// ToArray
int[] asArray = arr.ToArray();
t("ToArray: length == 5", asArray.Length == 5);
t("ToArray: [0] == 42", asArray[0] == 42);

// Copy between UnsafeArrays
var arr2 = new BovineLabs.Core.Collections.UnsafeArray<int>(5, Unity.Collections.Allocator.Temp);
BovineLabs.Core.Collections.UnsafeArray<int>.Copy(arr, arr2);
t("Static Copy: arr2[0] == 42", arr2[0] == 42);
t("Static Copy: arr2[2] == 99", arr2[2] == 99);

arr2.Dispose();
arr.Dispose();
t("Dispose succeeds", true);

// --- Claim: No AtomicSafetyHandle (check field absence) ---
bool hasSafetyHandle = fields.Any(f => f.Name.Contains("m_Safety") || f.Name.Contains("safety"));
t("NO AtomicSafetyHandle field (unsafe)", !hasSafetyHandle);

// --- Claim: Enumerator starts at index -1 ---
var enumType = type.GetNestedType("Enumerator");
t("Enumerator nested type exists", enumType != null);
if (enumType != null)
{
    t("Enumerator is struct", enumType.IsValueType);
    var moveNext = enumType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
    t("Enumerator has MoveNext()", moveNext != null);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
