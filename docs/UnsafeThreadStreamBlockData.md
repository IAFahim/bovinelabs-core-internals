# UnsafeThreadStreamBlockData

## Inner Workings Diagram

```
 UnsafeThreadStreamBlockData
 ======================================================================
 Namespace:  BovineLabs.Core.Collections

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ struct                   UnsafeThreadStreamBlock                   │
 │ UnsafeThreadStreamBloc   Next                                      │
 │ struct                   UnsafeThreadStreamRange                   │
 │ UnsafeThreadStreamBloc   Block                                     │
 │ int                      OffsetInFirstBlock                        │
 │ int                      ElementCount                              │
 │ int                      LastOffset                                │
 │ int                      NumberOfBlocks                            │
 │ UnsafeThreadStreamBloc   CurrentBlock                              │
 │ byte*                    CurrentPtr                                │
 │ byte*                    CurrentBlockEnd                           │
 │ struct                   UnsafeThreadStreamBlockData               │
 │ int                      AllocationSize                            │
 │ UnsafeThreadStreamBloc   Blocks                                    │
 │ UnsafeThreadStreamRang   Ranges                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
UnsafeThreadStreamBlock: TYPE NOT FOUND
UnsafeThreadStreamRange: TYPE NOT FOUND
UnsafeThreadStreamBlockData: TYPE NOT FOUND
Verified: 0 checks, 3 failures
```

## Source

- [BovineLabs.Core/Collections/EventStream/UnsafeThreadStreamBlockData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Collections/EventStream/UnsafeThreadStreamBlockData.cs)
