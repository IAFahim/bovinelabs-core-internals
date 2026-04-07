# ObjectInstantiateSystem

## Inner Workings Diagram

```
 ObjectInstantiateSystem
 ======================================================================
 Namespace:  BovineLabs.Core.ObjectManagement

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ComponentTypeHandle<Ob   ObjectInstantiateHandle                   │
 │ ComponentTypeHandle<Lo   LocalToWorldHandle                        │
 │ NativeArray<Entity>      Prefabs                                   │
 │ NativeArray<Ptr<Entity   Instances                                 │
 │ Ptr<LocalToWorld>>       Data                                      │
 │ ComponentLookup<LocalT   LocalToWorlds                             │
 │ ComponentLookup<LocalT   LocalTransforms                           │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnCreate(ref SystemState state)                                    │
 │   → void                                                           │
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 │ Execute(int index)                                                 │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/ObjectManagement/ObjectInstantiateSystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/ObjectManagement/ObjectInstantiateSystem.cs)
