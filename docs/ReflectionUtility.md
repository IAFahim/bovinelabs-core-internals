# ReflectionUtility — Inner Workings

## Overview

ReflectionUtility caches assembly, type, and method lookups in static dictionaries
and lazy arrays to avoid the significant overhead of repeated reflection calls at
runtime.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                 ReflectionUtility (static class)                            │
│                                                                             │
│  ┌─ Caches ──────────────────────────────────────────────────────────────┐  │
│  │  Dictionary<Assembly, Type[]>  AssemblyTypes                         │  │
│  │  Dictionary<Assembly, Type[]>  AssemblyNonGenericTypes               │  │
│  │  Dictionary<Assembly, MethodInfo[]> AssemblyMethods                  │  │
  │  Assembly[] allAssemblies (lazy)                                      │  │
│  │  Type[] allTypes (lazy)              Type[] allUnmanagedTypes (lazy) │  │
│  │  Type[] allTypesWithImplementation (lazy)                            │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  GetTypes(asm)  GetNonGenericTypes(asm)  GetMethods(asm)                    │
│  GetAllImplementations<T>()  GetAllWithAttribute<T>()                       │
│  GetCustomImplementation<T,TD>()  GetAllWithGenericDefinition()            │
│  GetMethodsWithAttribute<T>()  IsAssemblyReferencingAssembly()             │
│  GetFieldInBase()  GetAllAssemblyAttributes<T>()                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Lazy Initialization Chain

```
  AllAssemblies (accessed first)
         │
         ▼  allAssemblies ??= AppDomain.CurrentDomain.GetAssemblies()
         │
         ├──── AllTypes
         │       │  allTypes ??= AllAssemblies.SelectMany(GetTypes)
         │       │
         │       ├── AllUnmanagedTypes
         │       │     allUnmanagedTypes ??= AllTypes.Where(IsUnmanaged)
         │       │
         │       └── AllTypesWithImplementation
         │             │  allTypesWithImpl ??= AllTypes.Where(!abstract && !interface)
         │             │
         │             └── AllTypesWithImplementationNoGeneric
         │                   ??= above.Where(!ContainsGenericParameters)
         │
         └──── Per-Assembly caches (on demand)
                 AssemblyTypes[asm]     → asm.GetTypes() with error handling
                 AssemblyNonGenericTypes → filtered for non-generic
                 AssemblyMethods[asm]    → all methods via reflection

  ┌───────────────────────────────────────────────────────────────┐
  │  CACHE LAYER:                                                 │
  │                                                               │
  │  Assembly ──► Dictionary lookup ──► Cached Type[]            │
  │              (O(1) after first call)  (no reflection needed)  │
  │                                                               │
  │  First call:  reflection + LINQ + ToArray (expensive)        │
  │  Subsequent:  dictionary lookup only (O(1))                   │
  └───────────────────────────────────────────────────────────────┘
```

## GetTypes — Error Handling

```
  GetTypes(assembly)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────────┐
  │  if AssemblyTypes.TryGetValue(assembly, out types):               │
  │    return types  ← CACHE HIT                                     │
  │                                                                   │
  │  CACHE MISS:                                                      │
  │  try:                                                             │
  │    types = assembly.GetTypes()                                    │
  │  catch ReflectionTypeLoadException:                               │
  │    BLGlobalLogger.LogWarningString(...)                           │
  │    types = Array.Empty<Type>()  ← graceful degradation            │
  │                                                                   │
  │  AssemblyTypes[assembly] = types  ← STORE IN CACHE                │
  │  return types                                                     │
  └───────────────────────────────────────────────────────────────────┘
```

## Assembly Reference Filtering

```
  GetAllImplementations<T>()
         │
         ▼
  ┌───────────────────────────────────────────────────────────────────┐
  │  coreAssembly = typeof(T).Assembly                                │
  │                                                                   │
  │  AllAssemblies                                                    │
  │    .Where(asm => asm.IsAssemblyReferencingAssembly(coreAssembly)) │
  │    // ↑ Quick filter: skip assemblies that can't possibly        │
  │    //   contain types implementing T                             │
  │    .SelectMany(asm => GetTypes(asm))                              │
  │    .Where(t => !t.IsAbstract && !t.IsInterface)                   │
  │    .Where(t => typeof(T).IsAssignableFrom(t))                     │
  │                                                                   │
  │  ┌─────────────────────────────────────────────────────────────┐  │
  │  │  IsAssemblyReferencingAssembly check:                        │  │
  │  │    assembly == reference?  → YES (same assembly)            │  │
  │  │    assembly references reference? → check GetName list      │  │
  │  │    This eliminates 90%+ of loaded assemblies quickly         │  │
  │  └─────────────────────────────────────────────────────────────┘  │
  └───────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Lazy statics**: All collections use `??=` pattern — computed on first access,
  then cached forever. Zero startup cost if unused.
- **Error-tolerant**: `ReflectionTypeLoadException` is caught and logged rather
  than crashing — important for assemblies with missing dependencies.
- **Assembly reference filtering**: Before scanning types, checks if an assembly
  even references the target assembly — avoids loading types from irrelevant assemblies.
- **Editor vs Player**: Uses `TypeCache.GetMethodsWithAttribute<T>()` in Editor
  (faster Unity-native cache) and manual reflection in Player builds.

## Source File

- `BovineLabs.Core/Utility/ReflectionUtility.cs`

## Source

- [BovineLabs.Core/Utility/ReflectionUtility.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/ReflectionUtility.cs)
- [BovineLabs.Core.Tests/Utility/ReflectionUtilityTests.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Tests/Utility/ReflectionUtilityTests.cs)
- [BovineLabs.Core/Functions/FunctionsBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Functions/FunctionsBuilder.cs)
