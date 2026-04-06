# BovineLabs Core - Inner Workings

ASCII architecture diagrams for every topic in `com.bovinelabs.core`.

Each topic lives on its own branch. Switch to a branch to see the detailed
README.md with full ASCII diagrams explaining the internal data structures,
algorithms, and design decisions.

## How to Use

```bash
# List all topic branches
git branch -a

# View a specific topic
git checkout topic/NativeThreadStream

# View the diagram
cat README.md
```

## Topics

### Core Collections

- [NativeThreadStream](../../tree/topic/NativeThreadStream)
- [NativeCounter](../../tree/topic/NativeCounter)
- [NativeKeyedMap](../../tree/topic/NativeKeyedMap)
- [NativeLinearCongruentialGenerator](../../tree/topic/NativeLinearCongruentialGenerator)
- [NativeParallelMultiHashMapFallback](../../tree/topic/NativeParallelMultiHashMapFallback)
- [NativePartialKeyedMap](../../tree/topic/NativePartialKeyedMap)
- [ThreadList](../../tree/topic/ThreadList)
- [ThreadRandom](../../tree/topic/ThreadRandom)
- [UnsafeArray](../../tree/topic/UnsafeArray)
- [BitArray256](../../tree/topic/BitArray256)
- [FixedArray](../../tree/topic/FixedArray)
- [NativeHashMapExtensions.GetOrAddRef](../../tree/topic/NativeHashMapExtensions_GetOrAddRef)
- [NativeHashMapExtensions.ClearAndAddBatchUnsafe](../../tree/topic/NativeHashMapExtensions_ClearAndAddBatchUnsafe)
- [NativeListExtensions.ReserveNoResize](../../tree/topic/NativeListExtensions_ReserveNoResize)
- [NativeThreadStreamExTests](../../tree/topic/NativeThreadStreamExTests)
- [BitArray8_16_32_64](../../tree/topic/BitArray8_16_32_64)
- [BitArray128](../../tree/topic/BitArray128)
- [BitArrayUtilities](../../tree/topic/BitArrayUtilities)
- [NativeThreadStream.Reader](../../tree/topic/NativeThreadStream_Reader)
- [NativeThreadStream.Writer](../../tree/topic/NativeThreadStream_Writer)
- [NativeWorkQueue](../../tree/topic/NativeWorkQueue)
- [NativePerfectHashMap](../../tree/topic/NativePerfectHashMap)
- [NativeUntypedHashMap](../../tree/topic/NativeUntypedHashMap)
- [UnsafePartialKeyedMap](../../tree/topic/UnsafePartialKeyedMap)
- [UnsafePerfectHashMap](../../tree/topic/UnsafePerfectHashMap)
- [NativeListExtensions.ClearAddRange](../../tree/topic/NativeListExtensions_ClearAddRange)
- [NativeParallelMultiHashMapExtensions.GetUniqueKeyArray](../../tree/topic/NativeParallelMultiHashMapExtensions_GetUniqueKeyArray)

### Blob System

- [BlobHashMap](../../tree/topic/BlobHashMap)
- [BlobPerfectHashMap](../../tree/topic/BlobPerfectHashMap)
- [BlobCurve](../../tree/topic/BlobCurve)
- [BlobBuilderExtensions](../../tree/topic/BlobBuilderExtensions)
- [BlobHashMapTests](../../tree/topic/BlobHashMapTests)
- [BlobCurve2_3_4](../../tree/topic/BlobCurve2_3_4)
- [BlobCurveCache](../../tree/topic/BlobCurveCache)
- [BlobCurveHeader](../../tree/topic/BlobCurveHeader)
- [BlobCurveSampler](../../tree/topic/BlobCurveSampler)
- [BlobCurveSegment](../../tree/topic/BlobCurveSegment)
- [BlobShared](../../tree/topic/BlobShared)
- [IBlobCurve](../../tree/topic/IBlobCurve)
- [BlobBuilderExtensions.Allocate](../../tree/topic/BlobBuilderExtensions_Allocate)
- [BlobBuilderExtensions.ConstructHashMap](../../tree/topic/BlobBuilderExtensions_ConstructHashMap)
- [BlobBuilderHashMap](../../tree/topic/BlobBuilderHashMap)
- [BlobBuilderMultiHashMap](../../tree/topic/BlobBuilderMultiHashMap)
- [BlobBuilderPerfectHashMap](../../tree/topic/BlobBuilderPerfectHashMap)
- [BlobHashMapData](../../tree/topic/BlobHashMapData)
- [BlobMultiHashMapIterator](../../tree/topic/BlobMultiHashMapIterator)
- [BlobSpline](../../tree/topic/BlobSpline)
- [BlobAssetOwnerInspector](../../tree/topic/BlobAssetOwnerInspector)
- [EntityBlobBakedData](../../tree/topic/EntityBlobBakedData)
- [EntityBlobBakingSystem](../../tree/topic/EntityBlobBakingSystem)

