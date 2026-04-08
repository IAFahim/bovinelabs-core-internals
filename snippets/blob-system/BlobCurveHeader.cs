// Run: cat snippets/blob-system/BlobCurveHeader.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobCurveHeader.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var hdrType = typeof(BlobCurveHeader);
t("BlobCurveHeader: type exists", hdrType != null);
t("BlobCurveHeader: is struct", hdrType.IsValueType);

// Claims: WrapModePrev and WrapModePost are WrapMode (short enum)
t("BlobCurveHeader: has WrapModePrev", hdrType.GetField("WrapModePrev") != null);
t("BlobCurveHeader: has WrapModePost", hdrType.GetField("WrapModePost") != null);

// Claims: WrapMode enum with Clamp=0, Loop=1, PingPong=2
var wrapModeEnum = hdrType.GetNestedType("WrapMode");
t("BlobCurveHeader: has WrapMode enum", wrapModeEnum != null);
t("BlobCurveHeader: WrapMode is enum", wrapModeEnum?.IsEnum ?? false);
if (wrapModeEnum != null)
{
    t("BlobCurveHeader: WrapMode.Clamp == 0", Convert.ToInt32(Enum.Parse(wrapModeEnum, "Clamp")) == 0);
    t("BlobCurveHeader: WrapMode.Loop == 1", Convert.ToInt32(Enum.Parse(wrapModeEnum, "Loop")) == 1);
    t("BlobCurveHeader: WrapMode.PingPong == 2", Convert.ToInt32(Enum.Parse(wrapModeEnum, "PingPong")) == 2);
    // Claims: underlying type is short
    t("BlobCurveHeader: WrapMode underlying type is short", Enum.GetUnderlyingType(wrapModeEnum) == typeof(short));
}

// Claims: SegmentCount is int
t("BlobCurveHeader: has SegmentCount", hdrType.GetField("SegmentCount") != null);
t("BlobCurveHeader: SegmentCount is int", hdrType.GetField("SegmentCount")?.FieldType == typeof(int));

// Claims: StartTime, EndTime are float
t("BlobCurveHeader: has StartTime", hdrType.GetField("StartTime") != null);
t("BlobCurveHeader: StartTime is float", hdrType.GetField("StartTime")?.FieldType == typeof(float));
t("BlobCurveHeader: has EndTime", hdrType.GetField("EndTime") != null);
t("BlobCurveHeader: EndTime is float", hdrType.GetField("EndTime")?.FieldType == typeof(float));

// Claims: Times is BlobArray<float>
var timesField = hdrType.GetField("Times", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobCurveHeader: has Times field", timesField != null);

// Claims: Duration property
t("BlobCurveHeader: has Duration property", hdrType.GetProperty("Duration") != null);
t("BlobCurveHeader: Duration is float", hdrType.GetProperty("Duration")?.PropertyType == typeof(float));

// Claims: Search methods exist
var searchMethods = hdrType.GetMethods().Where(m => m.Name == "Search").ToArray();
t("BlobCurveHeader: has Search overloads", searchMethods.Length >= 2);

var searchIgnore = hdrType.GetMethods().Where(m => m.Name == "SearchIgnoreWrapMode").ToArray();
t("BlobCurveHeader: has SearchIgnoreWrapMode overloads", searchIgnore.Length >= 2);

// Search returns int (segment index)
t("BlobCurveHeader: Search returns int", searchMethods.Length > 0 && searchMethods[0].ReturnType == typeof(int));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
