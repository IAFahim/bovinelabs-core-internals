// Run: cat snippets/utility/ButtonEvent.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Utility,System,System.Reflection,System.Runtime.InteropServices"
var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (name, ok) => { if(ok) pass++; else fail++; };

var type = typeof(BovineLabs.Core.Utility.ButtonEvent);
var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

sb.AppendLine("ButtonEvent");
sb.AppendLine($"  Kind: {(type.IsValueType ? "struct" : "class")}");
sb.AppendLine($"  Size: {Marshal.SizeOf(type)} bytes");
sb.AppendLine();

sb.AppendLine("Fields:");
foreach (var fld in type.GetFields(bf))
    sb.AppendLine($"  [{Marshal.OffsetOf(type, fld.Name)}] {fld.FieldType.Name} {fld.Name}  ({(fld.IsPublic?"public":"private")})");
sb.AppendLine();

sb.AppendLine("Properties:");
foreach (var prp in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
    sb.AppendLine($"  {prp.PropertyType.Name} {prp.Name} {{ {(prp.CanRead?"get":"")};{(prp.CanWrite?"set":"")} }}");
sb.AppendLine();

sb.AppendLine("Methods:");
foreach (var mth in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    sb.AppendLine($"  {mth.ReturnType.Name} {mth.Name}({string.Join(", ", mth.GetParameters().Select(px => (px.IsOut?"out ":"")+px.ParameterType.Name))})");
sb.AppendLine();

sb.AppendLine("Runtime Behavior:");
var btn = new BovineLabs.Core.Utility.ButtonEvent();
sb.AppendLine($"  default: Value={btn.Value}");
bool p1 = btn.TryProduce(true);
sb.AppendLine($"  TryProduce(true) on idle: returns {p1}, Value={btn.Value}");
bool p2 = btn.TryProduce(true);
sb.AppendLine($"  TryProduce(true) on pending: returns {p2}, Value={btn.Value}");
bool c1 = btn.TryConsume();
sb.AppendLine($"  TryConsume() on pending: returns {c1}, Value={btn.Value}");
bool c2 = btn.TryConsume();
sb.AppendLine($"  TryConsume() on consumed: returns {c2}, Value={btn.Value}");
btn = new BovineLabs.Core.Utility.ButtonEvent();
bool p3 = btn.TryProduce(false);
sb.AppendLine($"  TryProduce(false) on idle: returns {p3}, Value={btn.Value}");
btn = new BovineLabs.Core.Utility.ButtonEvent();
bool pd = btn.TryProduce();
sb.AppendLine($"  TryProduce() default: returns {pd}, Value={btn.Value}");

check("Size==1", Marshal.SizeOf(type) == 1);
check("default Value=false", !new BovineLabs.Core.Utility.ButtonEvent().Value);
check("TryProduce(true) on idle", p1 == true);
check("TryProduce(true) dedup", p2 == false);
check("TryConsume on pending", c1 == true);
check("TryConsume double", c2 == false);
check("TryProduce(false) no-op", p3 == false);

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
