// Run: cat snippets/core-collections/BitArrayUtilities.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Linq"
// Verifies: docs/BitArrayUtilities.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var utilType = typeof(BovineLabs.Core.Collections.BitArrayUtilities);
t("BitArrayUtilities: type exists", utilType != null);
t("BitArrayUtilities: is static class", utilType.IsAbstract && utilType.IsSealed);

var bf = BindingFlags.Public | BindingFlags.Static;

// --- Get methods exist ---
var get8 = utilType.GetMethod("Get8", bf);
var get16 = utilType.GetMethod("Get16", bf);
var get32 = utilType.GetMethod("Get32", bf);
var get64 = utilType.GetMethod("Get64", bf);
t("BitArrayUtilities: has Get8", get8 != null);
t("BitArrayUtilities: has Get16", get16 != null);
t("BitArrayUtilities: has Get32", get32 != null);
t("BitArrayUtilities: has Get64", get64 != null);

// Get128 may have overloads - find the right one
var get128Methods = utilType.GetMethods(bf).Where(m => m.Name == "Get128").ToArray();
t("BitArrayUtilities: has Get128 (at least 1 overload)", get128Methods.Length >= 1);

// --- Set methods exist ---
var set8 = utilType.GetMethod("Set8", bf);
var set16 = utilType.GetMethod("Set16", bf);
var set32 = utilType.GetMethod("Set32", bf);
var set64 = utilType.GetMethod("Set64", bf);
t("BitArrayUtilities: has Set8", set8 != null);
t("BitArrayUtilities: has Set16", set16 != null);
t("BitArrayUtilities: has Set32", set32 != null);
t("BitArrayUtilities: has Set64", set64 != null);

var set128Methods = utilType.GetMethods(bf).Where(m => m.Name == "Set128").ToArray();
t("BitArrayUtilities: has Set128 (at least 1 overload)", set128Methods.Length >= 1);

// --- Functional: Get32 returns true when bit is set ---
// Get32(uint index, uint data) → bool
if (get32 != null)
{
    bool result = (bool)get32.Invoke(null, new object[] { 5u, 0b100000u });
    t("BitArrayUtilities: Get32(5, 0b100000) == true", result == true);

    result = (bool)get32.Invoke(null, new object[] { 3u, 0b100000u });
    t("BitArrayUtilities: Get32(3, 0b100000) == false", result == false);

    result = (bool)get32.Invoke(null, new object[] { 0u, 1u });
    t("BitArrayUtilities: Get32(0, 1) == true", result == true);
}
else
{
    t("BitArrayUtilities: Get32(5, 0b100000) == true", false);
    t("BitArrayUtilities: Get32(3, 0b100000) == false", false);
    t("BitArrayUtilities: Get32(0, 1) == true", false);
}

// --- Functional: Get64 ---
if (get64 != null)
{
    bool result = (bool)get64.Invoke(null, new object[] { 10u, (ulong)1024 });
    t("BitArrayUtilities: Get64(10, 1024) == true", result == true);

    result = (bool)get64.Invoke(null, new object[] { 5u, (ulong)1024 });
    t("BitArrayUtilities: Get64(5, 1024) == false", result == false);
}
else
{
    t("BitArrayUtilities: Get64(10, 1024) == true", false);
    t("BitArrayUtilities: Get64(5, 1024) == false", false);
}

// --- Functional: Get8 ---
if (get8 != null)
{
    bool result = (bool)get8.Invoke(null, new object[] { 2u, (byte)36 });
    t("BitArrayUtilities: Get8(2, 36) == true", result == true);

    result = (bool)get8.Invoke(null, new object[] { 0u, (byte)36 });
    t("BitArrayUtilities: Get8(0, 36) == false", result == false);
}
else
{
    t("BitArrayUtilities: Get8(2, 36) == true", false);
    t("BitArrayUtilities: Get8(0, 36) == false", false);
}

// --- Functional: Get16 ---
if (get16 != null)
{
    bool result = (bool)get16.Invoke(null, new object[] { 15u, (ushort)0x8000 });
    t("BitArrayUtilities: Get16(15, 0x8000) == true", result == true);

    result = (bool)get16.Invoke(null, new object[] { 0u, (ushort)0x8000 });
    t("BitArrayUtilities: Get16(0, 0x8000) == false", result == false);
}
else
{
    t("BitArrayUtilities: Get16(15, 0x8000) == true", false);
    t("BitArrayUtilities: Get16(0, 0x8000) == false", false);
}

// --- Get128: dual field dispatch ---
// Find overload with (int, ulong, ulong) -> bool
var get128 = get128Methods.FirstOrDefault(m => {
    var p = m.GetParameters();
    return p.Length == 3 && p[0].ParameterType == typeof(int) && p[1].ParameterType == typeof(ulong) && p[2].ParameterType == typeof(ulong);
});
if (get128 != null)
{
    t("BitArrayUtilities: Get128(int,ulong,ulong) overload found", true);
    bool result = (bool)get128.Invoke(null, new object[] { 0, 1UL, 0UL });
    t("BitArrayUtilities: Get128(0, 1, 0) == true", result == true);

    result = (bool)get128.Invoke(null, new object[] { 64, 0UL, 1UL });
    t("BitArrayUtilities: Get128(64, 0, 1) == true", result == true);

    result = (bool)get128.Invoke(null, new object[] { 5, 0UL, 0UL });
    t("BitArrayUtilities: Get128(5, 0, 0) == false", result == false);
}
else
{
    t("BitArrayUtilities: Get128(int,ulong,ulong) overload found", false);
    t("BitArrayUtilities: Get128(0, 1, 0) == true", false);
    t("BitArrayUtilities: Get128(64, 0, 1) == true", false);
    t("BitArrayUtilities: Get128(5, 0, 0) == false", false);
}

// --- Check Get256/Set256 exist ---
var get256 = utilType.GetMethods(bf).FirstOrDefault(m => m.Name == "Get256");
t("BitArrayUtilities: has Get256", get256 != null);
var set256 = utilType.GetMethods(bf).FirstOrDefault(m => m.Name == "Set256");
t("BitArrayUtilities: has Set256", set256 != null);

// --- Set32 parameter signature check ---
if (set32 != null)
{
    var p = set32.GetParameters();
    t("BitArrayUtilities: Set32 has 3 params", p.Length == 3);
    t("BitArrayUtilities: Set32 param 0 is uint", p[0].ParameterType == typeof(uint));
    t("BitArrayUtilities: Set32 param 2 is bool", p[2].ParameterType == typeof(bool));
}
else
{
    t("BitArrayUtilities: Set32 has 3 params", false);
    t("BitArrayUtilities: Set32 param 0 is uint", false);
    t("BitArrayUtilities: Set32 param 2 is bool", false);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
