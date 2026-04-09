# EntityQueryExtensions.GetSingletonUntypedBuffer

## Inner Workings Diagram

```
 EntityQueryExtensions.GetSingletonUntypedBuffer
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    EntityQueryExtensions                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetFirstEntity(this EntityQuery query)                             │
 │   → Entity                                                         │
 │ GetSingletonUntypedBuffer(this EntityQuery query, ComponentType compo
 │   → UntypedDynamicB                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
EntityQueryExtensions: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/EntityQueryExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityQueryExtensions.cs)
