# SwitchBuilder

## Inner Workings Diagram

```
 SwitchBuilder
 ======================================================================
 Defined as: SwitchBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ SwitchBuilder                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    SwitchBuilder                             │
 │ bool                     Expression                                │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ AddCase(string @case)                                              │
 │   → SwitchCaseBuild                                                │
 │ Close()                                                            │
 │   → ICodeWriter                                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/CodeGenHelpers/SwitchBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/SwitchBuilder.cs)
