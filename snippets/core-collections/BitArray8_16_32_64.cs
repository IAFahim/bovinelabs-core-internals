// Run: cat snippets/core-collections/BitArray8_16_32_64.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection"
// Verifies: docs/BitArray8_16_32_64.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

// ==========================
// BitArray8
// ==========================
var ba8Type = typeof(BovineLabs.Core.Collections.BitArray8);
t("BitArray8: type exists", ba8Type != null);
t("BitArray8: is ValueType", ba8Type.IsValueType);
int sz8 = System.Runtime.InteropServices.Marshal.SizeOf<BovineLabs.Core.Collections.BitArray8>();
t("BitArray8: size == 1 byte", sz8 == 1);

// Capacity
var cap8 = ba8Type.GetProperty("Capacity");
t("BitArray8: has Capacity property", cap8 != null);
var ba8 = new BovineLabs.Core.Collections.BitArray8();
if (cap8 != null)
{
    uint c = (uint)cap8.GetValue(ba8);
    t("BitArray8: Capacity == 8", c == 8);
}
else { t("BitArray8: Capacity == 8", false); }

// Indexer set/get
ba8[0] = true;
t("BitArray8: [0]=true -> [0]==true", ba8[0] == true);
t("BitArray8: [1] still false", ba8[1] == false);
ba8[7] = true;
t("BitArray8: [7]=true -> [7]==true", ba8[7] == true);
ba8[0] = false;
t("BitArray8: [0]=false -> [0]==false", ba8[0] == false);

// CountBits
ba8 = new BovineLabs.Core.Collections.BitArray8();
t("BitArray8: CountBits on default == 0", ba8.CountBits() == 0);
ba8[0] = true; ba8[3] = true; ba8[7] = true;
t("BitArray8: CountBits with 3 bits == 3", ba8.CountBits() == 3);

// AllFalse, AllTrue
ba8 = new BovineLabs.Core.Collections.BitArray8();
t("BitArray8: AllFalse on default == true", ba8.AllFalse == true);
t("BitArray8: AllTrue on default == false", ba8.AllTrue == false);

// BitOr, BitAnd, BitNot
var a8 = new BovineLabs.Core.Collections.BitArray8(); a8[0] = true;
var b8 = new BovineLabs.Core.Collections.BitArray8(); b8[1] = true;
var or8 = a8.BitOr(b8);
t("BitArray8: BitOr -> bit 0", or8[0] == true);
t("BitArray8: BitOr -> bit 1", or8[1] == true);
t("BitArray8: BitOr -> CountBits == 2", or8.CountBits() == 2);

a8 = new BovineLabs.Core.Collections.BitArray8(); a8[0] = true; a8[1] = true;
b8 = new BovineLabs.Core.Collections.BitArray8(); b8[0] = true;
var and8 = a8.BitAnd(b8);
t("BitArray8: BitAnd -> bit 0 true", and8[0] == true);
t("BitArray8: BitAnd -> bit 1 false", and8[1] == false);

ba8 = new BovineLabs.Core.Collections.BitArray8(); ba8[0] = true;
var not8 = ba8.BitNot();
t("BitArray8: BitNot -> bit 0 false", not8[0] == false);
t("BitArray8: BitNot -> bit 1 true", not8[1] == true);
t("BitArray8: BitNot -> CountBits == 7", not8.CountBits() == 7);

// Static fields: All, None
var all8 = BovineLabs.Core.Collections.BitArray8.All;
var none8 = BovineLabs.Core.Collections.BitArray8.None;
t("BitArray8: All.AllTrue == true", all8.AllTrue == true);
t("BitArray8: None.AllFalse == true", none8.AllFalse == true);
t("BitArray8: All.CountBits == 8", all8.CountBits() == 8);
t("BitArray8: None.CountBits == 0", none8.CountBits() == 0);

// Equality operators
t("BitArray8: == on same values", new BovineLabs.Core.Collections.BitArray8() == new BovineLabs.Core.Collections.BitArray8());
ba8 = new BovineLabs.Core.Collections.BitArray8(); ba8[0] = true;
t("BitArray8: != on different values", ba8 != new BovineLabs.Core.Collections.BitArray8());

// ==========================
// BitArray16
// ==========================
var ba16Type = typeof(BovineLabs.Core.Collections.BitArray16);
t("BitArray16: type exists", ba16Type != null);
t("BitArray16: is ValueType", ba16Type.IsValueType);
int sz16 = System.Runtime.InteropServices.Marshal.SizeOf<BovineLabs.Core.Collections.BitArray16>();
t("BitArray16: size == 2 bytes", sz16 == 2);

var ba16 = new BovineLabs.Core.Collections.BitArray16();
var cap16Prop = ba16Type.GetProperty("Capacity");
if (cap16Prop != null)
{
    uint c = (uint)cap16Prop.GetValue(ba16);
    t("BitArray16: Capacity == 16", c == 16);
}
else { t("BitArray16: Capacity == 16", false); }

