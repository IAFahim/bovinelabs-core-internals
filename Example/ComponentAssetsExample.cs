// Example: Component Assets — ScriptableObject wrappers for ECS component types
// Tests: API surface, type hierarchy, attribute verification
// When to use: Authoring-time component references, type-safe ECS configuration
//
// Run: cat Example/ComponentAssetsExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

Type findType(string name) {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}

// === ComponentAssetBase ===
sb.AppendLine("=== ComponentAssetBase ===");
var cab = typeof(BovineLabs.Core.ComponentAssetBase);
check("is abstract class", cab.IsAbstract);
check("extends ScriptableObject", cab.BaseType?.Name == "ScriptableObject");
var cabMethods = cab.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", cabMethods));
check("has GetStableTypeHash", cabMethods.Any(m => m.Contains("GetStableTypeHash")));
check("has GetComponentType", cabMethods.Any(m => m.Contains("GetComponentType")));
// Check CustomValidation
var cabProtected = cab.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).Select(m => m.Name).ToList();
sb.AppendLine("  Protected methods: " + string.Join(", ", cabProtected));
check("has CustomValidation", cabProtected.Contains("CustomValidation"));

// === ComponentAsset ===
sb.AppendLine();
sb.AppendLine("=== ComponentAsset ===");
var ca = typeof(BovineLabs.Core.ComponentAsset);
check("extends ComponentAssetBase", ca.BaseType?.Name == "ComponentAssetBase");
var caAttrs = ca.GetCustomAttributes(false).Select(a => a.GetType().Name).ToList();
sb.AppendLine("  Attributes: " + string.Join(", ", caAttrs));
check("has CreateAssetMenu", caAttrs.Any(a => a.Contains("CreateAssetMenu")));
var caFields = ca.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", caFields));

// === EnableableComponentAsset ===
sb.AppendLine();
sb.AppendLine("=== EnableableComponentAsset ===");
var eca = typeof(BovineLabs.Core.EnableableComponentAsset);
check("extends ComponentAssetBase", eca.BaseType?.Name == "ComponentAssetBase");
sb.AppendLine("  Validates that target type implements IEnableableComponent in CustomValidation");

// === ComponentFieldAsset ===
sb.AppendLine();
sb.AppendLine("=== ComponentFieldAsset ===");
var cfa = typeof(BovineLabs.Core.ComponentFieldAsset);
check("extends ScriptableObject", cfa.BaseType?.Name == "ScriptableObject");
var cfaFields = cfa.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", cfaFields));
var cfaMethods = cfa.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", cfaMethods));

// === TypeAsset ===
sb.AppendLine();
sb.AppendLine("=== TypeAsset ===");
var ta = typeof(BovineLabs.Core.TypeAsset);
check("extends ScriptableObject", ta.BaseType?.Name == "ScriptableObject");
var taConst = ta.GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Constants: " + string.Join(", ", taConst));
check("has SearchProviderType", taConst.Any(f => f.Contains("SearchProviderType")));
var taMethods = ta.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", taMethods));

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                   USE THIS");
sb.AppendLine("  ──────────────────────────────────────────────────────────────");
sb.AppendLine("  Reference a component type in Inspector     ComponentAsset");
sb.AppendLine("  Enableable component reference              EnableableComponentAsset");
sb.AppendLine("  Get field offset for generic serialization  ComponentFieldAsset");
sb.AppendLine("  Reference any C# type in Inspector          TypeAsset");
sb.AppendLine("  Base class for component-type assets        ComponentAssetBase");
sb.AppendLine();
sb.AppendLine("  HIERARCHY:");
sb.AppendLine("  ScriptableObject");
sb.AppendLine("    +-- ComponentAssetBase (abstract) -- GetStableTypeHash, GetComponentType");
sb.AppendLine("    |     +-- ComponentAsset -- [CreateAssetMenu]");
sb.AppendLine("    |     +-- EnableableComponentAsset -- validates IEnableableComponent");
sb.AppendLine("    +-- ComponentFieldAsset -- field offset resolution");
sb.AppendLine("    +-- TypeAsset -- generic C# type reference");
sb.AppendLine();
sb.AppendLine("  USE CASE: Authoring/Baking pipeline");
sb.AppendLine("  1. Designer creates ComponentAsset in Inspector");
sb.AppendLine("  2. Baker reads asset.GetStableTypeHash() for TypeManager lookup");
sb.AppendLine("  3. Baker uses asset.GetComponentType() to add components to entities");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
