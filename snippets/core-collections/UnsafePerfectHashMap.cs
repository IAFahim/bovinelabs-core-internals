// Run: cat snippets/core-collections/UnsafePerfectHashMap.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,Unity.Collections,System.Linq"
// Verifies: docs/UnsafePerfectHashMap.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- Type exists ---
var mapType = typeof(BovineLabs.Core.Collections.UnsafePerfectHashMap<int, int>);
t("UnsafePerfectHashMap<int,int>: type exists", mapType != null);
t("UnsafePerfectHashMap<int,int>: is a struct (ValueType)", mapType.IsValueType);

// --- Has expected methods ---
var tryGetValue = mapType.GetMethod("TryGetValue");
t("UnsafePerfectHashMap: has TryGetValue", tryGetValue != null);

var disposeMethod = mapType.GetMethod("Dispose");
t("UnsafePerfectHashMap: has Dispose", disposeMethod != null);

var isCreatedProp = mapType.GetProperty("IsCreated");
t("UnsafePerfectHashMap: has IsCreated property", isCreatedProp != null);

// --- Functional tests ---
// UnsafePerfectHashMap requires unique hash codes for all keys (no collisions).
// It finds the smallest power-of-2 size that avoids hash collisions.

// Use keys with unique hash codes
var keys = new NativeArray<int>(3, Unity.Collections.Allocator.Temp);
var values = new NativeArray<int>(3, Unity.Collections.Allocator.Temp);

// Pick keys that have different hash codes. Int GetHashcode() returns the int itself.
keys[0] = 1; values[0] = 100;
keys[1] = 2; values[1] = 200;
keys[2] = 3; values[2] = 300;

var map = new BovineLabs.Core.Collections.UnsafePerfectHashMap<int, int>(
    keys, values, -1, Unity.Collections.Allocator.Temp);
t("UnsafePerfectHashMap: IsCreated after construction", map.IsCreated);

// TryGetValue for existing keys
bool found = map.TryGetValue(1, out int val);
t("UnsafePerfectHashMap: TryGetValue(1) found", found);
t("UnsafePerfectHashMap: TryGetValue(1) value == 100", val == 100);

found = map.TryGetValue(2, out val);
t("UnsafePerfectHashMap: TryGetValue(2) found", found);
t("UnsafePerfectHashMap: TryGetValue(2) value == 200", val == 200);

found = map.TryGetValue(3, out val);
t("UnsafePerfectHashMap: TryGetValue(3) found", found);
t("UnsafePerfectHashMap: TryGetValue(3) value == 300", val == 300);

// TryGetValue for non-existing key: Since this is a "perfect" hash, lookup is by index.
// Key 999 hashes to slot 999%4==3, which has key=3. The map returns the value at slot 3
// and checks if it equals NullValue. Since it doesn't, it returns true.
// This is expected behavior: the map assumes all lookups are for known keys.
// To verify "not found" behavior, we can use a key whose slot has the NullValue.
// With keys {1,2,3} and size=4, slot 0 has NullValue.
// Any key hashing to slot 0 would return false. E.g. key=0 (0%4==0), key=4 (4%4==0), key=8 (8%4==0)
found = map.TryGetValue(0, out val);
t("UnsafePerfectHashMap: TryGetValue(0) not found (empty slot)", !found);
t("UnsafePerfectHashMap: TryGetValue(0) returns default/nullValue", val == -1 || val == 0);

// Indexer get
val = map[1];
t("UnsafePerfectHashMap: map[1] == 100", val == 100);

// Indexer set (update existing)
map[1] = 111;
found = map.TryGetValue(1, out val);
t("UnsafePerfectHashMap: after map[1]=111, value updated", found && val == 111);

// Dispose
map.Dispose();
t("UnsafePerfectHashMap: Dispose completed", true);

// --- Test with Alloc/Free pointer API ---
keys.Dispose();
values.Dispose();

// Test with more keys to verify size scaling
var keys2 = new NativeArray<int>(5, Unity.Collections.Allocator.Temp);
var values2 = new NativeArray<int>(5, Unity.Collections.Allocator.Temp);
for (int i = 0; i < 5; i++)
{
    keys2[i] = i + 10;
    values2[i] = (i + 10) * 10;
}

var map2 = new BovineLabs.Core.Collections.UnsafePerfectHashMap<int, int>(
    keys2, values2, 0, Unity.Collections.Allocator.Temp);
t("UnsafePerfectHashMap(5 entries): IsCreated", map2.IsCreated);

for (int i = 0; i < 5; i++)
{
    found = map2.TryGetValue(i + 10, out val);
    t($"UnsafePerfectHashMap(5 entries): TryGetValue({i+10}) found", found);
    t($"UnsafePerfectHashMap(5 entries): TryGetValue({i+10}) value == {(i+10)*10}", val == (i+10)*10);
}

map2.Dispose();
keys2.Dispose();
values2.Dispose();

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
