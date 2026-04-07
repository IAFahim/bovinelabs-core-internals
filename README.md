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

## Source

- [BovineLabs.Core.Extensions/SubScenes/SubSceneLoadData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/SubScenes/SubSceneLoadData.cs)
