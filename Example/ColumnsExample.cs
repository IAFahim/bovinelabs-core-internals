// Example: IColumn + MultiHashColumn + OrderedListColumn — Secondary data maps for DynamicVariableMap
// Tests: API surface, method signatures, decision matrix
// When to use: MultiHashColumn for tag/multi-value lookups, OrderedListColumn for sorted iteration
//
// Run: cat Example/ColumnsExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq,BovineLabs.Core.Iterators.Columns"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
var bfStatic = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

// === IColumn<T> Interface ===
sb.AppendLine("=== IColumn<T> Interface ===");
var ic = typeof(BovineLabs.Core.Iterators.Columns.IColumn<>);
check("is interface", ic.IsInterface);
check("1 generic param", ic.GetGenericArguments().Length == 1);
var icMethods = ic.GetMethods(bf).Select(m => m.ReturnType.Name + " " + m.Name).OrderBy(m => m).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", icMethods));
check("has Initialize", icMethods.Any(m => m.Contains("Initialize")));
check("has CalculateDataSize", icMethods.Any(m => m.Contains("CalculateDataSize")));
check("has Add", icMethods.Any(m => m.Contains("Add")));
check("has Replace", icMethods.Any(m => m.Contains("Replace")));
check("has Remove", icMethods.Any(m => m.Contains("Remove")));
check("has Clear", icMethods.Any(m => m.Contains("Clear")));
check("has StartResize", icMethods.Any(m => m.Contains("StartResize")));
check("has ApplyResize", icMethods.Any(m => m.Contains("ApplyResize")));
check("has GetValue", icMethods.Any(m => m.Contains("GetValue")));

// === MultiHashColumn<T> ===
sb.AppendLine();
sb.AppendLine("=== MultiHashColumn<T> ===");
var mhc = typeof(MultiHashColumn<>);
check("is struct", mhc.IsValueType);
var mhcMethods = mhc.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", mhcMethods));
check("has TryGetFirst", mhcMethods.Any(m => m.Contains("TryGetFirst")));
check("has TryGetNext", mhcMethods.Any(m => m.Contains("TryGetNext")));
// Check internal layout
var mhcFields = mhc.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Internal fields: " + string.Join(", ", mhcFields));
check("has keys/next/buckets offsets", mhcFields.Count >= 4);

// === OrderedListColumn<T> ===
sb.AppendLine();
sb.AppendLine("=== OrderedListColumn<T> ===");
var olc = typeof(OrderedListColumn<>);
check("is struct", olc.IsValueType);
var olcMethods = olc.GetMethods(bf).Where(m => !m.IsSpecialName).Select(m => m.ReturnType.Name + " " + m.Name).ToList();
sb.AppendLine("  Methods: " + string.Join(", ", olcMethods));
check("has TryGetFirst", olcMethods.Any(m => m.Contains("TryGetFirst")));
check("has TryGetNext", olcMethods.Any(m => m.Contains("TryGetNext")));
check("has GetFirst", olcMethods.Any(m => m.Contains("GetFirst")));
check("has GetNext", olcMethods.Any(m => m.Contains("GetNext")));
check("has GetValue", olcMethods.Any(m => m.Contains("GetValue")));
var olcFields = olc.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Select(f => f.FieldType.Name + " " + f.Name).ToList();
sb.AppendLine("  Internal fields: " + string.Join(", ", olcFields));

// === OrderedListIterator ===
sb.AppendLine();
sb.AppendLine("=== OrderedListIterator ===");
// OrderedListIterator is a nested type inside OrderedListColumn<T>
// Runtime verified: 8 bytes, EntryIndex + NextEntryIndex fields
sb.AppendLine("  OrderedListIterator: nested in OrderedListColumn<T>, 8 bytes");

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                              USE THIS");
sb.AppendLine("  ─────────────────────────────────────────────────────────");
sb.AppendLine("  Multiple values per key (tags, groups)  MultiHashColumn<T>");
sb.AppendLine("  Sorted iteration (leaderboard, queue)   OrderedListColumn<T>");
sb.AppendLine("  Need hash-speed O(1) lookup             MultiHashColumn<T>");
sb.AppendLine("  Need in-order traversal                 OrderedListColumn<T>");
sb.AppendLine("  Custom column logic                     Implement IColumn<T>");
sb.AppendLine();
sb.AppendLine("  KEY DIFFERENCES:");
sb.AppendLine("  MultiHashColumn:  T : unmanaged, IEquatable<T>");
sb.AppendLine("                    Hash buckets + linked chain per bucket");
sb.AppendLine("                    Data: [Keys*cap] [Next*cap] [Buckets*cap*2]");
sb.AppendLine("  OrderedListColumn: T : unmanaged, IEquatable<T>, IComparable<T>");
sb.AppendLine("                    Sorted doubly-linked list with head pointer");
sb.AppendLine("                    Data: [Keys*cap] [Next*cap] [Prev*cap]");
sb.AppendLine("                    Replace optimization: checks neighbors first");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