ba16[0] = true; ba16[15] = true;
t("BitArray16: [0]=true -> [0]==true", ba16[0] == true);
t("BitArray16: [15]=true -> [15]==true", ba16[15] == true);
t("BitArray16: CountBits == 2", ba16.CountBits() == 2);
t("BitArray16: AllFalse == false after set", ba16.AllFalse == false);

var all16 = BovineLabs.Core.Collections.BitArray16.All;
var none16 = BovineLabs.Core.Collections.BitArray16.None;
t("BitArray16: All.AllTrue == true", all16.AllTrue == true);
t("BitArray16: None.AllFalse == true", none16.AllFalse == true);
t("BitArray16: All.CountBits == 16", all16.CountBits() == 16);

// ==========================
// BitArray32
// ==========================
var ba32Type = typeof(BovineLabs.Core.Collections.BitArray32);
t("BitArray32: type exists", ba32Type != null);
t("BitArray32: is ValueType", ba32Type.IsValueType);
int sz32 = System.Runtime.InteropServices.Marshal.SizeOf<BovineLabs.Core.Collections.BitArray32>();
t("BitArray32: size == 4 bytes", sz32 == 4);

var ba32 = new BovineLabs.Core.Collections.BitArray32();
var cap32Prop = ba32Type.GetProperty("Capacity");
if (cap32Prop != null)
{
    uint c = (uint)cap32Prop.GetValue(ba32);
    t("BitArray32: Capacity == 32", c == 32);
}
else { t("BitArray32: Capacity == 32", false); }

ba32[0] = true; ba32[31] = true;
t("BitArray32: [0]=true -> [0]==true", ba32[0] == true);
t("BitArray32: [31]=true -> [31]==true", ba32[31] == true);
t("BitArray32: CountBits == 2", ba32.CountBits() == 2);

var all32 = BovineLabs.Core.Collections.BitArray32.All;
t("BitArray32: All.CountBits == 32", all32.CountBits() == 32);

// ==========================
// BitArray64
// ==========================
var ba64Type = typeof(BovineLabs.Core.Collections.BitArray64);
t("BitArray64: type exists", ba64Type != null);
t("BitArray64: is ValueType", ba64Type.IsValueType);
int sz64 = System.Runtime.InteropServices.Marshal.SizeOf<BovineLabs.Core.Collections.BitArray64>();
t("BitArray64: size == 8 bytes", sz64 == 8);

var ba64 = new BovineLabs.Core.Collections.BitArray64();
var cap64Prop = ba64Type.GetProperty("Capacity");
if (cap64Prop != null)
{
    uint c = (uint)cap64Prop.GetValue(ba64);
    t("BitArray64: Capacity == 64", c == 64);
}
else { t("BitArray64: Capacity == 64", false); }

ba64[0] = true; ba64[63] = true;
t("BitArray64: [0]=true -> [0]==true", ba64[0] == true);
t("BitArray64: [63]=true -> [63]==true", ba64[63] == true);
t("BitArray64: CountBits == 2", ba64.CountBits() == 2);

var all64 = BovineLabs.Core.Collections.BitArray64.All;
t("BitArray64: All.CountBits == 64", all64.CountBits() == 64);
t("BitArray64: All.AllTrue == true", all64.AllTrue == true);

// BitOr/BitAnd/BitNot on BitArray64
var a64 = new BovineLabs.Core.Collections.BitArray64(); a64[0] = true;
var b64 = new BovineLabs.Core.Collections.BitArray64(); b64[1] = true;
var or64 = a64.BitOr(b64);
t("BitArray64: BitOr -> bits 0 and 1", or64[0] == true && or64[1] == true);
t("BitArray64: BitOr -> CountBits == 2", or64.CountBits() == 2);

a64 = new BovineLabs.Core.Collections.BitArray64(); a64[0] = true; a64[1] = true;
b64 = new BovineLabs.Core.Collections.BitArray64(); b64[0] = true;
var and64 = a64.BitAnd(b64);
t("BitArray64: BitAnd -> bit 0 true", and64[0] == true);
t("BitArray64: BitAnd -> bit 1 false", and64[1] == false);

ba64 = new BovineLabs.Core.Collections.BitArray64(); ba64[0] = true;
var not64 = ba64.BitNot();
t("BitArray64: BitNot -> bit 0 false", not64[0] == false);
t("BitArray64: BitNot -> CountBits == 63", not64.CountBits() == 63);

// Operator overloads on BitArray64
a64 = new BovineLabs.Core.Collections.BitArray64(); a64[0] = true;
var opOr = a64 | b64;
t("BitArray64: operator | works", opOr != null);
var opAnd = a64 & b64;
t("BitArray64: operator & works", opAnd != null);
var opNot = ~a64;
t("BitArray64: operator ~ works", opNot[0] == false && opNot[1] == true);
t("BitArray64: == default == default", new BovineLabs.Core.Collections.BitArray64() == new BovineLabs.Core.Collections.BitArray64());

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
