# FunctionsHash — Hash-Based Dynamic Dispatch

## Overview

FunctionsHash<T, TO> is a Burst-compatible native container that holds function pointers in a
NativeHashMap<long, FunctionData>. Unlike Functions<T,TO> which uses index-based dispatch,
FunctionsHash uses a hash key to look up and execute a specific function at runtime.

```
┌─────────────────────────────────────────────────────────────────────┐
│                 FunctionsHash<T, TO>                                │
│                                                                     │
│  NativeHashMap<long, FunctionData>                                 │
│  ┌────────────┬────────────────────────────────┐                   │
│  │  Hash Key   │  FunctionData                  │                   │
│  ├────────────┼────────────────────────────────┤                   │
│  │  0x3A2F... │  Target, Execute, Update, Dest │                   │
│  │  0x7B1C... │  Target, Execute, Update, Dest │                   │
│  │  0xE4D8... │  Target, Execute, Update, Dest │                   │
│  └────────────┴────────────────────────────────┘                   │
│                                                                     │
│  TryExecute(hash, ref data, out result) → bool                      │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  1. functions.TryGetValue(hash, out entry)                    │  │
│  │  2. If not found: result = default, return false              │  │
│  │  3. ptr = UnsafeUtility.AddressOf(ref data)                   │  │
│  │  4. TO resultValue = default                                  │  │
│  │  5. entry.ExecuteFunction.Invoke(entry.Target, ptr, &result) │  │
│  │  6. result = resultValue, return true                         │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Length` | int | Count via functions.Count |
| `Update(ref state)` | void | Calls UpdateFunction on all entries |
| `TryExecute(hash, ref data, out result)` | bool | Hash lookup + execute, false if not found |
| `OnDestroy(ref state)` | void | Destroys all + frees memory + disposes map |

## Key Design Decisions

- **Hash-based dispatch**: Use when the function to call is determined at runtime by a type hash
  (e.g., component type hash, prefab hash, mod-provided identifier).
- **TryExecute pattern**: Returns bool instead of throwing — callers can gracefully handle
  unregistered function types.
- **[ReadOnly] NativeHashMap**: Marked [ReadOnly] for use inside Burst jobs.
- **Same lifecycle as Functions**: Update/OnDestroy iterate all entries; only Execute differs
  (hash lookup vs index).

## Verified Data

```
FunctionsBuilder<T,TO>.BuildHash() creates NativeHashMap<long, FunctionData>
FunctionData: 32 bytes (Target, DestroyFunction, ExecuteFunction, UpdateFunction)

Verified: 1 checks, 0 failures
```


> Tested example: [Example/FunctionsExample.cs](../Example/FunctionsExample.cs)

## Source

- [BovineLabs.Core/Functions/FunctionsHash.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/FunctionsHash.cs)
