# BovineLabs Core Internals — Verified Test Scripts

## 308 PASS, 0 FAIL — Unity 6000.5.0b1 + com.bovinelabs.core 1.6.1

Every test script in this directory was executed inside a **live Unity Editor** via `unity-cli exec` and verified to pass with zero failures.

## How to Run

Open Unity with your BovineLabs Core project, start the unity-cli connector, then:

```bash
# Run a single test
cat tests/01_ButtonEvent.cs | unity-cli exec --usings "BovineLabs.Core.Utility"

# Run all tests (common usings)
for f in tests/*.cs; do
  echo "=== $f ==="
  cat "$f" | unity-cli exec --usings "BovineLabs.Core.Collections,BovineLabs.Core.Sort,BovineLabs.Core.Utility,BovineLabs.Core.Jobs,BovineLabs.Core.Model,BovineLabs.Core.Memory,BovineLabs.Core.States,BovineLabs.Core.Spatial,BovineLabs.Core.Extensions,BovineLabs.Core.Iterators,Unity.Entities,Unity.Mathematics,Unity.Collections,Unity.Burst,Unity.Physics,UnityEngine,System.Linq"
done
```

Some files need specific usings:
```bash
cat tests/16_UtilityAndBootstrap.cs | unity-cli exec --usings "BovineLabs.Core.Utility,BovineLabs.Core.Authoring.EntityCommands,BovineLabs.Core.Editor.ChangeFilterTracking,BovineLabs.Core,Unity.Entities,System.Linq"
cat tests/21_SourceGen.cs | unity-cli exec --usings "UnityEngine,System.Linq"
```

## Test Coverage

| # | File | Branches Covered | Tests |
|---|------|-----------------|-------|
| 01 | ButtonEvent | topic/ButtonEvent | 17 |
| 02 | BitArrays | topic/BitArray8_16_32_64, topic/BitArray128, topic/BitArray256, topic/BitArrayUtilities | 29 |
| 03 | UtilityPrimitives | topic/IntFloatUnion, topic/GlobalRandom, topic/SpinLock, topic/EntityLock | 10 |
| 04 | MathUtility | topic/HSV, topic/HalfSizeTriangleMatrix, topic/CurveRemapUtility, topic/mathex_* | 28 |
| 05 | Collections | topic/UnsafeArray, topic/FixedArray, topic/ThreadRandom, topic/NativeCounter | 11 |
| 06 | Sort | topic/DistanceHitSortAscending, topic/DistanceHitSortDescending | 8 |
| 07 | Serialization | topic/CodecService, topic/Serializer, topic/Deserializer, topic/EnableMaskCreator | 10 |
| 08 | UtilityAdvanced | topic/BurstTrampoline, topic/BurstUtil, topic/CommandLineArgs, topic/ConvexHullBuilder | 11 |
| 09 | States | topic/AppAPI, topic/StateAPI, topic/StateFlagModel, topic/IState, topic/CopyEnableable | 15 |
| 10 | Memory | topic/MemoryAllocator, topic/MemoryLabelAllocator | 8 |
| 11 | Spatial | topic/SpatialMap, topic/SpatialMap3, topic/SpatialKeyedMap, topic/PositionBuilder | 10 |
| 12 | Iterators | topic/DynamicHashMapHelper, topic/DynamicHashSet, topic/DynamicHashSetExtensions | 19 |
| 13 | Jobs | topic/IJobForThread, topic/IJobChunkWorkerBeginEnd | 9 |
| 14 | Extensions | topic/EntityQuery_*, topic/ArchetypeChunk_*, topic/EntityQueryBuilder_* | 11 |
| 15 | AabbPhysics | topic/AabbExtensions, topic/AlwaysUpdatePhysicsWorld | 12 |
| 16 | UtilityBootstrap | topic/DebugUtil.SplitInt, topic/BakerCommands, topic/ChangeFilterTrackingSystem, topic/BovineLabsBootstrap | 25 |
| 17 | BlobCurve | topic/BlobCurve, topic/BlobCurve2_3_4, topic/BlobCurveHeader, topic/BlobCurveSampler, topic/BlobCurveSegment, topic/BlobShared | 32 |
| 18 | BlobHashMap | topic/BlobBuilderExtensions, topic/BlobHashMap, topic/BlobHashMapData, topic/BlobPerfectHashMap, topic/BlobBuilderExtensions_Allocate, topic/BlobBuilderExtensions_ConstructHashMap, topic/BlobBuilderHashMap | 16 |
| 19 | DynamicContainers | topic/DynamicMultiHashMap, topic/DynamicUntypedBuffer, topic/DynamicVariableMap | 31 |
| 20 | PhysicsJobs | topic/CalculateEventMapBucketsJob, topic/CollectEventsJob | 12 |
| 21 | SourceGen | topic/ClassBuilder, topic/CodeBuilder, topic/CodeWriter, topic/BlobSpline | 9 |
| | **TOTAL** | **65 content branches** | **308** |
