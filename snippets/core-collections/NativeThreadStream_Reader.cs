// Run: cat snippets/core-collections/NativeThreadStream_Reader.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq"
// Verifies: docs/NativeThreadStream_Reader.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);

// --- Reader is a nested type ---
var readerType = ntsType.GetNestedType("Reader", BindingFlags.Public | BindingFlags.NonPublic);
t("Reader nested type exists", readerType != null);
t("Reader is struct", readerType != null && readerType.IsValueType);

if (readerType != null)
{
    // --- Fields: wraps an UnsafeThreadStream.Reader ---
    var fields = readerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    var fieldNames = fields.Select(f => f.Name).ToList();
    r.Add($"INFO: Reader fields: {string.Join(", ", fieldNames)}");
    
    t("Has 'reader' field (wraps internal reader)", fieldNames.Contains("reader"));
    t("Has remainingBlocks field", fieldNames.Contains("remainingBlocks"));
    t("Has m_Safety field (debug)", fieldNames.Contains("m_Safety"));
    t("Has exactly 3 fields", fields.Length == 3);

    // --- Methods ---
    var methods = readerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Select(m => m.Name).Distinct().ToList();
    
    t("Has BeginForEachIndex", methods.Contains("BeginForEachIndex"));
    t("Has EndForEachIndex", methods.Contains("EndForEachIndex"));
    t("Has Read<T>", methods.Contains("Read"));
    t("Has ReadUnsafePtr", methods.Contains("ReadUnsafePtr"));
    t("Has ReadLarge", methods.Contains("ReadLarge"));
    t("Has Count method", methods.Contains("Count"));
    
    // --- Properties ---
    var remainingProp = readerType.GetProperty("RemainingItemCount", BindingFlags.Public | BindingFlags.Instance);
    t("Has RemainingItemCount property", remainingProp != null);
    
    var fecProp = readerType.GetProperty("ForEachCount", BindingFlags.Public | BindingFlags.Instance);
    t("Has ForEachCount property", fecProp != null);
    
    r.Add($"INFO: Reader methods: {string.Join(", ", methods)}");
}

// --- Writer nested type ---
var writerType = ntsType.GetNestedType("Writer", BindingFlags.Public | BindingFlags.NonPublic);
t("Writer nested type exists", writerType != null);
t("Writer is struct", writerType != null && writerType.IsValueType);

if (writerType != null)
{
    var wMethods = writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Select(m => m.Name).Distinct().ToList();
    t("Writer has Write<T>", wMethods.Contains("Write"));
    t("Writer has Allocate", wMethods.Contains("Allocate"));
    t("Writer has WriteLarge", wMethods.Contains("WriteLarge"));
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
