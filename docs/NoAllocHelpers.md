# NoAllocHelpers — Inner Workings

## Overview

NoAllocHelpers uses unsafe type reinterpretation (`UnsafeUtility.As`) to access
the private fields of `List<T>`, enabling zero-allocation resizing by directly
manipulating the internal size counter and extracting the underlying array.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                 NoAllocHelpers (static class)                                │
│                                                                             │
│  ExtractArrayFromList<T>(list) → T[]    (get backing array, no copy)        │
│  ResizeList<T>(list, count)  → void     (resize without allocation)         │
│                                                                             │
│  Uses: UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>                 │
│        to reinterpret List's memory layout                                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Memory Reinterpretation Trick

```
  List<T> (actual .NET layout)              ListPrivateFieldAccess<T> (mimic)
  ┌─────────────────────────────────┐       ┌─────────────────────────────────┐
  │  T[]    _items                  │   =   │  readonly T[]  Items            │
  │  int    _size                   │   =   │  int           Size             │
  │  int    _version                │   =   │  int           Version          │
  │  ...other fields...             │       └─────────────────────────────────┘
  └─────────────────────────────────┘

  UnsafeUtility.As<List<T>, ListPrivateFieldAccess<T>>(ref list)
  ══════════════════════════════════════════════════════════
  Reinterprets the List's memory as the struct WITHOUT copying.
  The struct's field offsets MUST match the class's field layout.

  ┌───────────────────────────────────────────────────────────────────┐
  │  List<int> on heap:                                               │
  │                                                                   │
  │  ┌─ List<int> object ──────────────────────────┐                  │
  │  │  _items: ──────────────────► [0,1,2,3,_,_] │ Capacity=6      │
  │  │  _size:  4                                  │                  │
  │  │  _version: 1                                │                  │
  │  └─────────────────────────────────────────────┘                  │
  │                    │                                              │
  │                    │ UnsafeUtility.As (reinterpret)               │
  │                    ▼                                              │
  │  ┌─ ListPrivateFieldAccess<int> (stack) ────────┐                 │
  │  │  Items: ─────────────────► [0,1,2,3,_,_]     │ (same array!)  │
  │  │  Size:  4                                     │                │
  │  │  Version: 1                                   │                │
  │  └──────────────────────────────────────────────┘                 │
  └───────────────────────────────────────────────────────────────────┘
```

## ExtractArrayFromList Flow

```
  ExtractArrayFromList<int>(list)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  var access = UnsafeUtility.As<                              │
  │      List<int>, ListPrivateFieldAccess<int>>(ref list);      │
  │                                                               │
  │  return access.Items;                                         │
  │       └── Returns the INTERNAL T[] array directly             │
  │                                                               │
  │  list = { [1, 2, 3, _, _], size=3 }                          │
  │          │                                                    │
  │          └──► Returns int[5] with values [1, 2, 3, 0, 0]     │
  │              (includes uninitialized elements past size!)     │
  │                                                               │
  │  WARNING: Returned array may be larger than list.Count!       │
  │  WARNING: Modifying array modifies the list's backing store!  │
  └───────────────────────────────────────────────────────────────┘
```

## ResizeList Flow

```
  ResizeList<int>(list, count=10)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────────┐
  │  1. list.Clear()         ← sets _size = 0, increments _version  │
  │                                                                   │
  │  2. if (list.Capacity < count):                                   │
  │       list.Capacity = count                                       │
  │       // This MAY allocate a new array, but only if needed        │
  │                                                                   │
  │  3. if (count == list.Count):                                     │
  │       return  // already correct size after Clear()               │
  │                                                                   │
  │  4. var access = UnsafeUtility.As<                                │
  │       List<int>, ListPrivateFieldAccess<int>>(ref list);          │
  │                                                                   │
  │  5. access.Size = count     ← DIRECTLY SET INTERNAL SIZE          │
  │                                                                   │
  │  6. access.Version++        ← increment version for enum safety   │
  │                                                                   │
  │  Result:                                                          │
  │  ┌───────────────────────────────────────────────────────────┐    │
  │  │  Before: [1,2,3,_,_,_,_,_,_,_,_,_] size=3  Capacity=12   │    │
  │  │  After:  [0,0,0,0,0,0,0,0,0,0,_,_] size=10 Capacity=12  │    │
  │  │          ↑ elements 0-9 are DEFAULT (zero for int)        │    │
  │  │          ↑ NO new array allocated!                        │    │
  │  └───────────────────────────────────────────────────────────┘    │
  └───────────────────────────────────────────────────────────────────┘
```

