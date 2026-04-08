// Run: cat snippets/dynamic-buffers/DynamicHashMapPerformanceTests.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicHashMapPerformanceTests.md claims

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => {
    if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}
};

// --- DynamicHashMapPerformanceTests is defined in BovineLabs.Core.Tests ---
// The test assembly is not loaded at runtime, so we verify the source file exists
// and verify the type via source code parsing.

var sourcePath = System.IO.Path.Combine(
    System.IO.Directory.GetParent(typeof(DynamicMultiHashMap<int, byte>).Assembly.Location).FullName,
    "..", "..", "..",
    "Library", "PackageCache",
    "com.bovinelabs.core@d49052fed7b0",
    "BovineLabs.Core.Tests", "Iterators", "DynamicHashMapPerformanceTests.cs"
);

// Try multiple possible paths
var baseDir = System.IO.Path.GetDirectoryName(typeof(DynamicMultiHashMap<int, byte>).Assembly.Location);
var searchRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", ".."));
t("Search root exists", System.IO.Directory.Exists(searchRoot));

// Find the file by searching for it
var found = false;
string foundPath = null;
try
{
    var files = System.IO.Directory.GetFiles(searchRoot, "DynamicHashMapPerformanceTests.cs", System.IO.SearchOption.AllDirectories);
    if (files.Length > 0) { found = true; foundPath = files[0]; }
}
catch { }

t("DynamicHashMapPerformanceTests.cs: source file found", found);

if (found && foundPath != null)
{
    var src = System.IO.File.ReadAllText(foundPath);
    t("DynamicHashMapPerformanceTests.cs: contains class declaration",
        src.Contains("DynamicHashMapPerformanceTests"));
    t("Source: has Insert_Sequential method", src.Contains("Insert_Sequential"));
    t("Source: has Insert_Random method", src.Contains("Insert_Random"));
    t("Source: has IndexerWrite_ExistingKeys method", src.Contains("IndexerWrite_ExistingKeys"));
    t("Source: has IndexerWrite_NewKeys method", src.Contains("IndexerWrite_NewKeys"));
    t("Source: has IndexerWrite_Mixed method", src.Contains("IndexerWrite_Mixed"));
    t("Source: has TryGetValue_Sequential method", src.Contains("TryGetValue_Sequential"));
    t("Source: has TryGetValue_Random method", src.Contains("TryGetValue_Random"));
    t("Source: has Enumerate_Small method", src.Contains("Enumerate_Small"));
    t("Source: has Enumerate_Large method", src.Contains("Enumerate_Large"));
    t("Source: has Resize_Growth method", src.Contains("Resize_Growth"));
    t("Source: in BovineLabs.Core.Tests.Iterators namespace",
        src.Contains("BovineLabs.Core.Tests.Iterators"));
}
else
{
    t("DynamicHashMapPerformanceTests.cs: contains class declaration", false);
    foreach (var mn in new[] {
        "Insert_Sequential", "Insert_Random", "IndexerWrite_ExistingKeys",
        "IndexerWrite_NewKeys", "IndexerWrite_Mixed", "TryGetValue_Sequential",
        "TryGetValue_Random", "Enumerate_Small", "Enumerate_Large", "Resize_Growth"
    })
        t("Source: has " + mn + " method", false);
    t("Source: in BovineLabs.Core.Tests.Iterators namespace", false);
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
