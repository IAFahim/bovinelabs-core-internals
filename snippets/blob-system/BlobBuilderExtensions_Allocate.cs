// Run: cat snippets/blob-system/BlobBuilderExtensions_Allocate.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobBuilderExtensions_Allocate.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var extType = typeof(BlobBuilderExtensions);

// Claims: Allocate(this ref BlobBuilder blobBuilder, int size) -> void*
var allocOverloads = extType.GetMethods().Where(m => m.Name == "Allocate").ToArray();
t("Allocate: has Allocate overloads", allocOverloads.Length >= 1);

// Find the raw byte allocation overload
var allocRaw = allocOverloads.FirstOrDefault(m => {
    var p = m.GetParameters();
    return p.Length == 1 && p[0].ParameterType == typeof(Unity.Entities.BlobBuilder).MakeByRefType();
});
t("Allocate: has Allocate(ref BlobBuilder, int)", allocRaw != null);

// Claims: GetListPtr returns IntPtr
var getListPtr = extType.GetMethod("GetListPtr");
t("Allocate: has GetListPtr", getListPtr != null);
t("Allocate: GetListPtr returns IntPtr", getListPtr?.ReturnType == typeof(IntPtr));

// Claims: BlobBuilderInternal has nested types
var nestedBlobAlloc = extType.GetNestedType("BlobBuilderInternal", BindingFlags.NonPublic);
t("Allocate: BlobBuilderInternal exists", nestedBlobAlloc != null);

if (nestedBlobAlloc != null)
{
    // Claims: BlobAllocation struct has Size (int) and P (byte*)
    var blobAllocType = nestedBlobAlloc.GetNestedType("BlobAllocation");
    t("Allocate: BlobAllocation nested type exists", blobAllocType != null);
    if (blobAllocType != null)
    {
        t("Allocate: BlobAllocation has Size field", blobAllocType.GetField("Size") != null);
        t("Allocate: BlobAllocation has P field", blobAllocType.GetField("P") != null);
    }

    // Claims: BlobDataRef has AllocIndex and Offset
    var blobDataRef = nestedBlobAlloc.GetNestedType("BlobDataRef");
    t("Allocate: BlobDataRef nested type exists", blobDataRef != null);
    if (blobDataRef != null)
    {
        t("Allocate: BlobDataRef has AllocIndex", blobDataRef.GetField("AllocIndex") != null);
        t("Allocate: BlobDataRef has Offset", blobDataRef.GetField("Offset") != null);
    }

    // Claims: OffsetPtrPatch has OffsetPtr, Target, Length
    var offsetPtrPatch = nestedBlobAlloc.GetNestedType("OffsetPtrPatch");
    t("Allocate: OffsetPtrPatch nested type exists", offsetPtrPatch != null);
    if (offsetPtrPatch != null)
    {
        t("Allocate: OffsetPtrPatch has OffsetPtr", offsetPtrPatch.GetField("OffsetPtr") != null);
        t("Allocate: OffsetPtrPatch has Target", offsetPtrPatch.GetField("Target") != null);
        t("Allocate: OffsetPtrPatch has Length", offsetPtrPatch.GetField("Length") != null);
    }
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
