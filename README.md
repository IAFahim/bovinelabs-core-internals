# LibraryLoader — Inner Workings

## Overview

LibraryLoader wraps platform-specific native DLL loading APIs (dlopen/LoadLibrary/etc.)
into a unified cross-platform interface for Unity ECS projects, supporting Windows,
Linux, macOS, Android, and iOS.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    LibraryLoader (static class)                              │
│                                                                             │
│  LoadLibrary(name) → IntPtr handle                                          │
│  GetSymbol(handle, name) → IntPtr symbol                                    │
│  GetSymbolDelegate<T>(handle, name) → T delegate                            │
│  FreeLibrary(handle) → void                                                 │
│                                                                             │
│         │                                                                    │
│         ▼                                                                    │
│  ┌─────────────┬──────────────┬──────────────┬───────────┬──────────┐      │
│  │   Win32     │    Linux     │     Mac      │  Android  │   iOS    │      │
│  │ Kernel32    │  libdl.so    │ libSystem    │  __Internal│ __Internal│     │
│  │ .dll        │  libdl.so.2  │  .dylib      │           │          │      │
│  └─────────────┴──────────────┴──────────────┴───────────┴──────────┘      │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Platform Dispatch Architecture

```
  LoadLibrary("mylib.so")
         │
         ▼
  ┌───────────────────────────────────────────────────────────────────────┐
  │  switch (Application.platform)                                        │
  │                                                                       │
  │  ┌──────────────────────────────────┐  ┌───────────────────────────┐  │
  │  │ Windows*:                        │  │ Linux*:                   │  │
  │  │   Win32.LoadLibrary("mylib.so")  │  │   Linux.dlopen("mylib.so")│  │
  │  │   → Kernel32.dll P/Invoke        │  │   → libdl.so P/Invoke     │  │
  │  │   → returns HMODULE              │  │   → try libdl.so.2 first  │  │
  │  └──────────────────────────────────┘  │   → fallback libdl.so     │  │
  │                                        └───────────────────────────┘  │
  │  ┌──────────────────────────────────┐  ┌───────────────────────────┐  │
  │  │ macOS*:                          │  │ Android:                  │  │
  │  │   Mac.dlopen("mylib.so")         │  │   Android.dlopen("mylib") │  │
  │  │   → /usr/lib/libSystem.dylib     │  │   → __Internal P/Invoke   │  │
  │  └──────────────────────────────────┘  └───────────────────────────┘  │
  │  ┌──────────────────────────────────┐                                  │
  │  │ iOS:                             │  * = Editor + Player + Server  │
  │  │   iOS.dlopen("mylib")            │                                  │
  │  │   → __Internal P/Invoke          │                                  │
  │  └──────────────────────────────────┘                                  │
  │  else: throw PlatformNotSupportedException                              │
  └───────────────────────────────────────────────────────────────────────┘
```

## Linux Fallback Mechanism

```
  Linux.dlopen(path)
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  1. Try: dlopen2(path, RTLD_LAZY) via libdl.so.2             │
  │     └── succeeds? → return handle                             │
  │                                                               │
  │  2. Catch DllNotFoundException:                               │
  │     useSystemLibrary2 = false                                 │
  │     → dlopen1(path, RTLD_LAZY) via libdl.so                  │
  │     → some distros only have libdl.so.2, some only libdl.so  │
  │                                                               │
  │  All subsequent calls (dlsym, dlclose) check flag:            │
  │    useSystemLibrary2 ? dlsym2 : dlsym1                        │
  └───────────────────────────────────────────────────────────────┘
```

## Native API Mapping Table

```
  ┌────────────┬───────────────────┬───────────────────┬──────────────────┐
  │  Operation │ Windows           │ Linux/macOS       │ Android/iOS      │
  │            │                   │                   │                  │
  │  Load      │ LoadLibrary()     │ dlopen()          │ dlopen()         │
  │            │ Kernel32.dll      │ libdl.so          │ __Internal       │
  │            │                   │ /usr/lib/         │                  │
  │            │                   │ libSystem.dylib   │                  │
  ├────────────┼───────────────────┼───────────────────┼──────────────────┤
  │  Get Sym   │ GetProcAddress()  │ dlsym()           │ dlsym()          │
  ├────────────┼───────────────────┼───────────────────┼──────────────────┤
  │  Free      │ FreeLibrary()     │ dlclose()         │ dlclose()        │
  └────────────┴───────────────────┴───────────────────┴──────────────────┘

  Flags:
    RTLD_LAZY = 1  (resolve symbols on first use)
    RTLD_NOW  = 2  (resolve all symbols immediately)
```

## GetSymbolDelegate Flow

```
  GetSymbolDelegate<FuncPtr>(handle, "my_function")
         │
         ▼
  ┌───────────────────────────────────────────────────────────────┐
  │  1. symbol = GetSymbol(handle, "my_function")                 │
  │     └── returns IntPtr (raw function pointer)                 │
  │                                                               │
  │  2. if symbol == IntPtr.Zero:                                 │
  │       throw EntryPointNotFoundException                       │
  │                                                               │
  │  3. return Marshal.GetDelegateForFunctionPointer<FuncPtr>     │
  │     (symbol)                                                  │
  │     └── Wraps native function pointer as managed delegate     │
  └───────────────────────────────────────────────────────────────┘
```

## Conditional Compilation

```
  ┌────────────────────────────────────────────────────────────────────┐
  │  Android & iOS:                                                    │
  │                                                                    │
  │  #if UNITY_ANDROID / UNITY_IOS                                     │
  │    [DllImport("__Internal")] extern IntPtr dlopen(...)              │
  │    // Links directly into the native binary                        │
  │  #else                                                             │
  │    // Stub implementations return default/0                        │
  │    // (compiles on non-target platforms without errors)            │
  │  #endif                                                            │
  └────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

- **Platform detection at runtime**: Uses `Application.platform` switch rather than
  `#if` platform defines, so a single binary handles all platforms.
- **Linux dual-library**: Tries `libdl.so.2` first (modern), falls back to `libdl.so`
  with a static flag for subsequent calls.
- **Stubs for mobile**: Non-mobile builds get no-op stubs for Android/iOS APIs to
  ensure compilation without conditional errors.
- **Delegate creation**: `GetSymbolDelegate<T>` bridges native function pointers to
  managed delegates via `Marshal.GetDelegateForFunctionPointer`.

## Source File

- `BovineLabs.Core/Utility/LibraryLoader.cs`
