# EventBuilder

## Inner Workings Diagram

```
 EventBuilder
 ======================================================================
 Defined as: EventBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ EventBuilder                                                       │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   Name                                      │
 │ ClassBuilder             Class                                     │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ MakeStatic(bool isStatic = true)                                   │
 │   → EventBuilder                                                   │
 │ WithBackingField(string backingField)                              │
 │   → ClassBuilder                                                   │
 │ WithAddHandler(Action<ICodeWriter> addDelegate)                    │
 │   → EventBuilder                                                   │
 │ WithAddExpression(string addDelegateExpression)                    │
 │   → EventBuilder                                                   │
 │ WithRemoveHandler(Action<ICodeWriter> addDelegate)                 │
 │   → EventBuilder                                                   │
 │ WithRemoveExpression(string addDelegateExpression)                 │
 │   → EventBuilder                                                   │
 │ WithExplicitImplementation(string @inferfaceName)                  │
 │   → EventBuilder                                                   │
 │ WithDelegateHandler(string handlerType)                            │
 │   → EventBuilder                                                   │
 │ WithDelegateHandler(INamedTypeSymbol handlerType)                  │
 │   → EventBuilder                                                   │
 │ WithAccessibility(Accessibility accessibility)                     │
 │   → EventBuilder                                                   │
 └────────────────────────────────────────────────────────────────────┘
```

## Source

- [SourceGenerators~/CodeGenHelpers/EventBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/EventBuilder.cs)
