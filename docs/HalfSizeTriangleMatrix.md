# HalfSizeTriangleMatrix — Inner Workings

## Overview

HalfSizeTriangleMatrix maps 2D (row, column) coordinates into a 1D array index for
upper-triangular storage of symmetric distance matrices, reducing memory from N²
to N*(N+1)/2 — roughly a 2x savings.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│              HalfSizeTriangleMatrix (static class)                          │
│                                                                             │
│  GetIndex(row, column, n) → flat index into triangular array                │
│                                                                             │
│  Automatically swaps row↔column if row > column                             │
│  (symmetric: dist[A][B] == dist[B][A])                                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Full Matrix vs Triangular Storage

```
  FULL MATRIX (N×N = N² elements)              TRIANGULAR (N*(N+1)/2 elements)
  N=4: 16 elements                              N=4: 10 elements (37.5% savings!)

  ┌───┬───┬───┬───┐                             ┌───┬───┬───┬───┐
  │ 0 │ 1 │ 2 │ 3 │  row 0                     │ 0 │ 1 │ 2 │ 3 │
  ├───┼───┼───┼───┤                             ├───┼───┼───┤
  │ 4 │ 5 │ 6 │ 7 │  row 1                     │ 4 │ 5 │ 6 │
  ├───┼───┼───┼───┤  Diagonal and lower         ├───┼───┤
  │ 8 │ 9 │10 │11 │  triangle are redundant     │ 7 │ 8 │
  ├───┼───┼───┼───┤  for symmetric matrices      ├───┤
  │12 │13 │14 │15 │                             │ 9 │
  └───┴───┴───┴───┘                             └───┘

  Memory: 4×4 = 16 floats                       10 floats
  (for N=100: 10000 vs 5050 = ~50% savings)
```

## Index Calculation Algorithm

```
  GetIndex(row, column, n)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────────────┐
  │  if row <= column:                                                    │
  │    return row * n - ((row-1)*row)/2 + column - row                    │
  │                                                                       │
  │  else:                                                                │
  │    return column * n - ((column-1)*column)/2 + row - column           │
  │    // (swap row/column — same result by symmetry)                     │
  └───────────────────────────────────────────────────────────────────────┘

  Expanded formula for row <= column:
    index = row*n - (row*(row-1))/2 + (column - row)
           ╰──────────────────────╯   ╰──────────────╯
             skip rows above          offset within row

  Row skip formula derivation:
  ┌────────────────────────────────────────────────────────────────┐
  │  Row 0: starts at index 0     (0 elements before it)          │
  │  Row 1: starts at index n     (n elements before it)          │
  │  Row 2: starts at index 2n-1  (n + (n-1) before it)          │
  │  Row r: starts at r*n - r*(r-1)/2                             │
  │                                                                │
  │  This is because each row is 1 element shorter than the last: │
  │  Row 0: n elements                                             │
  │  Row 1: n-1 elements                                           │
  │  Row 2: n-2 elements                                           │
  │  ...                                                           │
  │  Row r: n-r elements                                           │
  └────────────────────────────────────────────────────────────────┘
```

## Concrete Example (N=4)

```
  Flat array: [0] [1] [2] [3] [4] [5] [6] [7] [8] [9]
               │   │   │   │   │   │   │   │   │   │
               │   │   │   │   │   │   │   │   │   └─ (3,3)
               │   │   │   │   │   │   │   │   └───── (2,3)
               │   │   │   │   │   │   └─────────── (2,2)
               │   │   │   │   │   └─────────────── (1,3)
               │   │   │   │   └─────────────────── (1,2)
               │   │   │   └─────────────────────── (1,1)
               │   │   └─────────────────────────── (0,3)
               │   └─────────────────────────────── (0,2)
               └─────────────────────────────────── (0,1)
               └─────────────────────────────────── (0,0)

  GetIndex(0, 0, 4) = 0*4 - (-0)/2 + 0 - 0 = 0  ✓
  GetIndex(0, 3, 4) = 0*4 - 0    + 3 - 0 = 3    ✓
  GetIndex(1, 2, 4) = 1*4 - 0    + 2 - 1 = 5    ✓
  GetIndex(2, 3, 4) = 2*4 - 1    + 3 - 2 = 8    ✓
  GetIndex(3, 1, 4) = (swapped to (1,3)) = 5    ✓ (symmetric!)
```

## Bounds Checking

```
  #if ENABLE_UNITY_COLLECTIONS_CHECKS
    if (row >= n)    throw ArgumentException
    if (column >= n) throw ArgumentException
  #endif

  Only in debug builds — stripped in release for performance.
```

## Key Design Decisions

- **Symmetric swap**: If `row > column`, the formula swaps them automatically,
  since `dist[A][B] == dist[B][A]` for distance/similarity matrices.
- **Integer arithmetic only**: All operations are integer multiply/add/divide — no
  floating point, no branches in release mode (after the row<=column check).
- **No storage allocation**: Just index calculation — caller provides the flat array.
- **O(1) lookup**: Constant-time index computation regardless of matrix size.

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source File

- `BovineLabs.Core/Utility/HalfSizeTriangleMatrix.cs`

## Source

- [BovineLabs.Core/Utility/HalfSizeTriangleMatrix.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/HalfSizeTriangleMatrix.cs)
