// ============================================================================
// TEST: BitArray8/16/32/64/128/256 + BitArrayUtilities
// Branches: topic/BitArray8_16_32_64, topic/BitArray128, topic/BitArray256,
//           topic/BitArrayUtilities
// Source: BovineLabs.Core/Collections/BitArray.cs
// Run: cat 02_BitArrays.cs | unity-cli exec --usings "BovineLabs.Core.Collections"
// ============================================================================
// BitArrayN is a fixed-size bitfield struct with N bits.
// Indexer [int] for get/set, Capacity gives bit count.
// AllFalse/AllTrue for bulk check, BitAnd/BitOr/BitNot for bitwise ops.
// CountBits counts set bits. BitArrayUtilities provides static helpers.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- BitArray8 ---
var ba8 = new BitArray8();
t("BitArray8 Capacity is 8", ba8.Capacity == 8);
ba8[0] = true; ba8[3] = true; ba8[7] = true;
t("BitArray8 [0] set", ba8[0] == true);
t("BitArray8 [3] set", ba8[3] == true);
t("BitArray8 [7] set", ba8[7] == true);
t("BitArray8 [1] not set", ba8[1] == false);
t("BitArray8 !AllFalse after sets", ba8.AllFalse == false);
var allSet8 = new BitArray8();
for(int i=0;i<8;i++) allSet8[i]=true;
t("BitArray8 AllTrue when all set", allSet8.AllTrue == true);
t("BitArray8 CountBits=3 on ba8", ba8.CountBits() == 3);
var not8 = ba8.BitNot();
t("BitArray8 BitNot flips bits", not8[0]==false && not8[1]==true);

// --- BitArray16 ---
var ba16 = new BitArray16();
t("BitArray16 Capacity is 16", ba16.Capacity == 16);
ba16[15] = true;
t("BitArray16 [15] set", ba16[15] == true);
t("BitArray16 [0] not set", ba16[0] == false);
var allSet16 = new BitArray16();
for(int i=0;i<16;i++) allSet16[i]=true;
t("BitArray16 AllTrue", allSet16.AllTrue);

// --- BitArray32 ---
var ba32 = new BitArray32();
t("BitArray32 Capacity is 32", ba32.Capacity == 32);
ba32[31] = true; ba32[0] = true;
t("BitArray32 [31] and [0] set", ba32[31] && ba32[0]);
t("BitArray32 CountBits=2", ba32.CountBits() == 2);

// --- BitArray64 ---
var ba64 = new BitArray64();
t("BitArray64 Capacity is 64", ba64.Capacity == 64);
ba64[63] = true;
t("BitArray64 [63] set", ba64[63] == true);
t("BitArray64 CountBits=1", ba64.CountBits() == 1);

// --- BitArray128 ---
var ba128 = new BitArray128();
t("BitArray128 Capacity is 128", ba128.Capacity == 128);
ba128[0] = true; ba128[127] = true;
t("BitArray128 [0] and [127] set", ba128[0] && ba128[127]);
t("BitArray128 CountBits=2", ba128.CountBits() == 2);

// --- BitArray256 ---
var ba256 = new BitArray256();
t("BitArray256 Capacity is 256", ba256.Capacity == 256);
ba256[255] = true; ba256[128] = true; ba256[0] = true;
t("BitArray256 [255],[128],[0] set", ba256[255] && ba256[128] && ba256[0]);
t("BitArray256 CountBits=3", ba256.CountBits() == 3);

// --- BitOr / BitAnd ---
var a8 = new BitArray8(); a8[0]=true; a8[2]=true;
var b8 = new BitArray8(); b8[2]=true; b8[4]=true;
var or8 = a8.BitOr(b8);
t("BitArray8 BitOr", or8[0] && or8[2] && or8[4] && !or8[1]);
var and8 = a8.BitAnd(b8);
t("BitArray8 BitAnd", !and8[0] && and8[2] && !and8[4]);

// --- BitArrayUtilities ---
t("BitArrayUtilities type exists", typeof(BitArrayUtilities) != null);
var baUtilMethods = typeof(BitArrayUtilities).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
t("BitArrayUtilities has static methods", baUtilMethods.Length > 0);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
