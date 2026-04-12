// Run: cat snippets/ecs-extensions/IteratorsVerify.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: Unmanaged lookup types

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

// === EntityCache ===
var ec = typeof(BovineLabs.Core.Extensions.EntityCache);
sb.AppendLine("EntityCache");
sb.AppendLine("  Kind: " + (ec.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(ec) + " bytes");
check("is struct", ec.IsValueType);
var ecFields = ec.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", ecFields));
var ecProps = ec.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", ecProps));
check("has Entity field", ecFields.Any(f => f.Contains("Entity")));

// === UnsafeComponentLookup<T> ===
var ucl = typeof(BovineLabs.Core.Iterators.UnsafeComponentLookup<>);
sb.AppendLine();
sb.AppendLine("UnsafeComponentLookup<T>");
sb.AppendLine("  Kind: " + (ucl.IsValueType ? "struct" : "class") + ", generic");
check("is struct", ucl.IsValueType);
var uclFields = ucl.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", uclFields));
var uclMethods = ucl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", uclMethods));
check("has HasComponent", uclMethods.Contains("HasComponent"));
check("has TryGetComponent", uclMethods.Contains("TryGetComponent"));
check("has DidChange", uclMethods.Contains("DidChange"));
check("has Update", uclMethods.Contains("Update"));
check("has IsComponentEnabled", uclMethods.Contains("IsComponentEnabled"));
check("has SetComponentEnabled", uclMethods.Contains("SetComponentEnabled"));

// === UnsafeBufferLookup<T> ===
var ubl = typeof(BovineLabs.Core.Iterators.UnsafeBufferLookup<>);
sb.AppendLine();
sb.AppendLine("UnsafeBufferLookup<T>");
sb.AppendLine("  Kind: " + (ubl.IsValueType ? "struct" : "class"));
check("is struct", ubl.IsValueType);
var ublMethods = ubl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", ublMethods));
check("has TryGetBuffer", ublMethods.Contains("TryGetBuffer"));
check("has HasBuffer", ublMethods.Contains("HasBuffer"));

// === UnsafeEntityDataAccess ===
var ued = typeof(BovineLabs.Core.Iterators.UnsafeEntityDataAccess);
sb.AppendLine();
sb.AppendLine("UnsafeEntityDataAccess");
sb.AppendLine("  Kind: " + (ued.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(ued) + " bytes");
check("is struct", ued.IsValueType);
var uedMethods = ued.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", uedMethods));
check("has Exists", uedMethods.Contains("Exists"));
check("has HasComponent", uedMethods.Contains("HasComponent"));

// === UnsafeEnableableLookup ===
var uel = typeof(BovineLabs.Core.Iterators.UnsafeEnableableLookup);
sb.AppendLine();
sb.AppendLine("UnsafeEnableableLookup");
sb.AppendLine("  Kind: " + (uel.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(uel) + " bytes");
check("is struct", uel.IsValueType);
var uelMethods = uel.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", uelMethods));
check("has 3 methods", uelMethods.Count == 3);

// === ChangeFilterLookup<T> ===
var cfl = typeof(BovineLabs.Core.Iterators.ChangeFilterLookup<>);
sb.AppendLine();
sb.AppendLine("ChangeFilterLookup<T>");
sb.AppendLine("  Kind: " + (cfl.IsValueType ? "struct" : "class"));
check("is struct", cfl.IsValueType);
var cflMethods = cfl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", cflMethods));
check("has DidChange", cflMethods.Contains("DidChange"));
check("has SetChangeFilter", cflMethods.Contains("SetChangeFilter"));

// === SharedComponentLookup<T> ===
var scl = typeof(BovineLabs.Core.Iterators.SharedComponentLookup<>);
sb.AppendLine();
sb.AppendLine("SharedComponentLookup<T>");
sb.AppendLine("  Kind: " + (scl.IsValueType ? "struct" : "class"));
check("is struct", scl.IsValueType);
var sclMethods = scl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", sclMethods));

// === SharedComponentDataFromIndex<T> ===
var scfi = typeof(BovineLabs.Core.Iterators.SharedComponentDataFromIndex<>);
sb.AppendLine();
sb.AppendLine("SharedComponentDataFromIndex<T>");
sb.AppendLine("  Kind: " + (scfi.IsValueType ? "struct" : "class"));
check("is struct", scfi.IsValueType);

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
