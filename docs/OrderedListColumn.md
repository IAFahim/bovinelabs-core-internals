# OrderedListColumn — Sorted Linked-List Column

## Overview

OrderedListColumn<T> implements IColumn<T> using a sorted doubly-linked list. Elements are
maintained in ascending order via IComparable<T>, enabling efficient sequential access through
GetFirst/GetNext iteration.

```
┌─────────────────────────────────────────────────────────────────────┐
│  OrderedListColumn<T> : IColumn<T>                                 │
│    where T : unmanaged, IEquatable<T>, IComparable<T>              │
│                                                                     │
│  Fields:                                                            │
│    int keysOffset, nextOffset, prevOffset, head, capacity          │
│                                                                     │
│  Pointer Properties:                                                │
│    T* Keys, int* Next, int* Prev                                    │
│                                                                     │
│  Iteration:                                                         │
│    int GetFirst() → head index (-1 if empty)                        │
│    int GetNext(int current) → Next[current]                         │
│    TryGetFirst(out T value, out OrderedListIterator it) → bool      │
│    TryGetNext(out T value, ref OrderedListIterator it) → bool       │
│                                                                     │
│  OrderedListIterator:                                               │
│    int EntryIndex       ← current element                           │
│    int NextEntryIndex   ← next element in chain                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Sorted Insertion

```
AddInternal(key, idx)
       │
       ▼
  keys[idx] = key, next[idx] = -1, prev[idx] = -1
       │
       ├─ head == -1? → head = idx (list was empty)
       │
       ├─ key < keys[head]? → insert before head
       │    next[idx] = head
       │    prev[head] = idx
       │    head = idx
       │
       └─ Walk forward: current = head
            while next[current] != -1 && keys[next[current]] < key
              current = next[current]
            Insert after current:
              next[idx] = next[current]
              next[current] = idx
              prev[idx] = current
              if next[idx] != -1: prev[next[idx]] = idx
```

## Replace Optimization

```
Replace(newKey, idx)
       │
       ├─ newKey == oldKey? → nothing to do
       │
       ├─ Can stay in place?
       │    prev[idx] == -1 || keys[prev] <= newKey
       │    next[idx] == -1 || newKey <= keys[next]
       │    → just update keys[idx] = newKey
       │
       └─ Otherwise: RemoveInternal(idx) + AddInternal(newKey, idx)
```

## Key Design Decisions

- **Doubly-linked**: Next and Prev arrays enable O(1) removal at any position.
- **IComparable<T> required**: Unlike MultiHashColumn, this column needs ordering, so T must
  implement IComparable.
- **Replace optimization**: Checks if the new value can stay in the same position (between the
  same neighbors) before doing a full remove + re-add.
- **Resize preserves order**: Copies old Next/Prev arrays directly, only initializing new slots
  to -1.

## Verified Data

```
OrderedListColumn<T>
  Kind: struct, generic (1 param, where T: unmanaged, IEquatable<T>, IComparable<T>)
  Public Methods: T GetValue, Int32 GetFirst, Int32 GetNext, Boolean TryGetFirst, Boolean TryGetNext

OrderedListIterator
  Kind: struct, 8 bytes
  Fields: Int32 EntryIndex, Int32 NextEntryIndex

Verified: 2 checks, 0 failures
```

## Source

- [BovineLabs.Core/Iterators/DynamicHashMap/Columns/OrderedListColumn.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/Columns/OrderedListColumn.cs)
