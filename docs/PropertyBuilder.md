# PropertyBuilder

## Inner Workings Diagram

```
 PropertyBuilder
 ======================================================================
 Defined as: PropertyBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ PropertyBuilder                                                    │
 ├────────────────────────────────────────────────────────────────────┤
 │ FieldType                FieldTypeValue                            │
 │ ValueType                PropertyValueType                         │
 │ string                   Name                                      │
 │ string?                  Type                                      │
 │ ClassBuilder             Class                                     │
 │ Accessibility?           AccessModifier                            │
 │ bool                     IsStatic                                  │
 │ bool                     IsRef                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → PropertyBuilder                                                │
 │ WithInheritDoc(bool inherit = true)                                │
 │   → PropertyBuilder                                                │
 │ WithInheritDoc(string from)                                        │
 │   → PropertyBuilder                                                │
 │ AddNamespaceImport(string importedNamespace)                       │
 │   → PropertyBuilder                                                │
 │ AddNamespaceImport(ISymbol symbol)                                 │
 │   → PropertyBuilder                                                │
 │ AddNamespaceImport(INamespaceSymbol symbol)                        │
 │   → PropertyBuilder                                                │
 │ SetType(string type)                                               │
 │   → PropertyBuilder                                                │
 │ SetType(INamedTypeSymbol symbol)                                   │
 │   → PropertyBuilder                                                │
 │ SetType(Type type)                                                 │
 │   → PropertyBuilder                                                │
 │ SetWarning(string warning)                                         │
 │   → PropertyBuilder                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/PropertyBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/PropertyBuilder.cs)