### Memory & Allocators

- [PooledNativeList](../../tree/topic/PooledNativeList)
- [UnmanagedPool](../../tree/topic/UnmanagedPool)
- [UnsafeSlabAllocator](../../tree/topic/UnsafeSlabAllocator)
- [MemoryLabelAllocator](../../tree/topic/MemoryLabelAllocator)
- [MemoryAllocator](../../tree/topic/MemoryAllocator)
- [NoAllocHelpers](../../tree/topic/NoAllocHelpers)
- [UnsafeListPoolTests](../../tree/topic/UnsafeListPoolTests)
- [NativeSlabAllocator](../../tree/topic/NativeSlabAllocator)
- [UnsafeParallelPoolAllocator](../../tree/topic/UnsafeParallelPoolAllocator)
- [UnsafeFixedPoolAllocator](../../tree/topic/UnsafeFixedPoolAllocator)
- [UnsafePoolAllocator](../../tree/topic/UnsafePoolAllocator)
- [NativeArrayExtensions.WhereNoAlloc](../../tree/topic/NativeArrayExtensions_WhereNoAlloc)

### Dynamic Buffers

- [Hydrodynamics](../../tree/topic/Hydrodynamics)
- [DynamicMultiHashMap](../../tree/topic/DynamicMultiHashMap)
- [DynamicHashSet](../../tree/topic/DynamicHashSet)
- [DynamicUntypedBuffer](../../tree/topic/DynamicUntypedBuffer)
- [DynamicVariableMap](../../tree/topic/DynamicVariableMap)
- [ArchetypeChunk.GetDynamicBufferAccessor](../../tree/topic/ArchetypeChunk_GetDynamicBufferAccessor)
- [DynamicHashMapPerformanceTests](../../tree/topic/DynamicHashMapPerformanceTests)
- [UnsafeUntypedDynamicBuffer](../../tree/topic/UnsafeUntypedDynamicBuffer)
- [UnsafeUntypedDynamicBufferAccessor](../../tree/topic/UnsafeUntypedDynamicBufferAccessor)
- [UntypedDynamicBuffer](../../tree/topic/UntypedDynamicBuffer)
- [DynamicGenerator](../../tree/topic/DynamicGenerator)

### ECS Extensions

