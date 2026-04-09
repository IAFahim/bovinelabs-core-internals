# DistanceHitSortDescending

**Compares and sorts Unity physics raycast hits furthest to nearest**

The mirror of `DistanceHitSortAscending`. Orders `DistanceHit` results by
descending distance, placing the furthest hits first.

---

## Structure

```
  ┌──────────────────────────────────────────────┐
  │  DistanceHitSortDescending                   │
  │  : IComparer<DistanceHit>                    │
  │                                              │
  │  Compare(DistanceHit x, DistanceHit y)       │
  │    → y.Distance.CompareTo(x.Distance)        │
  │         ↑ NOTE: swapped order                │
  └──────────────────────────────────────────────┘
```

## Sort Visualization

```
  Unsorted DistanceHit results:

    ┌────────┬────────┬────────┬────────┬────────┐
    │ Hit[0] │ Hit[1] │ Hit[2] │ Hit[3] │ Hit[4] │
    │ d=4.2  │ d=1.1  │ d=3.0  │ d=0.5  │ d=2.7  │
    └────────┴────────┴────────┴────────┴────────┘
         │
         │  Array.Sort(hits, new DistanceHitSortDescending())
         │
         ▼
    ┌────────┬────────┬────────┬────────┬────────┐
    │ Hit[0] │ Hit[2] │ Hit[4] │ Hit[1] │ Hit[3] │
    │ d=4.2  │ d=3.0  │ d=2.7  │ d=1.1  │ d=0.5  │
    └────────┴────────┴────────┴────────┴────────┘
     furthest ─────────────────────────── nearest

  Compare with ascending:
  ┌──────────────────┬──────────────────────────────┐
  │ Ascending        │  x.Distance.CompareTo(y)     │
  │                  │  → nearest first              │
  ├──────────────────┼──────────────────────────────┤
  │ Descending       │  y.Distance.CompareTo(x)     │
  │                  │  → furthest first             │
  └──────────────────┴──────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

`BovineLabs.Core/Sort/DistanceHitSortDescending.cs`

## Source

- [BovineLabs.Core/Sort/DistanceHitSortDescending.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Sort/DistanceHitSortDescending.cs)
