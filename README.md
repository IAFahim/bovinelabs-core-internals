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

## Source

- [BovineLabs.Core/Extensions/EntityCommandBufferExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/EntityCommandBufferExtensions.cs)
