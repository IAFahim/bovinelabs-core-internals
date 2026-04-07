# UnsafeUntypedDynamicBufferAccessor

## Inner Workings Diagram

```
 UnsafeUntypedDynamicBufferAccessor
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ struct                   UnsafeUntypedDynamicBufferAccessor        │
 │ int                      Length                                    │
 │ int                      ElementSize                               │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetUntypedBuffer(int index)                                        │
 │   → UnsafeUntypedDy                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Collections/UnsafeUntypedDynamicBufferAccessor.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafeUntypedDynamicBufferAccessor.cs)
