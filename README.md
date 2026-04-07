# BakerCommands

## Inner Workings Diagram

```
 BakerCommands
 ======================================================================
 Defined as: BakerCommands
 Namespace:  BovineLabs.Core.Authoring.EntityCommands

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ BakerCommands                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ Entity                   Entity                                    │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CreateEntity()                                                     │
 │   → Entity                                                         │
 │ Instantiate(Entity prefab)                                         │
 │   → Entity                                                         │
 │ SetName(FixedString64Bytes name)                                   │
 │   → void                                                           │
 │ SetName(Entity entity, FixedString64Bytes name)                    │
 │   → void                                                           │
 │ AddComponent(in ComponentTypeSet components)                       │
 │   → void                                                           │
 │ AddComponent(Entity entity, in ComponentTypeSet components)        │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Authoring/EntityCommands/BakerCommands.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Authoring/EntityCommands/BakerCommands.cs)
