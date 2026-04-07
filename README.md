# EnumBuilder

## Inner Workings Diagram

```
 EnumBuilder
 ======================================================================
 Defined as: EnumBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ EnumBuilder                                                        │
 ├────────────────────────────────────────────────────────────────────┤
 │ CodeBuilder              Builder                                   │
 │ string                   Name                                      │
 │ string                   FullyQualifiedName                        │
 │ Accessibility?           AccessModifier                            │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → EnumBuilder                                                    │
 │ WithInheritDoc(bool inherit = true)                                │
 │   → EnumBuilder                                                    │
 │ WithInheritDoc(string from)                                        │
 │   → EnumBuilder                                                    │
 │ AddValue(string name, int? numericValue = null)                    │
 │   → EnumValueBuilde                                                │
 │ AddNamespaceImport(string importedNamespace)                       │
 │   → EnumBuilder                                                    │
 │ AddNamespaceImport(ISymbol symbol)                                 │
 │   → EnumBuilder                                                    │
 │ AddNamespaceImport(INamespaceSymbol symbol)                        │
 │   → EnumBuilder                                                    │
 │ AddAttribute(string attribute)                                     │
 │   → EnumBuilder                                                    │
 │ MakePublicEnum()                                                   │
 │   → EnumBuilder                                                    │
 │ MakeInternalEnum()                                                 │
 │   → EnumBuilder                                                    │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/CodeGenHelpers/EnumBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/EnumBuilder.cs)
