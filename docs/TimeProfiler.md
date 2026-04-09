# TimeProfiler

## Inner Workings Diagram

```
 TimeProfiler
 ======================================================================
 Namespace:  BovineLabs.Core.Utility


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Start(FixedString64Bytes text, LogLevel logLevel = LogLe)          │
 │   → TimeProfiler                                                   │
 │ StartWithMin(FixedString64Bytes text, int min, LogLevel logLeve)   │
 │   → TimeProfiler                                                   │
 │ StartString(string text, LogLevel logLevel = LogLevel.Verbose)     │
 │   → TimeProfiler                                                   │
 │ StartStringWithMin(string text, int min, LogLevel logLevel = LogLevel
 │   → TimeProfiler                                                   │
 │ Dispose()                                                          │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [BovineLabs.Core/Utility/TimeProfiler.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/BovineLabs.Core/Utility/TimeProfiler.cs)
