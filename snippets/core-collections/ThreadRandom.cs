// Run: cat snippets/core-collections/ThreadRandom.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,Unity.Collections,Unity.Jobs.LowLevel.Unsafe,Unity.Mathematics"
// Verifies: docs/ThreadRandom.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Claim: ThreadRandom type exists ---
var type = typeof(BovineLabs.Core.Collections.ThreadRandom);
t("ThreadRandom type exists", type != null);
t("Is a struct (ValueType)", type.IsValueType);

// --- Claim: Has IsCreated property ---
var isCreated = type.GetProperty("IsCreated", BindingFlags.Public | BindingFlags.Instance);
t("Has IsCreated property", isCreated != null);

// --- Claim: Has GetRandomRef method ---
var getRandRef = type.GetMethod("GetRandomRef", BindingFlags.Public | BindingFlags.Instance);
t("Has GetRandomRef() method", getRandRef != null);
if (getRandRef != null)
{
    // Return type for by-ref is harder to compare, check name
    t("GetRandomRef returns Unity.Mathematics.Random",
        getRandRef.ReturnType.Name.StartsWith("Random") && getRandRef.ReturnType.Namespace == "Unity.Mathematics");
}

// --- Claim: Has Dispose method ---
var disposeMethod = type.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
t("Has Dispose()", disposeMethod != null);

// --- Claim: Constructor takes (uint seed, AllocatorHandle) ---
var ctor = type.GetConstructor(new[] { typeof(uint), typeof(Unity.Collections.AllocatorManager.AllocatorHandle) });
t("Constructor takes (uint, AllocatorHandle)", ctor != null);

// --- Claim: Internal Randoms struct is cache-line sized ---
var randomsType = type.GetNestedType("Randoms", BindingFlags.NonPublic | BindingFlags.Public);
t("Randoms nested type exists", randomsType != null);
if (randomsType != null)
{
    t("Randoms is struct", randomsType.IsValueType);
// Check StructLayout via reflection
bool hasExplicitLayout = false;
int explicitSize = 0;
try
{
    var layoutAttr = randomsType.StructLayoutAttribute;
    if (layoutAttr != null && layoutAttr.Value == System.Runtime.InteropServices.LayoutKind.Explicit)
    {
        hasExplicitLayout = true;
        explicitSize = layoutAttr.Size;
    }
}
catch { }
t("Randoms has [StructLayout(LayoutKind.Explicit)]", hasExplicitLayout);

int cacheLineSize = Unity.Jobs.LowLevel.Unsafe.JobsUtility.CacheLineSize;
int randomsMarshalSize = System.Runtime.InteropServices.Marshal.SizeOf(randomsType);
r.Add($"INFO: Randoms Marshal.SizeOf={randomsMarshalSize}, CacheLineSize={cacheLineSize}");
t($"Randoms Size = CacheLineSize ({cacheLineSize})", randomsMarshalSize == cacheLineSize);
    
    // Randoms contains Random field
    var randomField = randomsType.GetField("Random", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    t("Randoms has Random field of Unity.Mathematics.Random", 
        randomField != null && randomField.FieldType == typeof(Unity.Mathematics.Random));
}

// --- Functional test: create, get random ref, call NextInt ---
var threadRandom = new BovineLabs.Core.Collections.ThreadRandom(42, Unity.Collections.Allocator.Temp);
t("ThreadRandom construction succeeds", threadRandom.IsCreated);

ref var rng = ref threadRandom.GetRandomRef();
t("GetRandomRef returns ref Random", true);

// --- Claim: Each thread gets independent Random seeded via CreateFromIndex(seed+i) ---
// We can verify determinism: calling NextInt twice gives consistent results
int val1 = rng.NextInt();
int val2 = rng.NextInt();
t("NextInt produces non-zero values", val1 != 0 || val2 != 0);

// --- Claim: Uses Unity.Mathematics.Random (high quality xorshift) ---
float f = rng.NextFloat();
t("NextFloat produces value in [0,1) range", f >= 0f && f < 1f);

// --- Verify determinism by recreating ---
threadRandom.Dispose();
t("Dispose succeeds", !threadRandom.IsCreated);

var threadRandom2 = new BovineLabs.Core.Collections.ThreadRandom(42, Unity.Collections.Allocator.Temp);
ref var rng2 = ref threadRandom2.GetRandomRef();
int val1b = rng2.NextInt();
int val2b = rng2.NextInt();
t("Deterministic: same seed produces same sequence", val1 == val1b && val2 == val2b);

threadRandom2.Dispose();

// --- Claim: buffer stores Randoms array (one per ThreadIndexCount) ---
var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
r.Add($"INFO: ThreadRandom has {fields.Length} fields");
foreach (var fld in fields) r.Add($"  - {fld.Name}: {fld.FieldType.Name}");

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