- [ArchetypeChunk.DidChange](../../tree/topic/ArchetypeChunk_DidChange)
- [ArchetypeChunk.GetNativeArrayReadOnly](../../tree/topic/ArchetypeChunk_GetNativeArrayReadOnly)
- [BufferAccessor.GetUnsafe](../../tree/topic/BufferAccessor_GetUnsafe)
- [BufferLookup.GetROAndChunk](../../tree/topic/BufferLookup_GetROAndChunk)
- [ComponentLookup.GetOptionalComponentDataRW](../../tree/topic/ComponentLookup_GetOptionalComponentDataRW)
- [ComponentLookup.SetChangeFilter](../../tree/topic/ComponentLookup_SetChangeFilter)
- [EntityQueryBuilder.WithAllRW](../../tree/topic/EntityQueryBuilder_WithAllRW)
- [EntityQuery.QueryHasSharedFilter](../../tree/topic/EntityQuery_QueryHasSharedFilter)
- [EntityQuery.ReplaceSharedComponentFilter](../../tree/topic/EntityQuery_ReplaceSharedComponentFilter)
- [EntityQuery.GetFirstEntity](../../tree/topic/EntityQuery_GetFirstEntity)
- [EntityQuery.GetSingletonBufferNoSync](../../tree/topic/EntityQuery_GetSingletonBufferNoSync)
- [SystemState.GetSingletonEntity](../../tree/topic/SystemState_GetSingletonEntity)
- [SystemState.GetManagedSingleton](../../tree/topic/SystemState_GetManagedSingleton)
- [World.IsClientWorld](../../tree/topic/World_IsClientWorld)
- [CopyEnableable](../../tree/topic/CopyEnableable)
- [TimerEnableable](../../tree/topic/TimerEnableable)
- [StateModelEnableable](../../tree/topic/StateModelEnableable)
- [EnableMaskCreator](../../tree/topic/EnableMaskCreator)
- [EntityLock](../../tree/topic/EntityLock)
- [EntityLockTests](../../tree/topic/EntityLockTests)
- [ChangeFilterTrackingAttribute](../../tree/topic/ChangeFilterTrackingAttribute)
- [TypeManagerEx](../../tree/topic/TypeManagerEx)
- [TypeManagerOverrides](../../tree/topic/TypeManagerOverrides)
- [TypeManagerUtil](../../tree/topic/TypeManagerUtil)
- [TypeUtility](../../tree/topic/TypeUtility)
- [WriteGroupMatcher](../../tree/topic/WriteGroupMatcher)
- [EntityDataAccessExtensions.GetComponentDataWithTypeRW](../../tree/topic/EntityDataAccessExtensions_GetComponentDataWithTypeRW)
- [EntityManagerExtensions.GetChunkBuffer](../../tree/topic/EntityManagerExtensions_GetChunkBuffer)
- [EntityManagerExtensions.GetOrCreateSingletonEntity](../../tree/topic/EntityManagerExtensions_GetOrCreateSingletonEntity)
- [EntityQueryExtensions.GetSingletonUntypedBuffer](../../tree/topic/EntityQueryExtensions_GetSingletonUntypedBuffer)
- [EntitySceneReferenceExtensions.SceneGUID](../../tree/topic/EntitySceneReferenceExtensions_SceneGUID)
- [EntityStorageInfoLookupExtensions.GetNameUnsafe](../../tree/topic/EntityStorageInfoLookupExtensions_GetNameUnsafe)
- [RefRWExtensions.Create](../../tree/topic/RefRWExtensions_Create)
- [SystemStateExtensions.GetUnsafeEntityDataAccess](../../tree/topic/SystemStateExtensions_GetUnsafeEntityDataAccess)
- [ChangeFilterTrackingSystem](../../tree/topic/ChangeFilterTrackingSystem)

### Jobs & Threading

- [IJobParallelForDeferExtensions](../../tree/topic/IJobParallelForDeferExtensions)
- [IJobChunkWorkerBeginEnd](../../tree/topic/IJobChunkWorkerBeginEnd)
- [IJobForThread](../../tree/topic/IJobForThread)
- [IJobHashMapDefer](../../tree/topic/IJobHashMapDefer)
- [IJobParallelForDeferBatch](../../tree/topic/IJobParallelForDeferBatch)
- [IJobParallelForDeferExtensions.Schedule](../../tree/topic/IJobParallelForDeferExtensions_Schedule)

### State & Model

- [TimerFixed](../../tree/topic/TimerFixed)
- [TimerTriggerResetJob](../../tree/topic/TimerTriggerResetJob)
- [StateFlagModel](../../tree/topic/StateFlagModel)
- [StateModelWithHistory](../../tree/topic/StateModelWithHistory)
- [StatefulCollisionEvent](../../tree/topic/StatefulCollisionEvent)
- [StatefulTriggerEvent](../../tree/topic/StatefulTriggerEvent)
- [StatefulCollisionEventClearSystem](../../tree/topic/StatefulCollisionEventClearSystem)
- [StatefulTriggerEventClearSystem](../../tree/topic/StatefulTriggerEventClearSystem)
- [StateFlagModelTests](../../tree/topic/StateFlagModelTests)
- [IState](../../tree/topic/IState)
- [StateAPI](../../tree/topic/StateAPI)
- [StateInstanceUtil](../../tree/topic/StateInstanceUtil)
- [DestroyTimer](../../tree/topic/DestroyTimer)

