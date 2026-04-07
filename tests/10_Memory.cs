// ============================================================================
// TEST: MemoryAllocator + MemoryLabelAllocator
// Branches: topic/MemoryAllocator, topic/MemoryLabelAllocator
// Sources: BovineLabs.Core/Memory/MemoryAllocator.cs, MemoryLabelAllocator.cs
// Run: cat 10_Memory.cs | unity-cli exec --usings "BovineLabs.Core.Memory,Unity.Collections,System.Linq"
// ============================================================================
// MemoryAllocator: native memory allocator wrapping UnsafeUtility.Malloc/Free.
// Allocate(int size, int alignment, int label) returns void*.
// MemoryLabelAllocator: same but with debug labels for tracking allocations.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- MemoryAllocator ---
var ma = new MemoryAllocator();
t("MemoryAllocator: can instantiate", true);
var maMethods = typeof(MemoryAllocator).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
var maNames = maMethods.Select(m => m.Name).ToList();
t("MemoryAllocator has Allocate", maNames.Contains("Allocate"));
t("MemoryAllocator has Dispose", maNames.Contains("Dispose"));

// Verify Allocate signature: returns void*, takes (int size, int alignment, int label)
var allocMethod = maMethods.First(m => m.Name == "Allocate");
t("MemoryAllocator.Allocate returns void*", allocMethod.ReturnType.Name == "Void*");
t("MemoryAllocator.Allocate takes 3 params", allocMethod.GetParameters().Length == 3);

// --- MemoryLabelAllocator: uses Try(Block&) for scoped allocations ---
var mla = new MemoryLabelAllocator();
t("MemoryLabelAllocator: can instantiate", true);
var mlaMethods = typeof(MemoryLabelAllocator).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
var mlaNames = mlaMethods.Select(m => m.Name).ToList();
t("MemoryLabelAllocator has Try", mlaNames.Contains("Try"));
t("MemoryLabelAllocator has Dispose", mlaNames.Contains("Dispose"));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
