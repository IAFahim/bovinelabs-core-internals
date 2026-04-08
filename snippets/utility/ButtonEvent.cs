// Run: cat snippets/utility/ButtonEvent.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Utility"
// Verifies: docs/ButtonEvent.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists and is a struct ---
var btn = new BovineLabs.Core.Utility.ButtonEvent();
t("ButtonEvent: type exists and can be default-constructed", btn.GetType().IsValueType);

// --- Default state: Value should be false ---
t("ButtonEvent: default Value is false", btn.Value == false);

// --- TryProduce(true) on idle (Value=false) returns true, sets Value=true ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
bool produced = btn.TryProduce(true);
t("TryProduce(true) on idle returns true", produced == true);
t("TryProduce(true) on idle sets Value=true", btn.Value == true);

// --- TryProduce(true) on pending (Value=true) returns false, Value stays true ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
btn.TryProduce(true); // set to pending
produced = btn.TryProduce(true); // try again
t("TryProduce(true) on pending returns false (dedup)", produced == false);
t("TryProduce(true) on pending Value stays true", btn.Value == true);

// --- TryProduce(false) on idle returns false, Value stays false ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
produced = btn.TryProduce(false);
t("TryProduce(false) on idle returns false (no-op)", produced == false);
t("TryProduce(false) on idle Value stays false", btn.Value == false);

// --- TryProduce(false) on pending returns false, Value stays true ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
btn.TryProduce(true); // set to pending
produced = btn.TryProduce(false);
t("TryProduce(false) on pending returns false", produced == false);
t("TryProduce(false) on pending Value stays true", btn.Value == true);

// --- TryConsume on pending (Value=true) returns true, resets Value to false ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
btn.TryProduce(true); // set to pending
bool consumed = btn.TryConsume();
t("TryConsume on pending returns true", consumed == true);
t("TryConsume on pending resets Value to false", btn.Value == false);

// --- TryConsume on idle (Value=false) returns false, Value stays false ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
consumed = btn.TryConsume();
t("TryConsume on idle returns false", consumed == false);
t("TryConsume on idle Value stays false", btn.Value == false);

// --- Double consume (single-consumption guarantee) ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
btn.TryProduce(true);
btn.TryConsume(); // first consume
consumed = btn.TryConsume(); // second consume
t("TryConsume after TryConsume returns false", consumed == false);

// --- Full produce-consume-produce-consume cycle ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
t("Cycle: initial Value=false", btn.Value == false);
t("Cycle: TryProduce(true) returns true", btn.TryProduce(true) == true);
t("Cycle: Value=true after produce", btn.Value == true);
t("Cycle: TryConsume() returns true", btn.TryConsume() == true);
t("Cycle: Value=false after consume", btn.Value == false);
t("Cycle: TryConsume() returns false (no event)", btn.TryConsume() == false);
t("Cycle: TryProduce(true) returns true again", btn.TryProduce(true) == true);
t("Cycle: TryConsume() returns true again", btn.TryConsume() == true);

// --- TryProduce with no argument (default true) ---
btn = new BovineLabs.Core.Utility.ButtonEvent();
produced = btn.TryProduce();
t("TryProduce() default param behaves like TryProduce(true)", produced == true);
t("TryProduce() sets Value=true", btn.Value == true);

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