### Spatial & Physics

- [AabbExtensions](../../tree/topic/AabbExtensions)
- [AlwaysUpdatePhysicsWorld](../../tree/topic/AlwaysUpdatePhysicsWorld)
- [IntersectionTests](../../tree/topic/IntersectionTests)
- [ConvexHullBuilder](../../tree/topic/ConvexHullBuilder)
- [MeshSimplifier](../../tree/topic/MeshSimplifier)
- [TerrainToMesh](../../tree/topic/TerrainToMesh)
- [PhysicsLayerUtil](../../tree/topic/PhysicsLayerUtil)
- [AlwaysUpdatePhysicsWorldSystem](../../tree/topic/AlwaysUpdatePhysicsWorldSystem)
- [PhysicsTags](../../tree/topic/PhysicsTags)
- [LocalSpatialMap](../../tree/topic/LocalSpatialMap)
- [PositionBuilder](../../tree/topic/PositionBuilder)
- [SpatialKeyedMap](../../tree/topic/SpatialKeyedMap)
- [SpatialMap](../../tree/topic/SpatialMap)
- [SpatialMap3](../../tree/topic/SpatialMap3)
- [DistanceHitSortAscending](../../tree/topic/DistanceHitSortAscending)
- [DistanceHitSortDescending](../../tree/topic/DistanceHitSortDescending)
- [PhysicsExtensions.Raycast](../../tree/topic/PhysicsExtensions_Raycast)
- [PhysicsMassOverrideAuthoring](../../tree/topic/PhysicsMassOverrideAuthoring)
- [RemovePhysicsVelocityAuthoring](../../tree/topic/RemovePhysicsVelocityAuthoring)

### Utility

- [Ptr](../../tree/topic/Ptr)
- [BurstTrampoline](../../tree/topic/BurstTrampoline)
- [BurstUtil.IsEmpty](../../tree/topic/BurstUtil_IsEmpty)
- [ButtonEvent](../../tree/topic/ButtonEvent)
- [CurveRemapUtility](../../tree/topic/CurveRemapUtility)
- [DebugUtil.SplitInt](../../tree/topic/DebugUtil_SplitInt)
- [GlobalRandom](../../tree/topic/GlobalRandom)
- [InitSystemBase](../../tree/topic/InitSystemBase)
- [LibraryLoader](../../tree/topic/LibraryLoader)
- [SceneInitializeSystem](../../tree/topic/SceneInitializeSystem)
- [WorldSafeShutdown](../../tree/topic/WorldSafeShutdown)
- [IFixedSize](../../tree/topic/IFixedSize)
- [MiniString](../../tree/topic/MiniString)
- [Pin](../../tree/topic/Pin)
- [QueryEntityEnumerator](../../tree/topic/QueryEntityEnumerator)
- [ReflectionUtility](../../tree/topic/ReflectionUtility)
- [SpinLock](../../tree/topic/SpinLock)
- [TransformUtility](../../tree/topic/TransformUtility)
- [WorldUtility](../../tree/topic/WorldUtility)
- [GhostComponentAttribute](../../tree/topic/GhostComponentAttribute)
- [GhostFieldAttribute](../../tree/topic/GhostFieldAttribute)
- [InitializeAllOnLoadExt](../../tree/topic/InitializeAllOnLoadExt)
- [CloneTransformSystem](../../tree/topic/CloneTransformSystem)

### Extension Methods

