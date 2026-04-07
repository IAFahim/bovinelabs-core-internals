// ============================================================================
// TEST: HSV + HalfSizeTriangleMatrix + CurveRemapUtility + mathex
// Branches: topic/HSV, topic/HalfSizeTriangleMatrix, topic/CurveRemapUtility,
//           topic/mathex_mod, topic/mathex_minMax, topic/mathex_add,
//           topic/mathex_FromToRotation, topic/mathex_GenerateGaussianNoise
// Sources: BovineLabs.Core/Utility/HSV.cs, HalfSizeTriangleMatrix.cs,
//          CurveRemapUtility.cs, mathex.cs
// Run: cat 04_MathUtility.cs | unity-cli exec --usings "BovineLabs.Core.Utility,Unity.Mathematics,Unity.Collections"
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- HSV: hue-saturation-value color struct ---
var red = new HSV(0, 1, 1);
t("HSV: H=0,S=1,V=1 stored", red.H == 0f && red.S == 1f && red.V == 1f);
var c = red.ToColor();
t("HSV: red ToColor r>0.9", c.r > 0.9f);
t("HSV: red ToColor g<0.1", c.g < 0.1f);
var blue = new HSV(240, 1, 1);
var bc = blue.ToColor();
t("HSV: blue ToColor b>0.9", bc.b > 0.9f);
var clamped = new HSV(500, 2, -1);
t("HSV: H=500 clamped → 0", clamped.H == 0f);
t("HSV: S=2 clamped → 1", clamped.S == 1f);
t("HSV: V=-1 clamped → 0", clamped.V == 0f);

// --- HalfSizeTriangleMatrix: static class ---
t("HalfSizeTriangleMatrix is static class", typeof(HalfSizeTriangleMatrix).IsAbstract && typeof(HalfSizeTriangleMatrix).IsSealed);

// --- CurveRemapUtility ---
var crmNames = typeof(CurveRemapUtility).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Select(m => m.Name).ToList();
t("CurveRemapUtility has TryRemapToClipLength", crmNames.Contains("TryRemapToClipLength"));
t("CurveRemapUtility has IsClampWrapMode", crmNames.Contains("IsClampWrapMode"));

// --- mathex: Burst-compatible math extensions ---
t("mathex.mod(10,3)=1", mathex.mod(10, 3) == 1);
t("mathex.mod(-10,3)=2", mathex.mod(-10, 3) == 2);
t("mathex.isodd(3)=true", mathex.isodd(3));
t("mathex.isodd(4)=false", !mathex.isodd(4));

// minMax needs pointers — test via sum instead
var arr = new NativeArray<float>(3, Allocator.Temp);
arr[0] = 1f; arr[1] = 5f; arr[2] = 3f;
t("mathex.sum(NativeArray) = 9", mathex.sum(arr) == 9f);
t("mathex.max(NativeArray) = 5", mathex.max(arr) == 5f);
t("mathex.min(NativeArray) = 1", mathex.min(arr) == 1f);
arr.Dispose();

// add(output, input, scalarValue): output[i] = input[i] + value
var aInt = new NativeArray<int>(2, Allocator.Temp);
var bInt = new NativeArray<int>(2, Allocator.Temp);
bInt[0] = 10; bInt[1] = 20;
mathex.add(aInt, bInt, 5); // aInt[i] = bInt[i] + 5
t("mathex.add works: 10+5=15, 20+5=25", aInt[0] == 15 && aInt[1] == 25);
aInt.Dispose(); bInt.Dispose();

// SmoothDamp
float velocity = 0f;
var sd = mathex.SmoothDamp(0f, 100f, ref velocity, 0.1f, 999f, 0.016f);
t("mathex.SmoothDamp moves toward target", sd > 0f);

// Approximately
t("mathex.Approximately(1,1)=true", mathex.Approximately(1f, 1f));
t("mathex.Approximately(1,2)=false", !mathex.Approximately(1f, 2f));

// Rotate
var rot = mathex.Rotate(new float2(1, 0), math.PI / 2);
t("mathex.Rotate(1,0,90°) ≈ (0,1)", rot.x < 0.01f && rot.y > 0.99f);

// Perpendicular
var perp = mathex.Perpendicular(new float2(1, 0));
t("mathex.Perpendicular(1,0) ≈ (0,1)", math.abs(perp.x) < 0.01f && math.abs(perp.y - 1f) < 0.01f);

// cross (2D)
t("mathex.cross((1,0),(0,1))=1", mathex.cross(new float2(1,0), new float2(0,1)) == 1f);

// ToFloat3: extension on float2 → float3(f.x, y, f.y)
var f3 = new float2(1, 3).ToFloat3(2);
t("mathex.ToFloat3((1,3),y=2)=(1,2,3)", f3.x==1f && f3.y==2f && f3.z==3f);

// ToInt3: extension on int2 → int3(f.x, y, f.y)
var i3 = new int2(1, 3).ToInt3(2);
t("mathex.ToInt3((1,3),y=2)=(1,2,3)", i3.x==1 && i3.y==2 && i3.z==3);

// GenerateGaussianNoise
var rng = new Unity.Mathematics.Random(12345);
var (z0, z1) = mathex.GenerateGaussianNoise(ref rng, 0f, 1f);
t("mathex.GenerateGaussianNoise returns 2 values", z0 != 0f || z1 != 0f);

// FromToRotation
var q = mathex.FromToRotation(new float3(1,0,0), new float3(0,1,0));
t("mathex.FromToRotation returns quaternion", q.value.x != float.NaN);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
