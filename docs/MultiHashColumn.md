# MultiHashColumn — Multi-Map Column for Dynamic Hashmaps

## Overview

MultiHashColumn<T> implements IColumn<T> using a bucket-chain hash table that allows multiple
entries to share the same column value. It provides forward iteration via HashMapIterator<T>.

```
┌─────────────────────────────────────────────────────────────────────┐
│  MultiHashColumn<T> : IColumn<T>                                   │
│    where T : unmanaged, IEquatable<T>                               │
│                                                                     │
│  Fields (all private):                                              │
│    int keysOffset, nextOffset, bucketsOffset, capacity              │
│                                                                     │
│  Pointer Properties (computed from offsets):                        │
│    T* Keys     = (T*)((byte*)&this + keysOffset)                    │
│    int* Next   = (int*)((byte*)&this + nextOffset)                  │
│    int* Buckets = (int*)((byte*)&this + bucketsOffset)              │
│                                                                     │
│  Bucket capacity = capacity * 2                                     │
│                                                                     │
│  Iteration:                                                         │
│    TryGetFirst(T column, out HashMapIterator<T> it) → bool          │
│    TryGetNext(ref HashMapIterator<T> it) → bool                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Data Structure

```
Buckets (capacity * 2 entries, initialized to -1)
┌────┬────┬────┬────┬────┬────┐
│ -1 │  3 │ -1 │  0 │ -1 │ -1 │  ← head indices per bucket
└────┴────┴────┴────┴────┴────┘

Keys (capacity entries)
┌──────┬──────┬──────┬──────┐
│ "A"  │ "B"  │ "A"  │ "B"  │
└──────┴──────┴──────┴──────┘
  [0]    [1]    [2]    [3]

Next (capacity entries, linked list)
┌────┬────┬────┬────┐
│ -1 │  2 │ -1 │  0 │
└────┴────┴────┴────┘

Example: bucket for "B" hash → Buckets[bucket]=3 → Next[3]=0 → Next[0]=-1
  (entries 3 and 0 both have key "B")
```

## Key Design Decisions

- **Separate chaining**: Uses linked lists per bucket rather than open addressing, allowing
  O(1) insert and straightforward multi-value iteration.
- **2x bucket capacity**: Buckets array is twice the entry capacity to maintain low load factor.
- **Replace optimization**: If old and new keys hash to the same bucket, updates in place;
  otherwise does remove + re-add.
- **Resize via rehash**: StartResize saves all old data; ApplyResize re-inserts every entry
  into the new (larger) bucket array.

## Verified Data

```
MultiHashColumn<T>
  Kind: struct, generic (1 param)
  Public Methods: Boolean TryGetFirst, Boolean TryGetNext
  IColumn<T> methods: Initialize, CalculateDataSize, GetValue, Add, Replace, Remove, Clear, StartResize, ApplyResize, GetValueOld

Verified: 1 checks, 0 failures
```


> Tested example: [Example/ColumnsExample.cs](../Example/ColumnsExample.cs)

## Source

- [BovineLabs.Core/Iterators/DynamicHashMap/Columns/MultiHashColumn.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/Columns/MultiHashColumn.cs)