- [ListExtensions.AddRangeNative](../../tree/topic/ListExtensions_AddRangeNative)
- [NativeStreamExtensions.WriteLarge](../../tree/topic/NativeStreamExtensions_WriteLarge)
- [EntityCommandBufferExtensions.AddUntypedBuffer](../../tree/topic/EntityCommandBufferExtensions_AddUntypedBuffer)
- [EntityCommandBufferExtensions.UnsafeAddComponent](../../tree/topic/EntityCommandBufferExtensions_UnsafeAddComponent)
- [EnumerableExtensions.IndexOf](../../tree/topic/EnumerableExtensions_IndexOf)
- [GameObjectExtensions.IsPrefab](../../tree/topic/GameObjectExtensions_IsPrefab)
- [NativeArrayExtensions.ElementAtRO](../../tree/topic/NativeArrayExtensions_ElementAtRO)
- [NativeArrayExtensions.Select](../../tree/topic/NativeArrayExtensions_Select)
- [NativeSliceExtensions.ReadArrayElementWithStrideRef](../../tree/topic/NativeSliceExtensions_ReadArrayElementWithStrideRef)
- [NativeStreamExtensions.ReadLarge](../../tree/topic/NativeStreamExtensions_ReadLarge)
- [StringExtensions.ToDotNotation](../../tree/topic/StringExtensions_ToDotNotation)
- [SystemStateExtensions.GetAllSystemDependencies](../../tree/topic/SystemStateExtensions_GetAllSystemDependencies)
- [UnsafeHashMapExtensions.GetOrAddRef](../../tree/topic/UnsafeHashMapExtensions_GetOrAddRef)
- [UnsafeParallelHashMapDataExtensions.ReserveParallel](../../tree/topic/UnsafeParallelHashMapDataExtensions_ReserveParallel)
- [WorldUnmanagedExtensions.GetTrackedJobHandle](../../tree/topic/WorldUnmanagedExtensions_GetTrackedJobHandle)

### ConfigVars

- [KSettingsBase](../../tree/topic/KSettingsBase)
- [ConfigVarAttribute](../../tree/topic/ConfigVarAttribute)
- [ConfigVarManager](../../tree/topic/ConfigVarManager)
- [SharedStaticStringContainer](../../tree/topic/SharedStaticStringContainer)
- [CodecService](../../tree/topic/CodecService)
- [CommandLineArgs](../../tree/topic/CommandLineArgs)
- [Deserializer](../../tree/topic/Deserializer)
- [Serializer](../../tree/topic/Serializer)
- [FixedNameValue](../../tree/topic/FixedNameValue)
- [KAttribute](../../tree/topic/KAttribute)
- [KSettings](../../tree/topic/KSettings)
- [ConfigVarPanel](../../tree/topic/ConfigVarPanel)

### Authoring & Baking

- [BakerExtensions.AddEnabledComponent](../../tree/topic/BakerExtensions_AddEnabledComponent)
- [BakerExtensions.AddEnabledBuffer](../../tree/topic/BakerExtensions_AddEnabledBuffer)
- [BakerCommands](../../tree/topic/BakerCommands)
- [AuthoringSettingsUtility](../../tree/topic/AuthoringSettingsUtility)
- [SettingsAuthoring](../../tree/topic/SettingsAuthoring)
- [TagAuthoring](../../tree/topic/TagAuthoring)
- [TransformAuthoring](../../tree/topic/TransformAuthoring)
- [GameObjectHelper.AddAuthoringComponent](../../tree/topic/GameObjectHelper_AddAuthoringComponent)
- [CloneTransformAuthoring](../../tree/topic/CloneTransformAuthoring)
- [LifeCycleAuthoring](../../tree/topic/LifeCycleAuthoring)
- [LookupAuthoring](../../tree/topic/LookupAuthoring)

### Editor Tools

