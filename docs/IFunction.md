# IFunction — Burst-Compatible Function Interface

## Overview

IFunction<T> is the base interface for defining Burst-compiled function pointers that can be
dynamically dispatched inside ECS jobs. It provides a standardized lifecycle (Create → Update →
Execute → Destroy) and uses raw void* pointers for zero-allocation argument passing.

```
┌─────────────────────────────────────────────────────────────────────┐
│                    IFunction<T> Lifecycle                           │
│                                                                     │
│  FunctionsBuilder                                                   │
│  ════════════════                                                   │
│                                                                     │
│  1. ReflectAll() discovers all IFunction<T> implementations         │
│  2. For each implementation:                                        │
│     ┌───────────────────────────────────────────────────────────┐   │
│     │  a. Allocate pinned memory (UnsafeUtility.MallocTracked) │   │
│     │  b. Call OnCreate(ref state)                              │   │
│     │  c. Compile ExecuteFunction via BurstCompiler             │   │
│     │  d. Compile UpdateFunction via BurstCompiler (optional)   │   │
│     │  e. Marshal DestroyFunction (optional, NOT Burst)         │   │
│     └───────────────────────────────────────────────────────────┘   │
│  3. Build() or BuildHash() creates the container                    │
│                                                                     │
│  At Runtime (inside Burst Job)                                      │
│  ═════════════════════════                                          │
│                                                                     │
│  functions.Update(ref state)   → calls UpdateFunction on each      │
│  functions.Execute(idx, ref T) → calls ExecuteFunction, returns TO  │
│  functions.OnDestroy(ref state)→ calls DestroyFunction, frees mem   │
└─────────────────────────────────────────────────────────────────────┘
```

## Delegate Types

Three unsafe delegate types define the function signatures:

```
UpdateFunction:  void (void* target, ref SystemState state)
DestroyFunction: void (void* target, ref SystemState state)
ExecuteFunction: void (void* target, void* data, void* result)

  target → pointer to the pinned IFunction instance
  data   → void* pointing to input of type T
  result → void* pointing to output of type TO
```

## Interface Members

```
IFunction<T> where T : unmanaged
┌─────────────────────────────────────────────────────────────────┐
│  Properties:                                                     │
│    DestroyFunction  (DestroyFunction?)  — Optional cleanup      │
│    UpdateFunction   (UpdateFunction?)   — Optional per-frame    │
│    ExecuteFunction  (ExecuteFunction)   — Required execution    │
│                                                                  │
│  Methods:                                                        │
│    OnCreate(ref SystemState state)      — Optional, virtual      │
│      Default implementation does nothing                          │
└─────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **T as grouping key**: The generic parameter T serves dual purpose — it's the data type passed
  to Execute, and it groups all IFunction<T> implementations for ReflectAll discovery.
- **void* pointers**: All delegate signatures use raw pointers for Burst compatibility and
  zero-allocation dispatch.
- **DestroyFunction is NOT Burst-compiled**: Uses Marshal.GetFunctionPointerForDelegate instead
  of BurstCompiler.CompileFunctionPointer, allowing managed operations during cleanup.
- **UpdateFunction and ExecuteFunction ARE Burst-compiled**: Uses BurstCompiler.CompileFunctionPointer
  for maximum performance inside jobs.
- **OnCreate is optional**: Default implementation is empty; override to initialize lookups or
  allocate resources.

## Verified Data

```
IFunction<T>
  Kind: interface, 1 generic param
  Properties: DestroyFunction DestroyFunction, UpdateFunction UpdateFunction, ExecuteFunction ExecuteFunction
  Methods: Void OnCreate(Microsoft)

Delegates:
  UpdateFunction: delegate
  DestroyFunction: delegate
  ExecuteFunction: delegate

Verified: 4 checks, 0 failures
```


> Tested example: [Example/FunctionsExample.cs](../Example/FunctionsExample.cs)

## Source

- [https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/IFunction.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/IFunction.cs)
