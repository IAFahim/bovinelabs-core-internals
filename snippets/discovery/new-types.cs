// Run: cat snippets/discovery/new-types.cs | unity-cli exec --project ~/Github/bovinelabs-core-internals/BovineLabs --usings "System,System.Reflection,System.Linq"
// Discovers which new doc types are available at runtime

Type findType(string name) {
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { var t = asm.GetType(name); if (t != null) return t; } catch { }
    }
    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
        try { foreach (var t in asm.GetTypes()) { if (t.Name == name) return t; } } catch { }
    }
    return null;
}

var sb = new System.Text.StringBuilder();
var types = new[] {
    "BovineLabs.Core.Functions.IFunction`1",
    "BovineLabs.Core.Functions.FunctionsBuilder`2",
    "BovineLabs.Core.Functions.Functions`2",
    "BovineLabs.Core.Functions.FunctionsHash`2",
    "BovineLabs.Core.Functions.FunctionData",
    "BovineLabs.Core.EntityCommands.IEntityCommands",
    "BovineLabs.Core.EntityCommands.EntityManagerCommands",
    "BovineLabs.Core.EntityCommands.CommandBufferCommands",
    "BovineLabs.Core.EntityCommands.CommandBufferParallelCommands",
    "BovineLabs.Core.ComponentAssetBase",
    "BovineLabs.Core.ComponentAsset",
    "BovineLabs.Core.EnableableComponentAsset",
    "BovineLabs.Core.ComponentFieldAsset",
    "BovineLabs.Core.TypeAsset",
    "BovineLabs.Core.Extensions.EntityCache",
    "BovineLabs.Core.Iterators.UnsafeComponentLookup`1",
    "BovineLabs.Core.Iterators.UnsafeBufferLookup`1",
    "BovineLabs.Core.Iterators.UnsafeEntityDataAccess",
    "BovineLabs.Core.Iterators.UnsafeEnableableLookup",
    "BovineLabs.Core.Iterators.ChangeFilterLookup`1",
    "BovineLabs.Core.Iterators.SharedComponentLookup`1",
    "BovineLabs.Core.Iterators.SharedComponentDataFromIndex`1",
    "BovineLabs.Core.Settings.SettingsSingleton",
    "BovineLabs.Core.Settings.SettingsSingleton`1",
    "BovineLabs.Core.Settings.SettingsGroupAttribute",
    "BovineLabs.Core.Settings.SettingSubDirectoryAttribute",
    "BovineLabs.Core.Settings.SettingsWorldAttribute",
    "BovineLabs.Core.Iterators.Columns.IColumn`1",
    "BovineLabs.Core.Iterators.Columns.MultiHashColumn`1",
    "BovineLabs.Core.Iterators.Columns.OrderedListColumn`1",
    "BovineLabs.Core.Iterators.ICustomChunkIterator",
    "BovineLabs.Core.Iterators.CustomChunkIterator`1",
    "BovineLabs.Core.Collections.Reference`1",
    "BovineLabs.Core.Collections.ReferenceData",
    "BovineLabs.Core.Internal.RuntimeContentCatalogUtility",
    "BovineLabs.Core.Editor.Inspectors.ElementEditor",
    "BovineLabs.Core.Editor.Inspectors.ElementProperty",
    "BovineLabs.Core.Editor.UI.ObjectSelectionProxy",
    "BovineLabs.Core.Editor.UI.SearchElement",
    "BovineLabs.Core.Utility.Ptr`1",
    "BovineLabs.Core.Memory.Ptr`1",
    "BovineLabs.Core.Iterators.OrderedListIterator",
};

foreach (var typeName in types)
{
    var found = findType(typeName);
    if (found != null)
    {
        var kind = found.IsInterface ? "interface" : found.IsAbstract ? "abstract" : found.IsValueType ? "struct" : "class";
        var gen = found.IsGenericTypeDefinition ? $"<{found.GetGenericArguments().Length}>" : "";
        sb.AppendLine($"FOUND  {kind,10} {found.Name}{gen} ({found.Namespace})");
    }
    else
    {
        // Try just the short name
        var shortName = typeName.Split('.').Last().Split('`').First();
        var byShort = findType(shortName);
        if (byShort != null)
        {
            var kind = byShort.IsInterface ? "interface" : byShort.IsAbstract ? "abstract" : byShort.IsValueType ? "struct" : "class";
            sb.AppendLine($"FOUND* {kind,10} {byShort.FullName} (via short name)");
        }
        else
        {
            sb.AppendLine($"MISS   {typeName}");
        }
    }
}

return sb.ToString();