- [AssemblyBuilderWindow](../../tree/topic/AssemblyBuilderWindow)
- [ComponentAssetBaseDrawer](../../tree/topic/ComponentAssetBaseDrawer)
- [TypeSearchProvider](../../tree/topic/TypeSearchProvider)
- [CoreBuildSetup](../../tree/topic/CoreBuildSetup)
- [CreateEditorWorld](../../tree/topic/CreateEditorWorld)
- [EditorMenus.DataModeHierarchySet](../../tree/topic/EditorMenus_DataModeHierarchySet)
- [InspectorSearch](../../tree/topic/InspectorSearch)
- [SelectedEntityEditorSystem](../../tree/topic/SelectedEntityEditorSystem)
- [AssemblyGraphWindow](../../tree/topic/AssemblyGraphWindow)
- [ComponentDependencyWindow](../../tree/topic/ComponentDependencyWindow)
- [SystemDependencyWindow](../../tree/topic/SystemDependencyWindow)
- [CoreEditorPreferencesProvider](../../tree/topic/CoreEditorPreferencesProvider)
- [BitFieldAttributeEditor](../../tree/topic/BitFieldAttributeEditor)
- [HalfDrawer](../../tree/topic/HalfDrawer)
- [InlineObjectProperty](../../tree/topic/InlineObjectProperty)
- [PrefabElementEditor](../../tree/topic/PrefabElementEditor)
- [StableTypeHashAttributeDrawer](../../tree/topic/StableTypeHashAttributeDrawer)
- [ToggleOption](../../tree/topic/ToggleOption)
- [UnityObjectRefInspector](../../tree/topic/UnityObjectRefInspector)
- [WeakObjectReferenceInspector](../../tree/topic/WeakObjectReferenceInspector)
- [EntitySelection.GetAllSelectionsInWorld](../../tree/topic/EntitySelection_GetAllSelectionsInWorld)
- [LoadPrefabsAsEntities](../../tree/topic/LoadPrefabsAsEntities)
- [ReloadToolbarButton](../../tree/topic/ReloadToolbarButton)
- [WelcomeWindow](../../tree/topic/WelcomeWindow)
- [BaseObjectWindow](../../tree/topic/BaseObjectWindow)
- [FeatureToggle](../../tree/topic/FeatureToggle)
- [MainToolbarPresetPostProcessor](../../tree/topic/MainToolbarPresetPostProcessor)
- [ComponentInspectorWindow](../../tree/topic/ComponentInspectorWindow)
- [StartupSceneSwap](../../tree/topic/StartupSceneSwap)
- [ViewModelToolbar](../../tree/topic/ViewModelToolbar)

### Source Generators

- [FacetAttribute](../../tree/topic/FacetAttribute)
- [FacetOptionalAttribute](../../tree/topic/FacetOptionalAttribute)
- [IFacet](../../tree/topic/IFacet)
- [FacetGenerator](../../tree/topic/FacetGenerator)
- [BuilderBase](../../tree/topic/BuilderBase)
- [ClassBuilder](../../tree/topic/ClassBuilder)
- [CodeBuilder](../../tree/topic/CodeBuilder)
- [ConstructorBuilder](../../tree/topic/ConstructorBuilder)
- [DelegateBuilder](../../tree/topic/DelegateBuilder)
- [EnumBuilder](../../tree/topic/EnumBuilder)
- [EventBuilder](../../tree/topic/EventBuilder)
- [ExpressionBlockBuilder](../../tree/topic/ExpressionBlockBuilder)
- [LogicalConditionBuilder](../../tree/topic/LogicalConditionBuilder)
- [MethodBuilder](../../tree/topic/MethodBuilder)
- [PropertyBuilder](../../tree/topic/PropertyBuilder)
- [RecordBuilder](../../tree/topic/RecordBuilder)
- [SwitchBuilder](../../tree/topic/SwitchBuilder)
- [CodeWriter](../../tree/topic/CodeWriter)
- [SymbolHelpers](../../tree/topic/SymbolHelpers)

### SubScene System

