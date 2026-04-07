# InputBounds

## Inner Workings Diagram

```
 InputBounds
 ======================================================================
 Defined as: InputBounds
 Namespace:  BovineLabs.Core.Relevancy

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ InputBounds                                                        │
 ├────────────────────────────────────────────────────────────────────┤
 │ float3                   Min                                       │
 │ float3                   Max                                       │
 │ float3                   Center                                    │
 │ MinMaxAABB               AABB                                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core.Extensions/Relevancy/InputBounds.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Relevancy/InputBounds.cs)
