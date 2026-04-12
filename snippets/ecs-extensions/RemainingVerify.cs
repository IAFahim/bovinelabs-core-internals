// Run: cat snippets/ecs-extensions/RemainingVerify.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: Collections, Columns, Settings, Component, Editor types

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

// === Reference<T> ===
var refT = typeof(BovineLabs.Core.Collections.Reference<>);
sb.AppendLine("Reference<T>");
sb.AppendLine("  Kind: " + (refT.IsValueType ? "struct" : "class"));
check("is struct", refT.IsValueType);
var refProps = refT.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", refProps));
var refMethods = refT.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", refMethods));
var refStatic = refT.GetMethods(bfStatic).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Static Methods: " + string.Join(", ", refStatic));
check("has IsCreated", refProps.Any(p => p.Contains("IsCreated")));
check("has Value", refProps.Any(p => p.Contains("Value")));
check("has Create", refStatic.Any(m => m.Contains("Create")));

// === ReferenceData ===
var rd = typeof(BovineLabs.Core.Collections.ReferenceData);
sb.AppendLine();
sb.AppendLine("ReferenceData");
sb.AppendLine("  Kind: " + (rd.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(rd) + " bytes");
check("is struct", rd.IsValueType);
var layoutAttr = rd.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "StructLayoutAttribute");
sb.AppendLine("  Layout: " + (layoutAttr != null ? layoutAttr.GetType().GetProperty("Value")?.GetValue(layoutAttr)?.ToString() : "none"));
check("has StructLayout", layoutAttr != null);
var rdFields = rd.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", rdFields));

// === MultiHashColumn<T> ===
var mhc = typeof(BovineLabs.Core.Iterators.Columns.MultiHashColumn<>);
sb.AppendLine();
sb.AppendLine("MultiHashColumn<T>");
sb.AppendLine("  Kind: " + (mhc.IsValueType ? "struct" : "class"));
check("is struct", mhc.IsValueType);
var mhcMethods = mhc.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", mhcMethods));

// === OrderedListColumn<T> ===
var olc = typeof(BovineLabs.Core.Iterators.Columns.OrderedListColumn<>);
sb.AppendLine();
sb.AppendLine("OrderedListColumn<T>");
sb.AppendLine("  Kind: " + (olc.IsValueType ? "struct" : "class"));
check("is struct", olc.IsValueType);
var olcMethods = olc.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", olcMethods));

// === OrderedListIterator ===
Type findType(string name) {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}
var oli = findType("OrderedListIterator");
if (oli != null) {
    sb.AppendLine();
    sb.AppendLine("OrderedListIterator");
    sb.AppendLine("  Kind: " + (oli.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(oli) + " bytes");
    var oliFields = oli.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
    sb.AppendLine("  Fields: " + string.Join(", ", oliFields));
}

// === CustomChunkIterator<T> ===
var cci = typeof(BovineLabs.Core.Iterators.CustomChunkIterator<>);
sb.AppendLine();
sb.AppendLine("CustomChunkIterator<T>");
sb.AppendLine("  Kind: " + (cci.IsValueType ? "struct" : "class"));
check("is struct", cci.IsValueType);

// === SettingsSingleton ===
var ss = typeof(BovineLabs.Core.Settings.SettingsSingleton);
sb.AppendLine();
sb.AppendLine("SettingsSingleton");
sb.AppendLine("  Kind: " + (ss.IsAbstract ? "abstract class" : "class"));
check("is abstract", ss.IsAbstract);
check("extends ScriptableObject", ss.BaseType?.Name == "ScriptableObject");
var ssMethods = ss.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Static Methods: " + string.Join(", ", ssMethods));

// === ComponentAssetBase ===
var cab = typeof(BovineLabs.Core.ComponentAssetBase);
sb.AppendLine();
sb.AppendLine("ComponentAssetBase");
sb.AppendLine("  Kind: " + (cab.IsAbstract ? "abstract class" : "class"));
check("is abstract", cab.IsAbstract);
check("extends ScriptableObject", cab.BaseType?.Name == "ScriptableObject");
var cabMethods = cab.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", cabMethods));
check("has GetStableTypeHash", cabMethods.Contains("GetStableTypeHash"));
check("has GetComponentType", cabMethods.Contains("GetComponentType"));

// === ComponentAsset ===
var ca = typeof(BovineLabs.Core.ComponentAsset);
sb.AppendLine();
sb.AppendLine("ComponentAsset");
check("extends ComponentAssetBase", ca.BaseType?.Name == "ComponentAssetBase");
var caAttrs = ca.GetCustomAttributes(false).Select(a => a.GetType().Name).ToList();
sb.AppendLine("  Attributes: " + string.Join(", ", caAttrs));

// === EnableableComponentAsset ===
var eca = typeof(BovineLabs.Core.EnableableComponentAsset);
sb.AppendLine();
sb.AppendLine("EnableableComponentAsset");
check("extends ComponentAssetBase", eca.BaseType?.Name == "ComponentAssetBase");

// === TypeAsset ===
var ta = typeof(BovineLabs.Core.TypeAsset);
sb.AppendLine();
sb.AppendLine("TypeAsset");
sb.AppendLine("  Kind: " + (ta.IsAbstract ? "abstract" : "class"));
check("extends ScriptableObject", ta.BaseType?.Name == "ScriptableObject");
var taConst = ta.GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Constants: " + string.Join(", ", taConst));
check("has SearchProviderType", taConst.Any(f => f.Contains("SearchProviderType")));

// === Settings Attributes ===
sb.AppendLine();
sb.AppendLine("SettingsGroupAttribute");
var sga = typeof(BovineLabs.Core.Settings.SettingsGroupAttribute);
check("extends Attribute", sga.BaseType?.Name == "Attribute");
sb.AppendLine("  Properties: " + string.Join(", ", sga.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name)));

sb.AppendLine();
sb.AppendLine("SettingsWorldAttribute");
var swa = typeof(BovineLabs.Core.Settings.SettingsWorldAttribute);
sb.AppendLine("  Properties: " + string.Join(", ", swa.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name)));

// === Editor types ===
sb.AppendLine();
sb.AppendLine("--- Editor Types ---");
var ee = findType("BovineLabs.Core.Editor.Inspectors.ElementEditor");
if (ee != null) {
    sb.AppendLine("ElementEditor: " + (ee.IsAbstract ? "abstract class" : "class") + ", base=" + ee.BaseType?.Name);
    check("ElementEditor found", true);
} else { check("ElementEditor found", false); }

var ep = findType("BovineLabs.Core.Editor.Inspectors.ElementProperty");
if (ep != null) {
    sb.AppendLine("ElementProperty: " + (ep.IsAbstract ? "abstract class" : "class") + ", base=" + ep.BaseType?.Name);
    check("ElementProperty found", true);
} else { check("ElementProperty found", false); }

var osp = findType("BovineLabs.Core.Editor.UI.ObjectSelectionProxy");
if (osp != null) {
    sb.AppendLine("ObjectSelectionProxy: class, base=" + osp.BaseType?.Name);
    check("ObjectSelectionProxy found", true);
} else { check("ObjectSelectionProxy found", false); }

var se = findType("BovineLabs.Core.Editor.UI.SearchElement");
if (se != null) {
    sb.AppendLine("SearchElement: class, base=" + se.BaseType?.Name);
    check("SearchElement found", true);
} else { check("SearchElement found", false); }

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
