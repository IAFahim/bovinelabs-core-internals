# DynamicGenerator

## Inner Workings Diagram

```
 DynamicGenerator
 ======================================================================
 Defined as: DynamicGenerator
 Namespace:  BovineLabs.DynamicGenerator

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ DynamicGenerator                                                   │
 ├────────────────────────────────────────────────────────────────────┤
 │ SymbolDisplayFormat      ShortTypeFormat                           │
 │ INamedTypeSymbol         TypeSymbol                                │
 │ string                   TypeName                                  │
 │ DynamicType              Type                                      │
 │ string                   Type1                                     │
 │ string                   Type2                                     │
 │ string                   Type3                                     │
 │ string                   Type4                                     │
 │ string                   Type5                                     │
 │ string                   Type6                                     │
 │ DynamicData              Data                                      │
 │ IReadOnlyList<Diagnost   Diagnostics                               │
 │ DynamicType              Type                                      │
 │ INamedTypeSymbol         InterfaceSymbol                           │
 │ TypeDeclarationSyntax    TypeSyntax                                │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ Initialize(IncrementalGeneratorInitializationContext context)      │
 │   → void                                                           │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/BovineLabs.DynamicGenerator/DynamicGenerator.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/BovineLabs.DynamicGenerator/DynamicGenerator.cs)
