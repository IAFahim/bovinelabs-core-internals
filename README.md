# ObjectDefinitionRegistrySystem

## Inner Workings Diagram

```
 ObjectDefinitionRegistrySystem
 ======================================================================
 Defined as: ObjectDefinitionRegistrySystem
 Namespace:  BovineLabs.Core.ObjectManagement


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnDestroy(ref SystemState state)                                   │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/ObjectManagement/ObjectDefinitionRegistrySystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/ObjectManagement/ObjectDefinitionRegistrySystem.cs)
