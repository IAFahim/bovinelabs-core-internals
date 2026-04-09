# ClassBuilder

## Inner Workings Diagram

```
 ClassBuilder
 ======================================================================
 Defined as: ClassBuilder
 Namespace:  CodeGenHelpers

 Structure:
 ┌────────────────────────────────────────────────────────────────────┐
 │ ClassBuilder                                                       │
 ├────────────────────────────────────────────────────────────────────┤
 │ string                   Name                                      │
 │ string                   FullyQualifiedName                        │
 │ IReadOnlyList<Construc   Constructors                              │
 │ IReadOnlyList<Property   Properties                                │
 │ IReadOnlyList<MethodBu   Methods                                   │
 │ IReadOnlyList<ClassBui   NestedClasses                             │
 │ CodeBuilder              Builder                                   │
 │ string?                  BaseClass                                 │
 │ Accessibility?           AccessModifier                            │
 │ TypeKind                 Kind                                      │
 │ bool                     IsStatic                                  │
 │ bool                     IsAbstract                                │
 │ bool                     IsSealed                                  │
 │ bool                     IsReadOnly                                │
 └────────────────────────────────────────────────────────────────────┘

 Key Methods:
 ┌────────────────────────────────────────────────────────────────────┐
 │ WithSummary(string summary)                                        │
 │   → ClassBuilder                                                   │
 │ WithInheritDoc(bool inherit = true)                                │
 │   → ClassBuilder                                                   │
 │ WithInheritDoc(string from)                                        │
 │   → ClassBuilder                                                   │
 │ Sealed()                                                           │
 │   → ClassBuilder                                                   │
 │ ReadOnly(bool isReadOnly = true)                                   │
 │   → ClassBuilder                                                   │
 │ IsStruct()                                                         │
 │   → ClassBuilder                                                   │
 │ OfType(TypeKind kind)                                              │
 │   → ClassBuilder                                                   │
 │ SetBaseClass(string baseClass)                                     │
 │   → ClassBuilder                                                   │
 │ SetBaseClass(INamedTypeSymbol symbol)                              │
 │   → ClassBuilder                                                   │
 │ AddGeneric(string name)                                            │
 │   → ClassBuilder                                                   │
 └────────────────────────────────────────────────────────────────────┘
```

## Verified Data

```
(no type refs)
Verified: 1 checks, 0 failures
```

## Source

- [SourceGenerators~/CodeGenHelpers/ClassBuilder.cs](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/SourceGenerators~/CodeGenHelpers/ClassBuilder.cs)
