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

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core.Extensions/Relevancy/InputBounds.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Relevancy/InputBounds.cs)
