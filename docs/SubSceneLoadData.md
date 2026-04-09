# SubSceneLoadData

## Inner Workings Diagram

```
 SubSceneLoadData
 ======================================================================
 Defined as: SubSceneLoadData
 Namespace:  BovineLabs.Core.SubScenes

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SubSceneLoadData                                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ SubSceneSetId            ID                                        │
 │ bool                     WaitForLoad                               │
 │ bool                     IsRequired                                │
 │ WorldFlags               TargetWorld                               │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/SubScenes/SubSceneLoadData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/SubScenes/SubSceneLoadData.cs)
