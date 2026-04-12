# IColumn — Dynamic Hashmap Column Interface

## Overview

IColumn<T> defines the interface for secondary data columns that map alongside a dynamic hashmap's
primary key-value storage. Columns allow attaching extra indexed data (like sorted orders or
multi-map groupings) to the same entries in a DynamicVariableMap or similar structure.

```
┌─────────────────────────────────────────────────────────────────────┐
│  IColumn<T>  where T : unmanaged, IEquatable<T>                    │
│                                                                     │
│  Lifecycle:                                                         │
│    void Initialize(int offset, int capacity)                        │
│    int CalculateDataSize(int capacity)                              │
│                                                                     │
│  Data Operations:                                                   │
│    T GetValue(int idx)                                              │
│    void Add(T key, int idx)                                         │
│    void Replace(T newKey, int idx)                                  │
│    void Remove(int idx)                                             │
│    void Clear()                                                     │
│                                                                     │
│  Resize:                                                            │
│    void* StartResize()                                              │
│    void ApplyResize(void* resizePtr)                                │
│    T GetValueOld(void* resizePtr, int idx)                          │
└─────────────────────────────────────────────────────────────────────┘
```

## Implementations

| Implementation | Behavior |
|---------------|----------|
| `MultiHashColumn<T>` | Allows multiple entries with same key (bucket-chain hash) |
| `OrderedListColumn<T>` | Maintains sorted doubly-linked list (T must be IComparable) |

## Memory Layout

Columns store their data relative to an offset from the parent structure's base pointer:

```
Parent Structure Memory
┌─────────────────────────────────────────────────────────┐
│  [primary hashmap data]                                  │
│  ...                                                     │
│  ┌─────────────────────────────────────────────────────┐│
│  │ Column data starts at 'offset'                      ││
│  │  Keys:   T[capacity]                                ││
│  │  Next:   int[capacity]                              ││
│  │  Prev/Buckets: int[capacity] or int[bucketCap]      ││
│  └─────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Offset-based storage**: Columns don't own their memory — they operate at offsets within a
  parent allocation, enabling zero-copy column addition to existing hash structures.
- **Manual resize protocol**: StartResize captures old state, ApplyResize migrates to new
  capacity. This two-phase approach allows the parent to coordinate resize across multiple columns.
- **IEquatable<T> constraint**: Required for key comparison in hash-based column operations.

## Verified Data

```
IColumn<T>
  Kind: interface, 1 generic param (where T: unmanaged, IEquatable<T>)

Verified: 1 checks, 0 failures
```


> Tested example: [Example/ColumnsExample.cs](../Example/ColumnsExample.cs)

## Source

- [BovineLabs.Core/Iterators/DynamicHashMap/Columns/IColumn.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Iterators/DynamicHashMap/Columns/IColumn.cs)