- [SubSceneLoadData](../../tree/topic/SubSceneLoadData)
- [SubSceneEntity](../../tree/topic/SubSceneEntity)
- [LoadSubScene](../../tree/topic/LoadSubScene)
- [SubSceneBuffer](../../tree/topic/SubSceneBuffer)
- [SubSceneLoadFlags](../../tree/topic/SubSceneLoadFlags)
- [SubSceneLoadFlagsUtility](../../tree/topic/SubSceneLoadFlagsUtility)
- [SubSceneLoadUtil](../../tree/topic/SubSceneLoadUtil)
- [SubSceneLoaded](../../tree/topic/SubSceneLoaded)
- [SubSceneLoadingManagedSystem](../../tree/topic/SubSceneLoadingManagedSystem)
- [SubSceneLoadingSystem](../../tree/topic/SubSceneLoadingSystem)
- [SubScenePostLoadCommandBufferSystem](../../tree/topic/SubScenePostLoadCommandBufferSystem)
- [SubSceneSetId](../../tree/topic/SubSceneSetId)
- [SubSceneUtil](../../tree/topic/SubSceneUtil)
- [SubSceneEditorSet](../../tree/topic/SubSceneEditorSet)
- [SubSceneEditorSystem](../../tree/topic/SubSceneEditorSystem)
- [SubSceneEditorToolbar](../../tree/topic/SubSceneEditorToolbar)
- [SubScenePrebakeSystem](../../tree/topic/SubScenePrebakeSystem)
- [DestroyOnSubSceneUnloadSystem](../../tree/topic/DestroyOnSubSceneUnloadSystem)

### Pause & Time

- [LimitedRateNoCatchUpManager](../../tree/topic/LimitedRateNoCatchUpManager)
- [PauseGame](../../tree/topic/PauseGame)
- [PauseLimitSystem](../../tree/topic/PauseLimitSystem)
- [PauseRateManager](../../tree/topic/PauseRateManager)
- [PauseUtility](../../tree/topic/PauseUtility)
- [FixedStepUpdatedSystem](../../tree/topic/FixedStepUpdatedSystem)
- [UpdateWorldTimeSystem](../../tree/topic/UpdateWorldTimeSystem)

### Relevancy & Netcode

- [InputBounds](../../tree/topic/InputBounds)
- [RelevanceAlways](../../tree/topic/RelevanceAlways)
- [RelevanceConfig](../../tree/topic/RelevanceConfig)
- [RelevanceManual](../../tree/topic/RelevanceManual)
- [RelevanceProvider](../../tree/topic/RelevanceProvider)
- [RelevancySystem](../../tree/topic/RelevancySystem)

### Singleton System

- [SingletonAttribute](../../tree/topic/SingletonAttribute)
- [SingletonInitialize](../../tree/topic/SingletonInitialize)
- [SingletonInitializeSystemGroup](../../tree/topic/SingletonInitializeSystemGroup)
- [SingletonInitializedSystem](../../tree/topic/SingletonInitializedSystem)
- [SingletonSystem](../../tree/topic/SingletonSystem)
- [ComponentSystemBaseInternal.RequireSingletonForUpdate](../../tree/topic/ComponentSystemBaseInternal_RequireSingletonForUpdate)
- [ISingletonCollection](../../tree/topic/ISingletonCollection)
- [SingletonCollectionUtil](../../tree/topic/SingletonCollectionUtil)

### Object Management

- [ObjectDefinition](../../tree/topic/ObjectDefinition)
- [ObjectGroupMatcher](../../tree/topic/ObjectGroupMatcher)
- [ObjectId](../../tree/topic/ObjectId)
- [UIDAttribute](../../tree/topic/UIDAttribute)
- [GroupId](../../tree/topic/GroupId)
- [ObjectCategories](../../tree/topic/ObjectCategories)
- [ObjectCategoryComponents](../../tree/topic/ObjectCategoryComponents)
- [ObjectDefinitionRegistrySystem](../../tree/topic/ObjectDefinitionRegistrySystem)
- [ObjectGroupRegistry](../../tree/topic/ObjectGroupRegistry)
- [ObjectInstantiateSystem](../../tree/topic/ObjectInstantiateSystem)
- [ObjectDefinitionAuthoring](../../tree/topic/ObjectDefinitionAuthoring)
- [ObjectInstantiate.Editor](../../tree/topic/ObjectInstantiate_Editor)

### Physics States

- [CalculateEventMapBucketsJob](../../tree/topic/CalculateEventMapBucketsJob)
- [CollectEventsJob](../../tree/topic/CollectEventsJob)
- [EnsureCurrentEventsCapacityJob](../../tree/topic/EnsureCurrentEventsCapacityJob)

### Life Cycle

