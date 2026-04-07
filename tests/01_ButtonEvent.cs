// ============================================================================
// TEST: ButtonEvent — BovineLabs.Core.Utility
// Branch: topic/ButtonEvent
// Source: BovineLabs.Core/Utility/ButtonEvent.cs
// Run: cat 01_ButtonEvent.cs | unity-cli exec --usings "BovineLabs.Core.Utility"
// ============================================================================
// ButtonEvent is a single-bool struct for producer/consumer event semantics.
// TryProduce(true) sets the event (returns true if was idle).
// TryConsume() reads and resets (returns true if was set).
// Multiple produces between consumes = coalesced to ONE event.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- Test 1: Default state is false (no event pending) ---
var btn = new ButtonEvent();
t("Default Value is false", btn.Value == false);

// --- Test 2: TryProduce(true) on idle → returns true, Value becomes true ---
t("TryProduce(true) returns true", btn.TryProduce(true) == true);
t("Value is now true", btn.Value == true);

// --- Test 3: Double produce is deduplicated ---
t("TryProduce(true) again returns false (dedup)", btn.TryProduce(true) == false);
t("Value still true after dedup", btn.Value == true);

// --- Test 4: TryConsume reads and resets ---
t("TryConsume returns true", btn.TryConsume() == true);
t("Value is false after consume", btn.Value == false);

// --- Test 5: Double consume → second returns false ---
t("TryConsume again returns false", btn.TryConsume() == false);
t("Value stays false", btn.Value == false);

// --- Test 6: TryProduce() with no args defaults to true ---
t("TryProduce() no-arg returns true", btn.TryProduce() == true);
btn.TryConsume(); // reset

// --- Test 7: TryProduce(false) is a no-op ---
t("TryProduce(false) returns false", btn.TryProduce(false) == false);
t("Value stays false after TryProduce(false)", btn.Value == false);

// --- Test 8: Full cycle works twice ---
btn.TryProduce(true);
btn.TryConsume();
t("Second cycle: TryProduce(true) returns true", btn.TryProduce(true) == true);
t("Second cycle: TryConsume returns true", btn.TryConsume() == true);

// --- Test 9: Direct Value field manipulation ---
btn.Value = true;
t("Manual Value=true then TryConsume returns true", btn.TryConsume() == true);
btn.Value = false;
t("Manual Value=false then TryProduce(true) returns true", btn.TryProduce(true) == true);

// --- Test 10: TryConsume on never-produced struct ---
var fresh = new ButtonEvent();
t("TryConsume on fresh struct returns false", fresh.TryConsume() == false);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
