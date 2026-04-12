// Example: Unmanaged Lookups — Unsafe alternatives to ComponentLookup/BufferLookup
// Tests: API surface, method counts, field layouts, decision matrix
// When to use: Burst jobs that need raw pointer access without safety handle overhead
//
// Run: cat Example/UnsafeLookupsExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq,BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,Unity.Entities"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

// === UnsafeComponentLookup<T> ===
sb.AppendLine("=== UnsafeComponentLookup<T> ===");
var ucl = typeof(UnsafeComponentLookup<>);
check("is struct", ucl.IsValueType);
var uclFields = ucl.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", uclFields));
check("has EntityDataAccess* pointer", uclFields.Any(f => f.Contains("access")));
check("has TypeIndex", uclFields.Any(f => f.Contains("typeIndex")));
check("has LookupCache", uclFields.Any(f => f.Contains("cache")));
var uclMethods = ucl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", uclMethods.Distinct()));
check("has indexer [Entity]", uclMethods.Contains("get_Item") || ucl.GetProperties(bf).Any(p => p.GetIndexParameters().Length > 0));
check("has HasComponent", uclMethods.Contains("HasComponent"));
check("has TryGetComponent", uclMethods.Contains("TryGetComponent"));
check("has Update", uclMethods.Contains("Update"));
check("has DidChange", uclMethods.Contains("DidChange"));
check("has IsComponentEnabled", uclMethods.Contains("IsComponentEnabled"));
check("has SetComponentEnabled", uclMethods.Contains("SetComponentEnabled"));
sb.AppendLine("  Constraint: T : unmanaged, IComponentData");
sb.AppendLine("  Obtained via: SystemState.GetUnsafeComponentLookup<T>()");

// === UnsafeBufferLookup<T> ===
sb.AppendLine();
sb.AppendLine("=== UnsafeBufferLookup<T> ===");
var ubl = typeof(UnsafeBufferLookup<>);
check("is struct", ubl.IsValueType);
var ublFields = ubl.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", ublFields));
check("has EntityDataAccess* pointer", ublFields.Any(f => f.Contains("access")));
check("has internalCapacity", ublFields.Any(f => f.Contains("internalCapacity")));
var ublMethods = ubl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", ublMethods.Distinct()));
check("has TryGetBuffer", ublMethods.Contains("TryGetBuffer"));
check("has HasBuffer", ublMethods.Contains("HasBuffer"));
check("has DidChange", ublMethods.Contains("DidChange"));
check("has IsBufferEnabled", ublMethods.Contains("IsBufferEnabled"));
check("has SetBufferEnabled", ublMethods.Contains("SetBufferEnabled"));
sb.AppendLine("  Constraint: T : unmanaged, IBufferElementData");
sb.AppendLine("  Returns: UnsafeDynamicBuffer<T> (not DynamicBuffer<T>)");
sb.AppendLine("  Obtained via: SystemState.GetUnsafeBufferLookup<T>()");

// === UnsafeEntityDataAccess ===
sb.AppendLine();
sb.AppendLine("=== UnsafeEntityDataAccess ===");
var ued = typeof(UnsafeEntityDataAccess);
check("is struct", ued.IsValueType);
var uedSize = System.Runtime.InteropServices.Marshal.SizeOf(ued);
sb.AppendLine("  Size: " + uedSize + " bytes");
check("16 bytes", uedSize == 16);
var uedMethods = ued.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName).Select(m => m.Name).Distinct().ToList();
sb.AppendLine("  Methods: " + string.Join(", ", uedMethods));
check("has GetComponentDataPtrRW", uedMethods.Any(m => m.Contains("GetComponentDataPtrRW")));
check("has GetComponentDataPtrRO", uedMethods.Any(m => m.Contains("GetComponentDataPtrRO")));
check("has HasComponent", uedMethods.Any(m => m.Contains("HasComponent")));
check("has Exists", uedMethods.Contains("Exists"));
sb.AppendLine("  Obtained via: SystemState.GetUnsafeEntityDataAccess()");

