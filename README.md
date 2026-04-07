# ConstructorBuilder

## Inner Workings Diagram

```
 ConstructorBuilder
 ======================================================================
 Defined as: ConstructorBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ConstructorBuilder                                                 │
 ├────────────────────────────────────────────────────────────────────┤
 │ IReadOnlyCollection<Pa   Parameters                                │
 │ Accessibility?           AccessModifier                            │
 │ ClassBuilder             Class                                     │
 │ int                      Count                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → ConstructorBuil                                                │
 │ WithInheritDoc(bool inherit = true)                                │
 │   → ConstructorBuil                                                │
 │ WithInheritDoc(string from)                                        │
 │   → ConstructorBuil                                                │
 │ WithParameterDoc(string paramName, string documentation)           │
 │   → ConstructorBuil                                                │
 │ AddAssemblyAttribute(string attribute)                             │
 │   → ConstructorBuil                                                │
 │ AddAttribute(string attribute)                                     │
 │   → ConstructorBuil                                                │
 │ WithBody(Action<ICodeWriter> writerDelegate)                       │
 │   → ConstructorBuil                                                │
 │ WithThisCall()                                                     │
 │   → ConstructorBuil                                                │
 │ WithThisCall(Dictionary<string, string> parameters)                │
 │   → ConstructorBuil                                                │
 │ WithThisCall(IEnumerable<(string Type, string Name)                │
 │   → ConstructorBuil                                                │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/CodeGenHelpers/ConstructorBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/ConstructorBuilder.cs)
