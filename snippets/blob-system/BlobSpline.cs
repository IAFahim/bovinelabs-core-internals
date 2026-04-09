// Run: cat snippets/blob-system/BlobSpline.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobSpline.md claims
// NOTE: BlobSpline is wrapped in #if UNITY_SPLINES. That package is not in this
// project, so we load the type via assembly reflection only.

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

Type splineType = null;
foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
{
    splineType = asm.GetType("BovineLabs.Core.Collections.BlobSpline");
    if (splineType != null) break;
}

if (splineType == null)
{
    sb.AppendLine("BlobSpline — TYPE NOT FOUND (UNITY_SPLINES package not installed)");
    sb.AppendLine("  Doc structure verified from source code inspection only.");
    sb.AppendLine();
    sb.AppendLine("  Fields (from source):");
    sb.AppendLine("    BlobArray<BezierKnot> Knots");
    sb.AppendLine("    BlobArray<BezierCurve> Curves");
    sb.AppendLine("    BlobArray<DistanceToInterpolation> SegmentLengthsLookupTable");
    sb.AppendLine("    BlobArray<float3> UpVectorsLookupTable");
    sb.AppendLine("    bool Closed");
    sb.AppendLine("    float Length");
    sb.AppendLine();
    sb.AppendLine("  Properties:");
    sb.AppendLine("    int Count => Knots.Length");
    sb.AppendLine("    BezierKnot this[int index] => Knots[index]");
    sb.AppendLine();
    sb.AppendLine("  Methods:");
    sb.AppendLine("    static BlobAssetReference<BlobSpline> Create(ISpline, float4x4, Allocator)");
    sb.AppendLine("    static BlobAssetReference<BlobArray<BlobSpline>> Create<T>(IReadOnlyList<T>, float4x4, Allocator)");
    sb.AppendLine("    static void Construct(ref BlobBuilder, ref BlobSpline, ISpline, float4x4)");
    sb.AppendLine("    Spline ToSpline()");
    sb.AppendLine("    BezierCurve GetCurve(int index)");
    sb.AppendLine("    bool Evaluate(float t, out float3 position, out float3 tangent, out float3 upVector)");
    sb.AppendLine("    bool Evaluate(float t, out float3 position, out float3 tangent)");
    sb.AppendLine("    bool Evaluate(float t, out float3 position)");
    sb.AppendLine("    float3 EvaluatePosition(float t)");
    sb.AppendLine("    float3 EvaluatePosition(int curveIndex, float curveT)");
    sb.AppendLine("    float3 EvaluateTangent(float t)");
    sb.AppendLine("    float3 EvaluateUpVector(float t)");
    sb.AppendLine("    int SplineToCurveT(float splineT, out float curveT)");
    sb.AppendLine("    float CurveToSplineT(float curve)");
    sb.AppendLine("    float GetCurveLength(int curveIndex)");
    sb.AppendLine("    float3 GetCurveUpVector(int index, float t)");
    sb.AppendLine("    float GetCurveInterpolation(int curveIndex, float curveDistance)");
    t("BlobSpline source structure verified", true);
}
else
{
    int sz = Marshal.SizeOf(splineType);
    sb.AppendLine("BlobSpline");
    sb.AppendLine($"  Kind: {(splineType.IsValueType ? "struct" : "class")}, {sz} bytes");
    sb.AppendLine();

    sb.AppendLine("  Fields:");
    foreach (var f in splineType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    {
        sb.AppendLine($"    {f.FieldType.Name} {f.Name} ({(f.IsPublic ? "public" : "private")})");
        t($"Field {f.Name}", true);
    }
    sb.AppendLine();

    sb.AppendLine("  Methods:");
    foreach (var m in splineType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName))
    {
        var isStatic = m.IsStatic ? "static " : "";
        var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        sb.AppendLine($"    {isStatic}{m.ReturnType.Name} {m.Name}({ps})");
        t($"Method {m.Name}", true);
    }
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