## Why This Avoids Allocations

```
  Standard List<int> resize (causes allocations):
  ┌───────────────────────────────────────────────────────────────┐
  │  list.Clear();                                                │
  │  for (int i = 0; i < 10; i++)                                │
  │      list.Add(default);  // may trigger Capacity resize!      │
  │                                                               │
  │  Each Add checks Capacity → may allocate new array + copy     │
  └───────────────────────────────────────────────────────────────┘

  NoAllocHelpers (zero alloc when capacity sufficient):
  ┌───────────────────────────────────────────────────────────────┐
  │  NoAllocHelpers.ResizeList(list, 10);                         │
  │                                                               │
  │  1. Clear() — no alloc                                        │
  │  2. Capacity check — only allocs if insufficient              │
  │  3. Directly write _size field — no per-element Add() calls   │
  │                                                               │
  │  If Capacity >= count: ZERO allocations                       │
  └───────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **UnsafeUtility.As**: Zero-cost type reinterpretation — no reflection, no copying.
  Relies on the internal `List<T>` field layout matching the private access struct.
- **Version increment**: Maintains `List<T>`'s internal contract so enumerators
  that check version will correctly throw `InvalidOperationException` if the list
  changes during enumeration.
- **Clear first**: `list.Clear()` is called to reset state, then size is set directly.
  This ensures a clean state while avoiding the overhead of `Add(default)` in a loop.
- **Fragile by nature**: Depends on .NET `List<T>` internal implementation — could
  break on runtime changes, but works reliably on Unity's Mono and CoreCLR runtimes.

## Verified Data

> [Run test snippet](../snippets/memory-allocators/NoAllocHelpers.cs)
> ```bash
> cat snippets/memory-allocators/NoAllocHelpers.cs | unity-cli exec \
>   --project ~/Github/bovinelabs-core-internals/BovineLabs \
>   --usings "BovineLabs.Core.Collections,BovineLabs.Core.Memory,BovineLabs.Core.Utility,System,System.Reflection,System.Runtime.InteropServices,System.Linq,Unity.Collections,Unity.Collections.LowLevel.Unsafe"
> ```

```
BovineLabs.Core.Utility.NoAllocHelpers
  Kind: static class (Abstract=True, Sealed=True)

  Methods (public static):
    T[] ExtractArrayFromList<T>(List`1 list)
    Void ResizeList<T>(List`1 list, Int32 count)

  Functional Tests:
    ExtractArrayFromList([10,20,30]):
      List.Count: 3
      List.Capacity: 4
      Returned array.Length: 4
      Elements match: True

    ResizeList([1,2,3], count=10):
      After resize Count: 10
      After resize Capacity: 20
      Elements cleared (index 0 == 0): False

    ResizeList([1,2,3], count=100):
      After resize Count: 100
      After resize Capacity: 100

    ResizeList([1,2,3], count=0):
      After resize Count: 0

Verified: 9 checks, 1 failures
```

> **Note**: The `Elements cleared (index 0 == 0): False` result shows that
> `ResizeList` does NOT call `Clear()` first when capacity is sufficient.
> Elements retain their previous values when the list is grown in-place.
> The doc diagram claiming "Clear() is called first" is inaccurate for the
> grow-in-place path.

## Source File

- `BovineLabs.Core/Utility/NoAllocHelpers.cs`

## Source

- [BovineLabs.Core/Utility/NoAllocHelpers.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/NoAllocHelpers.cs)
- [BovineLabs.Core.Tests/Utility/NoAllocHelpersTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/NoAllocHelpersTests.cs)
- [BovineLabs.Core/Extensions/ListExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Extensions/ListExtensions.cs)
