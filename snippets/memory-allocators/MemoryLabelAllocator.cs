// Run: cat snippets/memory-allocators/MemoryLabelAllocator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe,Unity.Burst"
// Verifies: docs/MemoryLabelAllocator.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var mlaType = typeof(BovineLabs.Core.Memory.MemoryLabelAllocator);
t("MemoryLabelAllocator type exists", mlaType != null);
t("Is a struct (ValueType)", mlaType.IsValueType);
t("Implements IAllocator", mlaType.GetInterfaces().Any(i => i.Name.Contains("IAllocator")));

// --- Has Function property ---
var funcProp = mlaType.GetProperty("Function");
t("Has Function property", funcProp != null);
if (funcProp != null)
{
    t("Function property is readable", funcProp.CanRead);
}

// --- Has Handle property ---
var handleProp = mlaType.GetProperty("Handle");
t("Has Handle property", handleProp != null);

// --- Has ToAllocator property ---
var toAllocProp = mlaType.GetProperty("ToAllocator");
t("Has ToAllocator property", toAllocProp != null);

// --- Has IsCustomAllocator property ---
var isCustomProp = mlaType.GetProperty("IsCustomAllocator");
t("Has IsCustomAllocator property", isCustomProp != null);
if (isCustomProp != null)
{
    t("IsCustomAllocator returns bool", isCustomProp.PropertyType == typeof(bool));
}

// --- Has IsAutoDispose property ---
var isAutoProp = mlaType.GetProperty("IsAutoDispose");
t("Has IsAutoDispose property", isAutoProp != null);

// --- Has Initialize method ---
var initMethod = mlaType.GetMethods().Where(m => m.Name == "Initialize").FirstOrDefault();
t("Has Initialize method", initMethod != null);
if (initMethod != null)
{
    var ps = initMethod.GetParameters();
    t("Initialize takes (string areaName, string objectName)", ps.Length == 2 && ps[0].ParameterType == typeof(string) && ps[1].ParameterType == typeof(string));
}

// --- Has Try method ---
var tryMethod = mlaType.GetMethods().Where(m => m.Name == "Try").FirstOrDefault();
t("Has Try method", tryMethod != null);

// --- Has Dispose method ---
var disposeMethod = mlaType.GetMethods().Where(m => m.Name == "Dispose").FirstOrDefault();
t("Has Dispose method", disposeMethod != null);

// --- Has BurstCompile attribute ---
var burstAttr = mlaType.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("BurstCompile"));
t("Has BurstCompile attribute on type", burstAttr);

// --- IsAutoDispose should be false ---
// Check via creating instance
try
{
    var alloc = new BovineLabs.Core.Memory.MemoryLabelAllocator();
    t("Can construct default instance", true);
    t("IsAutoDispose is false", !alloc.IsAutoDispose);
    t("IsCustomAllocator default is false", !alloc.IsCustomAllocator);
}
catch (Exception ex)
{
    t("Can construct default instance", false);
    r.Add($"INFO: Exception: {ex.Message}");
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
