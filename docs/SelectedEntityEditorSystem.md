# SelectedEntityEditorSystem

## Inner Workings Diagram

```
 SelectedEntityEditorSystem
 ======================================================================
 Defined as: SelectedEntityEditorSystem
 Namespace:  BovineLabs.Core.Editor

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SelectedEntityEditorSystem                                         │
 ├────────────────────────────────────────────────────────────────────┤
 │ SharedStatic<bool>       IsEnabled                                 │
 │ Entity>                  EntityLookup                              │
 │ Entity>                  EntityLookup                              │
 │ int                      Count                                     │
 │ ComponentTypeHandle<En   GuidType                                  │
 │ EntityTypeHandle         EntityType                                │
 │ NativeList<EntityId>     InstanceIDs                               │
 │ Entity>                  EntityLookup                              │
 │ NativeList<int>          InstanceIDs                               │
 │ Entity>                  EntityLookup                              │
 │ NativeList<Entity>       Entities                                  │
 │ ComponentLookup<Entity   EntityGuids                               │
 │ ComponentLookup<Select   SelectedEntitys                           │
 │ DynamicBuffer<Selected   SelectedEntities                          │
 │ Entity                   SingletonEntity                           │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Execute()                                                          │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Editor/Debug/SelectedEntityEditorSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Editor/Debug/SelectedEntityEditorSystem.cs)
