# EntityCommandBufferExtensions.AddUntypedBuffer

## Inner Workings Diagram

```
 EntityCommandBufferExtensions.AddUntypedBuffer
 ======================================================================
 Namespace:  BovineLabs.Core.Extensions

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ class                    EntityCommandBufferExtensions             │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ AddUntypedBuffer(this EntityCommandBuffer ecb, Entity e, ComponentT)│
 │   → UntypedDynamicB                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
EntityCommandBufferExtensions: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Extensions/EntityCommandBufferExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityCommandBufferExtensions.cs)
