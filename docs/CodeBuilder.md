# CodeBuilder

## Inner Workings Diagram

```
 CodeBuilder
 ======================================================================
 Defined as: CodeBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ CodeBuilder                                                        │
 ├────────────────────────────────────────────────────────────────────┤
 │ class                    CodeBuilder                               │
 │ string?                  Namespace                                 │
 │ IndentStyle              IndentStyle                               │
 │ IReadOnlyList<ClassBui   Classes                                   │
 │ IReadOnlyList<RecordBu   Records                                   │
 │ IReadOnlyList<EnumBuil   Enums                                     │
 │ IReadOnlyList<Delegate   Delegates                                 │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ TopLevelNamespace()                                                │
 │   → CodeBuilder                                                    │
 │ TopLevelNamespace(bool topLevel)                                   │
 │   → CodeBuilder                                                    │
 │ Nullable()                                                         │
 │   → CodeBuilder                                                    │
 │ Nullable(NullableState nullable)                                   │
 │   → CodeBuilder                                                    │
 │ CreateInGlobalNamespace(IndentStyle indentStyle = IndentStyle.Spaces)
 │   → CodeBuilder                                                    │
 │ Create(string clrNamespace, IndentStyle indentStyle = Ind)         │
 │   → CodeBuilder                                                    │
 │ Create(INamespaceSymbol namespaceSymbol, IndentStyle inde)         │
 │   → CodeBuilder                                                    │
 │ Create(ITypeSymbol typeSymbol, IndentStyle indentStyle = )         │
 │   → ClassBuilder                                                   │
 │ AddNamespaceImport(string importedNamespace)                       │
 │   → CodeBuilder                                                    │
 │ AddNamespaceImport(ISymbol symbol)                                 │
 │   → CodeBuilder                                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
CodeBuilder: TYPE NOT FOUND
Verified: 0 checks, 1 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/CodeBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/CodeBuilder.cs)
