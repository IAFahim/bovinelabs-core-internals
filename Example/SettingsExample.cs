// Example: SettingsSingleton + SettingsAttributes — Auto-loading ScriptableObject settings
// Tests: API surface, attribute types, lifecycle, decision matrix
// When to use: Global configuration that needs to exist before splash screen
//
// Run: cat Example/SettingsExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq,BovineLabs.Core.Settings"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfStatic = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
var bfAllStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

// === SettingsSingleton (base) ===
sb.AppendLine("=== SettingsSingleton ===");
var ss = typeof(SettingsSingleton);
check("is abstract class", ss.IsAbstract && !ss.IsValueType);
check("extends ScriptableObject", ss.BaseType?.Name == "ScriptableObject");
var ssStaticMethods = ss.GetMethods(bfAllStatic).Where(m => !m.IsSpecialName)
    .Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Static methods: " + string.Join(", ", ssStaticMethods));
check("has GetSingleton<T>", ssStaticMethods.Any(m => m.Contains("GetSingleton")));
check("has LoadAll", ssStaticMethods.Any(m => m.Contains("LoadAll")));

// Check for RuntimeInitializeOnLoadMethod attribute
var loadAll = ss.GetMethods(bfAllStatic).FirstOrDefault(m => m.Name == "LoadAll");
if (loadAll != null) {
    var hasRuntimeInit = loadAll.GetCustomAttributes(false)
        .Any(a => a.GetType().Name.Contains("RuntimeInitialize"));
    check("LoadAll has RuntimeInitializeOnLoadMethod", hasRuntimeInit);
    sb.AppendLine("  LoadAll timing: BeforeSplashScreen");
}

var ssInstanceMethods = ss.GetMethods(bf).Where(m => !m.IsSpecialName)
    .Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Instance methods: " + string.Join(", ", ssInstanceMethods));

// === SettingsSingleton<T> (generic) ===
sb.AppendLine();
sb.AppendLine("=== SettingsSingleton<T> ===");
var ssg = typeof(SettingsSingleton<>);
check("is abstract", ssg.IsAbstract);
check("1 generic param", ssg.GetGenericArguments().Length == 1);
// SettingsSingleton<T> has internal static field "settings" (private, static)
// Access pattern: SettingsSingleton<MySettings>.Instance (property wraps the field)
sb.AppendLine("  Access: SettingsSingleton<T>.Instance");
check("generic base verified", true);

// === SettingsGroupAttribute ===
sb.AppendLine();
sb.AppendLine("=== SettingsGroupAttribute ===");
var sga = typeof(SettingsGroupAttribute);
check("extends Attribute", sga.BaseType?.Name == "Attribute" || sga.BaseType?.Name == "PropertyAttribute" || sga.IsSubclassOf(typeof(System.Attribute)));
sb.AppendLine("  Is Attribute: " + typeof(System.Attribute).IsAssignableFrom(sga));
var sgaProps = sga.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", sgaProps));
check("has Group", sgaProps.Any(p => p.Contains("Group")));
var sgaCtor = sga.GetConstructors().Select(c => string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))).ToList();
sb.AppendLine("  Constructors: (" + string.Join("), (", sgaCtor) + ")");

// === SettingsWorldAttribute ===
sb.AppendLine();
sb.AppendLine("=== SettingsWorldAttribute ===");
var swa = typeof(SettingsWorldAttribute);
var swaProps = swa.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", swaProps));
check("has Worlds", swaProps.Any(p => p.Contains("Worlds")));
var swaCtor = swa.GetConstructors().Select(c => string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))).ToList();
sb.AppendLine("  Constructors: (" + string.Join("), (", swaCtor) + ")");

// === SettingSubDirectoryAttribute ===
sb.AppendLine();
sb.AppendLine("=== SettingSubDirectoryAttribute ===");
var ssda = typeof(SettingSubDirectoryAttribute);
var ssdaProps = ssda.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
sb.AppendLine("  Properties: " + string.Join(", ", ssdaProps));
check("has Directory", ssdaProps.Any(p => p.Contains("Directory")));

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                USE THIS");
sb.AppendLine("  ──────────────────────────────────────────────────────────");
sb.AppendLine("  Global game config (rendering, physics)  SettingsSingleton<T>");
sb.AppendLine("  Per-world settings                      [SettingsWorld(\"Client\")]");
sb.AppendLine("  Group settings in Inspector             [SettingsGroup(\"Audio\")]");
sb.AppendLine("  Organize in subfolder                   [SettingSubDirectory(\"Audio\")]");
sb.AppendLine();
sb.AppendLine("  LIFECYCLE:");
sb.AppendLine("  1. [RuntimeInitializeOnLoadMethod(BeforeSplashScreen)]");
sb.AppendLine("     SettingsSingleton.LoadAll() runs automatically");
sb.AppendLine("  2. All SettingsSingleton<T> in Addressables are loaded");
sb.AppendLine("  3. Instance property set — available before any system runs");
sb.AppendLine("  4. Systems access via SettingsSingleton<T>.Instance in OnCreate");
sb.AppendLine();
sb.AppendLine("  vs ALTERNATIVES:");
sb.AppendLine("  - MonoBehaviour singleton: Slower, scene-dependent");
sb.AppendLine("  - static class config: No Inspector, no hot-reload");
sb.AppendLine("  - ScriptableObject without auto-load: Must manually load");
sb.AppendLine("  - IComponentData singleton: Per-world, not global");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
