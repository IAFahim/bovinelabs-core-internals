# MethodBuilder

## Inner Workings Diagram

```
 MethodBuilder
 ======================================================================
 Defined as: MethodBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ MethodBuilder                                                      │
 ├────────────────────────────────────────────────────────────────────┤
 │ IReadOnlyCollection<Pa   Parameters                                │
 │ string                   Name                                      │
 │ string?                  ReturnType                                │
 │ bool                     IsAsync                                   │
 │ bool                     IsAbstract                                │
 │ bool                     HasBody                                   │
 │ ClassBuilder             Class                                     │
 │ Accessibility?           AccessModifier                            │
 │ bool                     IsStatic                                  │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → MethodBuilder                                                  │
 │ WithInheritDoc(bool inherit = true)                                │
 │   → MethodBuilder                                                  │
 │ WithInheritDoc(string from)                                        │
 │   → MethodBuilder                                                  │
 │ WithParameterDoc(string paramName, string documentation)           │
 │   → MethodBuilder                                                  │
 │ AddGeneric(string name)                                            │
 │   → MethodBuilder                                                  │
 │ AddGeneric(string name, Action<GenericBuilder> configureBuild)     │
 │   → MethodBuilder                                                  │
 │ AddNamespaceImport(string importedNamespace)                       │
 │   → MethodBuilder                                                  │
 │ AddNamespaceImport(ISymbol symbol)                                 │
 │   → MethodBuilder                                                  │
 │ AddNamespaceImport(INamespaceSymbol symbol)                        │
 │   → MethodBuilder                                                  │
 │ AddAssemblyAttribute(string attribute)                             │
 │   → MethodBuilder                                                  │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/MethodBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/MethodBuilder.cs)
