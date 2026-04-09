// Run: cat snippets/dynamic-buffers/DynamicHashMapPerformanceTests.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "BovineLabs.Core.Iterators,BovineLabs.Core.Extensions,System,System.Reflection,System.Linq,Unity.Collections,Unity.Entities"
// Verifies: docs/DynamicHashMapPerformanceTests.md claims

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok) pass++; else fail++; };

sb.AppendLine("DynamicHashMapPerformanceTests");
sb.AppendLine("  Namespace: BovineLabs.Core.Tests.Iterators");
sb.AppendLine("  Kind: test class (source-level verification)");
sb.AppendLine();

var baseDir = System.IO.Path.GetDirectoryName(typeof(DynamicMultiHashMap<int, byte>).Assembly.Location);
var searchRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", ".."));
t("Search root exists", System.IO.Directory.Exists(searchRoot));

string foundPath = null;
try
{
    var files = System.IO.Directory.GetFiles(searchRoot, "DynamicHashMapPerformanceTests.cs", System.IO.SearchOption.AllDirectories)
        .Where(f => !f.Contains("bovinelabs-core-internals/snippets/")).ToArray();
    if (files.Length > 0) foundPath = files[0];
}
catch { }

t("Source file found", foundPath != null);

if (foundPath != null)
{
    var src = System.IO.File.ReadAllText(foundPath);
    t("Contains class declaration", src.Contains("DynamicHashMapPerformanceTests"));
    t("In BovineLabs.Core.Tests.Iterators namespace", src.Contains("BovineLabs.Core.Tests.Iterators"));

    sb.AppendLine($"  Source: {foundPath}");
    sb.AppendLine();

    // Extract method declarations via regex
    var methodPattern = new System.Text.RegularExpressions.Regex(@"public\s+void\s+(\w+)\s*\(");
    var matches = methodPattern.Matches(src);

    sb.AppendLine("  Methods:");
    string[] expectedMethods = {
        "Insert_Sequential", "Insert_Random",
        "IndexerWrite_ExistingKeys", "IndexerWrite_NewKeys", "IndexerWrite_Mixed",
        "TryGetValue_Sequential", "TryGetValue_Random",
        "Enumerate_Small", "Enumerate_Large",
        "Resize_Growth"
    };

    foreach (var em in expectedMethods)
    {
        bool present = src.Contains(em);
        t($"{em} method present", present);
        sb.AppendLine($"    void {em}() — {(present ? "FOUND" : "NOT FOUND")}");
    }

    // Also show all unique public void methods found by regex
    var methodNames = matches.Cast<System.Text.RegularExpressions.Match>()
        .Select(m => m.Groups[1].Value)
        .Distinct()
        .OrderBy(n => n)
        .ToList();
    sb.AppendLine();
    sb.AppendLine($"  All public void methods ({methodNames.Count}):");
    foreach (var mn in methodNames)
        sb.AppendLine($"    void {mn}()");
}
else
{
    sb.AppendLine("  Source file NOT FOUND");
    foreach (var em in new[] {
        "Insert_Sequential", "Insert_Random",
        "IndexerWrite_ExistingKeys", "IndexerWrite_NewKeys", "IndexerWrite_Mixed",
        "TryGetValue_Sequential", "TryGetValue_Random",
        "Enumerate_Small", "Enumerate_Large",
        "Resize_Growth"
    })
    {
        t($"{em} method present", false);
        sb.AppendLine($"    void {em}() — NOT FOUND");
    }
}

sb.AppendLine();
sb.AppendLine($"Verified: {pass} checks, {fail} failures");
return sb.ToString();
