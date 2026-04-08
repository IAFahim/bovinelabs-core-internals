// Run: cat snippets/core-collections/FixedArray.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,Unity.Mathematics"
// Verifies: docs/FixedArray.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var faType = typeof(BovineLabs.Core.Collections.FixedArray<,>);
t("FixedArray: open generic type exists", faType != null);
t("FixedArray: is a struct (ValueType)", faType.IsValueType);

// --- FixedArray<float, float4> ---
var faFloat4 = typeof(BovineLabs.Core.Collections.FixedArray<float, Unity.Mathematics.float4>);
t("FixedArray<float,float4>: specialized type exists", faFloat4 != null);
t("FixedArray<float,float4>: is ValueType", faFloat4.IsValueType);

// --- Check Length property ---
var lengthProp = faFloat4.GetProperty("Length");
t("FixedArray<float,float4>: has Length property", lengthProp != null);

// --- Create instance and check Length = sizeof(float4)/sizeof(float) = 16/4 = 4 ---
var fa = Activator.CreateInstance(faFloat4);
if (lengthProp != null)
{
    int len = (int)lengthProp.GetValue(fa);
    t("FixedArray<float,float4>: Length == 4", len == 4);
}
else { t("FixedArray<float,float4>: Length == 4", false); }

// --- Indexer: set/get ---
var indexer = faFloat4.GetProperty("Item");
t("FixedArray<float,float4>: has indexer (Item property)", indexer != null);
if (indexer != null)
{
    var setMethod = indexer.GetSetMethod();
    var getMethod = indexer.GetGetMethod();
    t("FixedArray<float,float4>: indexer has getter", getMethod != null);
    t("FixedArray<float,float4>: indexer has setter", setMethod != null);

    if (setMethod != null && getMethod != null)
    {
        setMethod.Invoke(fa, new object[] { 0, 1.5f });
        setMethod.Invoke(fa, new object[] { 3, 42.0f });
        float val0 = (float)getMethod.Invoke(fa, new object[] { 0 });
        float val3 = (float)getMethod.Invoke(fa, new object[] { 3 });
        t("FixedArray<float,float4>: [0] = 1.5 after set", val0 == 1.5f);
        t("FixedArray<float,float4>: [3] = 42.0 after set", val3 == 42.0f);
    }
}
else
{
    t("FixedArray<float,float4>: indexer has getter", false);
    t("FixedArray<float,float4>: indexer has setter", false);
    t("FixedArray<float,float4>: [0] = 1.5 after set", false);
    t("FixedArray<float,float4>: [3] = 42.0 after set", false);
}

// --- ElementAt method ---
var elementAtMethod = faFloat4.GetMethod("ElementAt");
t("FixedArray<float,float4>: has ElementAt method", elementAtMethod != null);

// --- FixedArray<int, ulong> → Length = sizeof(ulong)/sizeof(int) = 8/4 = 2 ---
var faIntULong = typeof(BovineLabs.Core.Collections.FixedArray<int, ulong>);
t("FixedArray<int,ulong>: specialized type exists", faIntULong != null);
var faIntULongLenProp = faIntULong.GetProperty("Length");
if (faIntULongLenProp != null)
{
    var fa2 = Activator.CreateInstance(faIntULong);
    int len2 = (int)faIntULongLenProp.GetValue(fa2);
    t("FixedArray<int,ulong>: Length == 2", len2 == 2);
}
else { t("FixedArray<int,ulong>: Length == 2", false); }

// --- FixedArray<byte, float4> → Length = sizeof(float4)/sizeof(byte) = 16/1 = 16 ---
// Doc says float4 = 16 bytes, so 16/1 = 16
var faByteF4 = typeof(BovineLabs.Core.Collections.FixedArray<byte, Unity.Mathematics.float4>);
t("FixedArray<byte,float4>: specialized type exists", faByteF4 != null);
var faByteF4LenProp = faByteF4.GetProperty("Length");
if (faByteF4LenProp != null)
{
    var fa3 = Activator.CreateInstance(faByteF4);
    int len3 = (int)faByteF4LenProp.GetValue(fa3);
    t("FixedArray<byte,float4>: Length == 16", len3 == 16);
}
else { t("FixedArray<byte,float4>: Length == 16", false); }

// --- FixedArray<byte, Unity.Mathematics.float4x4> → Length = 64/1 = 64 ---
var faByteF4x4 = typeof(BovineLabs.Core.Collections.FixedArray<byte, Unity.Mathematics.float4x4>);
t("FixedArray<byte,float4x4>: specialized type exists", faByteF4x4 != null);
var faByteF4x4LenProp = faByteF4x4.GetProperty("Length");
if (faByteF4x4LenProp != null)
{
    var fa4 = Activator.CreateInstance(faByteF4x4);
    int len4 = (int)faByteF4x4LenProp.GetValue(fa4);
    t("FixedArray<byte,float4x4>: Length == 64", len4 == 64);
}
else { t("FixedArray<byte,float4x4>: Length == 64", false); }

// --- FixedArray<int, ulong> functional test ---
var faIUL = faIntULong;
var iulIndexer = faIUL.GetProperty("Item");
var iulSet = iulIndexer?.GetSetMethod();
var iulGet = iulIndexer?.GetGetMethod();
if (iulSet != null && iulGet != null)
{
    var faIULInst = Activator.CreateInstance(faIUL);
    iulSet.Invoke(faIULInst, new object[] { 0, 10 });
    iulSet.Invoke(faIULInst, new object[] { 1, 20 });
    int v0 = (int)iulGet.Invoke(faIULInst, new object[] { 0 });
    int v1 = (int)iulGet.Invoke(faIULInst, new object[] { 1 });
    t("FixedArray<int,ulong>: [0] = 10", v0 == 10);
    t("FixedArray<int,ulong>: [1] = 20", v1 == 20);
}
else
{
    t("FixedArray<int,ulong>: [0] = 10", false);
    t("FixedArray<int,ulong>: [1] = 20", false);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
