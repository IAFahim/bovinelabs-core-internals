// Run: cat snippets/blob-system/BlobMultiHashMapIterator.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Collections,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Mathematics"
// Verifies: docs/BlobMultiHashMapIterator.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

var iterType = typeof(BlobMultiHashMapIterator<int>);
t("BlobMultiHashMapIterator<int>: type exists", iterType != null);
t("BlobMultiHashMapIterator: is struct", iterType.IsValueType);

// Claims: Key field (TKey)
var keyField = iterType.GetField("Key", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobMultiHashMapIterator: has Key field", keyField != null);
t("BlobMultiHashMapIterator: Key is int", keyField?.FieldType == typeof(int));

// Claims: NextIndex field (int)
var nextIndexField = iterType.GetField("NextIndex", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
t("BlobMultiHashMapIterator: has NextIndex field", nextIndexField != null);
t("BlobMultiHashMapIterator: NextIndex is int", nextIndexField?.FieldType == typeof(int));

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
