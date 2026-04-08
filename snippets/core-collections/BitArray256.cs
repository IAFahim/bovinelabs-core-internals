// Run: cat snippets/core-collections/BitArray256.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection"
// Verifies: docs/BitArray256.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var baType = typeof(BovineLabs.Core.Collections.BitArray256);

// --- Type exists and is a struct ---
t("BitArray256: type exists", baType != null);
t("BitArray256: is a struct (ValueType)", baType.IsValueType);

// --- Size is 32 bytes ---
int size = System.Runtime.InteropServices.Marshal.SizeOf<BovineLabs.Core.Collections.BitArray256>();
t("BitArray256: Marshal.SizeOf == 32 bytes", size == 32);

// --- Has 4 ulong fields: data1, data2, data3, data4 (may be private) ---
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
var d1 = baType.GetField("data1", bf);
var d2 = baType.GetField("data2", bf);
var d3 = baType.GetField("data3", bf);
var d4 = baType.GetField("data4", bf);
t("BitArray256: has field data1", d1 != null);
t("BitArray256: has field data2", d2 != null);
t("BitArray256: has field data3", d3 != null);
t("BitArray256: has field data4", d4 != null);
t("BitArray256: data1 is ulong", d1?.FieldType == typeof(ulong));
t("BitArray256: data2 is ulong", d2?.FieldType == typeof(ulong));
t("BitArray256: data3 is ulong", d3?.FieldType == typeof(ulong));
t("BitArray256: data4 is ulong", d4?.FieldType == typeof(ulong));

// --- Capacity property returns 256 ---
var capProp = baType.GetProperty("Capacity");
t("BitArray256: has Capacity property", capProp != null);
var bits = new BovineLabs.Core.Collections.BitArray256();
if (capProp != null)
{
    uint cap = (uint)capProp.GetValue(bits);
    t("BitArray256: Capacity == 256", cap == 256);
}
else { t("BitArray256: Capacity == 256", false); }

// --- AllFalse on default ---
var allFalseProp = baType.GetProperty("AllFalse");
t("BitArray256: has AllFalse property", allFalseProp != null);
if (allFalseProp != null)
{
    t("BitArray256: default AllFalse == true", (bool)allFalseProp.GetValue(bits) == true);
}
else { t("BitArray256: default AllFalse == true", false); }

// --- CountBits ---
var countBitsMethod = baType.GetMethod("CountBits");
t("BitArray256: has CountBits method", countBitsMethod != null);
if (countBitsMethod != null)
{
    int bitCount = (int)countBitsMethod.Invoke(bits, null);
    t("BitArray256: CountBits() on zeroed == 0", bitCount == 0);
}
else { t("BitArray256: CountBits() on zeroed == 0", false); }

// --- Indexer: set/get bit 0 ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[0] = true;
t("BitArray256: set bit[0]=true -> get bit[0] == true", bits[0] == true);
t("BitArray256: CountBits after 1 bit set == 1", bits.CountBits() == 1);
bits[0] = false;
t("BitArray256: clear bit[0] -> get bit[0] == false", bits[0] == false);
t("BitArray256: CountBits after clear == 0", bits.CountBits() == 0);

// --- Bit 64 (data2 boundary) ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[64] = true;
t("BitArray256: set/get bit[64]", bits[64] == true);
t("BitArray256: CountBits after bit 64 == 1", bits.CountBits() == 1);

// --- Bit 128 (data3 boundary) ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[128] = true;
t("BitArray256: set/get bit[128]", bits[128] == true);
t("BitArray256: CountBits after bit 128 == 1", bits.CountBits() == 1);

// --- Bit 192 (data4 boundary) ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[192] = true;
t("BitArray256: set/get bit[192]", bits[192] == true);
t("BitArray256: CountBits after bit 192 == 1", bits.CountBits() == 1);

// --- Bit 255 (highest bit) ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[255] = true;
t("BitArray256: set/get bit[255]", bits[255] == true);
t("BitArray256: CountBits after bit 255 == 1", bits.CountBits() == 1);

// --- Multiple bits across fields ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[0] = true; bits[63] = true; bits[64] = true; bits[127] = true; bits[200] = true;
t("BitArray256: 5 bits across fields -> CountBits == 5", bits.CountBits() == 5);

// --- BitOr ---
var a = new BovineLabs.Core.Collections.BitArray256();
a[0] = true;
var b = new BovineLabs.Core.Collections.BitArray256();
b[1] = true;
var orResult = a.BitOr(b);
t("BitArray256: BitOr -> has bit 0", orResult[0] == true);
t("BitArray256: BitOr -> has bit 1", orResult[1] == true);
t("BitArray256: BitOr -> CountBits == 2", orResult.CountBits() == 2);

// --- BitAnd ---
a = new BovineLabs.Core.Collections.BitArray256();
a[0] = true; a[1] = true;
b = new BovineLabs.Core.Collections.BitArray256();
b[0] = true;
var andResult = a.BitAnd(b);
t("BitArray256: BitAnd -> has bit 0", andResult[0] == true);
t("BitArray256: BitAnd -> bit 1 cleared", andResult[1] == false);
t("BitArray256: BitAnd -> CountBits == 1", andResult.CountBits() == 1);

// --- BitNot ---
bits = new BovineLabs.Core.Collections.BitArray256();
bits[0] = true;
var notResult = bits.BitNot();
t("BitArray256: BitNot -> bit 0 cleared", notResult[0] == false);
t("BitArray256: BitNot -> bit 1 set", notResult[1] == true);
t("BitArray256: BitNot -> CountBits == 255", notResult.CountBits() == 255);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
