# BovineLabs Core - Inner Workings

ASCII architecture diagrams for every topic in `com.bovinelabs.core`.

Source code: [gitlab.com/tertle/com.bovinelabs.core](https://gitlab.com/tertle/com.bovinelabs.core)

Each topic's detailed README with full ASCII diagrams lives in the `docs/` folder.
Click any link below to jump straight to the internals.

## Structure

```
docs/
  NativeThreadStream.md
  NativeCounter.md
  ... (357 topics)
```

## Topics

### Core Collections

- [NativeThreadStream](docs/NativeThreadStream.md)
- [NativeCounter](docs/NativeCounter.md)
- [NativeKeyedMap](docs/NativeKeyedMap.md)
- [NativeLinearCongruentialGenerator](docs/NativeLinearCongruentialGenerator.md)
- [NativeParallelMultiHashMapFallback](docs/NativeParallelMultiHashMapFallback.md)
- [NativePartialKeyedMap](docs/NativePartialKeyedMap.md)
- [ThreadList](docs/ThreadList.md)
- [ThreadRandom](docs/ThreadRandom.md)
- [UnsafeArray](docs/UnsafeArray.md)
- [BitArray256](docs/BitArray256.md)
- [FixedArray](docs/FixedArray.md)
- [NativeHashMapExtensions.GetOrAddRef](docs/NativeHashMapExtensions_GetOrAddRef.md)
- [NativeHashMapExtensions.ClearAndAddBatchUnsafe](docs/NativeHashMapExtensions_ClearAndAddBatchUnsafe.md)
- [NativeListExtensions.ReserveNoResize](docs/NativeListExtensions_ReserveNoResize.md)
- [NativeThreadStreamExTests](docs/NativeThreadStreamExTests.md)
- [BitArray8_16_32_64](docs/BitArray8_16_32_64.md)
- [BitArray128](docs/BitArray128.md)
- [BitArrayUtilities](docs/BitArrayUtilities.md)
- [NativeThreadStream.Reader](docs/NativeThreadStream_Reader.md)
- [NativeThreadStream.Writer](docs/NativeThreadStream_Writer.md)
- [NativeWorkQueue](docs/NativeWorkQueue.md)
- [NativePerfectHashMap](docs/NativePerfectHashMap.md)
- [NativeUntypedHashMap](docs/NativeUntypedHashMap.md)
- [UnsafePartialKeyedMap](docs/UnsafePartialKeyedMap.md)
- [UnsafePerfectHashMap](docs/UnsafePerfectHashMap.md)
- [NativeListExtensions.ClearAddRange](docs/NativeListExtensions_ClearAddRange.md)
- [NativeParallelMultiHashMapExtensions.GetUniqueKeyArray](docs/NativeParallelMultiHashMapExtensions_GetUniqueKeyArray.md)

### Blob System

- [BlobHashMap](docs/BlobHashMap.md)
- [BlobPerfectHashMap](docs/BlobPerfectHashMap.md)
- [BlobCurve](docs/BlobCurve.md)
- [BlobBuilderExtensions](docs/BlobBuilderExtensions.md)
- [BlobHashMapTests](docs/BlobHashMapTests.md)
- [BlobCurve2_3_4](docs/BlobCurve2_3_4.md)
- [BlobCurveCache](docs/BlobCurveCache.md)
- [BlobCurveHeader](docs/BlobCurveHeader.md)
- [BlobCurveSampler](docs/BlobCurveSampler.md)
- [BlobCurveSegment](docs/BlobCurveSegment.md)
- [BlobShared](docs/BlobShared.md)
- [IBlobCurve](docs/IBlobCurve.md)
- [BlobBuilderExtensions.Allocate](docs/BlobBuilderExtensions_Allocate.md)
- [BlobBuilderExtensions.ConstructHashMap](docs/BlobBuilderExtensions_ConstructHashMap.md)
- [BlobBuilderHashMap](docs/BlobBuilderHashMap.md)
- [BlobBuilderMultiHashMap](docs/BlobBuilderMultiHashMap.md)
- [BlobBuilderPerfectHashMap](docs/BlobBuilderPerfectHashMap.md)
- [BlobHashMapData](docs/BlobHashMapData.md)
- [BlobMultiHashMapIterator](docs/BlobMultiHashMapIterator.md)
- [BlobSpline](docs/BlobSpline.md)
- [BlobAssetOwnerInspector](docs/BlobAssetOwnerInspector.md)
- [EntityBlobBakedData](docs/EntityBlobBakedData.md)
- [EntityBlobBakingSystem](docs/EntityBlobBakingSystem.md)

### Memory & Allocators

- [PooledNativeList](docs/PooledNativeList.md)
- [UnmanagedPool](docs/UnmanagedPool.md)
- [UnsafeSlabAllocator](docs/UnsafeSlabAllocator.md)
- [MemoryLabelAllocator](docs/MemoryLabelAllocator.md)
- [MemoryAllocator](docs/MemoryAllocator.md)
- [NoAllocHelpers](docs/NoAllocHelpers.md)
- [UnsafeListPoolTests](docs/UnsafeListPoolTests.md)
- [NativeSlabAllocator](docs/NativeSlabAllocator.md)
- [UnsafeParallelPoolAllocator](docs/UnsafeParallelPoolAllocator.md)
- [UnsafeFixedPoolAllocator](docs/UnsafeFixedPoolAllocator.md)
- [UnsafePoolAllocator](docs/UnsafePoolAllocator.md)
- [NativeArrayExtensions.WhereNoAlloc](docs/NativeArrayExtensions_WhereNoAlloc.md)

### Dynamic Buffers

- [DynamicMultiHashMap](docs/DynamicMultiHashMap.md)
- [DynamicHashSet](docs/DynamicHashSet.md)
- [DynamicUntypedBuffer](docs/DynamicUntypedBuffer.md)
- [DynamicVariableMap](docs/DynamicVariableMap.md)
- [ArchetypeChunk.GetDynamicBufferAccessor](docs/ArchetypeChunk_GetDynamicBufferAccessor.md)
- [DynamicHashMapPerformanceTests](docs/DynamicHashMapPerformanceTests.md)
- [UnsafeUntypedDynamicBuffer](docs/UnsafeUntypedDynamicBuffer.md)
- [UnsafeUntypedDynamicBufferAccessor](docs/UnsafeUntypedDynamicBufferAccessor.md)
- [UntypedDynamicBuffer](docs/UntypedDynamicBuffer.md)
- [DynamicGenerator](docs/DynamicGenerator.md)

### ECS Extensions

- [ArchetypeChunk.DidChange](docs/ArchetypeChunk_DidChange.md)
- [ArchetypeChunk.GetNativeArrayReadOnly](docs/ArchetypeChunk_GetNativeArrayReadOnly.md)
- [BufferAccessor.GetUnsafe](docs/BufferAccessor_GetUnsafe.md)
- [BufferLookup.GetROAndChunk](docs/BufferLookup_GetROAndChunk.md)
- [ComponentLookup.GetOptionalComponentDataRW](docs/ComponentLookup_GetOptionalComponentDataRW.md)
- [ComponentLookup.SetChangeFilter](docs/ComponentLookup_SetChangeFilter.md)
- [EntityQueryBuilder.WithAllRW](docs/EntityQueryBuilder_WithAllRW.md)
- [EntityQuery.QueryHasSharedFilter](docs/EntityQuery_QueryHasSharedFilter.md)
- [EntityQuery.ReplaceSharedComponentFilter](docs/EntityQuery_ReplaceSharedComponentFilter.md)
- [EntityQuery.GetFirstEntity](docs/EntityQuery_GetFirstEntity.md)
- [EntityQuery.GetSingletonBufferNoSync](docs/EntityQuery_GetSingletonBufferNoSync.md)
- [SystemState.GetSingletonEntity](docs/SystemState_GetSingletonEntity.md)
- [SystemState.GetManagedSingleton](docs/SystemState_GetManagedSingleton.md)
- [World.IsClientWorld](docs/World_IsClientWorld.md)
- [CopyEnableable](docs/CopyEnableable.md)
- [TimerEnableable](docs/TimerEnableable.md)
- [StateModelEnableable](docs/StateModelEnableable.md)
- [EnableMaskCreator](docs/EnableMaskCreator.md)
- [EntityLock](docs/EntityLock.md)
- [EntityLockTests](docs/EntityLockTests.md)
- [ChangeFilterTrackingAttribute](docs/ChangeFilterTrackingAttribute.md)
- [TypeManagerEx](docs/TypeManagerEx.md)
- [TypeManagerOverrides](docs/TypeManagerOverrides.md)
- [TypeManagerUtil](docs/TypeManagerUtil.md)
- [TypeUtility](docs/TypeUtility.md)
- [WriteGroupMatcher](docs/WriteGroupMatcher.md)
- [EntityDataAccessExtensions.GetComponentDataWithTypeRW](docs/EntityDataAccessExtensions_GetComponentDataWithTypeRW.md)
- [EntityManagerExtensions.GetChunkBuffer](docs/EntityManagerExtensions_GetChunkBuffer.md)
- [EntityManagerExtensions.GetOrCreateSingletonEntity](docs/EntityManagerExtensions_GetOrCreateSingletonEntity.md)
- [EntityQueryExtensions.GetSingletonUntypedBuffer](docs/EntityQueryExtensions_GetSingletonUntypedBuffer.md)
- [EntitySceneReferenceExtensions.SceneGUID](docs/EntitySceneReferenceExtensions_SceneGUID.md)
- [EntityStorageInfoLookupExtensions.GetNameUnsafe](docs/EntityStorageInfoLookupExtensions_GetNameUnsafe.md)
- [RefRWExtensions.Create](docs/RefRWExtensions_Create.md)
- [SystemStateExtensions.GetUnsafeEntityDataAccess](docs/SystemStateExtensions_GetUnsafeEntityDataAccess.md)
- [ChangeFilterTrackingSystem](docs/ChangeFilterTrackingSystem.md)

### Jobs & Threading

- [IJobParallelForDeferExtensions](docs/IJobParallelForDeferExtensions.md)
- [IJobChunkWorkerBeginEnd](docs/IJobChunkWorkerBeginEnd.md)
- [IJobForThread](docs/IJobForThread.md)
- [IJobHashMapDefer](docs/IJobHashMapDefer.md)
- [IJobParallelForDeferBatch](docs/IJobParallelForDeferBatch.md)
- [IJobParallelForDeferExtensions.Schedule](docs/IJobParallelForDeferExtensions_Schedule.md)

### State & Model

- [TimerFixed](docs/TimerFixed.md)
- [TimerTriggerResetJob](docs/TimerTriggerResetJob.md)
- [StateFlagModel](docs/StateFlagModel.md)
- [StateModelWithHistory](docs/StateModelWithHistory.md)
- [StatefulCollisionEvent](docs/StatefulCollisionEvent.md)
- [StatefulTriggerEvent](docs/StatefulTriggerEvent.md)
- [StatefulCollisionEventClearSystem](docs/StatefulCollisionEventClearSystem.md)
- [StatefulTriggerEventClearSystem](docs/StatefulTriggerEventClearSystem.md)
- [StateFlagModelTests](docs/StateFlagModelTests.md)
- [IState](docs/IState.md)
- [StateAPI](docs/StateAPI.md)
- [StateInstanceUtil](docs/StateInstanceUtil.md)
- [DestroyTimer](docs/DestroyTimer.md)

### Spatial & Physics

- [AabbExtensions](docs/AabbExtensions.md)
- [AlwaysUpdatePhysicsWorld](docs/AlwaysUpdatePhysicsWorld.md)
- [IntersectionTests](docs/IntersectionTests.md)
- [ConvexHullBuilder](docs/ConvexHullBuilder.md)
- [MeshSimplifier](docs/MeshSimplifier.md)
- [TerrainToMesh](docs/TerrainToMesh.md)
- [PhysicsLayerUtil](docs/PhysicsLayerUtil.md)
- [AlwaysUpdatePhysicsWorldSystem](docs/AlwaysUpdatePhysicsWorldSystem.md)
- [PhysicsTags](docs/PhysicsTags.md)
- [LocalSpatialMap](docs/LocalSpatialMap.md)
- [PositionBuilder](docs/PositionBuilder.md)
- [SpatialKeyedMap](docs/SpatialKeyedMap.md)
- [SpatialMap](docs/SpatialMap.md)
- [SpatialMap3](docs/SpatialMap3.md)
- [DistanceHitSortAscending](docs/DistanceHitSortAscending.md)
- [DistanceHitSortDescending](docs/DistanceHitSortDescending.md)
- [PhysicsExtensions.Raycast](docs/PhysicsExtensions_Raycast.md)
- [PhysicsMassOverrideAuthoring](docs/PhysicsMassOverrideAuthoring.md)
- [RemovePhysicsVelocityAuthoring](docs/RemovePhysicsVelocityAuthoring.md)

### Utility

- [Ptr](docs/Ptr.md)
- [BurstTrampoline](docs/BurstTrampoline.md)
- [BurstUtil.IsEmpty](docs/BurstUtil_IsEmpty.md)
- [ButtonEvent](docs/ButtonEvent.md)
- [CurveRemapUtility](docs/CurveRemapUtility.md)
- [DebugUtil.SplitInt](docs/DebugUtil_SplitInt.md)
- [GlobalRandom](docs/GlobalRandom.md)
- [InitSystemBase](docs/InitSystemBase.md)
- [LibraryLoader](docs/LibraryLoader.md)
- [SceneInitializeSystem](docs/SceneInitializeSystem.md)
- [WorldSafeShutdown](docs/WorldSafeShutdown.md)
- [IFixedSize](docs/IFixedSize.md)
- [MiniString](docs/MiniString.md)
- [Pin](docs/Pin.md)
- [QueryEntityEnumerator](docs/QueryEntityEnumerator.md)
- [ReflectionUtility](docs/ReflectionUtility.md)
- [SpinLock](docs/SpinLock.md)
- [TransformUtility](docs/TransformUtility.md)
- [WorldUtility](docs/WorldUtility.md)
- [GhostComponentAttribute](docs/GhostComponentAttribute.md)
- [GhostFieldAttribute](docs/GhostFieldAttribute.md)
- [InitializeAllOnLoadExt](docs/InitializeAllOnLoadExt.md)
- [CloneTransformSystem](docs/CloneTransformSystem.md)

### Extension Methods

- [ListExtensions.AddRangeNative](docs/ListExtensions_AddRangeNative.md)
- [NativeStreamExtensions.WriteLarge](docs/NativeStreamExtensions_WriteLarge.md)
- [EntityCommandBufferExtensions.AddUntypedBuffer](docs/EntityCommandBufferExtensions_AddUntypedBuffer.md)
- [EntityCommandBufferExtensions.UnsafeAddComponent](docs/EntityCommandBufferExtensions_UnsafeAddComponent.md)
- [EnumerableExtensions.IndexOf](docs/EnumerableExtensions_IndexOf.md)
- [GameObjectExtensions.IsPrefab](docs/GameObjectExtensions_IsPrefab.md)
- [NativeArrayExtensions.ElementAtRO](docs/NativeArrayExtensions_ElementAtRO.md)
- [NativeArrayExtensions.Select](docs/NativeArrayExtensions_Select.md)
- [NativeSliceExtensions.ReadArrayElementWithStrideRef](docs/NativeSliceExtensions_ReadArrayElementWithStrideRef.md)
- [NativeStreamExtensions.ReadLarge](docs/NativeStreamExtensions_ReadLarge.md)
- [StringExtensions.ToDotNotation](docs/StringExtensions_ToDotNotation.md)
- [SystemStateExtensions.GetAllSystemDependencies](docs/SystemStateExtensions_GetAllSystemDependencies.md)
- [UnsafeHashMapExtensions.GetOrAddRef](docs/UnsafeHashMapExtensions_GetOrAddRef.md)
- [UnsafeParallelHashMapDataExtensions.ReserveParallel](docs/UnsafeParallelHashMapDataExtensions_ReserveParallel.md)
- [WorldUnmanagedExtensions.GetTrackedJobHandle](docs/WorldUnmanagedExtensions_GetTrackedJobHandle.md)

### ConfigVars

- [KSettingsBase](docs/KSettingsBase.md)
- [ConfigVarAttribute](docs/ConfigVarAttribute.md)
- [ConfigVarManager](docs/ConfigVarManager.md)
- [SharedStaticStringContainer](docs/SharedStaticStringContainer.md)
- [CodecService](docs/CodecService.md)
- [CommandLineArgs](docs/CommandLineArgs.md)
- [Deserializer](docs/Deserializer.md)
- [Serializer](docs/Serializer.md)
- [FixedNameValue](docs/FixedNameValue.md)
- [KAttribute](docs/KAttribute.md)
- [KSettings](docs/KSettings.md)
- [ConfigVarPanel](docs/ConfigVarPanel.md)

### Authoring & Baking

- [BakerExtensions.AddEnabledComponent](docs/BakerExtensions_AddEnabledComponent.md)
- [BakerExtensions.AddEnabledBuffer](docs/BakerExtensions_AddEnabledBuffer.md)
- [BakerCommands](docs/BakerCommands.md)
- [AuthoringSettingsUtility](docs/AuthoringSettingsUtility.md)
- [SettingsAuthoring](docs/SettingsAuthoring.md)
- [TagAuthoring](docs/TagAuthoring.md)
- [TransformAuthoring](docs/TransformAuthoring.md)
- [GameObjectHelper.AddAuthoringComponent](docs/GameObjectHelper_AddAuthoringComponent.md)
- [CloneTransformAuthoring](docs/CloneTransformAuthoring.md)
- [LifeCycleAuthoring](docs/LifeCycleAuthoring.md)
- [LookupAuthoring](docs/LookupAuthoring.md)

### Editor Tools

- [AssemblyBuilderWindow](docs/AssemblyBuilderWindow.md)
- [ComponentAssetBaseDrawer](docs/ComponentAssetBaseDrawer.md)
- [TypeSearchProvider](docs/TypeSearchProvider.md)
- [CoreBuildSetup](docs/CoreBuildSetup.md)
- [CreateEditorWorld](docs/CreateEditorWorld.md)
- [EditorMenus.DataModeHierarchySet](docs/EditorMenus_DataModeHierarchySet.md)
- [InspectorSearch](docs/InspectorSearch.md)
- [SelectedEntityEditorSystem](docs/SelectedEntityEditorSystem.md)
- [AssemblyGraphWindow](docs/AssemblyGraphWindow.md)
- [ComponentDependencyWindow](docs/ComponentDependencyWindow.md)
- [SystemDependencyWindow](docs/SystemDependencyWindow.md)
- [CoreEditorPreferencesProvider](docs/CoreEditorPreferencesProvider.md)
- [BitFieldAttributeEditor](docs/BitFieldAttributeEditor.md)
- [HalfDrawer](docs/HalfDrawer.md)
- [InlineObjectProperty](docs/InlineObjectProperty.md)
- [PrefabElementEditor](docs/PrefabElementEditor.md)
- [StableTypeHashAttributeDrawer](docs/StableTypeHashAttributeDrawer.md)
- [ToggleOption](docs/ToggleOption.md)
- [UnityObjectRefInspector](docs/UnityObjectRefInspector.md)
- [WeakObjectReferenceInspector](docs/WeakObjectReferenceInspector.md)
- [EntitySelection.GetAllSelectionsInWorld](docs/EntitySelection_GetAllSelectionsInWorld.md)
- [LoadPrefabsAsEntities](docs/LoadPrefabsAsEntities.md)
- [ReloadToolbarButton](docs/ReloadToolbarButton.md)
- [WelcomeWindow](docs/WelcomeWindow.md)
- [BaseObjectWindow](docs/BaseObjectWindow.md)
- [FeatureToggle](docs/FeatureToggle.md)
- [MainToolbarPresetPostProcessor](docs/MainToolbarPresetPostProcessor.md)
- [ComponentInspectorWindow](docs/ComponentInspectorWindow.md)
- [StartupSceneSwap](docs/StartupSceneSwap.md)
- [ViewModelToolbar](docs/ViewModelToolbar.md)

### Source Generators

- [FacetAttribute](docs/FacetAttribute.md)
- [FacetOptionalAttribute](docs/FacetOptionalAttribute.md)
- [IFacet](docs/IFacet.md)
- [FacetGenerator](docs/FacetGenerator.md)
- [BuilderBase](docs/BuilderBase.md)
- [ClassBuilder](docs/ClassBuilder.md)
- [CodeBuilder](docs/CodeBuilder.md)
- [ConstructorBuilder](docs/ConstructorBuilder.md)
- [DelegateBuilder](docs/DelegateBuilder.md)
- [EnumBuilder](docs/EnumBuilder.md)
- [EventBuilder](docs/EventBuilder.md)
- [ExpressionBlockBuilder](docs/ExpressionBlockBuilder.md)
- [LogicalConditionBuilder](docs/LogicalConditionBuilder.md)
- [MethodBuilder](docs/MethodBuilder.md)
- [PropertyBuilder](docs/PropertyBuilder.md)
- [RecordBuilder](docs/RecordBuilder.md)
- [SwitchBuilder](docs/SwitchBuilder.md)
- [CodeWriter](docs/CodeWriter.md)
- [SymbolHelpers](docs/SymbolHelpers.md)

### SubScene System

- [SubSceneLoadData](docs/SubSceneLoadData.md)
- [SubSceneEntity](docs/SubSceneEntity.md)
- [LoadSubScene](docs/LoadSubScene.md)
- [SubSceneBuffer](docs/SubSceneBuffer.md)
- [SubSceneLoadFlags](docs/SubSceneLoadFlags.md)
- [SubSceneLoadFlagsUtility](docs/SubSceneLoadFlagsUtility.md)
- [SubSceneLoadUtil](docs/SubSceneLoadUtil.md)
- [SubSceneLoaded](docs/SubSceneLoaded.md)
- [SubSceneLoadingManagedSystem](docs/SubSceneLoadingManagedSystem.md)
- [SubSceneLoadingSystem](docs/SubSceneLoadingSystem.md)
- [SubScenePostLoadCommandBufferSystem](docs/SubScenePostLoadCommandBufferSystem.md)
- [SubSceneSetId](docs/SubSceneSetId.md)
- [SubSceneUtil](docs/SubSceneUtil.md)
- [SubSceneEditorSet](docs/SubSceneEditorSet.md)
- [SubSceneEditorSystem](docs/SubSceneEditorSystem.md)
- [SubSceneEditorToolbar](docs/SubSceneEditorToolbar.md)
- [SubScenePrebakeSystem](docs/SubScenePrebakeSystem.md)
- [DestroyOnSubSceneUnloadSystem](docs/DestroyOnSubSceneUnloadSystem.md)

### Pause & Time

- [LimitedRateNoCatchUpManager](docs/LimitedRateNoCatchUpManager.md)
- [PauseGame](docs/PauseGame.md)
- [PauseLimitSystem](docs/PauseLimitSystem.md)
- [PauseRateManager](docs/PauseRateManager.md)
- [PauseUtility](docs/PauseUtility.md)
- [FixedStepUpdatedSystem](docs/FixedStepUpdatedSystem.md)
- [UpdateWorldTimeSystem](docs/UpdateWorldTimeSystem.md)

### Relevancy & Netcode

- [InputBounds](docs/InputBounds.md)
- [RelevanceAlways](docs/RelevanceAlways.md)
- [RelevanceConfig](docs/RelevanceConfig.md)
- [RelevanceManual](docs/RelevanceManual.md)
- [RelevanceProvider](docs/RelevanceProvider.md)
- [RelevancySystem](docs/RelevancySystem.md)

### Singleton System

- [SingletonAttribute](docs/SingletonAttribute.md)
- [SingletonInitialize](docs/SingletonInitialize.md)
- [SingletonInitializeSystemGroup](docs/SingletonInitializeSystemGroup.md)
- [SingletonInitializedSystem](docs/SingletonInitializedSystem.md)
- [SingletonSystem](docs/SingletonSystem.md)
- [ComponentSystemBaseInternal.RequireSingletonForUpdate](docs/ComponentSystemBaseInternal_RequireSingletonForUpdate.md)
- [ISingletonCollection](docs/ISingletonCollection.md)
- [SingletonCollectionUtil](docs/SingletonCollectionUtil.md)

### Object Management

- [ObjectDefinition](docs/ObjectDefinition.md)
- [ObjectGroupMatcher](docs/ObjectGroupMatcher.md)
- [ObjectId](docs/ObjectId.md)
- [UIDAttribute](docs/UIDAttribute.md)
- [GroupId](docs/GroupId.md)
- [ObjectCategories](docs/ObjectCategories.md)
- [ObjectCategoryComponents](docs/ObjectCategoryComponents.md)
- [ObjectDefinitionRegistrySystem](docs/ObjectDefinitionRegistrySystem.md)
- [ObjectGroupRegistry](docs/ObjectGroupRegistry.md)
- [ObjectInstantiateSystem](docs/ObjectInstantiateSystem.md)
- [ObjectDefinitionAuthoring](docs/ObjectDefinitionAuthoring.md)
- [ObjectInstantiate.Editor](docs/ObjectInstantiate_Editor.md)

### Physics States

- [CalculateEventMapBucketsJob](docs/CalculateEventMapBucketsJob.md)
- [CollectEventsJob](docs/CollectEventsJob.md)
- [EnsureCurrentEventsCapacityJob](docs/EnsureCurrentEventsCapacityJob.md)

### Life Cycle

- [AfterSceneSystemGroup](docs/AfterSceneSystemGroup.md)
- [AfterTransformSystemGroup](docs/AfterTransformSystemGroup.md)
- [BeforeTransformSystemGroup](docs/BeforeTransformSystemGroup.md)
- [BeginSimulationSystemGroup](docs/BeginSimulationSystemGroup.md)
- [InstantiateCommandBufferSystem](docs/InstantiateCommandBufferSystem.md)
- [DestroyEntityCommandBufferSystem](docs/DestroyEntityCommandBufferSystem.md)
- [DestroyEntitySystem](docs/DestroyEntitySystem.md)
- [DestroyOnDestroySystem](docs/DestroyOnDestroySystem.md)
- [EndInitializeEntityCommandBufferSystem](docs/EndInitializeEntityCommandBufferSystem.md)
- [InitializeEntitySystem](docs/InitializeEntitySystem.md)

### Tests & Diagnostics

- [FaceReadonlyTest](docs/FaceReadonlyTest.md)
- [MathExPerformanceTests](docs/MathExPerformanceTests.md)
- [Check.Assume](docs/Check_Assume.md)
- [ReflectionTestHelper](docs/ReflectionTestHelper.md)
- [TestLeakDetectionAttribute](docs/TestLeakDetectionAttribute.md)

### Math Extensions

- [MathematicsExtensions.Encapsulate](docs/MathematicsExtensions_Encapsulate.md)
- [HSV](docs/HSV.md)
- [PolygonUtility](docs/PolygonUtility.md)
- [ShortHalfUnion](docs/ShortHalfUnion.md)
- [IntFloatUnion](docs/IntFloatUnion.md)
- [mathex.mod](docs/mathex_mod.md)
- [mathex.minMax](docs/mathex_minMax.md)
- [mathex.add](docs/mathex_add.md)
- [mathex.GenerateGaussianNoise](docs/mathex_GenerateGaussianNoise.md)
- [mathex.FromToRotation](docs/mathex_FromToRotation.md)
- [MinMaxAttributeDrawer](docs/MinMaxAttributeDrawer.md)

### Other

- [AssetLoad](docs/AssetLoad.md)
- [GameObjectCleanup](docs/GameObjectCleanup.md)
- [HalfSizeTriangleMatrix](docs/HalfSizeTriangleMatrix.md)
- [CalculateCurrentEventsBucketsJob](docs/CalculateCurrentEventsBucketsJob.md)
- [StripLocalAttribute](docs/StripLocalAttribute.md)
- [StripLocalSystem](docs/StripLocalSystem.md)
- [AssetLoadingSystem](docs/AssetLoadingSystem.md)
- [BovineLabsBootstrap](docs/BovineLabsBootstrap.md)
- [BovineLabsBootstrap.NetCode](docs/BovineLabsBootstrap_NetCode.md)
- [CollectionCreator.CreateHashMap](docs/CollectionCreator_CreateHashMap.md)
- [INativeStreamReader](docs/INativeStreamReader.md)
- [UnsafeThreadStreamBlockData](docs/UnsafeThreadStreamBlockData.md)
- [SyncEnableStateUtil](docs/SyncEnableStateUtil.md)
- [TimeProfiler](docs/TimeProfiler.md)
- [ReferenceT](docs/ReferenceT.md)
- [ReferenceData](docs/ReferenceData.md)
- [UnsafeListDispose](docs/UnsafeListDispose.md)
- [AppAPI](docs/AppAPI.md)
- [SerializedHelper.IterateAllChildren](docs/SerializedHelper_IterateAllChildren.md)
- [TextAssetHelper](docs/TextAssetHelper.md)
- [PrefabInstance](docs/PrefabInstance.md)
- [AnalyzersProjectFileGeneration](docs/AnalyzersProjectFileGeneration.md)



### Burst-Compiled Function Pointers

- [IFunction](docs/IFunction.md)
- [FunctionsBuilder](docs/FunctionsBuilder.md)
- [Functions](docs/Functions.md)
- [FunctionsHash](docs/FunctionsHash.md)
- [BurstTrampoline](docs/BurstTrampoline.md)

### Entity Commands Abstraction

- [IEntityCommands](docs/IEntityCommands.md)
- [EntityManagerCommands](docs/EntityManagerCommands.md)
- [CommandBufferCommands](docs/CommandBufferCommands.md)
- [CommandBufferParallelCommands](docs/CommandBufferParallelCommands.md)

### Component & Type Assets

- [ComponentAssetBase](docs/ComponentAssetBase.md)
- [ComponentAsset](docs/ComponentAsset.md)
- [EnableableComponentAsset](docs/EnableableComponentAsset.md)
- [ComponentFieldAsset](docs/ComponentFieldAsset.md)
- [TypeAsset](docs/TypeAsset.md)

### Unmanaged Lookups & Caching

- [EntityCache](docs/EntityCache.md)
- [UnsafeComponentLookup](docs/UnsafeComponentLookup.md)
- [UnsafeBufferLookup](docs/UnsafeBufferLookup.md)
- [UnsafeEntityDataAccess](docs/UnsafeEntityDataAccess.md)
- [UnsafeEnableableLookup](docs/UnsafeEnableableLookup.md)
- [ChangeFilterLookup](docs/ChangeFilterLookup.md)
- [SharedComponentLookup](docs/SharedComponentLookup.md)

### Settings Singleton Framework

- [SettingsSingleton](docs/SettingsSingleton.md)
- [SettingsAttributes](docs/SettingsAttributes.md)

### Dynamic Variable Map Columns

- [IColumn](docs/IColumn.md)
- [MultiHashColumn](docs/MultiHashColumn.md)
- [OrderedListColumn](docs/OrderedListColumn.md)

### Custom Chunk Iteration

- [QueryEntityEnumerator](docs/QueryEntityEnumerator.md)
- [CustomChunkIterator](docs/CustomChunkIterator.md)

### Reference Wrappers & Utilities

- [Reference](docs/Reference.md)
- [Ptr](docs/Ptr.md)
- [RuntimeContentCatalogUtility](docs/RuntimeContentCatalogUtility.md)

### Custom UI Elements & Inspectors

- [ElementEditor](docs/ElementEditor.md)
- [ObjectSelectionProxy](docs/ObjectSelectionProxy.md)
- [SearchElement](docs/SearchElement.md)

---

Total: 388 topics across 31 categories

## Examples

Runnable code examples with unit tests live in the `Example/` folder.
Each example is a self-contained `.cs` file demonstrating real-world usage patterns.
See `Example/TODO.md` for progress.
