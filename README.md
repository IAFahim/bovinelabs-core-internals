# CodeWriter

## Inner Workings Diagram

```
 CodeWriter
 ======================================================================
 Defined as: CodeWriter
 Namespace:  CodeGenHelpers


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ IncreaseIndent()                                                   │
 │   → void                                                           │
 │ DecreaseIndent()                                                   │
 │   → void                                                           │
 │ Block(string value, params string[] constraints)                   │
 │   → IDisposable                                                    │
 │ BlockWithDelimiter(string value, params string[] constraints)      │
 │   → IDisposable                                                    │
 │ BlockWriter(string? originalLine)                                  │
 │   → ICodeWriter                                                    │
 │ Append(string value)                                               │
 │   → void                                                           │
 │ AppendUnindented(string value)                                     │
 │   → void                                                           │
 │ NewLine()                                                          │
 │   → void                                                           │
 │ AppendLine(string value)                                           │
 │   → void                                                           │
 │ AppendUnindentedLine(string value)                                 │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/CodeGenHelpers/Internals/CodeWriter.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/Internals/CodeWriter.cs)
