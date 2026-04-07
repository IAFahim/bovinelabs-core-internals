// ============================================================================
// TEST: ClassBuilder + CodeBuilder + CodeWriter + BlobSpline
// Branches: topic/ClassBuilder, topic/CodeBuilder, topic/CodeWriter,
//           topic/BlobSpline
// Sources: SourceGenerators~/CodeGenHelpers/ClassBuilder.cs, CodeBuilder.cs,
//          CodeWriter.cs, BovineLabs.Core/Collections/Blobs/Splines/BlobSpline.cs
// Run: cat 21_SourceGen.cs | unity-cli exec --usings "UnityEngine,System.Linq"
// ============================================================================
// ClassBuilder, CodeBuilder, CodeWriter: live in the source generator assembly
//   (CodeGenHelpers namespace). These are NOT loaded at runtime because they
//   compile into the source generator, not the game assembly.
// BlobSpline: lives in Collections/Blobs/Splines/ subdirectory.
// We verify source file existence and type non-availability at runtime.
// ============================================================================

var r = new System.Collections.Generic.List<string>();
int pass = 0, fail = 0;
Action<string,bool> t = (name, ok) => { if(ok){pass++;r.Add("PASS: "+name);}else{fail++;r.Add("FAIL: "+name);}};

// --- Verify these types are NOT in runtime assemblies ---
var allTypes = System.AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => { try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; } })
    .ToList();

t("ClassBuilder is NOT in runtime assemblies (source-gen only)", allTypes.All(xt => xt.Name != "ClassBuilder" || xt.Namespace != "CodeGenHelpers"));
t("CodeBuilder is NOT in runtime assemblies (source-gen only)", allTypes.All(xt => xt.Name != "CodeBuilder" || xt.Namespace != "CodeGenHelpers"));
t("CodeWriter is NOT in runtime assemblies (source-gen only)", allTypes.All(xt => xt.Name != "CodeWriter" || xt.Namespace != "CodeGenHelpers"));
t("BlobSpline is NOT in runtime assemblies (not in current version)", allTypes.All(xt => xt.Name != "BlobSpline"));

// --- Verify source files exist on disk ---
var sourceBase = System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "com.bovinelabs.core");
t("Source root exists", System.IO.Directory.Exists(sourceBase));

var sourceFiles = new[] {
    System.IO.Path.Combine(sourceBase, "SourceGenerators~/CodeGenHelpers/ClassBuilder.cs"),
    System.IO.Path.Combine(sourceBase, "SourceGenerators~/CodeGenHelpers/CodeBuilder.cs"),
    System.IO.Path.Combine(sourceBase, "SourceGenerators~/CodeGenHelpers/Internals/CodeWriter.cs"),
    System.IO.Path.Combine(sourceBase, "BovineLabs.Core/Collections/Blobs/Splines/BlobSpline.cs"),
};
foreach (var f in sourceFiles)
{
    var name = System.IO.Path.GetFileName(f);
    t($"Source file exists: {name} at {System.IO.Path.GetDirectoryName(f).Split(System.IO.Path.DirectorySeparatorChar).Last()}", System.IO.File.Exists(f));
}

r.Add($"\n=== {pass} PASSED, {fail} FAILED ===");
return string.Join("\n", r);