// === UnsafeEnableableLookup ===
sb.AppendLine();
sb.AppendLine("=== UnsafeEnableableLookup ===");
var uel = typeof(UnsafeEnableableLookup);
check("is struct", uel.IsValueType);
var uelSize = System.Runtime.InteropServices.Marshal.SizeOf(uel);
sb.AppendLine("  Size: " + uelSize + " bytes");
check("8 bytes (just EntityDataAccess*)", uelSize == 8);
var uelMethods = uel.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", uelMethods));
check("has HasComponent", uelMethods.Contains("HasComponent"));
check("has IsComponentEnabled", uelMethods.Contains("IsComponentEnabled"));
check("has SetComponentEnabled", uelMethods.Contains("SetComponentEnabled"));
check("3 methods total", uelMethods.Count == 3);
sb.AppendLine("  NOT generic — takes ComponentType at call site");
sb.AppendLine("  Obtained via: SystemState.GetUnsafeEnableableLookup()");

// === EntityCache ===
sb.AppendLine();
sb.AppendLine("=== EntityCache ===");
var ec = typeof(EntityCache);
check("is struct", ec.IsValueType);
var ecSize = System.Runtime.InteropServices.Marshal.SizeOf(ec);
sb.AppendLine("  Size: " + ecSize + " bytes");
check("32 bytes", ecSize == 32);
var ecFields = ec.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", ecFields));
check("has Entity", ecFields.Any(f => f.Contains("Entity")));
check("has Exists", ecFields.Any(f => f.Contains("Exists")));
check("has Archetype pointer", ecFields.Any(f => f.Contains("Archetype")));
check("has EntityInChunk", ecFields.Any(f => f.Contains("EntityInChunk")));
sb.AppendLine("  Factory: EntityCache.Create<T>(ComponentLookup<T>, Entity)");
sb.AppendLine("  Caches archetype lookup so subsequent HasComponent calls are O(1)");

// === ChangeFilterLookup<T> ===
sb.AppendLine();
sb.AppendLine("=== ChangeFilterLookup<T> ===");
var cfl = typeof(ChangeFilterLookup<>);
check("is struct", cfl.IsValueType);
var cflMethods = cfl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", cflMethods.Distinct()));
check("has DidChange", cflMethods.Contains("DidChange"));
check("has SetChangeFilter", cflMethods.Contains("SetChangeFilter"));
check("has Update", cflMethods.Contains("Update"));
sb.AppendLine("  Constraint: T : unmanaged (NOT IComponentData — works with any type)");
sb.AppendLine("  Obtained via: SystemState.GetChangeFilterLookup<T>()");

// === SharedComponentLookup<T> ===
sb.AppendLine();
sb.AppendLine("=== SharedComponentLookup<T> ===");
var scl = typeof(SharedComponentLookup<>);
check("is struct", scl.IsValueType);
var sclMethods = scl.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", sclMethods.Distinct()));
check("has HasComponent", sclMethods.Contains("HasComponent"));
check("has TryGetComponent", sclMethods.Contains("TryGetComponent"));
check("has Update", sclMethods.Contains("Update"));
sb.AppendLine("  Constraint: T : unmanaged, ISharedComponentData");
sb.AppendLine("  Has [NativeContainer] attribute — safety handle support");
sb.AppendLine("  Obtained via: SystemState.GetSharedComponentLookup<T>()");

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                     USE THIS");
sb.AppendLine("  ──────────────────────────────────────────────────────────────");
sb.AppendLine("  Typed component access in Burst job           UnsafeComponentLookup<T>");
sb.AppendLine("  Typed buffer access in Burst job              UnsafeBufferLookup<T>");
sb.AppendLine("  Untyped/component-at-runtime access           UnsafeEntityDataAccess");
sb.AppendLine("  Enable/disable components without generics    UnsafeEnableableLookup");
sb.AppendLine("  Cache archetype for multi-component fetch     EntityCache");
sb.AppendLine("  Manual change version control                 ChangeFilterLookup<T>");
sb.AppendLine("  Shared component data in jobs                 SharedComponentLookup<T>");
sb.AppendLine();
sb.AppendLine("  vs UNITY BUILT-IN:");
sb.AppendLine("  ComponentLookup<T>:   Safe handles, managed overhead → UnsafeComponentLookup<T>");
sb.AppendLine("  BufferLookup<T>:      Safe handles, DynamicBuffer → UnsafeBufferLookup<T>");
sb.AppendLine("  ChangeVersion:        Only via ArchetypeChunk → ChangeFilterLookup<T> per-entity");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
