// ============================================================================
// TEST: CodecService + Serializer + Deserializer + EnableMaskCreator
// Branches: topic/CodecService, topic/Serializer, topic/Deserializer,
//           topic/EnableMaskCreator
// Sources: BovineLabs.Core/Utility/*.cs
// Run: cat 07_Serialization.cs | unity-cli exec --usings "BovineLabs.Core.Utility,Unity.Collections,System.Linq"
// ============================================================================
// CodecService: LZ4 codec wrapper for compressing/decompressing native buffers.
// GetBoundedSize(Codec codec, int srcSize) returns max compressed size.
// Serializer: unsafe struct for writing primitive types into a byte buffer.
// Deserializer: unsafe struct for reading primitive types from a byte buffer.
// EnableMaskCreator: generates enable-bit masks from ComponentType sets.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- CodecService ---
var csMethods = typeof(CodecService).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var csNames = csMethods.Select(m => m.Name).ToList();
t("CodecService has Compress", csNames.Contains("Compress"));
t("CodecService has Decompress", csNames.Contains("Decompress"));
t("CodecService has GetBoundedSize", csNames.Contains("GetBoundedSize"));

// GetBoundedSize(Codec.LZ4, 1024)
var maxSize = CodecService.GetBoundedSize(Codec.LZ4, 1024);
t("CodecService.GetBoundedSize(LZ4,1024) > 0", maxSize > 0);

// --- Codec enum ---
var codecNames = System.Enum.GetNames(typeof(Codec));
t("Codec enum has LZ4", codecNames.Contains("LZ4"));

// --- Serializer ---
var serType = typeof(Serializer);
t("Serializer type exists", serType != null);
t("Serializer is struct (value type)", serType.IsValueType);

// --- Deserializer ---
var desType = typeof(Deserializer);
t("Deserializer type exists", desType != null);
t("Deserializer is struct (value type)", desType.IsValueType);

// --- EnableMaskCreator ---
var emcMethods = typeof(EnableMaskCreator).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
var emcNames = emcMethods.Select(m => m.Name).ToList();
t("EnableMaskCreator has Create", emcNames.Contains("Create"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
