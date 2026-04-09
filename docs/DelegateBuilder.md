# DelegateBuilder

## Inner Workings Diagram

```
 DelegateBuilder
 ======================================================================
 Defined as: DelegateBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ DelegateBuilder                                                    │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   Name                                      │
 │ CodeBuilder              Builder                                   │
 │ Accessibility?           AccessModifier                            │
 │ string?                  ReturnType                                │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → DelegateBuilder                                                │
 │ WithParameterDoc(string paramName, string documentation)           │
 │   → DelegateBuilder                                                │
 │ AddGeneric(string name)                                            │
 │   → DelegateBuilder                                                │
 │ AddGeneric(string name, Action<GenericBuilder> configureBuild)     │
 │   → DelegateBuilder                                                │
 │ MakePublicDelegate()                                               │
 │   → DelegateBuilder                                                │
 │ MakeInternalDelegate()                                             │
 │   → DelegateBuilder                                                │
 │ WithAccessModifier(Accessibility accessModifier)                   │
 │   → DelegateBuilder                                                │
 │ AddAttribute(string attribute)                                     │
 │   → DelegateBuilder                                                │
 │ AddAssemblyAttribute(string attribute)                             │
 │   → DelegateBuilder                                                │
 │ AddNamespaceImport(string importedNamespace)                       │
 │   → DelegateBuilder                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/DelegateBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/DelegateBuilder.cs)
