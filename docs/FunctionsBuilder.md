# FunctionsBuilder — Reflection-Based Function Registration

## Overview

FunctionsBuilder<T, TO> collects IFunction<T> implementations (either via reflection or manual
addition), pins them in unmanaged memory, compiles their function pointers with Burst, and builds
either a Functions<T,TO> (index-based) or FunctionsHash<T,TO> (hash-based) container.

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    FunctionsBuilder<T, TO> Pipeline                       │
│                                                                           │
│  new FunctionsBuilder<T, TO>(Allocator.Temp)                              │
│         │                                                                 │
│         ▼                                                                 │
│  .ReflectAll(ref state)         ← discovers all IFunction<T> via          │
│         │                         ReflectionUtility.GetAllImplementations  │
│         │                         Caches MethodInfos for reuse             │
│         ▼                                                                 │
│  For each discovered type TF:                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │  1. MallocTracked(sizeof(TF)) → pinned unmanaged memory            │  │
│  │  2. *pinned = default(TF)                                          │  │
│  │  3. pinned->OnCreate(ref state)                                    │  │
│  │  4. BurstCompiler.CompileFunctionPointer(pinned->ExecuteFunction)  │  │
│  │  5. BurstCompiler.CompileFunctionPointer(pinned->UpdateFunction)   │  │
│  │     (only if UpdateFunction != null)                                │  │
│  │  6. Marshal.GetFunctionPointerForDelegate(pinned->DestroyFunction) │  │
│  │     (only if DestroyFunction != null)                               │  │
│  │  7. Add to NativeHashSet<BuildData> keyed by hash                   │  │
│  └─────────────────────────────────────────────────────────────────────┘  │
│         │                                                                 │
│         ├─ .Build()      → Functions<T, TO>   (NativeArray<FunctionData>) │
│         │                                                                 │
│         └─ .BuildHash()  → FunctionsHash<T, TO> (NativeHashMap<long, FD>) │
└──────────────────────────────────────────────────────────────────────────┘
```

## Struct Layout

```
FunctionsBuilder<T, TO> : IDisposable
┌──────────────────────────────────────────┐
│  NativeHashSet<BuildData> functions      │  ← deduplication + enumeration
│  static List<MethodInfo> cachedReflectAll │  ← per-type, cached across instances
└──────────────────────────────────────────┘

BuildData (private, IEquatable<BuildData>)
┌──────────────────────────────────────────┐
│  long Hash                               │  ← BurstRuntime.GetHashCode64<TF>()
│  FunctionData FunctionData               │  ← compiled pointers + target
└──────────────────────────────────────────┘
```

## Key Methods

| Method | Description |
|--------|-------------|
| `ReflectAll(ref state)` | Finds all unmanaged IFunction<T> implementations via reflection |
| `Add<TF>(ref state, fn)` | Manual add with auto-hash from BurstRuntime |
| `Add<TF>(ref state, fn, hash)` | Manual add with explicit hash |
| `Add<TF>(ref state)` | Manual add with default(TF) instance |
| `Build()` | Returns index-based Functions<T,TO> |
| `BuildHash()` | Returns hash-based FunctionsHash<T,TO> |

## Key Design Decisions

- **Pinned allocation**: Uses UnsafeUtility.MallocTracked with Allocator.Persistent so function
  pointers remain valid across job scheduling and execution.
- **Duplicate detection**: NativeHashSet keyed by hash prevents registering the same function type
  twice; logs an error if attempted.
- **Cached reflection**: `cachedReflectAll` is a static field that caches MethodInfos after first
  ReflectAll call, avoiding repeated reflection overhead.
- **Builder is temporary**: Designed to be used with Allocator.Temp, created and disposed within
  OnCreate of a system.

## Verified Data

```
FunctionsBuilder<T,TO>
  Kind: struct, 2 generic params
  Methods: Dispose, ReflectAll, Add (x3 overloads), Build, BuildHash
  Uses NativeHashSet<BuildData> internally
  ReflectAll caches MethodInfos statically

FunctionData
  Kind: struct, 32 bytes
  Fields: Void* Target (public), IntPtr DestroyFunction (public), FunctionPointer<ExecuteFunction> ExecuteFunction (public), FunctionPointer<UpdateFunction> UpdateFunction (public)

Verified: 6 checks, 0 failures
```

## Source

- [https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/FunctionsBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/FunctionsBuilder.cs)
