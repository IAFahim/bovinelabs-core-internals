// Run: cat snippets/ecs-extensions/EntityCommandsVerify.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Verifies: EntityCommands types surface

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else fail++; };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

// === IEntityCommands ===
var iec = typeof(BovineLabs.Core.EntityCommands.IEntityCommands);
sb.AppendLine("IEntityCommands");
sb.AppendLine("  Kind: " + (iec.IsInterface ? "interface" : iec.IsValueType ? "struct" : "class"));
check("is interface", iec.IsInterface);
var iecMethods = iec.GetMethods().Select(m => m.Name).OrderBy(n => n).Distinct().ToList();
sb.AppendLine("  Methods: " + string.Join(", ", iecMethods));
check("has CreateEntity", iecMethods.Contains("CreateEntity"));
check("has Instantiate", iecMethods.Contains("Instantiate"));
check("has AddComponent", iecMethods.Contains("AddComponent"));
check("has SetComponent", iecMethods.Contains("SetComponent"));
check("has AddBuffer", iecMethods.Contains("AddBuffer"));
check("has SetBuffer", iecMethods.Contains("SetBuffer"));
check("has AppendToBuffer", iecMethods.Contains("AppendToBuffer"));
check("has SetComponentEnabled", iecMethods.Contains("SetComponentEnabled"));
check("has AddBlobAsset", iecMethods.Contains("AddBlobAsset"));
check("has SetName", iecMethods.Contains("SetName"));
check("has Entity property", iec.GetProperty("Entity") != null);

// === EntityManagerCommands ===
var emc = typeof(BovineLabs.Core.EntityCommands.EntityManagerCommands);
sb.AppendLine();
sb.AppendLine("EntityManagerCommands");
sb.AppendLine("  Kind: " + (emc.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(emc) + " bytes");
check("is struct", emc.IsValueType);
check("implements IEntityCommands", typeof(BovineLabs.Core.EntityCommands.IEntityCommands).IsAssignableFrom(emc));
var emcFields = emc.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", emcFields));

// === CommandBufferCommands ===
var cbc = typeof(BovineLabs.Core.EntityCommands.CommandBufferCommands);
sb.AppendLine();
sb.AppendLine("CommandBufferCommands");
sb.AppendLine("  Kind: " + (cbc.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(cbc) + " bytes");
check("is struct", cbc.IsValueType);
check("implements IEntityCommands", typeof(BovineLabs.Core.EntityCommands.IEntityCommands).IsAssignableFrom(cbc));

// === CommandBufferParallelCommands ===
var cbpc = typeof(BovineLabs.Core.EntityCommands.CommandBufferParallelCommands);
sb.AppendLine();
sb.AppendLine("CommandBufferParallelCommands");
sb.AppendLine("  Kind: " + (cbpc.IsValueType ? "struct" : "class") + ", " + System.Runtime.InteropServices.Marshal.SizeOf(cbpc) + " bytes");
check("is struct", cbpc.IsValueType);
check("implements IEntityCommands", typeof(BovineLabs.Core.EntityCommands.IEntityCommands).IsAssignableFrom(cbpc));
// Check for sortKey field (private)
var cbpcFields = cbpc.GetFields(bfAll).Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Fields: " + string.Join(", ", cbpcFields));
check("has sortKey", cbpcFields.Any(f => f.Contains("sortKey")));

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
