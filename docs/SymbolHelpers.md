# SymbolHelpers

## Inner Workings Diagram

```
 SymbolHelpers
 ======================================================================
 Namespace:  CodeGenHelpers.Internals


 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ GetGloballyQualifiedTypeName(INamespaceOrTypeSymbol symbol)        │
 │   → string                                                         │
 │ GetFullMetadataName(INamespaceOrTypeSymbol symbol)                 │
 │   → string                                                         │
 │ GetFullName(INamespaceOrTypeSymbol type)                           │
 │   → string                                                         │
 │ GetQualifiedTypeName(ITypeSymbol typeSymbol)                       │
 │   → string                                                         │
 │ IsNullable(ITypeSymbol type)                                       │
 │   → bool                                                           │
 │ IsNullable(ITypeSymbol type, out ITypeSymbol? nullableType)        │
 │   → bool                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/Internals/SymbolHelpers.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/Internals/SymbolHelpers.cs)
