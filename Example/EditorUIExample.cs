// Example: Editor UI — ElementEditor, ObjectSelectionProxy, SearchElement
// Tests: API surface, inheritance, decision matrix
// When to use: Custom inspectors, object selection hacks, search dropdowns
//
// Run: cat Example/EditorUIExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

// Resolve editor types with assembly-qualified names
var eeType = System.Type.GetType("BovineLabs.Core.Editor.Inspectors.ElementEditor, BovineLabs.Core.Editor");
var epType = System.Type.GetType("BovineLabs.Core.Editor.Inspectors.ElementProperty, BovineLabs.Core.Editor");
var ospType = System.Type.GetType("BovineLabs.Core.Editor.UI.ObjectSelectionProxy, BovineLabs.Core.Editor");
var seType = System.Type.GetType("BovineLabs.Core.Editor.UI.SearchElement, BovineLabs.Core.Editor");

// === ElementEditor ===
sb.AppendLine("=== ElementEditor ===");
if (eeType != null) {
    check("found", true);
    check("is abstract class", eeType.IsAbstract);
    check("extends Editor", eeType.BaseType?.Name == "Editor");
    var eeMethods = eeType.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
    sb.AppendLine("  Methods: " + string.Join(", ", eeMethods));
    check("has CreateInspectorGUI", eeMethods.Any(m => m.Contains("CreateInspectorGUI")));
    sb.AppendLine("  Note: CreateInspectorGUI is sealed — you implement via ElementProperty");
} else { check("found", false); }

// === ElementProperty ===
sb.AppendLine();
sb.AppendLine("=== ElementProperty ===");
if (epType != null) {
    check("found", true);
    check("is abstract class", epType.IsAbstract);
    check("extends PropertyDrawer", epType.BaseType?.Name == "PropertyDrawer");
    var epFields = epType.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
    sb.AppendLine("  Fields: " + string.Join(", ", epFields));
    var epMethods = epType.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
    sb.AppendLine("  Methods: " + string.Join(", ", epMethods));
    check("has CreatePropertyGUI", epMethods.Any(m => m.Contains("CreatePropertyGUI")));
    sb.AppendLine("  Properties: ParentType, RootProperty, SerializedObject");
} else { check("found", false); }

// === ObjectSelectionProxy ===
sb.AppendLine();
sb.AppendLine("=== ObjectSelectionProxy ===");
if (ospType != null) {
    check("found", true);
    check("extends ScriptableObject", ospType.BaseType?.Name == "ScriptableObject");
    var ospProps = ospType.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
    sb.AppendLine("  Properties: " + string.Join(", ", ospProps));
    check("has Obj", ospProps.Any(p => p.Contains("Obj")));
    var ospInterfaces = ospType.GetInterfaces().Select(i => i.Name).ToList();
    sb.AppendLine("  Interfaces: " + string.Join(", ", ospInterfaces));
    check("implements ISerializationCallbackReceiver", ospInterfaces.Contains("ISerializationCallbackReceiver"));
    sb.AppendLine("  Hack: Wraps any Object so Inspector can display/inspect it");
} else { check("found", false); }

// === SearchElement ===
sb.AppendLine();
sb.AppendLine("=== SearchElement ===");
if (seType != null) {
    check("found", true);
    check("extends BaseField<int>", seType.BaseType?.Name == "BaseField`1");
    var seProps = seType.GetProperties(bf).Select(p => p.PropertyType.Name + " " + p.Name).ToList();
    sb.AppendLine("  Properties: " + string.Join(", ", seProps));
    check("has Text", seProps.Any(p => p.Contains("Text")));
    var seFields = seType.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
    sb.AppendLine("  Fields: " + string.Join(", ", seFields));
    sb.AppendLine("  Usage: Create SearchView with items, bind OnSelection callback");
} else { check("found", false); }

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                  USE THIS");
sb.AppendLine("  ────────────────────────────────────────────────────────────");
sb.AppendLine("  Custom ECS component inspector            ElementEditor + ElementProperty");
sb.AppendLine("  Complex PropertyDrawer for structs        ElementProperty");
sb.AppendLine("  Inspect raw C# objects/structs            ObjectSelectionProxy");
sb.AppendLine("  Search dropdown for types/components      SearchElement + SearchView");
sb.AppendLine();
sb.AppendLine("  PATTERN: ElementEditor + ElementProperty");
sb.AppendLine("  1. Create class : ElementEditor (sealed CreateInspectorGUI)");
sb.AppendLine("  2. Create drawer : ElementProperty (sealed CreatePropertyGUI)");
sb.AppendLine("  3. Editor creates PropertyField using [SerializableReference]");
sb.AppendLine("  4. PropertyDrawer reads RootProperty, builds VisualElement tree");
sb.AppendLine();
sb.AppendLine("  PATTERN: ObjectSelectionProxy");
sb.AppendLine("  1. Selection.activeObject = ObjectSelectionProxy.CreateInstance()");
sb.AppendLine("  2. proxy.Obj = your object");
sb.AppendLine("  3. Inspector now shows your object's fields");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
