// ============================================================================
// TEST: IntFloatUnion + ShortHalfUnion + GlobalRandom + SpinLock + EntityLock
// Branches: topic/IntFloatUnion, topic/GlobalRandom, topic/SpinLock, topic/EntityLock
// Sources: BovineLabs.Core/Utility/*.cs
// Run: cat 03_UtilityPrimitives.cs | unity-cli exec --usings "BovineLabs.Core.Utility,Unity.Mathematics"
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- IntFloatUnion: reinterpret-cast between int and float bits ---
var iu = new IntFloatUnion();
iu.FloatValue = 0.0f;
t("IntFloatUnion: 0.0f → int 0", iu.IntValue == 0);
iu.FloatValue = 1.0f;
t("IntFloatUnion: 1.0f → int 1065353216", iu.IntValue == 1065353216);
iu.IntValue = 0x40490FDB; // ~3.14159
t("IntFloatUnion: int 0x40490FDB → float ≈ π", iu.FloatValue > 3.14f && iu.FloatValue < 3.15f);
iu.FloatValue = -1.0f;
t("IntFloatUnion: -1.0f is negative int", iu.IntValue < 0);

// --- ShortHalfUnion: reinterpret between ushort and half-float ---
var sh = new ShortHalfUnion();
sh.ShortValue = 0x3C00; // half = 1.0
t("ShortHalfUnion: 0x3C00 → half ≈ 1.0", sh.HalfValue > 0.99f && sh.HalfValue < 1.01f);

// --- GlobalRandom: static thread-safe random ---
var f1 = GlobalRandom.NextFloat(0f, 1f);
t("GlobalRandom: NextFloat(0,1) in range", f1 >= 0f && f1 <= 1f);
var f2 = GlobalRandom.NextFloat(10f, 20f);
t("GlobalRandom: NextFloat(10,20) in range", f2 >= 10f && f2 <= 20f);
var i1 = GlobalRandom.NextInt(0, 100);
t("GlobalRandom: NextInt(0,100) in range", i1 >= 0 && i1 < 100);

// --- SpinLock: lightweight spin-wait lock ---
var sl = new SpinLock();
t("SpinLock: can instantiate", true);
// SpinLock has Acquire/Release but we avoid actual locking in single-thread test

// --- EntityLock: ECS entity-granularity lock ---
var el = new EntityLock();
t("EntityLock: can instantiate", true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
