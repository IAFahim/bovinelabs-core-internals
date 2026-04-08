// Run: cat snippets/core-collections/BitArray128.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection"
// Verifies: docs/BitArray128.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
var baType = typeof(BovineLabs.Core.Collections.BitArray128);

// --- Type exists and is a struct ---
t("BitArray128: type exists", baType != null);
t("BitArray128: is ValueType", baType.IsValueType);

// --- Size is 16 bytes (2 ulong fields) ---
int size = System.Runtime.InteropServices.Marshal.SizeOf<BovineLabs.Core.Collections.BitArray128>();
t("BitArray128: Marshal.SizeOf == 16 bytes", size == 16);

// --- Has data1 and data2 ulong fields ---
var d1 = baType.GetField("data1", bf);
var d2 = baType.GetField("data2", bf);
t("BitArray128: has field data1", d1 != null);
t("BitArray128: has field data2", d2 != null);
if (d1 != null) t("BitArray128: data1 is ulong", d1.FieldType == typeof(ulong));
else t("BitArray128: data1 is ulong", false);
if (d2 != null) t("BitArray128: data2 is ulong", d2.FieldType == typeof(ulong));
else t("BitArray128: data2 is ulong", false);

// --- Capacity = 128 ---
var capProp = baType.GetProperty("Capacity");
t("BitArray128: has Capacity property", capProp != null);
var bits = new BovineLabs.Core.Collections.BitArray128();
if (capProp != null)
{
    uint cap = (uint)capProp.GetValue(bits);
    t("BitArray128: Capacity == 128", cap == 128);
}
else { t("BitArray128: Capacity == 128", false); }

// --- AllFalse on default ---
var allFalseProp = baType.GetProperty("AllFalse");
t("BitArray128: has AllFalse property", allFalseProp != null);
if (allFalseProp != null)
    t("BitArray128: default AllFalse == true", (bool)allFalseProp.GetValue(bits) == true);
else t("BitArray128: default AllFalse == true", false);

// --- AllTrue on default ---
var allTrueProp = baType.GetProperty("AllTrue");
t("BitArray128: has AllTrue property", allTrueProp != null);
if (allTrueProp != null)
    t("BitArray128: default AllTrue == false", (bool)allTrueProp.GetValue(bits) == false);
else t("BitArray128: default AllTrue == false", false);

// --- CountBits ---
var countBitsMethod = baType.GetMethod("CountBits");
t("BitArray128: has CountBits method", countBitsMethod != null);
if (countBitsMethod != null)
    t("BitArray128: CountBits() on zeroed == 0", (int)countBitsMethod.Invoke(bits, null) == 0);
else t("BitArray128: CountBits() on zeroed == 0", false);

// --- Indexer: bit 0 (in data1) ---
bits = new BovineLabs.Core.Collections.BitArray128();
bits[0] = true;
t("BitArray128: set bit[0]=true -> get bit[0]==true", bits[0] == true);
t("BitArray128: CountBits after 1 bit == 1", bits.CountBits() == 1);
bits[0] = false;
t("BitArray128: clear bit[0] -> get bit[0]==false", bits[0] == false);
t("BitArray128: CountBits after clear == 0", bits.CountBits() == 0);

// --- Bit 63 (highest in data1) ---
bits = new BovineLabs.Core.Collections.BitArray128();
bits[63] = true;
t("BitArray128: set/get bit[63] (data1 boundary)", bits[63] == true);
t("BitArray128: CountBits after bit 63 == 1", bits.CountBits() == 1);

// --- Bit 64 (first in data2) ---
bits = new BovineLabs.Core.Collections.BitArray128();
bits[64] = true;
t("BitArray128: set/get bit[64] (data2 start)", bits[64] == true);
t("BitArray128: CountBits after bit 64 == 1", bits.CountBits() == 1);

// --- Bit 127 (highest bit) ---
bits = new BovineLabs.Core.Collections.BitArray128();
bits[127] = true;
t("BitArray128: set/get bit[127] (highest)", bits[127] == true);
t("BitArray128: CountBits after bit 127 == 1", bits.CountBits() == 1);

// --- Multiple bits across both fields ---
bits = new BovineLabs.Core.Collections.BitArray128();
bits[0] = true; bits[63] = true; bits[64] = true; bits[127] = true;
t("BitArray128: 4 bits across fields -> CountBits == 4", bits.CountBits() == 4);

// --- BitOr ---
var a = new BovineLabs.Core.Collections.BitArray128(); a[0] = true;
var b = new BovineLabs.Core.Collections.BitArray128(); b[1] = true;
var orResult = a.BitOr(b);
t("BitArray128: BitOr -> bit 0", orResult[0] == true);
t("BitArray128: BitOr -> bit 1", orResult[1] == true);
t("BitArray128: BitOr -> CountBits == 2", orResult.CountBits() == 2);

// --- BitAnd ---
a = new BovineLabs.Core.Collections.BitArray128(); a[0] = true; a[1] = true;
b = new BovineLabs.Core.Collections.BitArray128(); b[0] = true;
var andResult = a.BitAnd(b);
t("BitArray128: BitAnd -> bit 0 true", andResult[0] == true);
t("BitArray128: BitAnd -> bit 1 false", andResult[1] == false);
t("BitArray128: BitAnd -> CountBits == 1", andResult.CountBits() == 1);

// --- BitNot ---
bits = new BovineLabs.Core.Collections.BitArray128(); bits[0] = true;
var notResult = bits.BitNot();
t("BitArray128: BitNot -> bit 0 cleared", notResult[0] == false);
t("BitArray128: BitNot -> bit 1 set", notResult[1] == true);
t("BitArray128: BitNot -> CountBits == 127", notResult.CountBits() == 127);

// --- BitOr across data1/data2 boundary ---
a = new BovineLabs.Core.Collections.BitArray128(); a[5] = true;
b = new BovineLabs.Core.Collections.BitArray128(); b[100] = true;
var crossOr = a.BitOr(b);
t("BitArray128: BitOr cross-boundary -> bit 5", crossOr[5] == true);
t("BitArray128: BitOr cross-boundary -> bit 100", crossOr[100] == true);
t("BitArray128: BitOr cross-boundary -> CountBits == 2", crossOr.CountBits() == 2);

// --- Equality ---
t("BitArray128: == on default == default", new BovineLabs.Core.Collections.BitArray128() == new BovineLabs.Core.Collections.BitArray128());
bits = new BovineLabs.Core.Collections.BitArray128(); bits[0] = true;
t("BitArray128: != on different values", bits != new BovineLabs.Core.Collections.BitArray128());

// --- Operator overloads ---
a = new BovineLabs.Core.Collections.BitArray128(); a[0] = true;
var opOr = a | b;
t("BitArray128: operator | works", opOr != null && opOr[0] == true);
var opAnd = a & b;
t("BitArray128: operator & works", opAnd != null);
var opNot = ~a;
t("BitArray128: operator ~ works", opNot[0] == false && opNot[1] == true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
