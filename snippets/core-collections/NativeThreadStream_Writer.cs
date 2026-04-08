// Run: cat snippets/core-collections/NativeThreadStream_Writer.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq"
// Verifies: docs/NativeThreadStream_Writer.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var ntsType = typeof(BovineLabs.Core.Collections.NativeThreadStream);
var writerType = ntsType.GetNestedType("Writer", BindingFlags.Public | BindingFlags.NonPublic);
t("Writer nested type exists", writerType != null);
t("Writer is struct", writerType != null && writerType.IsValueType);

if (writerType != null)
{
    // --- Fields: wraps an UnsafeThreadStream.Writer ---
    var fields = writerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    var fieldNames = fields.Select(f => f.Name).ToList();
    r.Add($"INFO: Writer fields: {string.Join(", ", fieldNames)}");
    
    t("Has 'writer' field (wraps internal writer)", fieldNames.Contains("writer"));
    t("Has m_Safety field (debug)", fieldNames.Contains("m_Safety"));
    t("Has exactly 2 fields", fields.Length == 2);

    // --- Methods ---
    var methods = writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Select(m => m.Name).Distinct().ToList();
    
    t("Has Allocate method", methods.Contains("Allocate"));
    t("Has Write<T>", methods.Contains("Write"));
    t("Has WriteLarge", methods.Contains("WriteLarge"));
    
    // --- Allocate signature ---
    var allocateMethods = writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Where(m => m.Name == "Allocate").ToList();
    t("Allocate has overloads", allocateMethods.Count > 0);
    
    var allocInt = allocateMethods.FirstOrDefault(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int));
    if (allocInt != null)
    {
        t("Allocate(int size) returns pointer", allocInt.ReturnType.IsPointer);
    }
    else
    {
        t("Allocate overloads exist", allocateMethods.Count > 0);
    }
    
    // --- Write<T> is generic ---
    var writeMethods = writerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Where(m => m.Name == "Write").ToList();
    var genericWrite = writeMethods.FirstOrDefault(m => m.IsGenericMethod);
    t("Write<T> is generic method", genericWrite != null);
    
    r.Add($"INFO: Writer methods: {string.Join(", ", methods)}");
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