- [AfterSceneSystemGroup](../../tree/topic/AfterSceneSystemGroup)
- [AfterTransformSystemGroup](../../tree/topic/AfterTransformSystemGroup)
- [BeforeTransformSystemGroup](../../tree/topic/BeforeTransformSystemGroup)
- [BeginSimulationSystemGroup](../../tree/topic/BeginSimulationSystemGroup)
- [InstantiateCommandBufferSystem](../../tree/topic/InstantiateCommandBufferSystem)
- [DestroyEntityCommandBufferSystem](../../tree/topic/DestroyEntityCommandBufferSystem)
- [DestroyEntitySystem](../../tree/topic/DestroyEntitySystem)
- [DestroyOnDestroySystem](../../tree/topic/DestroyOnDestroySystem)
- [EndInitializeEntityCommandBufferSystem](../../tree/topic/EndInitializeEntityCommandBufferSystem)
- [InitializeEntitySystem](../../tree/topic/InitializeEntitySystem)

### Tests & Diagnostics

- [FaceReadonlyTest](../../tree/topic/FaceReadonlyTest)
- [MathExPerformanceTests](../../tree/topic/MathExPerformanceTests)
- [Check.Assume](../../tree/topic/Check_Assume)
- [ReflectionTestHelper](../../tree/topic/ReflectionTestHelper)
- [TestLeakDetectionAttribute](../../tree/topic/TestLeakDetectionAttribute)

### Math Extensions

- [MathematicsExtensions.Encapsulate](../../tree/topic/MathematicsExtensions_Encapsulate)
- [HSV](../../tree/topic/HSV)
- [PolygonUtility](../../tree/topic/PolygonUtility)
- [ShortHalfUnion](../../tree/topic/ShortHalfUnion)
- [IntFloatUnion](../../tree/topic/IntFloatUnion)
- [mathex.mod](../../tree/topic/mathex_mod)
- [mathex.minMax](../../tree/topic/mathex_minMax)
- [mathex.add](../../tree/topic/mathex_add)
- [mathex.GenerateGaussianNoise](../../tree/topic/mathex_GenerateGaussianNoise)
- [mathex.FromToRotation](../../tree/topic/mathex_FromToRotation)
- [MinMaxAttributeDrawer](../../tree/topic/MinMaxAttributeDrawer)

### Other

- [AssetLoad](../../tree/topic/AssetLoad)
- [GameObjectCleanup](../../tree/topic/GameObjectCleanup)
- [HalfSizeTriangleMatrix](../../tree/topic/HalfSizeTriangleMatrix)
- [CalculateCurrentEventsBucketsJob](../../tree/topic/CalculateCurrentEventsBucketsJob)
- [StripLocalAttribute](../../tree/topic/StripLocalAttribute)
- [StripLocalSystem](../../tree/topic/StripLocalSystem)
- [AssetLoadingSystem](../../tree/topic/AssetLoadingSystem)
- [BovineLabsBootstrap](../../tree/topic/BovineLabsBootstrap)
- [BovineLabsBootstrap.NetCode](../../tree/topic/BovineLabsBootstrap_NetCode)
- [CollectionCreator.CreateHashMap](../../tree/topic/CollectionCreator_CreateHashMap)
- [INativeStreamReader](../../tree/topic/INativeStreamReader)
- [UnsafeThreadStreamBlockData](../../tree/topic/UnsafeThreadStreamBlockData)
- [SyncEnableStateUtil](../../tree/topic/SyncEnableStateUtil)
- [TimeProfiler](../../tree/topic/TimeProfiler)
- [ReferenceT](../../tree/topic/ReferenceT)
- [ReferenceData](../../tree/topic/ReferenceData)
- [UnsafeListDispose](../../tree/topic/UnsafeListDispose)
- [AppAPI](../../tree/topic/AppAPI)
- [SerializedHelper.IterateAllChildren](../../tree/topic/SerializedHelper_IterateAllChildren)
- [TextAssetHelper](../../tree/topic/TextAssetHelper)
- [PrefabInstance](../../tree/topic/PrefabInstance)
- [AnalyzersProjectFileGeneration](../../tree/topic/AnalyzersProjectFileGeneration)


---

Total: 358 topics across 24 categories
