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

## Verified Data

```
UnsafeUntypedDynamicBufferAccessor: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [BovineLabs.Core/Collections/UnsafeUntypedDynamicBufferAccessor.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/UnsafeUntypedDynamicBufferAccessor.cs)
