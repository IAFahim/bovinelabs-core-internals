# DistanceHitSortAscending

**Compares and sorts Unity physics raycast hits nearest to furthest**

A simple `IComparer<DistanceHit>` implementation that orders `DistanceHit`
results by ascending distance. Used with `Array.Sort` or similar to sort
physics distance query results.

---

## Structure

```
  ┌──────────────────────────────────────────────┐
  │  DistanceHitSortAscending                    │
  │  : IComparer<DistanceHit>                    │
  │                                              │
  │  Compare(DistanceHit x, DistanceHit y)       │
  │    → x.Distance.CompareTo(y.Distance)        │
  └──────────────────────────────────────────────┘
```

## Sort Visualization

```
  Unsorted DistanceHit results from physics query:

    ┌────────┬────────┬────────┬────────┬────────┐
    │ Hit[0] │ Hit[1] │ Hit[2] │ Hit[3] │ Hit[4] │
    │ d=4.2  │ d=1.1  │ d=3.0  │ d=0.5  │ d=2.7  │
    └────────┴────────┴────────┴────────┴────────┘
         │
         │  Array.Sort(hits, new DistanceHitSortAscending())
         │
         ▼
    ┌────────┬────────┬────────┬────────┬────────┐
    │ Hit[3] │ Hit[1] │ Hit[4] │ Hit[2] │ Hit[0] │
    │ d=0.5  │ d=1.1  │ d=2.7  │ d=3.0  │ d=4.2  │
    └────────┴────────┴────────┴────────┴────────┘
     nearest ────────────────────────────── furthest

  Ray diagram:
                    ╱ start
                   ╱
        0.5 ╱─────●─── Hit[3] (closest)
           ╱
        1.1 ╱─────●─── Hit[1]
           ╱
        2.7 ╱─────●─── Hit[4]
           ╱
        3.0 ╱─────●─── Hit[2]
           ╱
        4.2 ╱─────●─── Hit[0] (furthest)
```

## Usage

```csharp
var hits = new NativeArray<DistanceHit>(..., Allocator.Temp);
// ... perform physics distance query ...
var sortedHits = hits.ToArray();
Array.Sort(sortedHits, new DistanceHitSortAscending());
// sortedHits[0] is now the nearest hit
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core/Sort/DistanceHitSortAscending.cs`

## Source

- [BovineLabs.Core/Sort/DistanceHitSortAscending.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Sort/DistanceHitSortAscending.cs)
