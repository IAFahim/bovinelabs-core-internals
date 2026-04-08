// Run: cat snippets/core-collections/NativeLinearCongruentialGenerator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices"
// Verifies: docs/NativeLinearCongruentialGenerator.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Claim: NativeLinearCongruentialGenerator type exists ---
var type = typeof(BovineLabs.Core.Collections.NativeLinearCongruentialGenerator);
t("Type exists", type != null);
t("Is a struct (ValueType)", type.IsValueType && !type.IsEnum);

// --- Claim: Implements IDisposable ---
t("Implements IDisposable", typeof(System.IDisposable).IsAssignableFrom(type));

// --- Claim: Has Next() method returning int ---
var nextMethod = type.GetMethod("Next", BindingFlags.Public | BindingFlags.Instance);
t("Has Next() method", nextMethod != null);
if (nextMethod != null)
    t("Next() returns int", nextMethod.ReturnType == typeof(int));

// --- Claim: Has Dispose method ---
var disposeMethod = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
t("Has Dispose() method", disposeMethod != null);

// --- Claim: Constructor takes (int seed, Allocator allocator) ---
var ctor = type.GetConstructor(new[] { typeof(int), typeof(Unity.Collections.Allocator) });
t("Constructor takes (int, Allocator)", ctor != null);

// --- Claim: Has int* current field (state pointer) ---
var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
var currentField = fields.FirstOrDefault(f => f.Name.Contains("current"));
t("Has 'current' field", currentField != null);
if (currentField != null)
    t("'current' field is int pointer", currentField.FieldType == typeof(int*));

// --- Claim: Has allocatorLabel field ---
var allocField = fields.FirstOrDefault(f => f.Name.Contains("allocatorLabel"));
t("Has 'allocatorLabel' field", allocField != null);

// --- Claim: Constants are Turbo Pascal LCG ---
// Multiplier = 134775813, Increment = 1, Modulus = int.MaxValue (0x7FFFFFFF)
// Formula: (134775813 * x + 1) & 0x7FFFFFFF

// --- Verify the LCG formula directly ---
long manualCalc = ((long)134775813 * 42 + 1) & 0x7FFFFFFF;
t("LCG formula: (134775813*42+1)&0x7FFFFFFF = 1365616851", manualCalc == 1365616851);

// --- DOC INACCURACY: Doc claims Seed=42 first output = 1312714347 ---
// Actual: (134775813 * 42 + 1) & 0x7FFFFFFF = 5660584147 & 0x7FFFFFFF = 1365616851
// The doc's example table has INCORRECT output values.

var rng = new BovineLabs.Core.Collections.NativeLinearCongruentialGenerator(42, Unity.Collections.Allocator.Temp);
int val1 = rng.Next();
t($"Seed=42 Next() #1 = 1365616851 (got {val1})", val1 == 1365616851);

int val2 = rng.Next();
long expected2 = ((long)134775813 * val1 + 1) & 0x7FFFFFFF;
t($"Seed=42 Next() #2 matches formula (got {val2}, expected {expected2})", val2 == (int)expected2);

int val3 = rng.Next();
long expected3 = ((long)134775813 * val2 + 1) & 0x7FFFFFFF;
t($"Seed=42 Next() #3 matches formula (got {val3}, expected {expected3})", val3 == (int)expected3);

// --- Claim: Deterministic (same seed = same sequence) ---
rng.Dispose();
var rng2 = new BovineLabs.Core.Collections.NativeLinearCongruentialGenerator(42, Unity.Collections.Allocator.Temp);
int det1 = rng2.Next();
int det2 = rng2.Next();
t("Deterministic: same seed same sequence", det1 == 1365616851 && det2 == val2);
rng2.Dispose();

// --- Claim: Values always non-negative (< int.MaxValue = 0x7FFFFFFF) ---
var rng3 = new BovineLabs.Core.Collections.NativeLinearCongruentialGenerator(1, Unity.Collections.Allocator.Temp);
bool allNonNeg = true;
for (int i = 0; i < 100; i++)
{
    int v = rng3.Next();
    if (v < 0 || v > 0x7FFFFFFF)
    {
        allNonNeg = false;
        break;
    }
}
t("All 100 values in [0, 0x7FFFFFFF] range", allNonNeg);
rng3.Dispose();

// --- Claim: Struct size info ---
int structSize = System.Runtime.InteropServices.Marshal.SizeOf(type);
r.Add($"INFO: Struct size = {structSize} bytes (doc: ~8+2+16 debug safety)");

r.Add($"\nDOC INACCURACY: Example table for seed=42 shows incorrect outputs.");
r.Add($"  Doc says first output = 1312714347, actual = 1365616851");

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
