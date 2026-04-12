# RuntimeContentCatalogUtility — Content Catalog Parser

## Overview

RuntimeContentCatalogUtility is a static helper class that parses Unity's RuntimeContentCatalogData
binary to extract SubScene GUIDs and archive GUIDs at runtime. Used during dynamic scene loading.

```
┌─────────────────────────────────────────────────────────────────────┐
│  RuntimeContentCatalogUtility (static class)                        │
│                                                                     │
│  Public API:                                                        │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  GetSubScenesAndArchives(catalogPath,                          │  │
│  │      out List<Hash128> scenes, out List<Hash128> archives)    │  │
│  │                                                               │  │
│  │  GetSubScenes(string catalogPath) → List<Hash128>             │  │
│  │  GetArchives(string catalogPath) → List<Hash128>              │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  Internal:                                                          │
│    GetSubScenes(BlobAssetReference<RuntimeContentCatalogData>, ...) │
│      → Filters Objects where ObjectId.GenerationType ==             │
│        WeakReferenceGenerationType.SubSceneObjectReferences         │
│      → Collects ObjectId.GlobalId.AssetGUID                        │
│                                                                     │
│    GetArchives(BlobAssetReference<RuntimeContentCatalogData>, ...)  │
│      → Filters Archives where ArchiveId.IsValid                     │
│      → Collects ArchiveId.Value                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Parse Flow

```
GetSubScenes(catalogPath)
       │
       ├─ path empty/null? → return empty list
       │
       └─ BlobAssetReference<RuntimeContentCatalogData>.TryRead(catalogPath, version=1)
            │
            ├─ TryRead failed → return empty list
            │
            └─ Success:
                 for each obj in catalogData.Value.Objects:
                   if obj.ObjectId.GenerationType == SubSceneObjectReferences:
                     scenes.Add(obj.ObjectId.GlobalId.AssetGUID)
                 catalogData.Dispose()
                 return scenes
```

## Key Design Decisions

- **BlobAssetReference.TryRead**: Uses Unity's blob serialization to read the catalog binary.
- **SubSceneObjectReferences filter**: Only extracts objects tagged as sub-scene references,
  ignoring regular assets and prefabs.
- **Disposes catalog**: Always disposes the BlobAssetReference after reading to avoid memory leaks.
- **Managed List<Hash128>**: Returns managed lists since this is typically called from main thread
  during initialization.

## Verified Data

```
RuntimeContentCatalogUtility
  Kind: static class (abstract), ns=BovineLabs.Core.Internal
  Uses BlobAssetReference<RuntimeContentCatalogData>.TryRead

Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Internal/RuntimeContentCatalogUtility.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Internal/RuntimeContentCatalogUtility.cs)
