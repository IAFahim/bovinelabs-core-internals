// Example: RuntimeContentCatalogUtility — Parse Unity's content catalog for SubScene fetching
// Tests: Type verification, method surface
// When to use: Dynamically discovering and loading SubScenes from Addressables at runtime
//
// Run: cat Example/RuntimeContentCatalogExample.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"

var sb = new System.Text.StringBuilder();
int pass = 0, fail = 0;
Action<string,bool> check = (n, ok) => { if(ok) pass++; else { fail++; sb.AppendLine("  FAIL: " + n); } };

var bfStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

// === RuntimeContentCatalogUtility ===
Type findType(string name) {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}

sb.AppendLine("=== RuntimeContentCatalogUtility ===");
var rcu = findType("RuntimeContentCatalogUtility");
if (rcu != null) {
    check("found", true);
    check("is static class", rcu.IsAbstract && rcu.IsSealed);
    check("in BovineLabs.Core.Internal", rcu.Namespace == "BovineLabs.Core.Internal");
    var methods = rcu.GetMethods(bfStatic).Where(m => !m.IsSpecialName)
        .Select(m => m.ReturnType.Name + " " + m.Name).ToList();
    sb.AppendLine("  Methods: " + string.Join(", ", methods));
    check("has static methods", methods.Count > 0);
    sb.AppendLine("  Purpose: Parse Unity's internal content catalog data");
    sb.AppendLine("  Uses: BlobAssetReference<RuntimeContentCatalogData>.TryRead");
} else { check("found", false); }

// === Decision Matrix ===
sb.AppendLine();
sb.AppendLine("=== Decision Matrix ===");
sb.AppendLine();
sb.AppendLine("  SCENARIO                                  USE THIS");
sb.AppendLine("  ────────────────────────────────────────────────────────────");
sb.AppendLine("  Load known SubScene by reference          Addressables.LoadSceneAsync");
sb.AppendLine("  Discover SubScenes from catalog           RuntimeContentCatalogUtility");
sb.AppendLine("  Dynamic content loading by label          Addressables + labels");
sb.AppendLine("  Parse internal catalog for custom logic   RuntimeContentCatalogUtility");
sb.AppendLine();
sb.AppendLine("  USE CASE: Modding / DLC systems");
sb.AppendLine("  1. DLC ships as Addressable bundle with SubScenes");
sb.AppendLine("  2. RuntimeContentCatalogUtility parses the catalog");
sb.AppendLine("  3. Your code discovers available SubScenes dynamically");
sb.AppendLine("  4. Load them on demand without hard references");
sb.AppendLine();
sb.AppendLine("  NOTE: This is internal infrastructure. Most games should use");
sb.AppendLine("  the standard Addressables API instead of this utility.");

sb.AppendLine();
sb.AppendLine("Verified: " + pass + " checks, " + fail + " failures");
return sb.ToString();
