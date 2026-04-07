// ============================================================================
// TEST: DistanceHitSortAscending + DistanceHitSortDescending
// Branches: topic/DistanceHitSortAscending, topic/DistanceHitSortDescending
// Sources: BovineLabs.Core/Sort/DistanceHitSortAscending.cs, DistanceHitSortDescending.cs
// Run: cat 06_Sort.cs | unity-cli exec --usings "BovineLabs.Core.Sort,Unity.Physics"
// ============================================================================
// DistanceHitSortAscending: IComparer<DistanceHit> that sorts by Distance ascending.
// DistanceHitSortDescending: same but descending order.
// Note: DistanceHit.Distance is read-only; we test with physics queries or reflection.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- DistanceHitSortAscending ---
var asc = new DistanceHitSortAscending();
t("DistanceHitSortAscending: instantiate", true);

// Verify it implements IComparer<DistanceHit>
var cmpType = typeof(DistanceHitSortAscending);
var ifaces = cmpType.GetInterfaces();
t("DistanceHitSortAscending implements IComparer", ifaces.Any(i => i.Name.Contains("IComparer")));

// Test Compare method exists with correct signature
var compareMethod = cmpType.GetMethod("Compare");
t("DistanceHitSortAscending has Compare", compareMethod != null);
var ps = compareMethod.GetParameters();
t("Compare takes 2 DistanceHit params", ps.Length == 2 && ps[0].ParameterType.Name == "DistanceHit" && ps[1].ParameterType.Name == "DistanceHit");
t("Compare returns int", compareMethod.ReturnType == typeof(int));

// --- DistanceHitSortDescending ---
var desc = new DistanceHitSortDescending();
t("DistanceHitSortDescending: instantiate", true);
var descIfaces = typeof(DistanceHitSortDescending).GetInterfaces();
t("DistanceHitSortDescending implements IComparer", descIfaces.Any(i => i.Name.Contains("IComparer")));
var descCompare = typeof(DistanceHitSortDescending).GetMethod("Compare");
t("DistanceHitSortDescending has Compare", descCompare != null);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
