# ExpressionBlockBuilder

## Inner Workings Diagram

```
 ExpressionBlockBuilder
 ======================================================================
 Defined as: ExpressionBlockBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ExpressionBlockBuilder                                             │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    ExpressionBlockBuilder                    │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithBody(Action<ICodeWriter> innerWrite)                           │
 │   → ICodeWriter                                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
ExpressionBlockBuilder: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/ExpressionBlockBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/ExpressionBlockBuilder.cs)
