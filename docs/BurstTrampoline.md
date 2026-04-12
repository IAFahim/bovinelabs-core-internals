# BurstTrampoline — Inner Workings

## Overview

BurstTrampoline bridges the **Burst-compiled (unmanaged) world** and the **managed C# world**,
allowing Burst jobs to invoke managed delegates through unmanaged function pointers. It works
by packing arguments into a single opaque pointer+size payload and routing through a shared
native wrapper trampoline.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         BURST COMPILED JOB                                 │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  trampoline.Invoke(ref args);                                         │  │
│  │       │                                                               │  │
│  │       ▼                                                               │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │  Invoke(ptr, size)                                              │  │  │
│  │  │    calls wrapperPtr(managedFunctionPtr, argumentsPtr, size)     │  │  │
│  │  └──────────────┬──────────────────────────────────────────────────┘  │  │
│  └─────────────────┼─────────────────────────────────────────────────────┘  │
│                     │ UNMANAGED → MANAGED BOUNDARY                         │
│                     ▼                                                        │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │  static Wrapper(managedFunctionPtr, argumentsPtr, argumentsSize)      │  │
│  │    │                                                                  │  │
│  │    │  Cast managedFunctionPtr → delegate*<void*, int, void>           │  │
│  │    │  Call it with (argumentsPtr, argumentsSize)                       │  │
│  │    ▼                                                                  │  │
│  │  MANAGED CALLBACK: void MyCallback(void* ptr, int size)               │  │
│  │    └─ ArgumentsFromPtr<T>(ptr, size) → ref T                          │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Struct Layout

```
 BurstTrampoline (readonly struct)
 ┌──────────────────────────────────────────┐
 │  IntPtr managedFunctionPtr               │  ← pointer to user's callback
 │  IntPtr wrapperPtr                       │  ← pointer to cached Wrapper delegate
 └──────────────────────────────────────────┘

 static cachedWrapperPtr (IntPtr)
 ┌──────────────────────────────────────────┐
 │  Lazily initialized once via             │
 │  Marshal.GetFunctionPointerForDelegate   │
 │  GCHandle.Alloc prevents GC collection   │
 └──────────────────────────────────────────┘
```

## Initialization Flow

```
  new BurstTrampoline(fnPtr)
         │
         ▼
  Initialize()
         │
         ├─ cachedWrapperPtr != default? ──YES──► skip (already cached)
         │
         └─ NO:
            ┌─────────────────────────────────────────┐
            │  1. WrapperDelegate wrapperDelegate       │
            │       = new WrapperDelegate(Wrapper);     │
            │  2. GCHandle.Alloc(wrapperDelegate);      │
            │     // prevents GC from collecting it     │
            │  3. cachedWrapperPtr =                     │
            │       Marshal.GetFunctionPointerForDelegate│
            │       (wrapperDelegate);                   │
            └─────────────────────────────────────────┘
```

## Invoke Data Flow

```
  Burst Job (unmanaged)                   Managed World
  ════════════════════                    ═════════════

  trampoline.Invoke(ref args);
         │
         │  fixed (T* ptr = &args)
         │  {
         │     Invoke(ptr, sizeof(T))
         │  }
         │
         ▼
  ┌──────────────────────────────────────────────────────────────┐
  │  unmanaged[Cdecl] call:                                      │
  │    wrapperPtr(managedFunctionPtr, argumentsPtr, argumentsSize)│
  └──────────────────────┬───────────────────────────────────────┘
                         │
                         ▼
              Wrapper() [MonoPInvokeCallback]
                         │
                         ▼
              ((delegate*)managedFunctionPtr)(argumentsPtr, size)
                         │
                         ▼
              User's managed callback receives raw pointer
              Uses ArgumentsFromPtr<T>() to rehydrate:
                ref T = *(T*)argumentsPtr   // zero-copy cast
```

## Extension Methods — Argument Packing

```
  Invoke()                          → BurstManagedNoArgs  (dummy byte)
  Invoke<T>(in T)                   → T passed directly
  Invoke<T1,T2>(in T1, in T2)       → BurstManagedPair<T1,T2>
  Invoke<T1,T2,T3>(...)             → BurstManagedTriple<T1,T2,T3>
  InvokeOut<TOut>(out TOut)         → TOut allocated, ref passed through
  InvokeOut<TIn,TOut>(in, out)      → BurstManagedPair<TIn,TOut>
  InvokeOut<T1,T2,TOut>(in,in,out)  → BurstManagedTriple<T1,T2,TOut>

  BurstManagedPair<TFirst, TSecond>
  ┌───────────────────────┐
  │  TFirst   First       │   sizeof(TFirst)
  │  TSecond  Second      │   sizeof(TSecond)
  └───────────────────────┘
  Packed into contiguous unmanaged memory, passed by pointer.
```

## Key Design Decisions

- **Single wrapper for all signatures**: Arguments are always packed as (void*, int),
  so one cached `WrapperDelegate` serves every BurstTrampoline instance.
- **GCHandle pinning**: The wrapper delegate is pinned forever with `GCHandle.Alloc`
  so it's never garbage collected.
- **Calling convention**: Uses `Cdecl` for compatibility between Burst and managed code.
- **No boxing**: Generic constraints (`where T : unmanaged`) ensure zero allocations.
- **Safety check**: `ArgumentsFromPtr` validates size matches `sizeof(T)` in
  `ENABLE_UNITY_COLLECTIONS_CHECKS` builds.

## Verified Data

```
BurstTrampoline
  Kind: readonly struct
  Uses cached static wrapperPtr for unmanaged-to-managed bridge
  Extension methods: Invoke (6 overloads) for argument packing
  BurstManagedPair<T1,T2> and BurstManagedTriple<T1,T2,T3> for multi-arg packing

Verified: 1 checks, 0 failures
```

## Source Files

- `BovineLabs.Core/Utility/BurstTrampoline.cs` — Core struct
- `BovineLabs.Core/Utility/BurstTrampolineExtensions.cs` — Generic overloads & pair/triple structs

## Source

- [BovineLabs.Core/Utility/BurstTrampoline.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/BurstTrampoline.cs)
- [BovineLabs.Core/Utility/BurstTrampolineExtensions.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/BurstTrampolineExtensions.cs)
