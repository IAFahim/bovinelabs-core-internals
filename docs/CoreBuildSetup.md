# CoreBuildSetup

## Inner Workings Diagram

```
 CoreBuildSetup
 ======================================================================
 Defined as: CoreBuildSetup
 Namespace:  BovineLabs.Core.Editor


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ PrepareForBuild(BuildPlayerContext buildPlayerContext)             │
 │   → void                                                           │
 │ OnPostprocessBuild(BuildReport report)                             │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Editor/CoreBuildSetup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/CoreBuildSetup.cs)
