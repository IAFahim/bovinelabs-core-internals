# RecordBuilder

## Inner Workings Diagram

```
 RecordBuilder
 ======================================================================
 Defined as: RecordBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ RecordBuilder                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   Name                                      │
 │ string                   FullyQualifiedName                        │
 │ CodeBuilder              Builder                                   │
 │ Accessibility?           AccessModifier                            │
 │ RecordPropertyType       PropertyType                              │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → RecordBuilder                                                  │
 │ WithInheritDoc(bool inherit = true)                                │
 │   → RecordBuilder                                                  │
 │ WithInheritDoc(string from)                                        │
 │   → RecordBuilder                                                  │
 │ UsePositionalProperties()                                          │
 │   → RecordBuilder                                                  │
 │ UseInitProperties()                                                │
 │   → RecordBuilder                                                  │
 │ MakePublicRecord()                                                 │
 │   → RecordBuilder                                                  │
 │ MakeInternalRecord()                                               │
 │   → RecordBuilder                                                  │
 │ WithAccessModifier(Accessibility accessModifier)                   │
 │   → RecordBuilder                                                  │
 │ Build()                                                            │
 │   → string                                                         │
 │ AddNamespaceImport(string importedNamespace)                       │
 │   → RecordBuilder                                                  │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/CodeGenHelpers/RecordBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/RecordBuilder.cs)
