1. **Delete the bad generator script**: If you used a Python/Node script to auto-generate these snippets based on reading the Markdown, discard it. It is fundamentally flawed.
2. **Fix the Package Dependencies**: Open the Unity project and install Splines so those exist
3. **Write a custom test runner for Extensions**: Write a C# script that parses the actual `.cs` source files to get the exact class names, rather than trying to guess them from Markdown titles.
4. **Manually write the Core functional tests**: Pick the top 20 most important files (Allocators, Dynamic Buffers, MathEx, ThreadStreams) and manually write `unity-cli` scripts that execute the logic. (You already did this successfully for `NativeHashSet` and `UnmanagedPool`!).


### 🎯 Burst-Compiled Function Pointers (`BovineLabs.Core.Functions`)
- [ ] `docs/IFunction.md` - Document the base interface for defining Burst-compatible functions and the required unmanaged delegates.
- [ ] `docs/FunctionsBuilder.md` - Explain how the builder pattern uses reflection to gather `IFunction` implementations and compiles them via `BurstCompiler.CompileFunctionPointer`.
- [ ] `docs/Functions.md` - Document the `Functions<T, TO>` native container and how it executes function pointers inside ECS jobs.
- [ ] `docs/FunctionsHash.md` - Document the `FunctionsHash<T, TO>` variant for hash-based dynamic dispatch.
- [ ] `docs/BurstTrampoline.md` - Explain the low-level wrapper handling unmanaged delegates and `Cdecl` calling conventions, along with `FunctionData`.

### 🎯 Entity Commands Abstraction (`BovineLabs.Core.EntityCommands`)
- [ ] `docs/IEntityCommands.md` - Explain the core interface for abstracting structural changes.
- [ ] `docs/EntityManagerCommands.md` - Document the immediate execution implementation using `EntityManager`.
- [ ] `docs/CommandBufferCommands.md` - Document the deferred execution implementation using `EntityCommandBuffer`.
- [ ] `docs/CommandBufferParallelCommands.md` - Document the deferred parallel execution implementation using `EntityCommandBuffer.ParallelWriter`.

### 🎯 Component & Type Assets (`BovineLabs.Core.Component`)
- [ ] `docs/ComponentAssetBase.md` - Document the base ScriptableObject for representing ECS components and retrieving `StableTypeHash`.
- [ ] `docs/ComponentAsset.md` - Explain standard component assets.
- [ ] `docs/EnableableComponentAsset.md` - Explain the validation logic ensuring the target type implements `IEnableableComponent`.
- [ ] `docs/ComponentFieldAsset.md` - Explain how this asset retrieves field offsets using `UnsafeUtility.GetFieldOffset`.
- [ ] `docs/TypeAsset.md` - Document the generic asset representing a C# Type.

### 🎯 Unmanaged Lookups & Caching (`BovineLabs.Core.Iterators`)
- [ ] `docs/EntityCache.md` - Explain how to use this struct to cache Archetype/Chunk lookups to rapidly fetch multiple components via raw pointers (`GetOptionalComponentDataWithTypeRO`).
- [ ] `docs/UnsafeComponentLookup.md` - Document the unmanaged, safety-bypassing alternative to `ComponentLookup<T>`.
- [ ] `docs/UnsafeBufferLookup.md` - Document the unmanaged, safety-bypassing alternative to `BufferLookup<T>`.
- [ ] `docs/UnsafeEntityDataAccess.md` - Explain the wrapper around `EntityDataAccess` for raw chunk and pointer manipulation.
- [ ] `docs/UnsafeEnableableLookup.md` - Document the unmanaged wrapper for checking and setting enableable components.
- [ ] `docs/ChangeFilterLookup.md` - Explain how to manually check and set chunk change versions.
- [ ] `docs/SharedComponentLookup.md` - Document `SharedComponentLookup<T>` and `SharedComponentDataFromIndex<T>`.

