# DestroyOnDestroySystem

## Inner Workings Diagram

```
 DestroyOnDestroySystem
 ======================================================================
 Defined as: DestroyOnDestroySystem
 Namespace:  BovineLabs.Core.LifeCycle

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ DestroyOnDestroySystem                                             │
 ├────────────────────────────────────────────────────────────────────┤
 │ BufferLookup<LinkedEnt   LinkedEntityGroups                        │
 │ ComponentLookup<Destro   DestroyEntitys                            │
 │ NativeQueue<Entity>      ToDestroy                                 │
 │ ComponentLookup<Destro   DestroyEntitys                            │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ OnUpdate(ref SystemState state)                                    │
 │   → void                                                           │
 │ Execute()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/LifeCycle/DestroyOnDestroySystem.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/LifeCycle/DestroyOnDestroySystem.cs)
