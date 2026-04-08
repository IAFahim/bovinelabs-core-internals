# GhostFieldAttribute

## Inner Workings Diagram

```
 GhostFieldAttribute
 ======================================================================
 Defined as: GhostFieldAttribute
 Namespace:  Unity.NetCode

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GhostFieldAttribute                                                │
 ├────────────────────────────────────────────────────────────────────┤
 │ int                      Quantization                              │
 │ bool                     Composite                                 │
 │ SmoothingAction          Smoothing                                 │
 │ int                      SubType                                   │
 │ bool                     SendData                                  │
 │ float                    MaxSmoothingDistance                      │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [BovineLabs.Core/Wrappers/GhostFieldAttribute.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Wrappers/GhostFieldAttribute.cs)
