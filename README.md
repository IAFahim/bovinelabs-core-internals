# CommandLineArgs — Inner Workings

## Overview

CommandLineArgs caches the full CLI argument array once at static initialization time,
then provides instant lookup without re-allocating string arrays on each call.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CommandLineArgs (static class)                         │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  static readonly List<string> Args                                  │    │
│  │  = new List<string>(Environment.GetCommandLineArgs())               │    │
│  │                                                                     │    │
│  │  Initialized ONCE at class load time, never re-created              │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                             │
│  ┌──────────────────────┐    ┌───────────────────────────────────────┐      │
│  │ TryGetArgument()     │    │ Contains()                            │      │
│  │ arg → index lookup   │    │ arg → bool                            │      │
│  │ value = next element │    │   List.Contains(arg)                  │      │
│  └──────────────────────┘    └───────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Memory Layout — Static Cache

```
  Environment.GetCommandLineArgs()
         │
         ▼  (called ONCE, at static init)
  ┌─────────────────────────────────────────────────────────────┐
  │  static List<string> Args (readonly)                        │
  │                                                             │
  │  Index:  0          1         2           3         4       │
  │        ┌──────────┬────────┬──────────┬────────┬─────────┐  │
  │        │ app.exe  │ -port  │ 8080     │ -mode  │ server  │  │
  │        └──────────┴────────┴──────────┴────────┴─────────┘  │
  │                                                             │
  │  Shared across ALL callers for app lifetime                 │
  └─────────────────────────────────────────────────────────────┘
```

## TryGetArgument Flow

```
  TryGetArgument("-port", out value)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  Step 1: idx = Args.IndexOf("-port")                          │
  │                                                               │
  │    Args: [app.exe, -port, 8080, -mode, server]                │
  │                    ^^^^                                        │
  │                    idx = 1                                     │
  │                                                               │
  │  Step 2: idx >= 0 ? YES                                       │
  │                                                               │
  │  Step 3: value = Args[idx + 1]                                │
  │                        ──────                                 │
  │    Args: [app.exe, -port, 8080, -mode, server]                │
  │                            ^^^^                               │
  │                            value = "8080"                     │
  │                                                               │
  │  Step 4: return true                                          │
  └───────────────────────────────────────────────────────────────┘

  Edge case: "-port" is last argument
  ┌───────────────────────────────────────────────────────────────┐
  │  idx = 4, but idx < Args.Count - 1 is FALSE                  │
  │  value = string.Empty                                         │
  │  return true  (flag exists but has no value)                  │
  └───────────────────────────────────────────────────────────────┘

  Not found:
  ┌───────────────────────────────────────────────────────────────┐
  │  idx = -1                                                     │
  │  value = string.Empty                                         │
  │  return false                                                 │
  └───────────────────────────────────────────────────────────────┘
```

## Contains Flow

```
  Contains("-mode")
         │
         ▼
  ┌───────────────────────────────────────┐
  │  return Args.Contains("-mode");       │
  │                                       │
  │  Args: [app.exe, -port, 8080, -mode]  │
  │                                ^^^^^  │
  │                                found  │
  │  → true                              │
  └───────────────────────────────────────┘
```

## Key Design Decisions

- **One-time allocation**: The argument list is built once at class load via
  `Environment.GetCommandLineArgs()` and stored as `static readonly`.
- **No per-call allocation**: Unlike calling `Environment.GetCommandLineArgs()`
  repeatedly (which allocates a new `string[]` every time), this caches forever.
- **List.IndexOf**: O(n) scan but CLI args are typically < 20 entries — negligible cost.
- **Value is next element**: Convention is `--flag value` where value is simply
  the adjacent element; returns `string.Empty` if the flag is the last entry.

## Source File

- `BovineLabs.Core/Utility/CommandLineArgs.cs`

## Source

- [BovineLabs.Core/Utility/CommandLineArgs.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/CommandLineArgs.cs)
- [BovineLabs.Core/ConfigVars/ConfigVarManager.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/ConfigVars/ConfigVarManager.cs)
