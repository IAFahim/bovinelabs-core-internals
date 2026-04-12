# Functions — Index-Based Function Pointer Container

## Overview

Functions<T, TO> is a Burst-compatible native container that holds an array of compiled function
pointers. It supports index-based dispatch — call Execute with an index to invoke a specific
function implementation inside a job.

```
┌─────────────────────────────────────────────────────────────────────┐
│                 Functions<T, TO>                                    │
│                                                                     │
│  NativeArray<FunctionData>                                         │
│  ┌───────┬───────┬───────┬───────┐                                 │
│  │  [0]  │  [1]  │  [2]  │  ...  │                                 │
│  └───┬───┴───┬───┴───┬───┴───────┘                                 │
│      │       │       │                                              │
│      ▼       ▼       ▼                                              │
│  Each FunctionData:                                                 │
│  ┌────────────────────────────────────────┐                         │
│  │  void* Target              ──────────► Pinned IFunction instance │
│  │  IntPtr DestroyFunction                 (managed delegate ptr)   │
│  │  FunctionPointer<ExecuteFunction>       (Burst-compiled)        │
│  │  FunctionPointer<UpdateFunction>        (Burst-compiled)        │
│  └────────────────────────────────────────┘                         │
│                                                                     │
│  Usage in a system:                                                 │
│    OnCreate: builder.ReflectAll(ref state).Build()                  │
│    OnUpdate: functions.Update(ref state)                            │
│    In Job:   functions.Execute(index, ref data) → TO                │
│    OnDestroy: functions.OnDestroy(ref state)                        │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Methods

```
Execute(int index, ref T data) → TO
  ┌───────────────────────────────────────────────────────────────────┐
  │  1. ref var entry = ref functions.ElementAt(index)               │
  │  2. ptr = UnsafeUtility.AddressOf(ref data)                      │
  │  3. TO result = default                                          │
  │  4. entry.ExecuteFunction.Invoke(entry.Target, ptr, &result)    │
  │  5. return result                                                 │
  └───────────────────────────────────────────────────────────────────┘

OnDestroy(ref SystemState state)
  ┌───────────────────────────────────────────────────────────────────┐
  │  foreach function:                                                │
  │    if DestroyFunction != IntPtr.Zero:                             │
  │      Marshal → delegate → invoke(target, ref state)              │
  │    UnsafeUtility.FreeTracked(target, Allocator.Persistent)       │
  │  functions.Dispose()                                              │
  └───────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Index-based dispatch**: Use when you know which function to call by position (e.g., iterating
  all registered implementations).
- **[ReadOnly] NativeArray**: The internal array is marked [ReadOnly] so it can be passed into
  Burst jobs without additional safety setup.
- **Burst-compiled execution**: The Execute path goes through FunctionPointer<ExecuteFunction>,
  which is fully Burst-compatible.
- **Managed destroy**: OnDestroy uses Marshal.GetDelegateForFunctionPointer to call back into
  managed code for cleanup — this cannot run inside a job.

## Verified Data

```
FunctionData
  Kind: struct, 32 bytes
  Fields: Void* Target (public), IntPtr DestroyFunction (public), FunctionPointer<ExecuteFunction> ExecuteFunction (public), FunctionPointer<UpdateFunction> UpdateFunction (public)

FunctionsBuilder<T,TO> methods: Dispose, ReflectAll, Add (x3), Build, BuildHash

Verified: 2 checks, 0 failures
```


> Tested example: [Example/FunctionsExample.cs](../Example/FunctionsExample.cs)

## Source

- [https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/Functions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/Functions.cs)
- [https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/FunctionData.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/FunctionData.cs)
