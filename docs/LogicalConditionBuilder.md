# LogicalConditionBuilder

## Inner Workings Diagram

```
 LogicalConditionBuilder
 ======================================================================
 Defined as: LogicalConditionBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ LogicalConditionBuilder                                            │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    LogicalConditionBuilder                   │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithBody(Action<ICodeWriter> innerWrite)                           │
 │   → LogicalConditio                                                │
 │ ElseIf(string condition)                                           │
 │   → LogicalConditio                                                │
 │ ElseIf(string condition, Action<ICodeWriter> innerWrite)           │
 │   → LogicalConditio                                                │
 │ Else()                                                             │
 │   → LogicalConditio                                                │
 │ Else(Action<ICodeWriter> innerWrite)                               │
 │   → LogicalConditio                                                │
 │ EndIf()                                                            │
 │   → ICodeWriter                                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
LogicalConditionBuilder: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/LogicalConditionBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/LogicalConditionBuilder.cs)
