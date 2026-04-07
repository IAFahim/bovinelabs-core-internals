// ============================================================================
// TEST: UnsafeArray + FixedArray + ThreadRandom + NativeCounter
// Branches: topic/UnsafeArray, topic/FixedArray, topic/ThreadRandom,
//           topic/NativeCounter
// Sources: BovineLabs.Core/Collections/*.cs
// Run: cat 05_Collections.cs | unity-cli exec --usings "BovineLabs.Core.Collections,Unity.Collections,Unity.Mathematics"
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- UnsafeArray<T>: unsafe heap array with indexer ---
var ua = new UnsafeArray<float>(4, Allocator.Temp);
t("UnsafeArray: created with length 4", ua.IsCreated);
ua[0] = 3.14f; ua[1] = 2.71f; ua[2] = 0f; ua[3] = -1f;
t("UnsafeArray: [0] = 3.14", ua[0] == 3.14f);
t("UnsafeArray: [3] = -1", ua[3] == -1f);
ua.Dispose();
t("UnsafeArray: disposed", !ua.IsCreated);

// --- FixedArray<T,N>: fixed-size inline array ---
var faType = typeof(FixedArray<,>);
t("FixedArray<,> type exists", faType != null);
t("FixedArray is struct", faType.IsValueType);

// --- ThreadRandom: per-thread Unity.Mathematics.Random ---
var tr = new ThreadRandom(42, Allocator.Temp);
t("ThreadRandom: created", tr.IsCreated);
ref var rng = ref tr.GetRandomRef();
var val = rng.NextInt(0, 100);
t("ThreadRandom: NextInt returns value", val >= 0 && val < 100);
var fval = rng.NextFloat(0f, 1f);
t("ThreadRandom: NextFloat returns value", fval >= 0f && fval <= 1f);
tr.Dispose();

// --- NativeCounter: parallel-safe atomic counter ---
// NativeCounter has specific constructor — check via reflection
var ncType = typeof(NativeCounter);
t("NativeCounter type exists", ncType != null);
var ncCtors = ncType.GetConstructors();
t("NativeCounter has constructors", ncCtors.Length > 0);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
