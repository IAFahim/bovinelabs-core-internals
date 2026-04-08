// Run: cat snippets/core-collections/NativeThreadStream.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
// Verifies: docs/NativeThreadStream.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var asm = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "BovineLabs.Core");

// === UnsafeThreadStream ===
var utsType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStream");
t("UnsafeThreadStream type exists", utsType != null);
t("UnsafeThreadStream is struct", utsType.IsValueType);

// Doc claim: "16 bytes on 64-bit"
int utsSize = System.Runtime.InteropServices.Marshal.SizeOf(utsType);
t("UnsafeThreadStream size = 16 bytes (doc claim)", utsSize == 16);

// Fields: blockData*, allocator
var utsFields = utsType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
var utsFieldNames = utsFields.Select(f => f.Name).ToList();
t("UnsafeThreadStream has blockData field", utsFieldNames.Contains("blockData"));
t("UnsafeThreadStream has allocator field", utsFieldNames.Contains("allocator"));
t("UnsafeThreadStream has exactly 2 fields", utsFields.Length == 2);

// === UnsafeThreadStreamBlockData ===
var bdataType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStreamBlockData");
t("UnsafeThreadStreamBlockData type exists", bdataType != null);
t("UnsafeThreadStreamBlockData is struct", bdataType.IsValueType);
int bdataSize = System.Runtime.InteropServices.Marshal.SizeOf(bdataType);
t("UnsafeThreadStreamBlockData size = 24 bytes (base 0x18)", bdataSize == 24);

// === UnsafeThreadStreamRange ===
var rangeType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStreamRange");
t("UnsafeThreadStreamRange type exists", rangeType != null);
t("UnsafeThreadStreamRange is struct", rangeType.IsValueType);
int rangeSize = System.Runtime.InteropServices.Marshal.SizeOf(rangeType);
// Doc claims 40 bytes, actual is 48 bytes (CORRECTION NEEDED)
t("UnsafeThreadStreamRange size = 48 bytes (doc says 40, actual is 48)", rangeSize == 48);

// Verify individual field offsets match doc
var rangeFields = rangeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
var getFieldOffset = new Func<string, int>(name => {
    var f = rangeFields.FirstOrDefault(x => x.Name == name);
    return f != null ? (int)System.Runtime.InteropServices.Marshal.OffsetOf(rangeType, name) : -1;
});

t("Range.Block at offset 0 (doc: 0x00)", getFieldOffset("Block") == 0);
t("Range.OffsetInFirstBlock at offset 8 (doc: 0x08)", getFieldOffset("OffsetInFirstBlock") == 8);
t("Range.ElementCount at offset 12 (doc: 0x0C)", getFieldOffset("ElementCount") == 12);
t("Range.LastOffset at offset 16 (doc: 0x10)", getFieldOffset("LastOffset") == 16);
t("Range.NumberOfBlocks at offset 20 (doc: 0x14)", getFieldOffset("NumberOfBlocks") == 20);
t("Range.CurrentBlock at offset 24 (doc: 0x18)", getFieldOffset("CurrentBlock") == 24);
t("Range.CurrentPtr at offset 32 (doc: 0x20)", getFieldOffset("CurrentPtr") == 32);
t("Range.CurrentBlockEnd at offset 40 (doc: 0x28)", getFieldOffset("CurrentBlockEnd") == 40);
t("Range has 8 fields total", rangeFields.Length == 8);

// === UnsafeThreadStreamBlock ===
var blockType = asm.GetType("BovineLabs.Core.Collections.UnsafeThreadStreamBlock");
t("UnsafeThreadStreamBlock type exists", blockType != null);
t("UnsafeThreadStreamBlock is struct", blockType.IsValueType);
int blockSize = System.Runtime.InteropServices.Marshal.SizeOf(blockType);
t("UnsafeThreadStreamBlock struct = 16 bytes (managed view: Next* + Data ptr)", blockSize == 16);

// === NativeThreadStream ===
var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);
t("NativeThreadStream type exists", ntsType != null);
t("NativeThreadStream is struct", ntsType.IsValueType);

// Check key methods exist
var ntsMethods = ntsType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name).Distinct().ToList();
t("NativeThreadStream has Dispose", ntsMethods.Contains("Dispose"));

var ntsPublicProps = ntsType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();
t("NativeThreadStream has IsCreated property", ntsPublicProps.Contains("IsCreated"));

// Check AsReader/AsWriter
t("NativeThreadStream has AsReader", ntsMethods.Contains("AsReader"));
t("NativeThreadStream has AsWriter", ntsMethods.Contains("AsWriter"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