### 🎯 Settings Singleton Framework (`BovineLabs.Core.Settings`)
- [ ] `docs/SettingsSingleton.md` - Explain the `ScriptableObject` framework that automatically loads and initializes settings before the Unity splash screen.
- [ ] `docs/SettingsAttributes.md` - Group documentation for `[SettingsGroup]`, `[SettingSubDirectory]`, and `[SettingsWorld]` and how they format the Editor UI and target specific ECS worlds.

### 🎯 Dynamic Variable Map Columns (`BovineLabs.Core.Iterators.Columns`)
- [ ] `docs/IColumn.md` - Document the interface defining how secondary data is mapped alongside hashmap keys.
- [ ] `docs/MultiHashColumn.md` - Explain how this column allows multiple entries with the same column value.
- [ ] `docs/OrderedListColumn.md` - Explain how this column maintains its elements in a sorted linked-list manner for rapid sequential access.

### 🎯 Custom Chunk Iteration (`BovineLabs.Core.Utility` / `Iterators`)
- [ ] `docs/QueryEntityEnumerator.md` - Explain how to manually iterate entities inside an `EntityQuery` while respecting `Enableable` masks.
- [ ] `docs/CustomChunkIterator.md` - Document the use of `v128` bitmask intrinsics (`EnabledBitUtility`) to rapidly iterate over valid entities in a chunk.

### 🎯 Reference Wrappers & Utilities (`BovineLabs.Core.Collections` / `Internal`)
- [ ] `docs/Reference.md` - Document `Reference<T>` and `ReferenceData` as unmanaged alternatives to `BlobAssetReference` for standard `MemoryAllocator` use.
- [ ] `docs/Ptr.md` - Explain the safe-ish unmanaged struct wrapper around raw `void*` pointers.
- [ ] `docs/RuntimeContentCatalogUtility.md` - Document the utility used to parse Unity's internal Content Catalogs to dynamically fetch SubScenes.

### 🎯 Custom UI Elements & Inspectors (`BovineLabs.Core.Editor`)
- [ ] `docs/ElementEditor.md` - Document `ElementEditor` and `ElementProperty` for building complex PropertyDrawers for ECS components.
- [ ] `docs/ObjectSelectionProxy.md` - Explain the hack/utility that allows raw C# objects/structs to be selected and drawn in the Unity Inspector.
- [ ] `docs/SearchElement.md` - Document `SearchElement` and `SearchView` dropdown UIs used for searching ECS types, components, and variables.



### 🎯 Burst-Compiled Function Pointers (`BovineLabs.Core.Functions`)
- [ ] `docs/IFunction.md` - Document the base interface for defining Burst-compatible functions and the required unmanaged delegates.
- [ ] `docs/FunctionsBuilder.md` - Explain how the builder pattern uses reflection to gather `IFunction` implementations and compiles them via `BurstCompiler.CompileFunctionPointer`.
- [ ] `docs/Functions.md` - Document the `Functions<T, TO>` native container and how it executes function pointers inside ECS jobs.
- [ ] `docs/FunctionsHash.md` - Document the `FunctionsHash<T, TO>` variant for hash-based dynamic dispatch.
- [ ] `docs/BurstTrampoline.md` - Explain the low-level wrapper handling unmanaged delegates and `Cdecl` calling conventions, along with `FunctionData`.

### 🎯 Entity Commands Abstraction (`BovineLabs.Core.EntityCommands`)
- [ ] `docs/IEntityCommands.md` - Explain the core interface for abstracting structural changes.
- [ ] `docs/EntityManagerCommands.md` - Document the immediate execution implementation using `EntityManager`.
- [ ] `docs/CommandBufferCommands.md` - Document the deferred execution implementation using `EntityCommandBuffer`.
- [ ] `docs/CommandBufferParallelCommands.md` - Document the deferred parallel execution implementation using `EntityCommandBuffer.ParallelWriter`.

