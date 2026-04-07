# BurstUtil.IsEmpty — Inner Workings

## Overview

BurstUtil provides a thin `[BurstCompile]` wrapper around `EntityQuery.IsEmpty`, enabling
Burst-compiled jobs to check query emptiness directly without leaving Burst context.

```
┌─────────────────────────────────────────────────────────────────┐
│                    BurstUtil (static class)                      │
│                                                                  │
│  ┌──────────────────────┐   ┌────────────────────────────────┐  │
│  │  IsEmpty()            │   │  SetNotBurstCompiled()         │  │
│  │  [BurstCompile]       │   │  [BurstDiscard]                │  │
│  │  ref EntityQuery      │   │  ref bool isBurstCompiled      │  │
│  │    → query.IsEmpty    │   │    → sets false in managed     │  │
│  └──────────────────────┘   └────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## The Problem It Solves

```
  WITHOUT BurstUtil:                          WITH BurstUtil:
  ════════════════════                        ═════════════════

  Bursted Job                                 Bursted Job
  ┌──────────────────┐                        ┌──────────────────┐
  │  Cannot access    │                        │  bool empty =     │
  │  EntityQuery      │                        │    BurstUtil      │
  │  .IsEmpty         │                        │      .IsEmpty(    │
  │  directly!        │                        │        ref query);│
  │                   │                        │                    │
  │  COMPILE ERROR    │                        │  ✅ Works!        │
  └──────────────────┘                        └──────────────────┘
```

## Data Flow

```
  Burst Compiled Job
  ┌─────────────────────────────────────────┐
  │                                         │
  │   EntityQuery myQuery;                  │
  │   bool empty = BurstUtil.IsEmpty(       │
  │       ref myQuery);                     │
  │                                         │
  │   if (empty) { /* no entities */ }      │
  │                                         │
  └───────────────┬─────────────────────────┘
                  │
                  │  [BurstCompile] inlines this:
                  ▼
         ┌─────────────────────┐
         │  return query       │
         │      .IsEmpty;      │
         │                     │
         │  // Direct access   │
         │  // to internal     │
         │  // EntityQuery     │
         │  // state           │
         └─────────────────────┘
```

## SetNotBurstCompiled Helper

```
  Burst Job                         Managed Fallback
  ════════════                      ════════════════

  bool isBurstCompiled = true;      
                                   
  BurstUtil.SetNotBurstCompiled(    
      ref isBurstCompiled);         
                                   
  // In Burst:   isBurstCompiled = true  (method discarded)
  // In managed: isBurstCompiled = false (method executes)
                                   
  if (isBurstCompiled)             
  {                                
      // Burst-specific path       
  }                                
```

## Key Design Decisions

- **Minimal wrapper**: Single line of code (`return query.IsEmpty`) — the entire point
  is just getting the `[BurstCompile]` attribute on the call site.
- **ref parameter**: `EntityQuery` is a struct; passing by ref avoids a copy and matches
  the internal API expectations.
- **BurstDiscard utility**: `SetNotBurstCompiled` uses `[BurstDiscard]` which strips
  the method from Burst builds — the bool stays `true`, allowing runtime detection of
  Burst vs managed execution.

## Source File

- `BovineLabs.Core/Utility/BurstUtil.cs`

## Source

- [BovineLabs.Core/Utility/BurstUtil.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/BurstUtil.cs)
- [BovineLabs.Core.Extensions/Settings/SingletonInitializeSystemGroup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/Settings/SingletonInitializeSystemGroup.cs)
- [BovineLabs.Core.Extensions/LifeCycle/InitializeSystemGroup.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core.Extensions/LifeCycle/InitializeSystemGroup.cs)
