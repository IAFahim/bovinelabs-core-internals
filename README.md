# Pin — Inner Workings

## Overview

Pin uses an explicit-layout union struct to reinterpret a managed object reference
as a `Pinnable` class, allowing the `fixed` statement to pin the object and give
raw `byte*` access to its memory — all without GCHandle or Marshal.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Pin (static class)                                   │
│                                                                             │
│  GetRawObjectData(object o) → ref byte                                      │
│                                                                             │
│  Usage:                                                                     │
│    fixed (byte* ptr = &Pin.GetRawObjectData(myObject)) { ... }             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## The Union Trick

```
  PinnableUnion (LayoutKind.Explicit, both at FieldOffset 0)
  ┌────────────────────────────────────────────────────────────────────┐
  │                                                                    │
  │  [FieldOffset(0)]                                                  │
  │  object Object;          ← references the managed object           │
  │                                                                    │
  │  [FieldOffset(0)]                                                  │
  │  Pinnable Pinnable;      ← overlaid on same memory                 │
  │                                                                    │
  │  ┌──────────────────────────────────────────────────────────────┐  │
  │  │  MEMORY OVERLAY:                                             │  │
  │  │                                                              │  │
  │  │  The object reference and Pinnable reference share the       │  │
  │  │  same pointer slot. When you assign Object = o, the         │  │
  │  │  runtime now ALSO sees Pinnable as pointing to the same      │  │
  │  │  object, but typed as the Pinnable class.                    │  │
  │  └──────────────────────────────────────────────────────────────┘  │
  └────────────────────────────────────────────────────────────────────┘
```

## How It Works Step by Step

```
  Pin.GetRawObjectData(myByteArray)
         │
         ▼
  ┌─────────────────────────────────────────────────────────────────────┐
  │  1. new PinnableUnion(myByteArray)                                  │
  │     ┌─────────────────────────────────────────────────────────────┐ │
  │     │  this = default;                                            │ │
  │     │  this.Object = myByteArray;                                 │ │
  │     │                                                             │ │
  │     │  Result: Pinnable field NOW points to myByteArray           │ │
  │     │  (same pointer, different type)                              │ │
  │     └─────────────────────────────────────────────────────────────┘ │
  │                                                                     │
  │  2. return ref PinnableUnion.Pinnable.Data                         │
  │     ┌─────────────────────────────────────────────────────────────┐ │
  │     │                                                             │ │
  │     │  myByteArray in memory:                                     │ │
  │     │  ┌──────────────────────────────────────────────────────┐   │ │
  │     │  │  Object Header │ Method Table │ Field1 │ Field2 │...│   │ │
  │     │  └──────────────────────────────────────────────────────┘   │ │
  │     │                                                             │ │
  │     │  Pinnable sees this as:                                     │ │
  │     │  ┌──────────────────────────────────────────────────────┐   │ │
  │     │  │  Object Header │ Method Table │ Data (byte) │ ...    │   │ │
  │     │  └──────────────────────────────────────────────────────┘   │ │
  │     │                              ^                              │ │
  │     │                              │ ref to this byte             │ │
  │     │                                                             │ │
  │     │  The first byte AFTER the object header+method table        │ │
  │     │  of the original object is now accessible via Data          │ │
  │     └─────────────────────────────────────────────────────────────┘ │
  │                                                                     │
  │  3. Caller does: fixed (byte* ptr = &result) { ... }               │
  │     This pins the object in GC memory for the duration of fixed    │
  └─────────────────────────────────────────────────────────────────────┘
```

## Memory Layout Visualization

```
  The key insight: Pinnable has a single byte field "Data" at the start
  of its instance data. When overlaid onto any object, "Data" points to
  the first byte of that object's instance data.

  Managed object in GC heap:
  ┌───────────────────────────────────────────────────┐
  │  SyncBlock  │  TypeHandle  │  Instance Data...    │
  │  (indirect) │  (method tbl)│                      │
  └───────────────────────────────────────────────────┘
                              ▲
                              │
  Pinnable.Data ──────────────┘
  (first field of Pinnable = first byte of instance data)

  For a byte[]:
  ┌─────────┬───────────┬──────────┬──┬──┬──┬──┬──┐
  │SyncBlk  │TypeHandle │Length    │b0│b1│b2│b3│..│
  └─────────┴───────────┴──────────┴──┴──┴──┴──┴──┘
  ▲                              ▲
  │                              │
  (not accessible)          Data = ref to b0
```

## Usage Example

```
  byte[] managedArray = GetSomeArray();

  // Get raw byte pointer to managed data
  fixed (byte* ptr = &Pin.GetRawObjectData(managedArray))
  {
      // ptr now points to the raw object bytes
      // Object is pinned by the fixed statement
      // Can read/write raw memory
      ptr[0] = 42;
  }
  // Object is unpinned when fixed scope ends
```

## Why Pinnable Is a Class (Not Struct)

```
  ┌────────────────────────────────────────────────────────────────────┐
  │  Pinnable MUST be a reference type (class):                        │
  │                                                                    │
  │  - The union overlays an OBJECT reference with a PINNABLE          │
  │    reference. Both must be heap pointers for the reinterpret       │
  │    to work.                                                        │
  │                                                                    │
  │  - If Pinnable were a struct, the overlay would break because     │
  │    the struct would be a value type (inline data) not a pointer.   │
  │                                                                    │
  │  - The sealed class has exactly one field (byte Data) which        │
  │    maps to the first byte of the target object's fields.           │
  └────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **No GCHandle**: Unlike `GCHandle.Alloc(obj, GCHandleType.Pinned)`, this approach
  doesn't require an explicit handle — the `fixed` statement does the pinning.
- **No Marshal**: No `StructureToPtr` copying — it's a direct reference into the
  managed object's memory.
- **Explicit layout union**: The `StructLayout(LayoutKind.Explicit)` with both fields
  at offset 0 is the core trick — the CLR treats both references as pointing to
  the same object but with different types.
- **Origin**: Adapted from a StackOverflow technique for pinning arbitrary objects.

## Source File

- `BovineLabs.Core/Utility/Pin.cs`

## Source

- [BovineLabs.Core/Utility/Pin.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/Pin.cs)
- [BovineLabs.Core/Utility/SpinLock.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/SpinLock.cs)
- [BovineLabs.Core.Extensions.Editor/ObjectManagement/ObjectGroupInspector.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions.Editor/ObjectManagement/ObjectGroupInspector.cs)