### 🎯 Component & Type Assets (`BovineLabs.Core.Component`)
- [ ] `docs/ComponentAssetBase.md` - Document the base ScriptableObject for representing ECS components and retrieving `StableTypeHash`.
- [ ] `docs/ComponentAsset.md` - Explain standard component assets.
- [ ] `docs/EnableableComponentAsset.md` - Explain the validation logic ensuring the target type implements `IEnableableComponent`.
- [ ] `docs/ComponentFieldAsset.md` - Explain how this asset retrieves field offsets using `UnsafeUtility.GetFieldOffset`.
- [ ] `docs/TypeAsset.md` - Document the generic asset representing a C# Type.

### 🎯 Unmanaged Lookups & Caching (`BovineLabs.Core.Iterators`)
- [ ] `docs/EntityCache.md` - Explain how to use this struct to cache Archetype/Chunk lookups to rapidly fetch multiple components via raw pointers (`GetOptionalComponentDataWithTypeRO`).
- [ ] `docs/UnsafeComponentLookup.md` - Document the unmanaged, safety-bypassing alternative to `ComponentLookup<T>`.
- [ ] `docs/UnsafeBufferLookup.md` - Document the unmanaged, safety-bypassing alternative to `BufferLookup<T>`.
- [ ] `docs/UnsafeEntityDataAccess.md` - Explain the wrapper around `EntityDataAccess` for raw chunk and pointer manipulation.
- [ ] `docs/UnsafeEnableableLookup.md` - Document the unmanaged wrapper for checking and setting enableable components.
- [ ] `docs/ChangeFilterLookup.md` - Explain how to manually check and set chunk change versions.
- [ ] `docs/SharedComponentLookup.md` - Document `SharedComponentLookup<T>` and `SharedComponentDataFromIndex<T>`.

### 🎯 Settings Singleton Framework (`BovineLabs.Core.Settings`)
- [ ] `docs/SettingsSingleton.md` - Explain the `ScriptableObject` framework that automatically loads and initializes settings before the Unity splash screen.
- [ ] `docs/SettingsAttributes.md` - Group documentation for `[SettingsGroup]`, `[SettingSubDirectory]`, and `[SettingsWorld]` and how they format the Editor UI and target specific ECS worlds.

### 🎯 Dynamic Variable Map Columns (`BovineLabs.Core.Iterators.Columns`)
- [ ] `docs/IColumn.md` - Document the interface defining how secondary data is mapped alongside hashmap keys.
- [ ] `docs/MultiHashColumn.md` - Explain how this column allows multiple entries with the same column value.
- [ ] `docs/OrderedListColumn.md` - Explain how this column maintains its elements in a sorted linked-list manner for rapid sequential access.

### 🎯 Custom Chunk Iteration (`BovineLabs.Core.Utility` / `Iterators`)
- [ ] `docs/QueryEntityEnumerator.md` - Explain how to manually iterate entities inside an `EntityQuery` while respecting `Enableable` masks.
- [ ] `docs/CustomChunkIterator.md` - Document the use of `v128` bitmask intrinsics (`EnabledBitUtility`) to rapidly iterate over valid entities in a chunk.

### 🎯 Reference Wrappers & Utilities (`BovineLabs.Core.Collections` / `Internal`)
- [ ] `docs/Reference.md` - Document `Reference<T>` and `ReferenceData` as unmanaged alternatives to `BlobAssetReference` for standard `MemoryAllocator` use.
- [ ] `docs/Ptr.md` - Explain the safe-ish unmanaged struct wrapper around raw `void*` pointers.
- [ ] `docs/RuntimeContentCatalogUtility.md` - Document the utility used to parse Unity's internal Content Catalogs to dynamically fetch SubScenes.

### 🎯 Custom UI Elements & Inspectors (`BovineLabs.Core.Editor`)
- [ ] `docs/ElementEditor.md` - Document `ElementEditor` and `ElementProperty` for building complex PropertyDrawers for ECS components.
- [ ] `docs/ObjectSelectionProxy.md` - Explain the hack/utility that allows raw C# objects/structs to be selected and drawn in the Unity Inspector.
- [ ] `docs/SearchElement.md` - Document `SearchElement` and `SearchView` dropdown UIs used for searching ECS types, components, and variables.
