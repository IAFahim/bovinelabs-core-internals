# MainToolbarPresetPostProcessor

## Inner Workings Diagram

```
 MainToolbarPresetPostProcessor
 ======================================================================
 Defined as: MainToolbarPresetPostProcessor
 Namespace:  BovineLabs.Core.Extensions.CodeGen


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetInstance()                                                      │
 │   → ILPostProcessor                                                │
 │ WillProcess(ICompiledAssembly compiledAssembly)                    │
 │   → bool                                                           │
 │ Process(ICompiledAssembly compiledAssembly)                        │
 │   → ILPostProcessRe                                                │
 │ Dispose()                                                          │
 │   → void                                                           │
 │ Resolve(AssemblyNameReference name)                                │
 │   → AssemblyDefinit                                                │
 │ Resolve(AssemblyNameReference name, ReaderParameters param)        │
 │   → AssemblyDefinit                                                │
 │ AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefin
 │   → void                                                           │
 │ GetReflectionImporter(ModuleDefinition module)                     │
 │   → IReflectionImpo                                                │
 │ ImportReference(AssemblyName reference)                            │
 │   → AssemblyNameRef                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions.CodeGen/MainToolbarPresetPostProcessor.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.CodeGen/MainToolbarPresetPostProcessor.cs)
