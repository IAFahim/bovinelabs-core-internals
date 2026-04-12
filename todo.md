### ✅ Burst-Compiled Function Pointers (`BovineLabs.Core.Functions`)
- [x] `docs/IFunction.md` - Document the base interface for defining Burst-compatible functions and the required unmanaged delegates.
- [x] `docs/FunctionsBuilder.md` - Explain how the builder pattern uses reflection to gather `IFunction` implementations and compiles them via `BurstCompiler.CompileFunctionPointer`.
- [x] `docs/Functions.md` - Document the `Functions<T, TO>` native container and how it executes function pointers inside ECS jobs.
- [x] `docs/FunctionsHash.md` - Document the `FunctionsHash<T, TO>` variant for hash-based dynamic dispatch.
- [x] `docs/BurstTrampoline.md` - Explain the low-level wrapper handling unmanaged delegates and `Cdecl` calling conventions, along with `FunctionData`.

### ✅ Entity Commands Abstraction (`BovineLabs.Core.EntityCommands`)
- [x] `docs/IEntityCommands.md` - Explain the core interface for abstracting structural changes.
- [x] `docs/EntityManagerCommands.md` - Document the immediate execution implementation using `EntityManager`.
- [x] `docs/CommandBufferCommands.md` - Document the deferred execution implementation using `EntityCommandBuffer`.
- [x] `docs/CommandBufferParallelCommands.md` - Document the deferred parallel execution implementation using `EntityCommandBuffer.ParallelWriter`.

### ✅ Component & Type Assets (`BovineLabs.Core.Component`)
- [x] `docs/ComponentAssetBase.md` - Document the base ScriptableObject for representing ECS components and retrieving `StableTypeHash`.
- [x] `docs/ComponentAsset.md` - Explain standard component assets.
- [x] `docs/EnableableComponentAsset.md` - Explain the validation logic ensuring the target type implements `IEnableableComponent`.
- [x] `docs/ComponentFieldAsset.md` - Explain how this asset retrieves field offsets using `UnsafeUtility.GetFieldOffset`.
- [x] `docs/TypeAsset.md` - Document the generic asset representing a C# Type.

### ✅ Unmanaged Lookups & Caching (`BovineLabs.Core.Iterators`)
- [x] `docs/EntityCache.md` - Explain how to use this struct to cache Archetype/Chunk lookups to rapidly fetch multiple components via raw pointers (`GetOptionalComponentDataWithTypeRO`).
- [x] `docs/UnsafeComponentLookup.md` - Document the unmanaged, safety-bypassing alternative to `ComponentLookup<T>`.
- [x] `docs/UnsafeBufferLookup.md` - Document the unmanaged, safety-bypassing alternative to `BufferLookup<T>`.
- [x] `docs/UnsafeEntityDataAccess.md` - Explain the wrapper around `EntityDataAccess` for raw chunk and pointer manipulation.
- [x] `docs/UnsafeEnableableLookup.md` - Document the unmanaged wrapper for checking and setting enableable components.
- [x] `docs/ChangeFilterLookup.md` - Explain how to manually check and set chunk change versions.
- [x] `docs/SharedComponentLookup.md` - Document `SharedComponentLookup<T>` and `SharedComponentDataFromIndex<T>`.

### ✅ Settings Singleton Framework (`BovineLabs.Core.Settings`)
- [x] `docs/SettingsSingleton.md` - Explain the `ScriptableObject` framework that automatically loads and initializes settings before the Unity splash screen.
- [x] `docs/SettingsAttributes.md` - Group documentation for `[SettingsGroup]`, `[SettingSubDirectory]`, and `[SettingsWorld]` and how they format the Editor UI and target specific ECS worlds.

### ✅ Dynamic Variable Map Columns (`BovineLabs.Core.Iterators.Columns`)
- [x] `docs/IColumn.md` - Document the interface defining how secondary data is mapped alongside hashmap keys.
- [x] `docs/MultiHashColumn.md` - Explain how this column allows multiple entries with the same column value.
- [x] `docs/OrderedListColumn.md` - Explain how this column maintains its elements in a sorted linked-list manner for rapid sequential access.

### ✅ Custom Chunk Iteration (`BovineLabs.Core.Utility` / `Iterators`)
- [x] `docs/QueryEntityEnumerator.md` - Explain how to manually iterate entities inside an `EntityQuery` while respecting `Enableable` masks.
- [x] `docs/CustomChunkIterator.md` - Document the use of `v128` bitmask intrinsics (`EnabledBitUtility`) to rapidly iterate over valid entities in a chunk.

### ✅ Reference Wrappers & Utilities (`BovineLabs.Core.Collections` / `Internal`)
- [x] `docs/Reference.md` - Document `Reference<T>` and `ReferenceData` as unmanaged alternatives to `BlobAssetReference` for standard `MemoryAllocator` use.
- [x] `docs/Ptr.md` - Explain the safe-ish unmanaged struct wrapper around raw `void*` pointers.
- [x] `docs/RuntimeContentCatalogUtility.md` - Document the utility used to parse Unity's internal Content Catalogs to dynamically fetch SubScenes.

### ✅ Custom UI Elements & Inspectors (`BovineLabs.Core.Editor`)
- [x] `docs/ElementEditor.md` - Document `ElementEditor` and `ElementProperty` for building complex PropertyDrawers for ECS components.
- [x] `docs/ObjectSelectionProxy.md` - Explain the hack/utility that allows raw C# objects/structs to be selected and drawn in the Unity Inspector.
- [x] `docs/SearchElement.md` - Document `SearchElement` and `SearchView` dropdown UIs used for searching ECS types, components, and variables.
